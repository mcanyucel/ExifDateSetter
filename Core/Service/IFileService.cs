namespace Core.Service;

public interface IFileService
{
    Task<DateOnly?> ExtractFileDateOnlyCreated(string filePath);
    Task<DateOnly?> ExtractFileDateOnlyModified(string filePath);
    Task<DateTime?> ExtractFileDateTimeCreated(string filePath);
    Task<DateTime?> ExtractFileDateTimeModified(string filePath);
    Task<bool> SetFileDateCreated(string filePath, DateTime date);
    Task<bool> SetFileDateModified(string filePath, DateTime date);
}