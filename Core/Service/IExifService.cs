using Core.Model;

namespace Core.Service;

public interface IExifService
{
    Task<DateOnly?> ExtractExifDateOnlyTag(string imagePath, ExifDateTag exifDateTag);
    Task<DateTime?> ExtractExifDateTimeTag(string imagePath, ExifDateTag exifDateTag);
    Task<bool> SetExifDateTag(string imagePath, DateTime date, ExifDateTag exifDateTag);
}