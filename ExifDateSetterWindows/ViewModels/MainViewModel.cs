using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using ExifDateSetterWindows.Model;

namespace ExifDateSetterWindows.ViewModels;

public partial class MainViewModel : ObservableObject
{
    
#pragma warning disable CA1822
    // ReSharper disable MemberCanBeMadeStatic.Global
    public IEnumerable<Actions> ActionList => Enum.GetValues<Actions>();
    public IEnumerable<SupportedFileTypes> SupportedFileTypesList => Enum.GetValues<SupportedFileTypes>();
    public IEnumerable<ExifDateTags> ExifDateTagsList => Enum.GetValues<ExifDateTags>();
    public IEnumerable<FileDateAttributes> FileDateAttributesList => Enum.GetValues<FileDateAttributes>();
    // ReSharper enable MemberCanBeMadeStatic.Global
#pragma warning restore CA1822
    public ObservableCollection<SupportedFileTypes> SelectedFileTypes { get; } = [];

    [ObservableProperty]
    private Actions _selectedAction = Actions.ExifToFileDate;
    [ObservableProperty]
    private ExifDateTags _selectedExifDateTag = ExifDateTags.DateTimeOriginal;
    [ObservableProperty]
    private FileDateAttributes _selectedFileDateAttribute = FileDateAttributes.DateCreated;
    
}