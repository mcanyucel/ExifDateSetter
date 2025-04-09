
using System.Collections.Concurrent;
using System.Windows.Media;
using Core.Model;
using Core.Service;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace ExifDateSetterWindows.Services;

public class WindowsExifService: IExifService
{
    public Task <DateOnly?> ExtractExifDateTag(string imagePath, ExifDateTag exifDateTag)
    {
        DateOnly? result = null;
        IEnumerable<Directory> directories = ImageMetadataReader.ReadMetadata(imagePath);
        var exifSubIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        if (exifSubIfd == null)
            return Task.FromResult(result);
        var tagId = MapExifDateTagToId(exifDateTag);
        if (!exifSubIfd.ContainsTag(tagId))
            return Task.FromResult(result);;
        var dateTime = exifSubIfd.GetDateTime(tagId);
        result  = DateOnly.FromDateTime(dateTime);
        return Task.FromResult(result);
    }

    private static int MapExifDateTagToId(ExifDateTag exifDateTag)
    {
        return exifDateTag switch
        {
            ExifDateTag.DateTimeOriginal => ExifDirectoryBase.TagDateTimeOriginal,
            _ => throw new ArgumentOutOfRangeException(nameof(exifDateTag), exifDateTag, null)
        };
    }
}