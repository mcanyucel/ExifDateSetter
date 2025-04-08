
using System.Collections.Concurrent;
using System.Windows.Media;
using Core.Model;
using Core.Service;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace ExifDateSetterWindows.Services;

public class WindowsExifService: IExifService
{
    public Task<ExifAnalysisResult> AnalyzeFiles(IEnumerable<string> filePaths, ExifDateTag exifDateTag, int maxNumberOfThreads, CancellationToken ct)
    {
        if (ct.IsCancellationRequested) return Task.FromCanceled<ExifAnalysisResult>(ct);

        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxNumberOfThreads == 0 ? -1 : maxNumberOfThreads };
        ConcurrentBag<DateOnly?> dates = [];
        /*
         * Storing all the dates is inefficient in terms of memory, but 4 bytes per record is acceptable rather than
         * partitioning the file list into smaller chunks and processing them in parallel, as we should not use
         * global locks in parallel processing.
         */
        Parallel.ForEach(filePaths, parallelOptions, filePath =>
        {
            var date = ExtractExifDateTag(filePath, exifDateTag);
            dates.Add(date);
        });
        var datesWithValue = dates.Where(d => d.HasValue).ToList();
        var minimumDate = datesWithValue.Min(d => d!.Value);
        var maximumDate = datesWithValue.Max(d => d!.Value);
        var numberOfFilesWithExifDate = datesWithValue.Count;
        return Task.FromResult(new ExifAnalysisResult(numberOfFilesWithExifDate, minimumDate, maximumDate));
    }

    private static DateOnly? ExtractExifDateTag(string imagePath, ExifDateTag exifDateTag)
    {
        IEnumerable<Directory> directories = ImageMetadataReader.ReadMetadata(imagePath);
        var exifSubIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        if (exifSubIfd == null)
            return null;
        var tagId = MapExifDateTagToId(exifDateTag);
        if (!exifSubIfd.ContainsTag(tagId))
            return null;
        var dateTime = exifSubIfd.GetDateTime(tagId);
        return DateOnly.FromDateTime(dateTime);
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