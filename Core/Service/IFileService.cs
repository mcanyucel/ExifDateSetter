namespace Core.Service;

public interface IFileService
{
    Task<DateOnly?> ExtractFileDateCreated(string filePath);
    Task<DateOnly?> ExtractFileDateModified(string filePath);
    Task<bool> SetFileDateCreated(string filePath, DateTime date);
    Task<bool> SetFileDateModified(string filePath, DateTime date);
}