using System.Windows;
using KhelrunAI.Services;
using LLama.Native;

namespace KhelrunAI;

public partial class App : Application
{
    public static AppSettings Settings { get; } = AppSettings.Load();

    public App()
    {
        // Configure native backend selection BEFORE any model is loaded.
        // Prefer modern CUDA → older CUDA → Vulkan (broad GPU support) → CPU fallback.
        // This makes the app work on virtually any Windows x64 machine with or without a GPU.
        try
        {
            NativeLibraryConfig.All
                .WithCuda(true)          // try CUDA backends if present
                .WithVulkan(true)        // try Vulkan (AMD / Intel / NVIDIA without CUDA toolkit)
                .WithAvx(AvxLevel.Avx2)  // good balance of speed + compatibility for modern CPUs
                .WithAutoFallback(true); // if preferred backend fails, fall back cleanly
        }
        catch
        {
            // Older LLamaSharp or edge cases — ignore and use defaults
        }

        DispatcherUnhandledException += (_, e) =>
        {
            MessageBox.Show(e.Exception.Message, "KHELRUN AI error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        ThemeService.Apply(Settings.Theme);
        base.OnStartup(e);
    }
}