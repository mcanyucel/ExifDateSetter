namespace Core.Service;

public interface IFileSystemService
{
    Task<IEnumerable<string>> GetFilesFromFolder(string folderPath, CancellationToken ct, string[]? extensions = null, bool isRecursive = true);
}