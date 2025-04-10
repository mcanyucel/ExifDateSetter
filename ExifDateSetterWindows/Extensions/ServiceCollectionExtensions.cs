﻿using Core.Factory;
using Core.Service;
using Core.Strategy;
using ExifDateSetterWindows.AutoUpdate;
using ExifDateSetterWindows.Factory;
using ExifDateSetterWindows.Services;
using ExifDateSetterWindows.Strategy;
using ExifDateSetterWindows.ViewModels;
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
        serviceCollection.AddSingleton<IExifService, WindowsExifService>();
        serviceCollection.AddSingleton<IProcessingService, ProcessingService>();
        serviceCollection.AddTransient<IProgressService, ProgressService>();
        return serviceCollection;
    }

    public static IServiceCollection AddDateCopyStrategies(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddKeyedSingleton<IDateCopyStrategy, ExifToFileCreationStrategy>(nameof(ExifToFileCreationStrategy));
        serviceCollection.AddKeyedSingleton<IDateCopyStrategy, ExifToFileLastModifiedStrategy>(nameof(ExifToFileLastModifiedStrategy));
        serviceCollection.AddKeyedSingleton<IDateCopyStrategy, FileCreationToExifDateStrategy>(nameof(FileCreationToExifDateStrategy));
        serviceCollection.AddKeyedSingleton<IDateCopyStrategy, FileLastModifiedToExifDateStrategy>(nameof(FileLastModifiedToExifDateStrategy));
        return serviceCollection;
    }
    
    public static IServiceCollection AddFactories(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IDateCopyStrategyFactory, DateCopyStrategyFactory>();
        serviceCollection.AddHttpClient();
        return serviceCollection;
    }

    public static IServiceCollection AddUpdateServices(this IServiceCollection services)
    {
        services.AddSingleton<UpdateEngine>();
        return services;
    }
}