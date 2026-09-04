using System.Text.Json;
using AIPhotoAnalyzer.Api.Models;
using AIPhotoAnalyzer.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Configuration.AddEnvironmentVariables();
builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("Gemini"));
builder.Services.AddHttpClient<IGeminiAnalyzer, GeminiAnalyzer>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseExceptionHandler(error => error.Run(async context =>
{
    context.Response.StatusCode = 500; context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(new { error = "Unexpected server error." });
}));
app.UseCors();
app.MapGet("/swagger", () => Results.Ok(new { openapi = "3.0.0", info = new { title = "AI Photo Analyzer API", version = "1.0" }, paths = new { health = "/api/health", analyzePhoto = "/api/analyze/photo" } }));

app.MapGet("/api/health", (IConfiguration config) => Results.Ok(new { status = "ok", demoMode = string.IsNullOrWhiteSpace(config["Gemini:ApiKey"] ?? config["GEMINI_API_KEY"]) }));
app.MapPost("/api/analyze/photo", async (HttpRequest request, IGeminiAnalyzer analyzer, CancellationToken ct) =>
{
    if (!request.HasFormContentType) return Results.BadRequest(new { error = "Content-Type must be multipart/form-data." });
    var form = await request.ReadFormAsync(ct); var file = form.Files.GetFile("photo");
    if (file is null || file.Length == 0) return Results.BadRequest(new { error = "A photo field is required." });
    if (file.Length > 10 * 1024 * 1024 || !file.ContentType.StartsWith("image/")) return Results.BadRequest(new { error = "Use an image up to 10 MB." });
    await using var stream = file.OpenReadStream();
    try
    {
        var result = await analyzer.AnalyzeAsync(stream, file.ContentType, ct);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}).DisableAntiforgery();
app.Run();
