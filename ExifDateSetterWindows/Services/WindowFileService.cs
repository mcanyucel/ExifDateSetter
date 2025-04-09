using System.IO;
using Core.Service;

namespace ExifDateSetterWindows.Services;

public class WindowFileService : IFileService
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
}