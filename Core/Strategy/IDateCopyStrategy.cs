using Core.Model;

namespace Core.Strategy;

public interface IDateCopyStrategy
{
    Task<List<ProcessItemResult>> CopyDate(List<string> fileList, ParallelOptions parallelOptions, DateTime defaultDateTime,
        IProgress<int> progress, ExifDateTag exifDateTag, CancellationToken ct);
}