using System.Collections.Concurrent;
using Core.Model;
using Core.Service;

namespace ExifDateSetterWindows.Services;

public class ProcessingService(IExifService exifService, IFileService fileService, IFileSystemService fileSystemService): IProcessingService
{
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
        var processedFilesCount = 1;
        var totalFilesCount = fileList.Count + 2; // +1 for flattening, +1 for the aggregation
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
            var current = Interlocked.Increment(ref processedFilesCount);
            if (ShouldReportProgress(current, totalFilesCount))
            {
                var currentPercentage = (int) Math.Round((double) current / totalFilesCount * 100);
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
        processedFilesCount++;
        progress.Report(processedFilesCount);
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
        List<string> results = [];
        switch (configuration.ActionType)
        {
            case ActionType.ExifToFileDate:
                results = configuration.AnalyzeConfig.FileDateAttribute switch
                {
                    FileDateAttribute.DateCreated => await CopyExifDateToFileCreationDate(fileList, parallelOptions,
                        configuration.AnalyzeConfig.ExifDateTag, configuration.DefaultDateTime, progress, configuration.AnalyzeConfig.CancellationToken),
                    FileDateAttribute.DateModified => await CopyExifDateToFileModifiedDate(fileList, parallelOptions,
                        configuration.AnalyzeConfig.ExifDateTag, configuration.DefaultDateTime, progress, configuration.AnalyzeConfig.CancellationToken),
                    _ => throw new ArgumentOutOfRangeException(nameof(configuration), configuration.AnalyzeConfig.FileDateAttribute, null)
                };
                break;
            case ActionType.FileDateToExif:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(configuration), configuration.ActionType, null);
        }

        return new ProcessResult();
    }

    private Task<List<string>> CopyExifDateToFileCreationDate(List<string> fileList, ParallelOptions parallelOptions, ExifDateTag exifDateTag, DateTime defaultDateTime, IProgress<int> progress, CancellationToken ct)
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
            if (ShouldReportProgress(current, totalFilesCount))
            {
                var currentPercentage = (int) Math.Round((double) current / totalFilesCount * 100);
                progress.Report(currentPercentage);
            }
        });
        return Task.FromResult(results.ToList());
    }
    
    private Task<List<string>> CopyExifDateToFileModifiedDate(List<string> fileList, ParallelOptions parallelOptions, ExifDateTag exifDateTag, DateTime defaultDateTime, IProgress<int> progress, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        
        ConcurrentBag<string> results = [];
        var processedFileCount = 0;
        var totalFilesCount = fileList.Count;
        
        Parallel.ForEachAsync(fileList, parallelOptions, async (filePath, _) =>
        {
            var exifDate = await exifService.ExtractExifDateTag(filePath, exifDateTag);
            var trueDate = exifDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) ?? defaultDateTime;
            var result = await fileService.SetFileDateModified(filePath, trueDate);
            results.Add($"File Path: {filePath}, Date: {trueDate}, Result: {result}");
            var current = Interlocked.Increment(ref processedFileCount);
            if (ShouldReportProgress(current, totalFilesCount))
            {
                var currentPercentage = (int) Math.Round((double) current / totalFilesCount * 100);
                progress.Report(currentPercentage);
            }
        });
        return Task.FromResult(results.ToList());
    }

    private Task<List<string>> CopyFileCreationDateToExifDate(List<string> fileList, ParallelOptions parallelOptions, DateTime defaultDateTime,
        IProgress<int> progress, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        
        ConcurrentBag<string> results = [];
        var processedFileCount = 0;
        var totalFilesCount = fileList.Count;
        
        Parallel.ForEachAsync(fileList, parallelOptions, async (filePath, _) =>
        {
            var fileDate = await fileService.ExtractFileDateCreated(filePath);
            var trueDate = fileDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) ?? defaultDateTime;
            var result = await exifService.SetExifDateTag(filePath, trueDate, ExifDateTag.DateTimeOriginal);
            results.Add($"File Path: {filePath}, Date: {trueDate}, Result: {result}");
            var current = Interlocked.Increment(ref processedFileCount);
            if (ShouldReportProgress(current, totalFilesCount))
            {
                var currentPercentage = (int) Math.Round((double) current / totalFilesCount * 100);
                progress.Report(currentPercentage);
            }
        });
        return Task.FromResult(results.ToList());
    }
    
    private Task<List<string>> CopyFileModifiedDateToExifDate(List<string> fileList, ParallelOptions parallelOptions, DateTime defaultDateTime,
        IProgress<int> progress, CancellationToken ct)
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
            if (ShouldReportProgress(current, totalFilesCount))
            {
                var currentPercentage = (int) Math.Round((double) current / totalFilesCount * 100);
                progress.Report(currentPercentage);
            }
        });
        return Task.FromResult(results.ToList());
    }


    private bool ShouldReportProgress(int currentCount, int totalFilesCount)
    {
        lock (_processReportLockObject)
        {
            var now = DateTime.Now;
            if (!((now - _lastReportTime).TotalMilliseconds >= ProgressReportIntervalMs) && currentCount != totalFilesCount) return false;
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