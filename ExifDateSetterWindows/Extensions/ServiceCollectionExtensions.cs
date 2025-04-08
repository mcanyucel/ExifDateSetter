using Core.Service;
using ExifDateSetterWindows.Services;
using ExifDateSetterWindows.ViewModels;
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
}