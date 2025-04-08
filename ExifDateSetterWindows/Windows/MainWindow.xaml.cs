using System.Windows;
using System.Windows.Controls;
using ExifDateSetterWindows.Model;
using ExifDateSetterWindows.ViewModels;

namespace ExifDateSetterWindows.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly MainViewModel _vm;
    public MainWindow()
    {
        _vm = new MainViewModel();
        DataContext = _vm;
        InitializeComponent();
    }
}