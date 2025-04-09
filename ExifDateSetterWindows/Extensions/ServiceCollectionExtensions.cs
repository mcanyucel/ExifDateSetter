using Core.Service;
using ExifDateSetterWindows.Services;
using ExifDateSetterWindows.ViewModels;
using ExifDateSetterWindows.Windows;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ExifDateSetterWindows.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<MainViewModel>();
        return serviceCollection;
    }

    public static IServiceCollection AddFileServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IFileService, WindowFileService>();
        serviceCollection.AddTransient<IFileSystemService, WindowsFileSystemService>();
        return serviceCollection;
    }

    public static IServiceCollection AddLoggerServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(Log.Logger);
        return serviceCollection;
    }

    public static IServiceCollection AddDialogServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient(_ => DialogCoordinator.Instance);
        serviceCollection.AddTransient<IDialogService, MahappsDialogService>();
        return serviceCollection;
    }

    public static IServiceCollection AddProcessingServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IExifService, WindowsExifService>();
        serviceCollection.AddTransient<IProcessingService, ProcessingService>();
        return serviceCollection;
    }

}