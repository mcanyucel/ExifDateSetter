using System.Collections.Concurrent;
using Core.Model;
using Core.Service;
using Core.Strategy;

namespace ExifDateSetterWindows.Strategy;

public class ExifToFileCreationStrategy(IExifService exifService, IFileService fileService, IProgressService progressService) : IDateCopyStrategy
{
    public async Task<List<ProcessItemResult>> CopyDate(List<string> fileList, ParallelOptions parallelOptions, DateTime defaultDateTime, 
        IProgress<int> progress, ExifDateTag exifDateTag, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        
        ConcurrentBag<ProcessItemResult> results = [];
        var processedFileCount = 0;
        var totalFilesCount = fileList.Count;
        
        await Parallel.ForEachAsync(fileList, parallelOptions, async (filePath, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            System.Diagnostics.Debug.WriteLine($"Processing {filePath}");
            var exifDate = await exifService.ExtractExifDateTimeTag(filePath, exifDateTag);
            var trueDate = exifDate ?? defaultDateTime;
            var isSet = await fileService.SetFileDateCreated(filePath, trueDate);
            results.Add(new ProcessItemResult(filePath, trueDate, isSet));
            var current = Interlocked.Increment(ref processedFileCount);
            if (progressService.ShouldReportProgress(current, totalFilesCount))
            {
                var currentPercentage = (int) Math.Round((double) current / totalFilesCount * 100);
                progress.Report(currentPercentage);
            }
        });
        return results.ToList();
    }
}