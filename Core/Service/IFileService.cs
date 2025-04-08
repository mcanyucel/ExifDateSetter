using Core.Model;

namespace Core.Service;

public interface IFileService
{
    public Task<FileAnalysisResult> AnalyzeFiles(List<string> filePaths, FileDateAttribute dateAttribute, int maxNumberOfThreads, CancellationToken ct);
}