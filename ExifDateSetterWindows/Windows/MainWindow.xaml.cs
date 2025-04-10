using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using ExifDateSetterWindows.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ExifDateSetterWindows.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly MainViewModel _vm;
    private Brush? _textBackground; 
    public MainWindow()
    {
        _vm = App.Current.ServiceProvider.GetRequiredService<MainViewModel>();
        DataContext = _vm;
        _vm.PropertyChanged += ViewModelPropertyChanged;
        InitializeComponent();
    }

    private void ViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var propertyName = e.PropertyName;

        if (propertyName == nameof(_vm.FileAndFolderCountStatus))
        {
            TextBlockDragDrop.Text = _vm.FileAndFolderCountStatus;
        }
    }

    private void FileDragDrop_OnDragEnter(object sender, DragEventArgs e)
    {
        _textBackground = TextBlockDragDrop.Background;
        TextBlockDragDrop.Background = Brushes.DarkGray;
        TextBlockDragDrop.Foreground = Brushes.Azure;
        TextBlockDragDrop.Text = GetTextBlockDragDropText(e);
        RectangleDragDrop.Fill = Brushes.DarkGray;
        RectangleDragDrop.StrokeDashArray = [0, 0];
    }

    private void FileDragDrop_OnDragLeave(object sender, DragEventArgs e)
    {
        TextBlockDragDrop.Background = _textBackground;
        TextBlockDragDrop.Foreground = Brushes.DarkGray;
        TextBlockDragDrop.Text = GetTextBlockDragDropText();
        RectangleDragDrop.Fill = _textBackground;
        _textBackground = null;
        RectangleDragDrop.StrokeDashArray = [6, 6];
    }

    private List<string>? _newFileNameList;
    private List<string>? _existingFileNameList;
    private List<string>? _newFolderNameList;
    private List<string>? _existingFolderNameList;
    
    
    private string GetTextBlockDragDropText(DragEventArgs? e = null)
    {
        if (e == null || !e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            _existingFileNameList = null;
            _existingFolderNameList = null;
            _newFileNameList = null;
            _newFolderNameList = null;
            return _vm.FileAndFolderCountStatus ?? "Add files by buttons above or drag and drop them here";
        }

        // get the file names
        var fileNames = e.Data.GetData(DataFormats.FileDrop) as string[];
        var fileCount = fileNames?.Length ?? 0;
        if (fileCount <= 0) return "No files found";

        var existingFileNames = fileNames?.Where(_vm.ContainsFileOrFolder).ToList() ?? [];
        var newFileNames = fileNames?.Except(existingFileNames).ToList() ?? [];
        
        // new files
        _newFolderNameList = newFileNames.Where(Directory.Exists).ToList();
        _newFileNameList = newFileNames.Except(_newFolderNameList).ToList();
        
        var numberOfNewFolders = _newFolderNameList.Count;
        var numberOfNewFiles = _newFileNameList.Count;
        string? newFileText = null;
        if (numberOfNewFiles != 0 || numberOfNewFolders != 0)
        {
            var newFilePart = numberOfNewFiles > 0 ? $"{numberOfNewFiles} file{(numberOfNewFiles != 1 ? "s" : "")}" : "";
            var newFolderPart = numberOfNewFolders > 0 ? $"{numberOfNewFolders} folder{(numberOfNewFolders != 1 ? "s" : "")}" : "";
            newFileText = $"{string.Join(" and ", new[] { newFilePart, newFolderPart }.Where(p => !string.IsNullOrEmpty(p)))} will be added to the list";
        }
        
        // existing files to be removed
        _existingFolderNameList = existingFileNames.Where(Directory.Exists).ToList();
        _existingFileNameList = existingFileNames.Except(_existingFolderNameList).ToList();
        var numberOfExistingFolders = _existingFolderNameList.Count;
        var numberOfExistingFiles = _existingFileNameList.Count;
        string? existingFileText = null;
        if (numberOfExistingFiles != 0 || numberOfExistingFolders != 0)
        {
            var existingFilePart = numberOfExistingFiles > 0 ? $"{numberOfExistingFiles} file{(numberOfExistingFiles != 1 ? "s" : "")}" : "";
            var existingFolderPart = numberOfExistingFolders > 0 ? $"{numberOfExistingFolders} folder{(numberOfExistingFolders != 1 ? "s" : "")}" : "";
            existingFileText = $"{string.Join(" and ", new[] { existingFilePart, existingFolderPart }.Where(p => !string.IsNullOrEmpty(p)))} will be removed from the list";
        }
        
        return $"{string.Join("\n", new [] { newFileText, existingFileText }.Where(p => !string.IsNullOrEmpty(p)))}";
    }

    private void FileDragDrop_OnDrop(object sender, DragEventArgs e)
    {

        if (_newFileNameList == null || _existingFileNameList == null || _newFolderNameList == null || _existingFolderNameList == null)
        {
            throw new InvalidOperationException("Drag and drop operation failed");
        }
        _vm.UpdateFilesAndFolders(_newFileNameList, _newFolderNameList, _existingFileNameList, _existingFolderNameList);
        FileDragDrop_OnDragLeave(sender, e);
    }
}