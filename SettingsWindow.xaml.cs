using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using KhelrunAI.Services;

namespace KhelrunAI;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _s;
    private readonly string _originalTheme;
    private readonly bool _ready;

    public SettingsWindow(AppSettings settings)
    {
        InitializeComponent();
        _s = settings;
        _originalTheme = settings.Theme;

        (_s.Theme == "Light" ? LightRadio : DarkRadio).IsChecked = true;
        FontSlider.Value = _s.FontSize;
        PromptBox.Text = _s.SystemPrompt;
        TempSlider.Value = _s.Temperature;
        TokensSlider.Value = _s.MaxTokens;
        WebCheck.IsChecked = _s.WebSearch;
        ResultsSlider.Value = _s.WebResults;
        ContextSlider.Value = _s.ContextSize;
        ThreadSlider.Value = _s.Threads;
        GpuLayersSlider.Value = _s.GpuLayers;
        BatchSlider.Value = _s.BatchSize;
        AutoLoadCheck.IsChecked = _s.AutoLoadLastModel;
        AutoBestCheck.IsChecked = _s.AutoDownloadBestModel;

        UpdateGpuLabel();
        _ready = true;
    }

    private void GpuLayersSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_ready) return;
        UpdateGpuLabel();
    }

    private void UpdateGpuLabel()
    {
        int v = (int)Math.Round(GpuLayersSlider.Value);
        GpuLayersLabel.Text = v < 0 ? "Auto" : v == 0 ? "CPU only" : v.ToString();
    }

    private void Theme_Checked(object sender, RoutedEventArgs e)
    {
        if (!_ready) return;
        ThemeService.Apply(LightRadio.IsChecked == true ? "Light" : "Dark");
    }

    private void Preset_Click(object sender, RoutedEventArgs e)
    {
        PromptBox.Text = ((Button)sender).Tag?.ToString() switch
        {
            "unrestricted" => AppSettings.UnrestrictedPrompt,
            "coder" => "You are KHELRUN AI by JTS, an expert programming assistant. Give correct, working code in fenced " +
                       "blocks with a short explanation. Ask a clarifying question if the request is ambiguous. " +
                       "If web results are supplied, use them and mention the source URLs.",
            "concise" => "You are KHELRUN AI. Answer as briefly as possible: short sentences, no filler. " +
                         "If web results are supplied, use them and mention the source URLs.",
            "writer" => "You are KHELRUN AI by JTS, a friendly writing coach. Improve clarity, tone and structure, and " +
                        "briefly explain the main changes you made.",
            _ => AppSettings.DefaultPrompt
        };
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _s.Theme = LightRadio.IsChecked == true ? "Light" : "Dark";
        _s.FontSize = FontSlider.Value;
        _s.SystemPrompt = string.IsNullOrWhiteSpace(PromptBox.Text) ? AppSettings.DefaultPrompt : PromptBox.Text.Trim();
        _s.Temperature = Math.Round(TempSlider.Value, 1);
        _s.MaxTokens = (int)Math.Round(TokensSlider.Value);
        _s.WebSearch = WebCheck.IsChecked == true;
        _s.WebResults = (int)Math.Round(ResultsSlider.Value);
        _s.ContextSize = (int)Math.Round(ContextSlider.Value);
        _s.Threads = (int)Math.Round(ThreadSlider.Value);
        _s.GpuLayers = (int)Math.Round(GpuLayersSlider.Value);
        _s.BatchSize = (int)Math.Round(BatchSlider.Value);
        _s.AutoLoadLastModel = AutoLoadCheck.IsChecked == true;
        _s.AutoDownloadBestModel = AutoBestCheck.IsChecked == true;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    protected override void OnClosing(CancelEventArgs e)
    {
        if (DialogResult != true) ThemeService.Apply(_originalTheme);
        base.OnClosing(e);
    }
}