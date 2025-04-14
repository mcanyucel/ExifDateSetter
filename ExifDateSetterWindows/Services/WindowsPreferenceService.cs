using Core.Service;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace ExifDateSetterWindows.Services;

public class WindowsPreferenceService(IConfigurationRoot configurationRoot, ILogger logger) : IPreferenceService
{
    public T? GetPreference<T>(string section, string key)
    {
        try
        {
            return configurationRoot.GetSection(section).GetValue<T>(key);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error getting preference");
            return default; // this can return actual values for non-nullable types
            
        }
    }

    public bool SetPreference(string section, string key, string value)
    {
        try
        {
            var sectionToUpdate = configurationRoot.GetSection(section);
            sectionToUpdate[key] = value;
            return true;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error setting preference");
            return false;
        }
    }
}