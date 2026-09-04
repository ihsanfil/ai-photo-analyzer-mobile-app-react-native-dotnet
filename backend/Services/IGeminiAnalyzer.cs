using AIPhotoAnalyzer.Api.Models;
namespace AIPhotoAnalyzer.Api.Services;
public interface IGeminiAnalyzer { Task<AnalysisResult> AnalyzeAsync(Stream image, string contentType, CancellationToken cancellationToken); }
