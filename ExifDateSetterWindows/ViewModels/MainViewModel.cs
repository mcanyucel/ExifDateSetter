using CommunityToolkit.Mvvm.ComponentModel;
using ExifDateSetterWindows.Model;

namespace ExifDateSetterWindows.ViewModels;

public partial class MainViewModel : ObservableObject
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

    [ObservableProperty]
    private Actions _selectedAction = Actions.ExifToFileDate;
    [ObservableProperty]
    private ExifDateTags _selectedExifDateTag = ExifDateTags.DateTimeOriginal;
    [ObservableProperty]
    private FileDateAttributes _selectedFileDateAttribute = FileDateAttributes.DateCreated;
    [ObservableProperty]
    private DateTime _defaultDateTime = DateTime.Now;
    [ObservableProperty] 
    private int _selectedNumberOfThreads;
    [ObservableProperty] 
    private bool _isFolderSearchRecursive = true;

}