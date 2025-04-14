namespace Core.Service;

public interface IThemeService
{
    void SetUserTheme();
    
    void SetTheme(string theme, string accent, bool savePreference);
    
    public const string ThemeSectionKey = "Theme";
    public const string ThemeKey = "Name";
    public const string AccentKey = "Accent";
    public const string ThemeDefault = "Light";
    public const string ThemeDark = "Dark";
    public const string ThemeLight = "Light";
    public const string DefaultAccent = "Blue";
}