namespace AIPhotoAnalyzer.Api.Models;
public sealed record AnalysisResult(int OverallScore, int CompositionScore, int LightingScore, int QualityScore, string Summary, string[] Strengths, string[] Improvements);
public sealed class GeminiOptions { public string ApiKey { get; set; } = ""; public string Model { get; set; } = "gemini-2.0-flash"; }
