namespace Core.Model;

public record FileAnalysisResult(
    DateOnly MinimumFileDate,
    DateOnly MaximumFileDate
    );