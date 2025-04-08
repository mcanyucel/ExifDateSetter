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
        InitializeComponent();
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
    
    private static string GetTextBlockDragDropText(DragEventArgs? e = null)
    {
        // if we are in a drag and drop operation
        if (e != null && e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            // get the file names
            var fileNames = e.Data.GetData(DataFormats.FileDrop) as string[];
            var fileCount = fileNames?.Length ?? 0;
            if (fileCount > 0)
            {
                var numberOfFolders = fileNames?.Count(Directory.Exists) ?? 0;
                var numberOfFiles = fileCount - numberOfFolders;
                var filePart = numberOfFiles > 0 ? $"{numberOfFiles} file{(numberOfFiles != 1 ? "s" : "")}" : "";
                var folderPart = numberOfFolders > 0 ? $"{numberOfFolders} folder{(numberOfFolders != 1 ? "s" : "")}" : "";
                return $"{string.Join(" and ", new[] { filePart, folderPart }.Where(p => !string.IsNullOrEmpty(p)))} will be added to the list";
            }
            else
            {
                return "No files found";
            }
        }
        else
        {
            return "Add files by buttons above or drag and drop them here";
        }
    }

    private void FileDragDrop_OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is not string[] fileNames) return;
        
    var folderNames =  fileNames.Where(Directory.Exists).ToList(); 
    var fileNamesWithoutFolders = fileNames.Except(folderNames).ToList();
        if (fileNamesWithoutFolders.Count > 0)
        {
            _vm.AddFiles(fileNamesWithoutFolders);
        }
        if (folderNames.Count > 0)
        {
            _vm.AddFolders(folderNames);
        }
        FileDragDrop_OnDragLeave(sender, e);
        
    }
}