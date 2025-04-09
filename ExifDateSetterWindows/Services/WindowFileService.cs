using System.IO;
using Core.Service;
using Serilog;

namespace ExifDateSetterWindows.Services;

public class WindowFileService(ILogger logger) : IFileService
{
    public Task<DateOnly?> ExtractFileDateCreated(string filePath)
    {
        DateOnly? result = DateOnly.FromDateTime(File.GetCreationTime(filePath));
        return Task.FromResult(result);
    }
    
    public Task<DateOnly?> ExtractFileDateModified(string filePath)
    {
        DateOnly? result = DateOnly.FromDateTime(File.GetLastWriteTime(filePath));
        return Task.FromResult(result);
    }

    public Task<bool> SetFileDateCreated(string filePath, DateTime date)
    {
        try
        {
            File.SetCreationTime(filePath, date);
            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            logger.Error(e, "Failed to set file date");
            return Task.FromResult(false);
        }
    }

    public Task<bool> SetFileDateModified(string filePath, DateTime date)
    {
        try
        {
            File.SetLastWriteTime(filePath, date);
            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            logger.Error(e, "Failed to set file date");
            return Task.FromResult(false);
        }
    }
}