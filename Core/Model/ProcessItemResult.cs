namespace Core.Model;

public record ProcessItemResult(
    string FilePath,
    DateTime FinalDateTime,
    bool IsSet);