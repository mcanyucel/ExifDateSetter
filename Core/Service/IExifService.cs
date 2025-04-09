using Core.Model;

namespace Core.Service;

public interface IExifService
{
    Task<DateOnly?> ExtractExifDateTag(string imagePath, ExifDateTag exifDateTag);
}