using System.Collections.Concurrent;
using Core.Model;
using Core.Service;
using Core.Strategy;

namespace ExifDateSetterWindows.Strategy;

public class ExifToFileCreationStrategy(IExifService exifService, IFileService fileService, IProgressService progressService) : IDateCopyStrategy
{
    public Task<List<string>> CopyDate(List<string> fileList, ParallelOptions parallelOptions, DateTime defaultDateTime, IProgress<int> progress, ExifDateTag exifDateTag, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        
        ConcurrentBag<string> results = [];
        var processedFileCount = 0;
        var totalFilesCount = fileList.Count;
        
        Parallel.ForEachAsync(fileList, parallelOptions, async (filePath, ct) =>
        {
            var exifDate = await exifService.ExtractExifDateTag(filePath, exifDateTag);
            var trueDate = exifDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) ?? defaultDateTime;
            var result = await fileService.SetFileDateCreated(filePath, trueDate);
            results.Add($"File Path: {filePath}, Date: {trueDate}, Result: {result}");
            var current = Interlocked.Increment(ref processedFileCount);
            if (progressService.ShouldReportProgress(current, totalFilesCount))
            {
                var currentPercentage = (int) Math.Round((double) current / totalFilesCount * 100);
                progress.Report(currentPercentage);
            }
        });
        return Task.FromResult(results.ToList());
    }
}