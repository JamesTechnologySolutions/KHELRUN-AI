using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Documents;
using KhelrunAI.Services;
using Microsoft.Win32;

namespace KhelrunAI;

public partial class MainWindow : Window
{
    private readonly AppSettings _settings = App.Settings;
    private readonly LlmService _llm;
    private readonly WebSearchService _web = new();
    private readonly List<(bool User, TextBlock Text)> _messages = new();
    private CancellationTokenSource? _cts;
    private string _modelName = "";

    public MainWindow()
    {
        InitializeComponent();
        _llm = new LlmService(_settings);

        WebToggle.IsChecked = _settings.WebSearch;
        TextElement.SetFontSize(ChatPanel, _settings.FontSize);
        UpdateThemeIcon();

        AddBubble(
            "Welcome to **KHELRUN AI** by JTS.\n\n" +
            "Private, unrestricted local AI that runs entirely on your PC.\n" +
            "• Chat models download automatically (best match for your hardware)\n" +
            "• Import any GGUF chat model or image model files\n" +
            "• Right-click any message to copy it\n\n" +
            "Getting ready…", false, track: false);

        Closed += (_, _) => { _settings.Save(); _llm.Dispose(); };
        Loaded += async (_, _) => await StartupModelAsync();
    }

    /// <summary>
    /// On launch: reload last model, else pick a bundled/downloaded one,
    /// else auto-recommend + download the best model for this machine.
    /// </summary>
    private async Task StartupModelAsync()
    {
        if (!_settings.AutoLoadLastModel) return;

        var last = _settings.LastModelPath;
        var path = !string.IsNullOrEmpty(last) && File.Exists(last) ? last : ModelCatalog.FirstInstalled();

        if (path != null)
        {
            await LoadModelAsync(path);
            return;
        }

        // No model present — auto-select best for this hardware
        if (_settings.AutoDownloadBestModel)
        {
            var best = ModelCatalog.RecommendBestChatModel();
            long ram = ModelCatalog.GetTotalRamGb();

            var msg =
                $"KHELRUN AI detected ~{ram} GB of RAM.\n\n" +
                $"Recommended model: {best.Name}\n" +
                $"{best.Description}\n" +
                $"Size ≈ {ModelCatalog.FormatBytes(best.ApproxBytes)}\n\n" +
                "Download and install it now? (You can change models later.)";

            var ans = MessageBox.Show(msg, "KHELRUN AI — Best model for your PC",
                MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (ans == MessageBoxResult.Yes)
            {
                StatusText.Text = "Downloading recommended model…";
                SetBusy(true);
                try
                {
                    var progress = new Progress<(long Done, long Total)>(p =>
                    {
                        if (p.Total > 0)
                            StatusText.Text = $"Downloading… {ModelCatalog.FormatBytes(p.Done)} / {ModelCatalog.FormatBytes(p.Total)}";
                    });
                    await ModelDownloader.DownloadAsync(best, progress, CancellationToken.None);
                    var installed = ModelCatalog.FindInstalled(best);
                    if (installed != null)
                        await LoadModelAsync(installed);
                    else
                        await PickModelAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Auto-download failed:\n" + ex.Message +
                        "\n\nYou can download a model manually from the Models panel.",
                        "KHELRUN AI", MessageBoxButton.OK, MessageBoxImage.Warning);
                    await PickModelAsync();
                }
                finally { SetBusy(false); }
                return;
            }
        }

        await PickModelAsync();
    }

    private async Task PickModelAsync()
    {
        var win = new ModelsWindow { Owner = this };
        if (win.ShowDialog() == true && win.SelectedPath != null)
            await LoadModelAsync(win.SelectedPath);
    }

    // ---------- Navigation ----------

    private void NavChat_Click(object sender, RoutedEventArgs e)
    {
        // Already on chat; could highlight later
    }

    private async void NavModels_Click(object sender, RoutedEventArgs e) => await PickModelAsync();

