using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using KhelrunAI.Services;
using Microsoft.Win32;

namespace KhelrunAI;

public partial class ModelsWindow : Window
{
    private CancellationTokenSource? _cts;
    private ModelKind _kind = ModelKind.Chat;

    /// <summary>The model file the user chose; set when the dialog closes with DialogResult = true.</summary>
    public string? SelectedPath { get; private set; }

    public ModelsWindow(ModelKind initialKind = ModelKind.Chat)
    {
        InitializeComponent();
        _kind = initialKind;
        UpdateTabStyles();
        BuildList();
    }

    private void TabChat_Click(object sender, RoutedEventArgs e)
    {
        _kind = ModelKind.Chat;
        UpdateTabStyles();
        BuildList();
    }

    private void TabImage_Click(object sender, RoutedEventArgs e)
    {
        _kind = ModelKind.Image;
        UpdateTabStyles();
        BuildList();
    }

    private void UpdateTabStyles()
    {
        // Visual indication of active tab (primary vs ghost)
        if (_kind == ModelKind.Chat)
        {
            TabChat.Style = null; // default primary
            TabImage.Style = (Style)FindResource("GhostButton");
        }
        else
        {
            TabImage.Style = null;
            TabChat.Style = (Style)FindResource("GhostButton");
        }
    }

