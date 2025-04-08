using System.IO;
using Core.Service;

namespace ExifDateSetterWindows.Services;

public class WindowsFileSystemService : IFileSystemService
{
    public async Task<IEnumerable<string>> GetFilesFromFolder(string folderPath, CancellationToken ct, string[]? extensions = null, bool isRecursive = true)
    {
        var result = new List<string>();
        if (!Directory.Exists(folderPath)) return result;
        
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
        var files = await Task.Run(() => Directory.EnumerateFiles(folderPath, searchPattern, searchOption), ct);
        result.AddRange(files);
        return result;
    }
}