using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Fm27Editor.Core.Diagnostics;
using Fm27Editor.Core.Game;
using Fm27Editor.Core.Settings;

namespace Fm27Editor.App;

public sealed partial class MainWindow
{
    static readonly string[] PluginSetupSteps =
    [
        "Quit Football Manager 27.",
        "Quit Steam completely (Steam menu > Quit Steam).",
        "Select Set up. The editor adds its plugin to the game's Steam launch options; the game's files are not changed.",
        "Start Steam, play Football Manager 27, load your save, then select Load Game.",
    ];

    bool pluginSetupOpen;

    /// <summary>
    /// macOS, Steam copy, System Integrity Protection on: the editor reads the game through its plugin, which the game
    /// loads at start from its Steam launch options. Shown at start and when the game refuses a connection; a Mac with
    /// debugging restrictions off never sees it. The editor cannot read the game without it, so it cannot be skipped.
    /// </summary>
    async Task OfferPluginSetupAsync(bool refused)
    {
        if (!OperatingSystem.IsMacOS()) return;
        var state = await Task.Run(() =>
        {
            // An editor update reaches the game at its next start; the launch options already name the fixed launcher.
            try { MacPluginSetup.RefreshPlugin(); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            return MacPluginSetup.State();
        });
        if (state == MacPluginState.Missing)
            await ShowPluginSetupAsync();
        else if (state == MacPluginState.Installed && refused)
            ShowNotice("Restart Football Manager 27",
                "The plugin is set up, but this game was started without it. Quit Football Manager 27, start it again from Steam, load your save, then select Load Game.",
                ("OK", true, null));
    }

    /// <summary>
    /// The setup window: why the plugin is needed, the steps, whether the game and Steam are still running (checked every
    /// second; Set up stays off until both have quit), and the result. It closes only once the plugin is set up.
    /// </summary>
    async Task ShowPluginSetupAsync()
    {
        if (pluginSetupOpen) return;
        pluginSetupOpen = true;
        bool done = false;

        var gameStatus = new TextBlock { FontWeight = FontWeight.SemiBold };
        var steamStatus = new TextBlock { FontWeight = FontWeight.SemiBold };
        var error = new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = UiKit.Brush("#F87171"), IsVisible = false };
        var setUp = Loc.BindContent(new Button { MinWidth = 120, HorizontalContentAlignment = HorizontalAlignment.Center, IsEnabled = false }, "Set up");
        setUp.Classes.Add("accent");

        // The window blocks the editor until the plugin is set up, so the language is chosen here too.
        var languages = new[]
        {
            new LanguageChoice(AppLanguage.English, LanguageSetting.DisplayName(AppLanguage.English)),
            new LanguageChoice(AppLanguage.Vietnamese, LanguageSetting.DisplayName(AppLanguage.Vietnamese)),
            new LanguageChoice(AppLanguage.ChineseSimplified, LanguageSetting.DisplayName(AppLanguage.ChineseSimplified)),
        };
        var language = new ComboBox { ItemsSource = languages, SelectedItem = languages.First(c => c.Language == LanguageSetting.Current), Width = 160 };
        language.SelectionChanged += (_, _) =>
        {
            if (language.SelectedItem is LanguageChoice chosen && chosen.Language != LanguageSetting.Current) LanguageSetting.Set(chosen.Language);
        };
        var languageRow = new StackPanel
        {
            Orientation = Orientation.Horizontal, Spacing = 10, HorizontalAlignment = HorizontalAlignment.Right,
            Children = { Loc.BindText(new TextBlock { VerticalAlignment = VerticalAlignment.Center, Foreground = UiKit.TextSecondary }, "Language"), language },
        };

        var steps = new StackPanel { Spacing = 8 };
        for (int i = 0; i < PluginSetupSteps.Length; i++)
        {
            var text = Loc.BindText(new TextBlock { TextWrapping = TextWrapping.Wrap }, PluginSetupSteps[i]);
            Grid.SetColumn(text, 1);
            steps.Children.Add(new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("28,*"),
                Children = { new TextBlock { Text = $"{i + 1}.", FontWeight = FontWeight.SemiBold }, text },
            });
        }

        var body = new StackPanel
        {
            Margin = new Thickness(28, 24), Spacing = 18,
            Children =
            {
                languageRow,
                Loc.BindText(new TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = 15 },
                    "This Mac has System Integrity Protection (SIP) on. To read and edit Football Manager 27 without turning SIP off, the editor needs its plugin in the game. You only do this once."),
                steps,
                new Border
                {
                    Padding = new Thickness(14, 10), CornerRadius = new CornerRadius(6), Background = UiKit.Brush("#1F2937"),
                    Child = new StackPanel { Spacing = 6, Children = { gameStatus, steamStatus } },
                },
                Loc.BindText(new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = UiKit.TextSecondary }, "The editor cannot read the game until the plugin is set up."),
                error,
                new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Children = { setUp } },
            },
        };
        var window = new Window
        {
            Title = Loc.S("Set up the editor plugin"), Width = 640, SizeToContent = SizeToContent.Height, CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner, ShowInTaskbar = false, Content = body,
        };
        window.Closing += (_, e) => e.Cancel = !done;
        void Retitle() => Dispatcher.UIThread.Post(() => { window.Title = Loc.S("Set up the editor plugin"); Refresh(); });
        LanguageSetting.Changed += Retitle;

        void Refresh()
        {
            bool game = MacPluginSetup.GameRunning(), steam = MacPluginSetup.SteamRunning();
            gameStatus.Text = Loc.S(game ? "Football Manager 27: running, quit it" : "Football Manager 27: closed");
            gameStatus.Foreground = UiKit.Brush(game ? "#FBBF24" : "#4ADE80");
            steamStatus.Text = Loc.S(steam ? "Steam: running, quit it" : "Steam: closed");
            steamStatus.Foreground = UiKit.Brush(steam ? "#FBBF24" : "#4ADE80");
            setUp.IsEnabled = !game && !steam;
        }
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) => Refresh();
        Refresh();
        timer.Start();

        setUp.Click += async (_, _) =>
        {
            setUp.IsEnabled = false;
            error.IsVisible = false;
            try
            {
                int entries = await Task.Run(MacPluginSetup.Install);
                FlightRecorder.Current?.Log("plugin", $"set up for {entries} Steam game entries");
                timer.Stop();
                done = true;
                var ok = Loc.BindContent(new Button { MinWidth = 120, HorizontalContentAlignment = HorizontalAlignment.Center }, "OK");
                ok.Classes.Add("accent");
                ok.Click += (_, _) => window.Close();
                body.Children.Clear();
                body.Children.Add(Loc.BindText(new TextBlock { FontSize = 18, FontWeight = FontWeight.SemiBold }, "Plugin set up"));
                body.Children.Add(Loc.BindText(new TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = 15 }, "Start Steam, play Football Manager 27, load your save, then select Load Game."));
                body.Children.Add(new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Children = { ok } });
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or InvalidDataException)
            {
                error.Text = UserMessage.For(ex);
                error.IsVisible = true;
                Refresh();
            }
        };

        try { await window.ShowDialog(this); }
        finally
        {
            LanguageSetting.Changed -= Retitle;
            timer.Stop();
            pluginSetupOpen = false;
        }
    }
}
