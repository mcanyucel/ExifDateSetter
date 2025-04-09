using Core.Model;

namespace ExifDateSetterWindows.Model;

public class FileTypeSelectionItem
{
    public SupportedFileType SupportedFileType { get; private init; }
    public bool IsSelected { get; set; }

    public static IEnumerable<FileTypeSelectionItem> GetFileTypeSelectionItems() =>
        Enum.GetValues<SupportedFileType>()
            .Select(fileType => new FileTypeSelectionItem
            {
                SupportedFileType = fileType,
                IsSelected = true
            });
}