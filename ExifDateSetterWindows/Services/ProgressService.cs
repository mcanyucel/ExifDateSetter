using Core.Service;

namespace ExifDateSetterWindows.Services;

public class ProgressService : IProgressService
{
    private DateTime _lastReportTime = DateTime.MinValue;
    private readonly Lock _processReportLockObject = new();
    private const int ProgressReportIntervalMs = 100;
    
    public bool ShouldReportProgress(int currentCount, int totalFilesCount)
    {
        lock (_processReportLockObject)
        {
            var now = DateTime.Now;
            if (!((now - _lastReportTime).TotalMilliseconds >= ProgressReportIntervalMs) && currentCount != totalFilesCount) return false;
            _lastReportTime = now;
            return true;
        }
    }
}