using Core.Model;
using Core.Service;

namespace ExifDateSetterWindows.Services;

public class WindowsExifService: IExifService
{
    public Task<ExifAnalysisResult> AnalyzeFiles(IEnumerable<string> filePaths, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}