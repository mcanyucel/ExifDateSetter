using Core.Model;
using Core.Strategy;

namespace Core.Factory;

public interface IDateCopyStrategyFactory
{
    IDateCopyStrategy GetCopyStrategy(ProcessConfig configuration);
}