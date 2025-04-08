using System.Collections.Concurrent;
using System.IO;
using Core.Model;
using Core.Service;

namespace ExifDateSetterWindows.Services;

public class WindowFileService : IFileService
{
    public Task<FileAnalysisResult> AnalyzeFiles(IEnumerable<string> filePaths, CancellationToken ct, FileDateAttribute dateAttribute, int maxNumberOfThreads)
    {
        if (ct.IsCancellationRequested) return Task.FromCanceled<FileAnalysisResult>(ct);
        
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxNumberOfThreads == 0 ? -1 : maxNumberOfThreads };
        ConcurrentBag<DateOnly> dates = [];
        /*
         * Storing all the dates is inefficient in terms of memory, but 4 bytes per record is acceptable rather than
         * partitioning the file list into smaller chunks and processing them in parallel, as we should not use
         * global locks in parallel processing.
         */
        Parallel.ForEach(filePaths, parallelOptions, filePath =>
        {
            var date = dateAttribute switch
            {
                FileDateAttribute.DateCreated => File.GetCreationTime(filePath),
                FileDateAttribute.DateModified => File.GetLastWriteTime(filePath),
                _ => throw new ArgumentOutOfRangeException(nameof(dateAttribute), dateAttribute, null)
            };
            dates.Add(DateOnly.FromDateTime(date));
        });
        var minimumDate = dates.Min();
        var maximumDate = dates.Max();
        return Task.FromResult(new FileAnalysisResult(minimumDate, maximumDate));
    }
}