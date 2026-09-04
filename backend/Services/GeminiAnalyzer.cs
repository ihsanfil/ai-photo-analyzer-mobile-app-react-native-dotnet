using System.Net.Http.Json;
using System.Text.Json;
using AIPhotoAnalyzer.Api.Models;
using Microsoft.Extensions.Options;
namespace AIPhotoAnalyzer.Api.Services;
public sealed class GeminiAnalyzer(HttpClient http, IOptions<GeminiOptions> options, ILogger<GeminiAnalyzer> log) : IGeminiAnalyzer
{
    private static readonly AnalysisResult Demo = new(82,78,86,81,"A strong image with balanced visual intent and room for a little more refinement.", ["Clear subject separation","Pleasant light","Intentional framing"],["Try a slightly cleaner background","Leave more breathing room around the subject"]);
    public async Task<AnalysisResult> AnalyzeAsync(Stream image, string contentType, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ApiKey))
            throw new InvalidOperationException("Gemini API key yapılandırılmamış.");
        using var ms = new MemoryStream(); await image.CopyToAsync(ms, ct);
        var prompt = "Analyze this photo. Return ONLY valid JSON with integer scores 0-100: overallScore, compositionScore, lightingScore, qualityScore, summary, strengths (array), improvements (array). Be concise and constructive.";
        var body = new { contents = new[] { new { parts = new object[] { new { text = prompt }, new { inline_data = new { mime_type = contentType, data = Convert.ToBase64String(ms.ToArray()) } } } } }, generationConfig = new { responseMimeType = "application/json" } };
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/models/{options.Value.Model}:generateContent");
                request.Headers.Add("x-goog-api-key", options.Value.ApiKey);
                request.Content = JsonContent.Create(body);
                using var response = await http.SendAsync(request, ct);
                var responseBody = await response.Content.ReadAsStringAsync(ct);
                if (!response.IsSuccessStatusCode)
                {
                    if (attempt < 3 && ((int)response.StatusCode == 429 || (int)response.StatusCode >= 500))
                    { await Task.Delay(TimeSpan.FromSeconds(attempt * 2), ct); continue; }
                    throw new InvalidOperationException($"Gemini API {(int)response.StatusCode} döndürdü: {responseBody[..Math.Min(responseBody.Length, 500)]}");
                }
                using var doc = JsonDocument.Parse(responseBody);
                var text = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "{}";
                var result = JsonSerializer.Deserialize<AnalysisResult>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result ?? throw new InvalidOperationException("Gemini boş analiz döndürdü.");
            }
            catch (HttpRequestException ex) when (attempt < 3)
            {
                log.LogWarning(ex, "Gemini request failed; retry {Attempt}/3", attempt);
                await Task.Delay(TimeSpan.FromSeconds(attempt * 2), ct);
            }
        }
        throw new InvalidOperationException("Gemini API üç denemede de yanıt vermedi.");
    }
}
