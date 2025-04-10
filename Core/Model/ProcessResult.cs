namespace Core.Model;

public record ProcessResult(
    int SuccessCount,
    int FailureCount
)
{
    public string Summarize()
    {
        return $"Processed {SuccessCount + FailureCount} files\n" +
               $"{SuccessCount} files updated\n" +
               $"{FailureCount} files failed to update\n";
    }
}