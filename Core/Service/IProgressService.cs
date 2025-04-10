namespace Core.Service;

public interface IProgressService
{
    bool ShouldReportProgress(int currentCount, int totalFilesCount);
}