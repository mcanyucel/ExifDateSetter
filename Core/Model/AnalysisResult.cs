namespace Core.Model;

public record AnalysisResult(
    int ProcessedFileCount,
    FileAnalysisResult FileAnalysisResult,
    ExifAnalysisResult ExifAnalysisResult)
{
    public string Summarize()
    {
        return $"Analyzed {ProcessedFileCount} files\n" +
                                    $"File Date Range: {FileAnalysisResult.MinimumFileDate} - {FileAnalysisResult.MaximumFileDate}\n" +
                                    $"Number of files with Exif date: {ExifAnalysisResult.NumberOfFilesWithExifDate}\n" +
                                    $"Exif Date Range: {ExifAnalysisResult.MinimumExifDate} - {ExifAnalysisResult.MaximumExifDate}\n";
    }
}