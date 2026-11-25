using Fm27Editor.Core.Settings;

namespace Fm27Editor.App.Tests;

public sealed class LocalizationTests : IDisposable
{
    readonly string file = Path.Combine(Path.GetTempPath(), "fm27-lang-test-" + Guid.NewGuid().ToString("N") + ".txt");

    public LocalizationTests()
    {
        LanguageSetting.SettingPathOverride = file;
        LanguageSetting.Set(AppLanguage.English);
    }

    public void Dispose()
    {
        LanguageSetting.Set(AppLanguage.English);
        LanguageSetting.SettingPathOverride = Path.Combine(Path.GetTempPath(), "fm27-app-tests-lang-" + Environment.ProcessId + ".txt");
        if (File.Exists(file)) File.Delete(file);
    }

    [Fact]
    public void Defaults_to_english_and_switches_to_vietnamese_for_strings_formatting_and_pairs()
    {
        Assert.False(Loc.IsVi);
        Assert.Equal("Players", Loc.S("Players"));
        Assert.Equal("Nothing changed (3 checked)", Loc.F("Nothing changed ({0:N0} checked)", 3));
        Assert.Equal("Save", Loc.T("Save", "Lưu"));

        LanguageSetting.Set(AppLanguage.Vietnamese);
        Assert.True(Loc.IsVi);
        Assert.Equal("Cầu thủ", Loc.S("Players"));
        Assert.Equal("Không có gì thay đổi (3 cầu thủ đã kiểm tra)", Loc.F("Nothing changed ({0:N0} checked)", 3));
        Assert.Equal("Lưu", Loc.T("Save", "Lưu"));

        LanguageSetting.Set(AppLanguage.ChineseSimplified);
        Assert.False(Loc.IsVi);
        Assert.True(Loc.IsZhCN);
        Assert.Equal("球员", Loc.S("Players"));
        Assert.Equal("没有更改（已检查 3 项）", Loc.F("Nothing changed ({0:N0} checked)", 3));
        Assert.Equal("保存", Loc.T("Save", "Lưu"));

        LanguageSetting.Set(AppLanguage.English);
        Assert.False(Loc.IsVi);
        Assert.False(Loc.IsZhCN);
        Assert.Equal("Players", Loc.S("Players"));
    }

    [Theory]
    [InlineData("Cond", "Thể lực", "Condition (0–10000)", "Thể lực (0–10000)")]
    [InlineData("Nat", "Q. tịch", "Nationality", "Quốc tịch")]
    [InlineData("Ht.", "Cao", "Height (cm)", "Chiều cao (cm)")]
    [InlineData("Build backroom", "Nâng cấp ban hỗ trợ", "Build a Backroom Team", "Nâng cấp đội ngũ hỗ trợ")]
    public void Compact_labels_keep_the_full_tooltip_in_vietnamese(
        string label, string vietnameseLabel, string tooltip, string vietnameseTooltip)
    {
        LanguageSetting.Set(AppLanguage.Vietnamese);

        var header = ToolGrid.Header(label, tooltip);

        Assert.Equal(vietnameseLabel, Assert.IsType<Avalonia.Controls.TextBlock>(header).Text);
        Assert.Equal(vietnameseTooltip, Avalonia.Controls.ToolTip.GetTip(header));
    }

    [Fact]
    public void Language_setting_persists_choice_and_defaults_to_english_when_missing()
    {
        LanguageSetting.Set(AppLanguage.Vietnamese);
        Assert.Equal("vi", File.ReadAllText(file).Trim());

        LanguageSetting.Load();
        Assert.Equal(AppLanguage.Vietnamese, LanguageSetting.Current);
        Assert.Equal("Tiếng Việt", LanguageSetting.DisplayName(AppLanguage.Vietnamese));

        LanguageSetting.Set(AppLanguage.ChineseSimplified);
        Assert.Equal("zh-CN", File.ReadAllText(file).Trim());
        LanguageSetting.Load();
        Assert.Equal(AppLanguage.ChineseSimplified, LanguageSetting.Current);
        Assert.Equal("简体中文", LanguageSetting.DisplayName(AppLanguage.ChineseSimplified));

        File.Delete(file);
        LanguageSetting.Load();
        Assert.Equal(AppLanguage.English, LanguageSetting.Current);
        Assert.Equal("English", LanguageSetting.DisplayName(AppLanguage.English));
    }

    [Fact]
    public void Vietnamese_table_entries_are_populated_without_blank_translations_and_include_retire_and_bulk_keys()
    {
        Assert.True(Loc.VietnameseTable.Count > 2000);
        foreach (var (key, value) in Loc.VietnameseTable)
        {
            Assert.False(string.IsNullOrWhiteSpace(key), $"Blank localization key found (value='{value}')");
            Assert.False(string.IsNullOrWhiteSpace(value), $"Blank Vietnamese translation for '{key}'");
        }

        string[] requiredKeys =
        [
            "Retiring",
            "Search retiring…",
            "Prevent retirement",
            "Actions ({0:N0} selected)",
            "Mass Edit…",
            "Language",
            "Save changes",
        ];
        foreach (var key in requiredKeys)
            Assert.True(Loc.VietnameseTable.ContainsKey(key), $"Missing Vietnamese key: {key}");
    }

    [Fact]
    public void Simplified_Chinese_table_has_core_ui_labels_and_nonblank_translations()
    {
        Assert.True(Loc.ChineseSimplifiedTable.Count >= 180);
        foreach (var (key, value) in Loc.ChineseSimplifiedTable)
        {
            Assert.False(string.IsNullOrWhiteSpace(key), "Blank Chinese localization key found");
            Assert.False(string.IsNullOrWhiteSpace(value), $"Blank Chinese translation for '{key}'");
        }

        string[] requiredKeys = ["Settings", "Language", "Players", "Clubs", "Save changes", "Load Game", "Development Speed", "Work Permit"];
        foreach (var key in requiredKeys)
            Assert.True(Loc.ChineseSimplifiedTable.ContainsKey(key), $"Missing Simplified Chinese key: {key}");
    }

    [Fact]
    public void Simplified_Chinese_save_labels_remain_recognised_by_the_single_save_workflow()
    {
        LanguageSetting.Set(AppLanguage.ChineseSimplified);

        Assert.True(SectionHost.IsSaveButton(new Avalonia.Controls.Button { Content = Loc.S("Save changes") }));
        Assert.True(SectionHost.IsSaveButton(new Avalonia.Controls.Button { Content = Loc.S("Save contract") }));
        Assert.True(SectionHost.IsSaveButton(new Avalonia.Controls.Button { Content = Loc.S("Save loan") }));
        Assert.True(SectionHost.IsSaveButton(new Avalonia.Controls.Button { Content = Loc.S("Save") }));
    }
}
