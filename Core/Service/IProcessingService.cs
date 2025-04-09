using Core.Model;

namespace Core.Service;

public interface IProcessingService
{
    public Task<AnalysisResult> AnalyzeFiles(List<string> foldersList,List<string> filesList, IProgress<int> progress, ProcessConfig configuration);
}