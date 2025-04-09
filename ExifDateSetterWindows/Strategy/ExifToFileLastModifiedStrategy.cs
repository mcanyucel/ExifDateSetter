using Core.Strategy;

namespace ExifDateSetterWindows.Strategy;

public class ExifToFileLastModifiedStrategy : IDateCopyStrategy
{
    public Task<List<string>> CopyDate(List<string> fileList, ParallelOptions parallelOptions, DateTime defaultDateTime, IProgress<int> progress, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}