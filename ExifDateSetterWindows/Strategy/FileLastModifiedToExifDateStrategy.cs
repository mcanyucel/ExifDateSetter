using System.Collections.Concurrent;
using Core.Model;
using Core.Service;
using Core.Strategy;

namespace ExifDateSetterWindows.Strategy;

public class FileLastModifiedToExifDateStrategy(IExifService exifService, IFileService fileService, IProgressService progressService) : IDateCopyStrategy
{
    public Task<List<string>> CopyDate(List<string> fileList, ParallelOptions parallelOptions, DateTime defaultDateTime, IProgress<int> progress, ExifDateTag exifDateTag,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        
        ConcurrentBag<string> results = [];
        var processedFileCount = 0;
        var totalFilesCount = fileList.Count;
        
        Parallel.ForEachAsync(fileList, parallelOptions, async (filePath, _) =>
        {
            var fileDate = await fileService.ExtractFileDateModified(filePath);
            var trueDate = fileDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) ?? defaultDateTime;
            var result = await exifService.SetExifDateTag(filePath, trueDate, ExifDateTag.DateTimeOriginal);
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