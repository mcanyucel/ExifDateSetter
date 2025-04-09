namespace Core.Model;

public record AnalyzeConfig(
    string[] Extensions,
    FileDateAttribute FileDateAttribute = FileDateAttribute.DateCreated,
    ExifDateTag ExifDateTag = ExifDateTag.DateTimeOriginal,
    int MaxNumberOfThreads = 0,
    bool IsFolderSearchRecursive = true,
    CancellationToken CancellationToken = default
)
{
    public int MaxDegreeOfParallelism => MaxNumberOfThreads == 0 ? -1 : MaxNumberOfThreads;
}