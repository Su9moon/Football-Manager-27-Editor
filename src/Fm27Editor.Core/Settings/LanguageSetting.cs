namespace Fm27Editor.Core.Settings;

/// <summary>The display languages supported by the editor UI.</summary>
public enum AppLanguage { English, Vietnamese, ChineseSimplified }

/// <summary>
/// The global UI language chosen by the user in Settings (English by default) and persisted between runs.
/// </summary>
public static class LanguageSetting
{
    static readonly string[] Codes = ["en", "vi", "zh-CN"];
    static string SettingPath => SettingPathOverride ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fm27Editor", "language.txt");

    /// <summary>Tests point this at a temporary file so they never touch the user's saved choice.</summary>
    public static string? SettingPathOverride { get; set; }

    public static AppLanguage Current { get; private set; } = AppLanguage.English;

    public static bool IsVietnamese => Current == AppLanguage.Vietnamese;
    public static bool IsChineseSimplified => Current == AppLanguage.ChineseSimplified;

    /// <summary>Raised after the language selection changes.</summary>
    public static event Action? Changed;

    public static string CodeOf(AppLanguage language) => Codes[(int)language];

    public static string DisplayName(AppLanguage language) => language switch
    {
        AppLanguage.Vietnamese => "Tiếng Việt",
        AppLanguage.ChineseSimplified => "简体中文",
        _ => "English",
    };

    public static void Load()
    {
        Current = AppLanguage.English;
        try
        {
            if (File.Exists(SettingPath) && Array.IndexOf(Codes, File.ReadAllText(SettingPath).Trim(), 0) is var at and >= 0)
                Current = (AppLanguage)at;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }

    public static void Set(AppLanguage language)
    {
        Current = language;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingPath)!);
            File.WriteAllText(SettingPath, CodeOf(language));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        Changed?.Invoke();
    }
}
