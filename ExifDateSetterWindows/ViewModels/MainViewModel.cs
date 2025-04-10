using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Service;
using Core.Model;
using ExifDateSetterWindows.Extensions;
using ExifDateSetterWindows.Model;
using Serilog;

namespace ExifDateSetterWindows.ViewModels;

public partial class MainViewModel(
    IProcessingService processingService, 
    IDialogService dialogService,
    ILogger logger) : ObservableObject
{

#pragma warning disable CA1822
    // ReSharper disable MemberCanBeMadeStatic.Global
    public IEnumerable<ActionType> ActionList => Enum.GetValues<ActionType>();
    public IEnumerable<FileTypeSelectionItem> FileTypeSelectionItems => FileTypeSelectionItem.GetFileTypeSelectionItems();
    public IEnumerable<ExifDateTag> ExifDateTagsList => Enum.GetValues<ExifDateTag>();
    public IEnumerable<FileDateAttribute> FileDateAttributesList => Enum.GetValues<FileDateAttribute>();

    // ReSharper enable MemberCanBeMadeStatic.Global
    public int MaxNumberOfThreads => Environment.ProcessorCount;
#pragma warning restore CA1822

    [ObservableProperty] private ActionType _selectedActionType = ActionType.ExifToFileDate;
    [ObservableProperty] private ExifDateTag _selectedExifDateTag = ExifDateTag.DateTimeOriginal;
    [ObservableProperty] private FileDateAttribute _selectedFileDateAttribute = FileDateAttribute.DateCreated;
    [ObservableProperty] private DateTime _defaultDateTime = DateTime.Now;
    [ObservableProperty] private int _selectedNumberOfThreads;
    [ObservableProperty] private bool _isFolderSearchRecursive = true;
    [ObservableProperty] private string? _fileAndFolderCountStatus;
    [ObservableProperty] private int _progressValue;

    [NotifyCanExecuteChangedFor(nameof(AnalyzeCommandWrapperCommand))]
    [NotifyCanExecuteChangedFor(nameof(ProcessCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelProcessCommand))]
    [ObservableProperty] private bool _isBusy;
    
    private readonly List<string> _folders = [];
    private readonly List<string> _files = [];
    private CancellationTokenSource? _processCts;

    private void UpdateFileAndFolderCountStatus()
    {
        var fileCount = _files.Count;
        var folderCount = _folders.Count;
        if (fileCount != 0 || folderCount != 0)
            FileAndFolderCountStatus = $"{fileCount} files and {folderCount} folders selected";
        else
            FileAndFolderCountStatus = null;
    }
    
    private bool IsBusyCanExecute() => !IsBusy;
    private bool IsBusyCanExecuteInverse() => IsBusy;

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
        if (_files.Count == 0 && _folders.Count == 0)
        {
            await dialogService.ShowError(this, "Error", "No files or folders selected for analysis.");
            return;
        }
        if (cancellationToken?.IsCancellationRequested == true) return;
        // if no cancellation token is provided, create a new one
        var ct = cancellationToken ?? new CancellationTokenSource().Token;
        try
        {
            IsBusy = true;
            var progress = new Progress<int>(value =>
            {
                ProgressValue = value;
            });
            var selectedExtensions = FileTypeSelectionItems
                .Where(item => item.IsSelected)
                .Select<FileTypeSelectionItem, string>(item => item.SupportedFileType.GetFileExtension())
                .ToArray();

            var analysisConfig = new AnalyzeConfig
                (selectedExtensions, SelectedFileDateAttribute, SelectedExifDateTag, SelectedNumberOfThreads, IsFolderSearchRecursive, ct);

            var analysisResult = await processingService.AnalyzeFiles(_folders, _files, progress, analysisConfig);
            
            if (showResult)
            {
                await dialogService.ShowInformation(this, "Analysis Result", analysisResult.Summarize());
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
            IsBusy = false;
        }
    }
    
    [RelayCommand(CanExecute = nameof(IsBusyCanExecute))]
    private async Task AnalyzeCommandWrapper() => await Analyze();

    [RelayCommand(CanExecute = nameof(IsBusyCanExecute))]
    private async Task Process()
    {
        if (_files.Count == 0 && _folders.Count == 0)
        {
            await dialogService.ShowError(this, "Error", "No files or folders selected for processing.");
            return;
        }
        try
        {
            IsBusy = true;
            _processCts = new CancellationTokenSource();
            var progress = new Progress<int>(value => { ProgressValue = value; });
            var selectedExtensions = FileTypeSelectionItems
                .Where(item => item.IsSelected)
                .Select<FileTypeSelectionItem, string>(item => item.SupportedFileType.GetFileExtension())
                .ToArray();
            var processConfig = new ProcessConfig(SelectedActionType, DefaultDateTime, new AnalyzeConfig(selectedExtensions,
                SelectedFileDateAttribute,
                SelectedExifDateTag, SelectedNumberOfThreads, IsFolderSearchRecursive, _processCts.Token));

            var processResult = await processingService.ProcessFiles(_folders, _files, progress, processConfig);
            await dialogService.ShowInformation(this, "Processing Result", processResult.Summarize());
                                            
        }
        catch(TaskCanceledException)
        {
            // Task was cancelled - do nothing
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error during processing");
            await dialogService.ShowError(this, "Error", $"An error occurred during processing: {ex.Message}");
        }
        finally
        {
            _processCts?.Dispose();
            _processCts = null;
            IsBusy = false;
            ProgressValue = 0;
        }
    }

    [RelayCommand(CanExecute = nameof(IsBusyCanExecuteInverse))]
    private void CancelProcess()
    {
        _processCts?.Cancel();
    }


}