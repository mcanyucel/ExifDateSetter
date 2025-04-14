using ControlzEx.Theming;
using Core.Service;
using Serilog;

namespace ExifDateSetterWindows.Services;

public class MahappsThemeService(IPreferenceService preferenceService, ILogger logger) : IThemeService
{

    public void SetUserTheme()
    {
        try
        {
            var theme = preferenceService.GetPreference<string>(IThemeService.ThemeSectionKey, IThemeService.ThemeKey);
            var accent = preferenceService.GetPreference<string>(IThemeService.ThemeSectionKey, IThemeService.AccentKey) ??
                         IThemeService.DefaultAccent;
            var saveTheme = string.IsNullOrEmpty(theme);
            SetTheme(theme ?? IThemeService.ThemeDefault, accent, saveTheme);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to set user theme");
        }
    }

    public void SetTheme(string theme, string accent, bool savePreference)
    {
        var newStyleName = $"{theme}.{accent}";
        ThemeManager.Current.ChangeTheme(App.Current, newStyleName);
        if (!savePreference) return;

        preferenceService.SetPreference(IThemeService.ThemeSectionKey, IThemeService.ThemeKey, theme);
        preferenceService.SetPreference(IThemeService.ThemeSectionKey, IThemeService.AccentKey, accent);
    }

}