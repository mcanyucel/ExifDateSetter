using System.IO;
using System.Windows;
using ExifDateSetterWindows.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ExifDateSetterWindows;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public IServiceProvider ServiceProvider { get; }

    public new static App Current => (App)Application.Current;
    public App()
    {
        ConfigureLogger();
        ServiceProvider = ConfigureServices();
    }

    private static void ConfigureLogger()
    {
        var logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "ExifDateSetter",
            "logs");
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        var loggerConfiguration = new LoggerConfiguration()
            .WriteTo
            .File(Path.Combine(logDirectory, "log-.txt"), rollingInterval: RollingInterval.Day);
#if DEBUG
        loggerConfiguration.MinimumLevel.Debug();
#else
        loggerConfiguration.MinimumLevel.Error();
#endif
        Log.Logger = loggerConfiguration.CreateLogger();
        Log.Information("Application started");
    }

    private static ServiceProvider ConfigureServices()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddViewModels()
            .AddFileServices()
            .AddProcessingServices()
            .AddDialogServices()
            .AddFactories()
            .AddDateCopyStrategies()
            .AddLoggerServices();
        return serviceCollection.BuildServiceProvider();
    }
}