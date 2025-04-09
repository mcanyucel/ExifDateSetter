namespace Core.Model;

public record AnalysisResult(int ProcessedFileCount, FileAnalysisResult FileAnalysisResult, ExifAnalysisResult ExifAnalysisResult);