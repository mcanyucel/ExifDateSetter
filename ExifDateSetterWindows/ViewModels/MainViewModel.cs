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
    IProcessingService processingService, 
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
    [ObservableProperty] private int _progressValue;

    private readonly List<string> _folders = [];
    private readonly List<string> _files = [];
    

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
            
            var progress = new Progress<int>(value =>
            {
                ProgressValue = value;
            });
            var selectedExtensions = FileTypeSelectionItems
                .Where(item => item.IsSelected)
                .Select<FileTypeSelectionItem, string>(item => item.SupportedFileType.GetFileExtension())
                .ToArray();

            var analysisConfig = new ProcessConfig
                (selectedExtensions, SelectedFileDateAttribute, SelectedExifDateTag, SelectedNumberOfThreads, IsFolderSearchRecursive, cts.Token);

            var analysisResult = await processingService.AnalyzeFiles(_folders, _files, progress, analysisConfig);
            
            if (showResult)
            {
                
                var analysisResultSummary = $"Analyzed {analysisResult.ProcessedFileCount} files\n" +
                                            $"File Date Range: {analysisResult.FileAnalysisResult.MinimumFileDate} - {analysisResult.FileAnalysisResult.MaximumFileDate}\n" +
                                            $"Number of files with Exif date: {analysisResult.ExifAnalysisResult.NumberOfFilesWithExifDate}\n" +
                                            $"Exif Date Range: {analysisResult.ExifAnalysisResult.MinimumExifDate} - {analysisResult.ExifAnalysisResult.MaximumExifDate}\n";
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
    private async Task AnalyzeCommandWrapper() => await Analyze();

    [RelayCommand]
    private async Task Process()
    {
        try
        {
            IsIndeterminateBusy = true;
            var cts = new CancellationTokenSource();
            await Analyze(false, cts.Token);
            IsIndeterminateBusy = false;
            ProgressValue = 0;
        }
        finally
        {
            IsIndeterminateBusy = false;
        }
    }

    /// <summary>
    /// Creates the file list from the folders and files, which contains all the files with the selected extensions
    /// and folders lists
    /// </summary>
    
}