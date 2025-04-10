namespace Core.Service;

public interface IFileSystemService
{
    Task<IEnumerable<string>> GetFilesFromFolder(string folderPath, bool isRecursive = true);
}