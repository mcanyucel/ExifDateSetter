using Core.Model;
using Core.Service;
using ExifLibrary;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Serilog;

namespace ExifDateSetterWindows.Services;

public class WindowsExifService(ILogger logger): IExifService
{
    private readonly Dictionary<ExifDateTag, int> _metadataExtractorTagMap = new()
    {
        { ExifDateTag.DateTimeOriginal, ExifDirectoryBase.TagDateTimeOriginal }
    };
    private readonly Dictionary<ExifDateTag, ExifTag> _exifLibraryTagMap = new()
    {
        { ExifDateTag.DateTimeOriginal, ExifTag.DateTimeOriginal }
    };
    
    public Task <DateOnly?> ExtractExifDateTag(string imagePath, ExifDateTag exifDateTag)
    {
        DateOnly? result = null;
        IEnumerable<Directory> directories = ImageMetadataReader.ReadMetadata(imagePath);
        var exifSubIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        if (exifSubIfd == null)
            return Task.FromResult(result);
        var tagId = _metadataExtractorTagMap[exifDateTag];
        if (!exifSubIfd.ContainsTag(tagId))
            return Task.FromResult(result);
        var dateTime = exifSubIfd.GetDateTime(tagId);
        result  = DateOnly.FromDateTime(dateTime);
        return Task.FromResult(result);
    }

    public Task<bool> SetExifDateTag(string imagePath, DateTime date, ExifDateTag exifDateTag)
    {
        try
        {
            var file = ImageFile.FromFile(imagePath);
            var exifTag = _exifLibraryTagMap[exifDateTag];
            file.Properties.Set(exifTag, date);
            file.Save(imagePath);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to set exif date");
            return Task.FromResult(false);
        }
    }
}