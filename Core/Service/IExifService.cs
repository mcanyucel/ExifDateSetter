using Core.Model;

namespace Core.Service;

public interface IExifService
{
    Task<ExifAnalysisResult> AnalyzeFiles(IEnumerable<string> filePaths, CancellationToken ct);
}