    private void NavImage_Click(object sender, RoutedEventArgs e)
    {
        var win = new ModelsWindow(ModelKind.Image) { Owner = this };
        win.ShowDialog();
        // Image models are tracked; full diffusion backend can be plugged in later.
        if (win.SelectedPath != null)
        {
            _settings.LastImageModelPath = win.SelectedPath;
            _settings.Save();
            StatusText.Text = "Image model registered: " + Path.GetFileName(win.SelectedPath);
            MessageBox.Show(
                "Image model imported and saved.\n\n" +
                "KHELRUN AI now tracks this model. Local image generation backends " +
                "(Stable Diffusion / SDXL / Flux) can be connected in a future update. " +
                "Your imported models stay ready on disk.",
                "KHELRUN AI — Image models", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // ---------- Model ----------

    private async void LoadModel_Click(object sender, RoutedEventArgs e) => await PickModelAsync();

    private async Task LoadModelAsync(string path)
    {
        SetBusy(true);
        StatusText.Text = "Loading model…";
        SendBtn.IsEnabled = false;
        try
        {
            await _llm.LoadAsync(path);
            _modelName = Path.GetFileName(path);
            StatusText.Text = "Model: " + _modelName;
            SendBtn.IsEnabled = true;
            _settings.LastModelPath = path;
            _settings.Save();

            // Clear the welcome bubble and show ready state
            if (_messages.Count == 0)
            {
                ChatPanel.Children.Clear();
                AddBubble("Model ready. Ask me anything — KHELRUN AI is unrestricted and private.", false, track: false);
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = "Load failed";
            MessageBox.Show("Could not load the model:\n" + ex.Message, "KHELRUN AI",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    // ---------- Toolbar ----------

    private void NewChat_Click(object sender, RoutedEventArgs e) => ClearChat();

    private void ClearChat()
    {
        _cts?.Cancel();
        ChatPanel.Children.Clear();
        _messages.Clear();
        _llm.NewChat();
        AddBubble("New conversation started.", false, track: false);
    }

    private void WebToggle_Click(object sender, RoutedEventArgs e)
    {
        _settings.WebSearch = WebToggle.IsChecked == true;
        _settings.Save();
    }

    private void Theme_Click(object sender, RoutedEventArgs e)
    {
        _settings.Theme = _settings.Theme == "Dark" ? "Light" : "Dark";
        ThemeService.Apply(_settings.Theme);
        UpdateThemeIcon();
        _settings.Save();
    }

    private void UpdateThemeIcon() =>
        ThemeIcon.Text = _settings.Theme == "Dark" ? "☀️" : "🌙";

    private void Export_Click(object sender, RoutedEventArgs e)
    {
        if (_messages.Count == 0)
        {
            MessageBox.Show("There is nothing to export yet.", "KHELRUN AI");
            return;
        }
        var dlg = new SaveFileDialog
        {
            Filter = "Markdown (*.md)|*.md|Text (*.txt)|*.txt",
            FileName = $"KHELRUN AI chat {DateTime.Now:yyyy-MM-dd HHmm}.md"
        };
        if (dlg.ShowDialog() != true) return;

        var sb = new StringBuilder();
        sb.AppendLine($"# KHELRUN AI chat — {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"*Made by JTS*\n");
        foreach (var (user, tb) in _messages)
            sb.AppendLine(user ? "**You:**" : "**KHELRUN AI:**").AppendLine(tb.Text).AppendLine();
        try { File.WriteAllText(dlg.FileName, sb.ToString()); }
        catch (Exception ex) { MessageBox.Show("Could not save the file:\n" + ex.Message, "KHELRUN AI"); }
    }

    private async void Settings_Click(object sender, RoutedEventArgs e)
    {
        var before = (_settings.ContextSize, _settings.Threads);
        var win = new SettingsWindow(_settings) { Owner = this };
        if (win.ShowDialog() != true) return;

        _settings.Save();
        ThemeService.Apply(_settings.Theme);
        UpdateThemeIcon();
        TextElement.SetFontSize(ChatPanel, _settings.FontSize);
        WebToggle.IsChecked = _settings.WebSearch;

        bool needsReload = before != (_settings.ContextSize, _settings.Threads);
        if (needsReload && _llm.IsLoaded && !string.IsNullOrEmpty(_settings.LastModelPath))
        {
            var ans = MessageBox.Show(
                "Context size or CPU threads changed. Reload the model now to apply them?\n(This starts a new chat.)",
                "KHELRUN AI", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ans == MessageBoxResult.Yes)
            {
                ClearChat();
                await LoadModelAsync(_settings.LastModelPath);
            }
        }
    }

    // ---------- Chat ----------

    private void Stop_Click(object sender, RoutedEventArgs e) => _cts?.Cancel();

    private void Send_Click(object sender, RoutedEventArgs e) => _ = SendAsync();

    private void InputBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
        {
            e.Handled = true;
            if (SendBtn.IsEnabled) _ = SendAsync();
        }
    }

    private async Task SendAsync()
    {
        var text = InputBox.Text.Trim();
        if (text.Length == 0 || !_llm.IsLoaded) return;
        InputBox.Clear();

        AddBubble(text, true);
        var reply = AddBubble("", false);

        _cts = new CancellationTokenSource();
        SetBusy(true);
        SendBtn.Visibility = Visibility.Collapsed;
        StopBtn.Visibility = Visibility.Visible;

        int tokens = 0;
        var clock = new Stopwatch();
        try
        {
            var prompt = text;
            if (WebToggle.IsChecked == true)
            {
                reply.Text = "Searching the web…";
                var results = await _web.SearchAsync(text, _settings.WebResults, _cts.Token);
                if (results != null)
                    prompt = results + "\nUsing the results above where helpful, answer this:\n" + text;
                reply.Text = "";
            }

            var sb = new StringBuilder();
            clock.Start();
            await foreach (var tok in _llm.ChatAsync(prompt, _cts.Token))
            {
                tokens++;
                sb.Append(tok);
                reply.Text = sb.ToString();
                ChatScroll.ScrollToEnd();
            }
        }
        catch (OperationCanceledException) { reply.Text += "\n[stopped]"; }
        catch (Exception ex) { reply.Text = "Error: " + ex.Message; }
        finally
        {
            clock.Stop();
            if (tokens > 0 && clock.Elapsed.TotalSeconds > 0.1)
                StatusText.Text = $"Model: {_modelName}  ·  {tokens} tokens  ·  {tokens / clock.Elapsed.TotalSeconds:F1} tok/s";

            SetBusy(false);
            StopBtn.Visibility = Visibility.Collapsed;
            SendBtn.Visibility = Visibility.Visible;
            SendBtn.IsEnabled = _llm.IsLoaded;
            InputBox.Focus();
        }
    }

    private void SetBusy(bool busy) =>
        Busy.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;

    private TextBlock AddBubble(string text, bool user, bool track = true)
    {
        var tb = new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 22
        };
        tb.SetResourceReference(TextBlock.ForegroundProperty, "Fg");

        var border = new Border
        {
            CornerRadius = new CornerRadius(user ? 16 : 16),
            Padding = new Thickness(16, 12, 16, 12),
            Margin = new Thickness(user ? 100 : 0, 6, user ? 0 : 100, 6),
            Child = tb,
            BorderThickness = new Thickness(1),
            BorderBrush = (System.Windows.Media.Brush)FindResource(user ? "AccentDim" : "Border")
        };
        border.SetResourceReference(Border.BackgroundProperty, user ? "UserBubble" : "BotBubble");

        var menu = new ContextMenu();
        var copy = new MenuItem { Header = "Copy message" };
        copy.Click += (_, _) => { try { Clipboard.SetText(tb.Text); } catch { /* clipboard busy */ } };
        menu.Items.Add(copy);
        border.ContextMenu = menu;

        ChatPanel.Children.Add(border);
        if (track) _messages.Add((user, tb));
        ChatScroll.ScrollToEnd();
        return tb;
    }
}
