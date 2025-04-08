using System.Windows;

namespace ExifDateSetterWindows.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        var vm = new ViewModels.MainViewModel();
        DataContext = vm;
        InitializeComponent();
    }
}