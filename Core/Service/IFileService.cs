using Core.Model;

namespace Core.Service;

public interface IFileService
{
    Task<DateOnly?> ExtractFileDateCreated(string filePath);
    Task<DateOnly?> ExtractFileDateModified(string filePath);
}