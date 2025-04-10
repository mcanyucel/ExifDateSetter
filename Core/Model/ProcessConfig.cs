namespace Core.Model;

public record ProcessConfig(
    ActionType ActionType,
    DateTime DefaultDateTime,
    AnalyzeConfig AnalyzeConfig
    );