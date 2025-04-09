using Core.Strategy;

namespace ExifDateSetterWindows.Strategy;

public class FileLastModifiedToExifDateStrategy : IDateCopyStrategy
{
    public Task<List<string>> CopyDate(List<string> fileList, ParallelOptions parallelOptions, DateTime defaultDateTime, IProgress<int> progress, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}