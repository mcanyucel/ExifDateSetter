using System.IO;
using Core.Service;

namespace ExifDateSetterWindows.Services;

public class WindowsFileSystemService : IFileSystemService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="isRecursive"></param>
    /// <returns></returns>
    public Task<IEnumerable<string>> GetFilesFromFolder(string folderPath, bool isRecursive = true)
    {
        if (!Directory.Exists(folderPath)) return Task.FromResult(Enumerable.Empty<string>());
        var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        return Task.FromResult(Directory.EnumerateFiles(folderPath, "*", searchOption));
    }
}