    private void BuildList()
    {
        ListPanel.Children.Clear();

        if (_kind == ModelKind.Image)
        {
            var tip = new TextBlock
            {
                Text = "Import Stable Diffusion, SDXL, Flux or other image models (.safetensors, .ckpt, .gguf). " +
                      "KHELRUN AI tracks them for you. Full local generation can be connected via a compatible backend.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 16),
                Foreground = (System.Windows.Media.Brush)FindResource("Muted")
            };
            ListPanel.Children.Add(tip);

            // Show any already imported image models
            try
            {
                if (Directory.Exists(ModelCatalog.UserImageDir))
                {
                    foreach (var f in Directory.GetFiles(ModelCatalog.UserImageDir)
                                 .Where(x => x.EndsWith(".safetensors", StringComparison.OrdinalIgnoreCase)
                                          || x.EndsWith(".ckpt", StringComparison.OrdinalIgnoreCase)
                                          || x.EndsWith(".gguf", StringComparison.OrdinalIgnoreCase)
                                          || x.EndsWith(".pt", StringComparison.OrdinalIgnoreCase)))
                    {
                        ListPanel.Children.Add(BuildImportedCard(f));
                    }
                }
            }
            catch { /* ignore */ }
        }

        var models = _kind == ModelKind.Chat ? ModelCatalog.ChatModels : ModelCatalog.ImageModels;
        foreach (var m in models)
            ListPanel.Children.Add(BuildCard(m));

        // Hardware recommendation banner for chat
        if (_kind == ModelKind.Chat)
        {
            var best = ModelCatalog.RecommendBestChatModel();
            var ram = ModelCatalog.GetTotalRamGb();
            var banner = new Border
            {
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 8, 0, 0),
                Background = (System.Windows.Media.Brush)FindResource("PanelElevated"),
                BorderBrush = (System.Windows.Media.Brush)FindResource("AccentDim"),
                BorderThickness = new Thickness(1)
            };
            var sp = new StackPanel();
            sp.Children.Add(new TextBlock
            {
                Text = $"Recommended for your PC (~{ram} GB RAM)",
                FontWeight = FontWeights.SemiBold,
                FontSize = 13
            });
            sp.Children.Add(new TextBlock
            {
                Text = best.Name + "  —  " + best.Description,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 0),
                Foreground = (System.Windows.Media.Brush)FindResource("Muted")
            });
            banner.Child = sp;
            ListPanel.Children.Insert(0, banner);
        }
    }

    private UIElement BuildImportedCard(string path)
    {
        var name = Path.GetFileName(path);
        var title = new TextBlock { Text = name, FontSize = 15, FontWeight = FontWeights.SemiBold };
        var info = new TextBlock
        {
            Text = "Imported image model  ·  " + ModelCatalog.FormatBytes(new FileInfo(path).Length),
            FontSize = 12,
            Margin = new Thickness(0, 4, 0, 0)
        };
        info.SetResourceReference(TextBlock.ForegroundProperty, "Muted");

        var use = new Button { Content = "Select", MinWidth = 96 };
        use.Click += (_, _) => { SelectedPath = path; DialogResult = true; };

        var del = new Button { Content = "Delete", MinWidth = 96, Margin = new Thickness(0, 6, 0, 0) };
        del.SetResourceReference(Button.BackgroundProperty, "Muted");
        del.Click += (_, _) =>
        {
            try { File.Delete(path); BuildList(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "KHELRUN AI"); }
        };

        var right = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(16, 0, 0, 0) };
        right.Children.Add(use);
        right.Children.Add(del);

        var left = new StackPanel();
        left.Children.Add(title);
        left.Children.Add(info);

        var top = new Grid();
        top.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        top.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(right, 1);
        top.Children.Add(left);
        top.Children.Add(right);

        var card = new Border
        {
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 0, 0, 12),
            Child = top
        };
        card.SetResourceReference(Border.BackgroundProperty, "Panel");
        return card;
    }

    private UIElement BuildCard(ModelInfo m)
    {
        var installed = ModelCatalog.FindInstalled(m);

        var title = new TextBlock { Text = m.Name, FontSize = 16, FontWeight = FontWeights.SemiBold };
        var desc = new TextBlock { Text = m.Description, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 0) };
        var info = new TextBlock
        {
            Text = m.ApproxBytes > 0
                ? $"{ModelCatalog.FormatBytes(m.ApproxBytes)}  ·  {m.Ram}  ·  {m.License}"
                : $"{m.Ram}  ·  {m.License}",
            FontSize = 12,
            Margin = new Thickness(0, 6, 0, 0)
        };
        info.SetResourceReference(TextBlock.ForegroundProperty, "Muted");

        var left = new StackPanel();
        left.Children.Add(title);
        left.Children.Add(desc);
        left.Children.Add(info);

        var bar = new ProgressBar
        {
            Height = 6, Minimum = 0, Maximum = 100,
            Margin = new Thickness(0, 10, 0, 0),
            Visibility = Visibility.Collapsed
        };
        bar.SetResourceReference(ProgressBar.ForegroundProperty, "Accent");

        var status = new TextBlock { FontSize = 12, Margin = new Thickness(0, 4, 0, 0), Visibility = Visibility.Collapsed };
        status.SetResourceReference(TextBlock.ForegroundProperty, "Muted");

        var right = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(16, 0, 0, 0) };
        if (installed != null)
        {
            var use = new Button { Content = "Use", MinWidth = 96 };
            use.Click += (_, _) => { SelectedPath = installed; DialogResult = true; };
            right.Children.Add(use);

            if (!ModelCatalog.IsBundled(installed))
            {
                var del = new Button { Content = "Delete", MinWidth = 96, Margin = new Thickness(0, 6, 0, 0) };
                del.SetResourceReference(Button.BackgroundProperty, "Muted");
                del.Click += (_, _) => DeleteModel(installed);
                right.Children.Add(del);
            }
        }
        else if (!string.IsNullOrEmpty(m.Url))
        {
            var download = new Button { Content = "Download", MinWidth = 96 };
            download.Click += async (_, _) => await DownloadClickAsync(m, download, bar, status);
            right.Children.Add(download);
        }
        else
        {
            var imp = new Button { Content = "Import…", MinWidth = 96 };
            imp.Click += (_, _) => Browse_Click(null!, null!);
            right.Children.Add(imp);
        }

        var top = new Grid();
        top.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        top.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(right, 1);
        top.Children.Add(left);
        top.Children.Add(right);

        var content = new StackPanel();
        content.Children.Add(top);
        content.Children.Add(bar);
        content.Children.Add(status);

        var card = new Border
        {
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 0, 0, 12),
            Child = content
        };
        card.SetResourceReference(Border.BackgroundProperty, "Panel");
        return card;
    }

    private async Task DownloadClickAsync(ModelInfo m, Button btn, ProgressBar bar, TextBlock status)
    {
        if (_cts != null)
        {
            if (ReferenceEquals(btn.Tag, _cts)) _cts.Cancel();
            else MessageBox.Show("Please wait for the current download to finish.", "KHELRUN AI");
            return;
        }

        var cts = _cts = new CancellationTokenSource();
        btn.Tag = cts;
        btn.Content = "Cancel";
        bar.Visibility = Visibility.Visible;
        status.Visibility = Visibility.Visible;
        status.Text = "Starting…";

        var progress = new Progress<(long Done, long Total)>(p =>
        {
            bar.Value = p.Total > 0 ? 100.0 * p.Done / p.Total : 0;
            status.Text = $"{ModelCatalog.FormatBytes(p.Done)} of {ModelCatalog.FormatBytes(p.Total)}";
        });

        try
        {
            await ModelDownloader.DownloadAsync(m, progress, cts.Token);
            BuildList();
        }
        catch (OperationCanceledException)
        {
            ResetCard(btn, bar, status);
        }
        catch (Exception ex)
        {
            ResetCard(btn, bar, status);
            MessageBox.Show("The download failed:\n" + ex.Message + "\n\nCheck your internet connection and try again.",
                "KHELRUN AI", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _cts = null;
            cts.Dispose();
        }
    }

    private static void ResetCard(Button btn, ProgressBar bar, TextBlock status)
    {
        btn.Tag = null;
        btn.Content = "Download";
        bar.Visibility = Visibility.Collapsed;
        status.Visibility = Visibility.Collapsed;
    }

    private void DeleteModel(string path)
    {
        var ans = MessageBox.Show($"Delete {Path.GetFileName(path)} from this PC?\nYou can download it again later.",
            "KHELRUN AI", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (ans != MessageBoxResult.Yes) return;
        try
        {
            File.Delete(path);
            BuildList();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not delete the file (is it the model currently loaded?):\n" + ex.Message, "KHELRUN AI");
        }
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var filter = _kind == ModelKind.Chat
            ? "GGUF chat models (*.gguf)|*.gguf|All files (*.*)|*.*"
            : "Image models (*.safetensors;*.ckpt;*.gguf;*.pt)|*.safetensors;*.ckpt;*.gguf;*.pt|All files (*.*)|*.*";

        var dlg = new OpenFileDialog
        {
            Filter = filter,
            Title = _kind == ModelKind.Chat ? "Import a chat model (.gguf)" : "Import an image model"
        };
        if (dlg.ShowDialog() != true) return;

        // For image models, copy into the tracked folder so the library can list them
        if (_kind == ModelKind.Image)
        {
            try
            {
                Directory.CreateDirectory(ModelCatalog.UserImageDir);
                var dest = Path.Combine(ModelCatalog.UserImageDir, Path.GetFileName(dlg.FileName));
                if (!string.Equals(dlg.FileName, dest, StringComparison.OrdinalIgnoreCase))
                    File.Copy(dlg.FileName, dest, overwrite: true);
                SelectedPath = dest;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not import the file:\n" + ex.Message, "KHELRUN AI");
                return;
            }
        }
        else
        {
            SelectedPath = dlg.FileName;
        }

        DialogResult = true;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    protected override void OnClosing(CancelEventArgs e)
    {
        _cts?.Cancel();
        base.OnClosing(e);
    }
}
