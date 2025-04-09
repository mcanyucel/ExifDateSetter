using System.Collections.Concurrent;
using Core.Model;
using Core.Service;

namespace ExifDateSetterWindows.Services;

public class ProcessingService(IExifService exifService, IFileService fileService, IFileSystemService fileSystemService): IProcessingService
{
    private int _processedFilesCount;
    private int _totalFilesCount;
    private readonly Lock _processReportLockObject = new ();
    private const int ProgressReportIntervalMs = 100; // 100ms
    private DateTime _lastReportTime = DateTime.MinValue;
    
    public async Task<AnalysisResult> AnalyzeFiles(List<string> foldersList, List<string> filesList, IProgress<int> progress, AnalyzeConfig configuration)
    {
        configuration.CancellationToken.ThrowIfCancellationRequested();
        
        var parallelOptions = new ParallelOptions
        {
            CancellationToken = configuration.CancellationToken,
            MaxDegreeOfParallelism = configuration.MaxDegreeOfParallelism
        };
        var fileList = await PrepareFileList(foldersList, filesList, configuration, parallelOptions); 
        _processedFilesCount = 1;
        _totalFilesCount = fileList.Count + 2; // +1 for flattening, +1 for the aggregation
        /*
         * Storing all the dates is inefficient in terms of memory, but 4 bytes per record is acceptable rather than
         * partitioning the file list into smaller chunks and processing them in parallel, as we should not use
         * global locks in parallel processing.
         */
        ConcurrentBag<DateOnly?> exifDates = [];
        ConcurrentBag<DateOnly?> fileDates = [];

        await Parallel.ForEachAsync(fileList, parallelOptions, async (filePath, _) =>
        {
            var exifDateExtractTask = exifService.ExtractExifDateTag(filePath, configuration.ExifDateTag);
            var fileDateExtractTask = configuration.FileDateAttribute switch
            {
                FileDateAttribute.DateCreated => fileService.ExtractFileDateCreated(filePath),
                FileDateAttribute.DateModified => fileService.ExtractFileDateModified(filePath),
                _ => throw new ArgumentOutOfRangeException(nameof(configuration), configuration.FileDateAttribute, null)
            };
            var results = await Task.WhenAll(exifDateExtractTask, fileDateExtractTask);
            exifDates.Add(results[0]);
            fileDates.Add(results[1]);
            var current = Interlocked.Increment(ref _processedFilesCount);
            if (ShouldReportProgress(current))
            {
                var currentPercentage = (int) Math.Round((double) current / _totalFilesCount * 100);
                progress.Report(currentPercentage);
            }
        });
        // consider these aggregate operations as another item in progress count
        var minimumExifDate = exifDates.Min() ?? DateOnly.MaxValue;
        var maximumExifDate = exifDates.Max() ?? DateOnly.MaxValue;
        var minimumFileDate = fileDates.Min() ?? DateOnly.MinValue;
        var maximumFileDate = fileDates.Max() ?? DateOnly.MaxValue;
        var filesWithExifDate = exifDates.Count(date => date != null);
        var exifAnalysisResult = new ExifAnalysisResult(filesWithExifDate, minimumExifDate, maximumExifDate);
        var filesAnalysisResult = new FileAnalysisResult(minimumFileDate, maximumFileDate);
        var analysisResult = new AnalysisResult(fileList.Count, filesAnalysisResult, exifAnalysisResult, fileList);
        // report the last progress
        _processedFilesCount++;
        progress.Report(_processedFilesCount);
        return analysisResult;
    }

    public async Task<ProcessResult> ProcessFiles(List<string> foldersList, List<string> filesList, IProgress<int> progress, ProcessConfig configuration)
    {
        configuration.AnalyzeConfig.CancellationToken.ThrowIfCancellationRequested();
        
        var parallelOptions = new ParallelOptions
        {
            CancellationToken = configuration.AnalyzeConfig.CancellationToken,
            MaxDegreeOfParallelism = configuration.AnalyzeConfig.MaxDegreeOfParallelism
        };
        var fileList = await PrepareFileList(foldersList, filesList, configuration.AnalyzeConfig, parallelOptions); 
        _processedFilesCount = 1;
        _totalFilesCount = fileList.Count + 1; // +1 for flattening
        await Parallel.ForEachAsync(fileList, parallelOptions, async (filePath, ct) =>
        {
            await Task.Delay(1000, ct); // Simulate processing time
            var current = Interlocked.Increment(ref _processedFilesCount);
            if (ShouldReportProgress(current))
            {
                var currentPercentage = (int) Math.Round((double) current / _totalFilesCount * 100);
                progress.Report(currentPercentage);
            }
        });
        return new ProcessResult();
    }


    private bool ShouldReportProgress(int currentCount)
    {
        lock (_processReportLockObject)
        {
            var now = DateTime.Now;
            if (!((now - _lastReportTime).TotalMilliseconds >= ProgressReportIntervalMs) && currentCount != _totalFilesCount) return false;
            _lastReportTime = now;
            return true;
        }
    }
    
    private async Task<List<string>> PrepareFileList(List<string> folders, 
        List<string> files,
        AnalyzeConfig configuration,
        ParallelOptions parallelOptions)
    {
        /*
         * We use dictionary to have unique file names with maximum performance:
         * +. Thread-safe with automatic deduplication
         * +. O(1) lookups - no performance hit inside loops
         * +. Maintains full parallelism
         * +. No post-processing required
         * -. Slightly higher memory usage
         */
        ConcurrentDictionary<string, byte> allFiles = [];
        
        var flattenFoldersTask = 
            Parallel.ForEachAsync (folders, parallelOptions, async (folder, _) =>
            {
                var filesInFolders = await fileSystemService.GetFilesFromFolder(folder, configuration.IsFolderSearchRecursive);
                foreach (var file in filesInFolders)
                {
                    allFiles.TryAdd(file, 0); // Value doesn't matter
                }
            });
        
        var addFilesTask = Task.Run(() =>
        {
            foreach (var file in files)
            {
                allFiles.TryAdd(file, 0);
            }
        }, configuration.CancellationToken);
        await Task.WhenAll(flattenFoldersTask, addFilesTask);
        
        var fileList = allFiles.Keys.ToList();
        
        return fileList
            .Where(file => configuration.Extensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }
}