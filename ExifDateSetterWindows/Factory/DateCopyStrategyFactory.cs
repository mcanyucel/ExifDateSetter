using Core.Factory;
using Core.Model;
using Core.Strategy;
using ExifDateSetterWindows.Strategy;

namespace ExifDateSetterWindows.Factory;

public class DateCopyStrategyFactory : IDateCopyStrategyFactory
{
    


    private readonly Dictionary<int, IDateCopyStrategy> _dateCopyStrategies = GenerateStrategies();
    public IDateCopyStrategy GetCopyStrategy(List<string> fileList, ProcessConfig configuration, IProgress<int> progress, CancellationToken ct)
    {
        var hasStrategy = _dateCopyStrategies.TryGetValue(HashCode.Combine(configuration.ActionType, configuration.AnalyzeConfig.FileDateAttribute), out var strategy);
        if (!hasStrategy)
        {
            throw new ArgumentOutOfRangeException(nameof(configuration), configuration, "No strategy found for the given configuration.");
        }
        else
        {
            return strategy!;
        }

    }
    private static Dictionary<int, IDateCopyStrategy> GenerateStrategies()
    {
        var actionTypes = Enum.GetValues<ActionType>().ToList();
        var fileDateAttributes = Enum.GetValues<FileDateAttribute>().ToList();
        
        var strategies = new Dictionary<int, IDateCopyStrategy>();
        foreach (var actionType in actionTypes)
        {
            foreach (var fileDateAttribute in fileDateAttributes)
            {
                var strategyHash = HashCode.Combine(actionType, fileDateAttribute);
                IDateCopyStrategy strategy = actionType switch
                {
                    ActionType.ExifToFileDate => fileDateAttribute switch
                    {
                        FileDateAttribute.DateCreated => new ExifToFileCreationStrategy(),
                        FileDateAttribute.DateModified => new ExifToFileLastModifiedStrategy(),
                        _ => throw new ArgumentOutOfRangeException(nameof(fileDateAttribute), fileDateAttribute, null)
                    },
                    ActionType.FileDateToExif => fileDateAttribute switch
                    {
                        FileDateAttribute.DateCreated => new FileCreationToExifDateStrategy(),
                        FileDateAttribute.DateModified => new FileLastModifiedToExifDateStrategy(),
                        _ => throw new ArgumentOutOfRangeException(nameof(fileDateAttribute), fileDateAttribute, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null)
                };
                strategies[strategyHash] = strategy;
            }
        }
        return strategies;
    }
}