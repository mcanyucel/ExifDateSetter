namespace Core.Service;

public interface IPreferenceService
{
    T? GetPreference<T>(string section, string key);
    bool SetPreference(string section, string key, string value);
}