using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Service;
using ExifDateSetterWindows.Model;

namespace ExifDateSetterWindows.ViewModels;

public partial class MainViewModel(IFileService _fileService, IFileSystemService _fileSystemService) : ObservableObject
{
    
#pragma warning disable CA1822
    // ReSharper disable MemberCanBeMadeStatic.Global
    public IEnumerable<Actions> ActionList => Enum.GetValues<Actions>();
    public IEnumerable<FileTypeSelectionItem> FileTypeSelectionItems => FileTypeSelectionItem.GetFileTypeSelectionItems();
    public IEnumerable<ExifDateTags> ExifDateTagsList => Enum.GetValues<ExifDateTags>();
    public IEnumerable<FileDateAttributes> FileDateAttributesList => Enum.GetValues<FileDateAttributes>();
    // ReSharper enable MemberCanBeMadeStatic.Global
    public int MaxNumberOfThreads => Environment.ProcessorCount;
#pragma warning restore CA1822

    [ObservableProperty] private Actions _selectedAction = Actions.ExifToFileDate;
    [ObservableProperty] private ExifDateTags _selectedExifDateTag = ExifDateTags.DateTimeOriginal;
    [ObservableProperty] private FileDateAttributes _selectedFileDateAttribute = FileDateAttributes.DateCreated;
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
    
    public void UpdateFilesAndFolders(IEnumerable<string> newFilesList, IEnumerable<string> newFoldersList, List<string> existingFileNameList, List<string> existingFolderNameList)
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
        
    }
}