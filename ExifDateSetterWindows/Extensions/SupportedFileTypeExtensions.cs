using Core.Model;

namespace ExifDateSetterWindows.Extensions;

public static class SupportedFileTypeExtensions
{
    public static string GetFileExtension(this SupportedFileType fileType)
    {
        return $".{fileType.ToString().ToLower()}";
    }
}