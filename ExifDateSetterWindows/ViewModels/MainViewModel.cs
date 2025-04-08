using System.Collections.Concurrent;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Service;
using Core.Model;
using ExifDateSetterWindows.Model;

namespace ExifDateSetterWindows.ViewModels;

public partial class MainViewModel(IFileService fileService, IFileSystemService fileSystemService) : ObservableObject
{

#pragma warning disable CA1822
    // ReSharper disable MemberCanBeMadeStatic.Global
    public IEnumerable<Actions> ActionList => Enum.GetValues<Actions>();
    public IEnumerable<FileTypeSelectionItem> FileTypeSelectionItems => FileTypeSelectionItem.GetFileTypeSelectionItems();
    public IEnumerable<ExifDateTags> ExifDateTagsList => Enum.GetValues<ExifDateTags>();

    public IEnumerable<FileDateAttribute> FileDateAttributesList => Enum.GetValues<FileDateAttribute>();

    // ReSharper enable MemberCanBeMadeStatic.Global
    public int MaxNumberOfThreads => Environment.ProcessorCount;
#pragma warning restore CA1822

    [ObservableProperty] private Actions _selectedAction = Actions.ExifToFileDate;
    [ObservableProperty] private ExifDateTags _selectedExifDateTag = ExifDateTags.DateTimeOriginal;
    [ObservableProperty] private FileDateAttribute _selectedFileDateAttribute = FileDateAttribute.DateCreated;
    [ObservableProperty] private DateTime _defaultDateTime = DateTime.Now;
    [ObservableProperty] private int _selectedNumberOfThreads;
    [ObservableProperty] private bool _isFolderSearchRecursive = true;
    [ObservableProperty] private bool _isIndeterminateBusy;
    [ObservableProperty] private string? _fileAndFolderCountStatus;

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

    [RelayCommand]
    private async Task Analyze()
    {
        try
        {
            IsIndeterminateBusy = true;
            var cts = new CancellationTokenSource();
            // flatten all the folders into files list
            ConcurrentDictionary<string, byte> allFiles = [];
            /*
             * We use dictionary to have unique file names with maximum performance:
             * 1. Thread-safe with automatic deduplication
             * 2. O(1) lookups - no performance hit inside loops
             * 3. Maintains full parallelism
             * 4. No post-processing required
             *  -. Slightly higher memory usage
             */
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = SelectedNumberOfThreads == 0 ? -1 : SelectedNumberOfThreads,
                CancellationToken = cts.Token
            };
            await Parallel.ForEachAsync(_folders, parallelOptions, async (folder, token) =>
            {
                var files = await fileSystemService.GetFilesFromFolder(folder, token, null, IsFolderSearchRecursive);
                foreach (var file in files)
                {
                    allFiles.TryAdd(file, 0); // Value doesn't matter
                }
            });
            var fileList = allFiles.Keys.ToList();
            var fileAnalysisResult = await fileService.AnalyzeFiles(fileList, cts.Token, SelectedFileDateAttribute, SelectedNumberOfThreads);
            
            
            var analysisResultSummary = $"Analyzed {fileList.Count} files\n" +
                                        $"File Date Range: {fileAnalysisResult.MinimumFileDate} - {fileAnalysisResult.MaximumFileDate}\n";
            
            System.Diagnostics.Debug.WriteLine(analysisResultSummary);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {

        }
        finally
        {
            IsIndeterminateBusy = false;
        }
    }
}