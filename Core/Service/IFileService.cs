using Core.Model;

namespace Core.Service;

public interface IFileService
{
    public Task<FileAnalysisResult> AnalyzeFiles(IEnumerable<string> filePaths, CancellationToken ct, FileDateAttribute dateAttribute, int maxNumberOfThreads);
}