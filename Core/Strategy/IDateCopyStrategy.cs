namespace Core.Strategy;

public interface IDateCopyStrategy
{
    Task<List<string>> CopyDate(List<string> fileList, ParallelOptions parallelOptions, DateTime defaultDateTime,
        IProgress<int> progress, CancellationToken ct);
}