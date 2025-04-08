namespace Core.Model;

public record ExifAnalysisResult(    
    int NumberOfFilesWithExifDate,
    DateTime MinimumExifDate,
    DateTime MaximumExifDate
    );