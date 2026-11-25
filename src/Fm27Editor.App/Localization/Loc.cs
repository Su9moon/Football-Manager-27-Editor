using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Fm27Editor.Core.Settings;

namespace Fm27Editor.App;

/// <summary>
/// Centralized UI text and game-term localization table (English default / Tiếng Việt / 简体中文).
/// Strings stay indexed by English key so English defaults and existing callers read naturally.
/// </summary>
public static partial class Loc
{
    static readonly Lazy<Dictionary<string, string>> AllViLazy = new(BuildAll);
    static readonly Lazy<Dictionary<string, string>> ViMapCiLazy = new(BuildCaseInsensitive);
    static readonly Lazy<Dictionary<string, string>> AllZhLazy = new(BuildChinese);
    static Dictionary<string, string> AllVi => AllViLazy.Value;
    static Dictionary<string, string> ViMapCi => ViMapCiLazy.Value;
    static Dictionary<string, string> AllZh => AllZhLazy.Value;

    static Dictionary<string, string> BuildAll()
    {
        var map = new Dictionary<string, string>(ViMap, StringComparer.Ordinal);
        RegisterLabels(map);
        RegisterMotives(map);
        RegisterShell(map);
        RegisterPlayers(map);
        RegisterEntities(map);
        return map;
    }

    static partial void RegisterLabels(Dictionary<string, string> m);
    static partial void RegisterMotives(Dictionary<string, string> m);
    static partial void RegisterShell(Dictionary<string, string> m);
    static partial void RegisterPlayers(Dictionary<string, string> m);
    static partial void RegisterEntities(Dictionary<string, string> m);

    static Dictionary<string, string> BuildCaseInsensitive()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (k, v) in AllVi)
            map.TryAdd(k, v);
        return map;
    }

    public static bool IsVi => LanguageSetting.IsVietnamese;
    public static bool IsZhCN => LanguageSetting.IsChineseSimplified;

    public static CultureInfo FormatCulture => IsVi
        ? CultureInfo.GetCultureInfo("vi-VN")
        : IsZhCN ? CultureInfo.GetCultureInfo("zh-CN") : CultureInfo.InvariantCulture;

    public static string T(string english, string vietnamese) =>
        IsVi ? vietnamese : IsZhCN ? TranslateChinese(english) : english;

    public static string S(string english) => Shortcuts.Label(Translate(english));

    static string Translate(string english)
    {
        if (string.IsNullOrEmpty(english)) return english;
        if (IsZhCN) return TranslateChinese(english);
        if (!IsVi) return english;
        if (AllVi.TryGetValue(english, out var exact)) return exact;
        if (ViMapCi.TryGetValue(english, out var ci)) return ci;
        return english;
    }

    static string TranslateChinese(string english) =>
        AllZh.TryGetValue(english, out var translated) ? translated : english;

    // A financial balance and a player's balance are different FM concepts.
    public static string Attribute(string english) => english == "Balance" ? T(english, "Thăng bằng") : S(english);

    public static string KitColour(string english) => english switch
    {
        "Foreground" => T(english, "Màu chính"),
        "Background" => T(english, "Màu nền"),
        _ => S(english),
    };

    public static string ClubField(string english) => english == "Training" ? T(english, "Cơ sở tập luyện") : S(english);

    public static string F(string englishTemplate, params object?[] args)
        => string.Format(FormatCulture, S(englishTemplate), args);

    public static IReadOnlyDictionary<string, string> VietnameseTable => AllVi;
    public static IReadOnlyDictionary<string, string> ChineseSimplifiedTable => AllZh;

    /// <summary>Keeps a control property synced with language changes while attached to the visual tree.</summary>
    public static T Bind<T>(T control, Action apply) where T : Visual
    {
        apply();
        control.AttachedToVisualTree += (_, _) => { apply(); LanguageSetting.Changed += apply; };
        control.DetachedFromVisualTree += (_, _) => LanguageSetting.Changed -= apply;
        return control;
    }

    public static TextBlock BindText(TextBlock tb, string key) => Bind(tb, () => tb.Text = S(key));
    public static Button BindContent(Button btn, string key) => Bind(btn, () => btn.Content = S(key));
    public static T BindTip<T>(T ctl, string key) where T : Control => Bind(ctl, () =>
    {
        ToolTip.SetTip(ctl, S(key));
        Avalonia.Automation.AutomationProperties.SetName(ctl, S(key));
    });
    public static T BindToolTip<T>(T ctl, string key) where T : Control => BindTip(ctl, key);
    public static TextBox BindWatermark(TextBox tb, string key) => Bind(tb, () => tb.Watermark = S(key));
}
