using System.Collections.Concurrent;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Service;
using Core.Model;
using ExifDateSetterWindows.Extensions;
using ExifDateSetterWindows.Model;
using Serilog;

namespace ExifDateSetterWindows.ViewModels;

public partial class MainViewModel(IFileService fileService, 
    IFileSystemService fileSystemService, 
    IExifService exifService,
    IDialogService dialogService,
    ILogger logger) : ObservableObject
{

#pragma warning disable CA1822
    // ReSharper disable MemberCanBeMadeStatic.Global
    public IEnumerable<Actions> ActionList => Enum.GetValues<Actions>();
    public IEnumerable<FileTypeSelectionItem> FileTypeSelectionItems => FileTypeSelectionItem.GetFileTypeSelectionItems();
    public IEnumerable<ExifDateTag> ExifDateTagsList => Enum.GetValues<ExifDateTag>();
    public IEnumerable<FileDateAttribute> FileDateAttributesList => Enum.GetValues<FileDateAttribute>();

    // ReSharper enable MemberCanBeMadeStatic.Global
    public int MaxNumberOfThreads => Environment.ProcessorCount;
#pragma warning restore CA1822

    [ObservableProperty] private Actions _selectedAction = Actions.ExifToFileDate;
    [ObservableProperty] private ExifDateTag _selectedExifDateTag = ExifDateTag.DateTimeOriginal;
    [ObservableProperty] private FileDateAttribute _selectedFileDateAttribute = FileDateAttribute.DateCreated;
    [ObservableProperty] private DateTime _defaultDateTime = DateTime.Now;
    [ObservableProperty] private int _selectedNumberOfThreads;
    [ObservableProperty] private bool _isFolderSearchRecursive = true;
    [ObservableProperty] private bool _isIndeterminateBusy;
    [ObservableProperty] private string? _fileAndFolderCountStatus;
    [ObservableProperty] private int _numberOfFiles;

    private readonly List<string> _folders = [];
    private readonly List<string> _files = [];
    private List<string> _fileListForProcessing = [];

    private void UpdateFileAndFolderCountStatus()
    {
        var fileCount = _files.Count;
        var folderCount = _folders.Count;
        if (fileCount != 0 || folderCount != 0)
            FileAndFolderCountStatus = $"{fileCount} files and {folderCount} folders selected";
        else
            FileAndFolderCountStatus = null;
    }

    public void UpdateFilesAndFolders(IEnumerable<string> newFilesList, IEnumerable<string> newFoldersList, List<string> existingFileNameList,
        List<string> existingFolderNameList)
    {
        _files.RemoveAll(existingFileNameList.Contains);
        _folders.RemoveAll(existingFolderNameList.Contains);
        _files.AddRange(newFilesList);
        _folders.AddRange(newFoldersList);
        UpdateFileAndFolderCountStatus();
    }

    public bool ContainsFileOrFolder(string fileName)
    {
        return _files.Contains(fileName) ||
               _folders.Contains(fileName);
    }

    private async Task Analyze(bool showResult = true, CancellationToken? cancellationToken = null)
    {
        if (cancellationToken?.IsCancellationRequested == true) return;
        // if no cancellation token is provided, create a new one
        var ct = cancellationToken ?? new CancellationTokenSource().Token;
        try
        {
            IsIndeterminateBusy = true;
            var cts = new CancellationTokenSource();
            await PrepareFileList(ct);
            NumberOfFiles = _files.Count;
            var fileAnalysisResult = await fileService.AnalyzeFiles(_fileListForProcessing, SelectedFileDateAttribute, SelectedNumberOfThreads, cts.Token);
            var exifAnalysisResult = await exifService.AnalyzeFiles(_fileListForProcessing, SelectedExifDateTag, SelectedNumberOfThreads, cts.Token);


            var analysisResultSummary = $"Analyzed {_fileListForProcessing.Count} files\n" +
                                        $"File Date Range: {fileAnalysisResult.MinimumFileDate} - {fileAnalysisResult.MaximumFileDate}\n" +
                                        $"Number of files with Exif date: {exifAnalysisResult.NumberOfFilesWithExifDate}\n" +
                                        $"Exif Date Range: {exifAnalysisResult.MinimumExifDate} - {exifAnalysisResult.MaximumExifDate}\n";
            if (showResult)
            {
                await dialogService.ShowInformation(this, "Analysis Result", analysisResultSummary);
            }
        }
        catch (OperationCanceledException)
        {
            // Operation was cancelled - do nothing
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error during analysis");
            await dialogService.ShowError(this, "Error", $"An error occurred during analysis: {ex.Message}");
        }
        finally
        {
            IsIndeterminateBusy = false;
        }
    }
    
    [RelayCommand]
    private async Task AnalyzeCommandWrapper()
    {
        await Analyze();
    }

    /// <summary>
    /// Creates the file list from the folders and files, which contains all the files with the selected extensions
    /// and folders lists
    /// </summary>
    private async Task PrepareFileList(CancellationToken cancellationToken)
    {
        _fileListForProcessing = [];
        ConcurrentDictionary<string, byte> allFiles = [];
        /*
         * We use dictionary to have unique file names with maximum performance:
         * +. Thread-safe with automatic deduplication
         * +. O(1) lookups - no performance hit inside loops
         * +. Maintains full parallelism
         * +. No post-processing required
         * -. Slightly higher memory usage
         */
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = SelectedNumberOfThreads == 0 ? -1 : SelectedNumberOfThreads,
            CancellationToken = cancellationToken
        };
        
        var flattenFoldersTask = Task.Run(() =>
        {
            Parallel.ForEach(_folders, parallelOptions, folder =>
            {
                var files = fileSystemService.GetFilesFromFolder(folder, IsFolderSearchRecursive).Result;
                foreach (var file in files)
                {
                    allFiles.TryAdd(file, 0); // Value doesn't matter
                }
            });
        }, cancellationToken);

        var addFilesTask = Task.Run(() =>
        {
            foreach (var file in _files)
            {
                allFiles.TryAdd(file, 0);
            }
        }, cancellationToken);
        await Task.WhenAll(flattenFoldersTask, addFilesTask);
        
        var fileList = allFiles.Keys.ToList();
        var extensions = FileTypeSelectionItems
            .Where(item => item.IsSelected)
            .Select<FileTypeSelectionItem, string>(item => item.SupportedFileType.GetFileExtension())
            .ToArray();
        
        _fileListForProcessing = fileList
            .Where(file => extensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }
}