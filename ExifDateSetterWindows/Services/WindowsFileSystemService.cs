using System.IO;
using Core.Service;

namespace ExifDateSetterWindows.Services;

public class WindowsFileSystemService : IFileSystemService
{
    public Task<IEnumerable<string>> GetFilesFromFolder(string folderPath, CancellationToken ct, string[]? extensions = null, bool isRecursive = true)
    {
        List<string> result = [];
        if (!Directory.Exists(folderPath)) return Task.FromResult(result.AsEnumerable());
        string searchPattern;
        if (extensions == null || extensions.Length == 0)
        {
            searchPattern = "*";
        }
        else 
        {
            searchPattern = string.Join("|", extensions.Select(ext => $"*.{ext}"));
        }
        var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = Directory.EnumerateFiles(folderPath, searchPattern, searchOption);
        result.AddRange(files);
        return Task.FromResult(result.AsEnumerable());
    }
}