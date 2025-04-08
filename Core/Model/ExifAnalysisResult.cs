namespace Core.Model;

public record ExifAnalysisResult(    
    int NumberOfFilesWithExifDate,
    DateOnly MinimumExifDate,
    DateOnly MaximumExifDate
    );