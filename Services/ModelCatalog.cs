using System.IO;
using System.Runtime.InteropServices;

namespace KhelrunAI.Services;

public enum ModelKind { Chat, Image }

public sealed record ModelInfo(
    string Id,
    string Name,
    string Description,
    string FileName,
    string Url,
    long ApproxBytes,
    string Ram,
    string License,
    ModelKind Kind = ModelKind.Chat,
    int MinRamGb = 4);

/// <summary>The built-in model library, plus helpers for finding models already on disk and recommending the best fit.</summary>
public static class ModelCatalog
{
    private static string Hf(string repo, string file) => $"https://huggingface.co/{repo}/resolve/main/{file}";

    public static IReadOnlyList<ModelInfo> All { get; } = new[]
    {
        // ---- Chat models (GGUF) ----
        new ModelInfo(
            "qwen25-1.5b", "Quick  ·  Qwen2.5 1.5B",
            "Fastest and lightest. Ideal for low-RAM machines and quick replies.",
            "Qwen2.5-1.5B-Instruct-Q4_K_M.gguf",
            Hf("bartowski/Qwen2.5-1.5B-Instruct-GGUF", "Qwen2.5-1.5B-Instruct-Q4_K_M.gguf"),
            990_000_000, "4 GB+ RAM", "Apache 2.0", ModelKind.Chat, 4),

        new ModelInfo(
            "phi35-mini", "Efficient  ·  Phi-3.5 Mini",
            "Strong small model from Microsoft. Excellent reasoning for its size.",
            "Phi-3.5-mini-instruct-Q4_K_M.gguf",
            Hf("bartowski/Phi-3.5-mini-instruct-GGUF", "Phi-3.5-mini-instruct-Q4_K_M.gguf"),
            2_300_000_000, "6 GB+ RAM", "MIT", ModelKind.Chat, 6),

        new ModelInfo(
            "llama32-3b", "Balanced  ·  Llama 3.2 3B",
            "Great everyday choice for most PCs: smart, reasonably fast on CPU.",
            "Llama-3.2-3B-Instruct-Q4_K_M.gguf",
            Hf("bartowski/Llama-3.2-3B-Instruct-GGUF", "Llama-3.2-3B-Instruct-Q4_K_M.gguf"),
            2_020_000_000, "8 GB+ RAM", "Llama 3.2 Community", ModelKind.Chat, 8),

        new ModelInfo(
            "qwen25-7b", "Smart  ·  Qwen2.5 7B",
            "The most capable option in the catalog. Best quality; slower on pure CPU.",
            "Qwen2.5-7B-Instruct-Q4_K_M.gguf",
            Hf("bartowski/Qwen2.5-7B-Instruct-GGUF", "Qwen2.5-7B-Instruct-Q4_K_M.gguf"),
            4_680_000_000, "12 GB+ RAM", "Apache 2.0", ModelKind.Chat, 12),

        new ModelInfo(
            "llama33-8b", "Power  ·  Llama 3.3 8B (Q4)",
            "High-end chat model. Excellent instruction following. Needs solid RAM.",
            "Llama-3.3-8B-Instruct-Q4_K_M.gguf",
            Hf("bartowski/Llama-3.3-8B-Instruct-GGUF", "Llama-3.3-8B-Instruct-Q4_K_M.gguf"),
            4_920_000_000, "14 GB+ RAM", "Llama 3.3 Community", ModelKind.Chat, 14),

        // ---- Image models (import-focused; generation backend is pluggable) ----
        new ModelInfo(
            "sdxl-turbo", "Image  ·  SDXL Turbo (placeholder)",
            "Import a Stable Diffusion / SDXL / Flux .safetensors or .gguf image model. " +
            "Full local generation requires a compatible backend; KHELRUN AI stores and tracks them for you.",
            "sdxl_turbo.safetensors",
            "", // no auto URL; user imports
            0, "8 GB+ VRAM recommended", "OpenRAIL / model-specific", ModelKind.Image, 8),
    };

    public static IEnumerable<ModelInfo> ChatModels => All.Where(m => m.Kind == ModelKind.Chat);
    public static IEnumerable<ModelInfo> ImageModels => All.Where(m => m.Kind == ModelKind.Image);

    /// <summary>Models the user downloaded or imported.</summary>
    public static string UserDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KhelrunAI", "Models");

    public static string UserImageDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KhelrunAI", "ImageModels");

    /// <summary>Models shipped inside the installer (a "Models" folder beside the exe).</summary>
    public static string BundledDir => Path.Combine(AppContext.BaseDirectory, "Models");

    public static string? FindInstalled(ModelInfo m)
    {
        var dirs = m.Kind == ModelKind.Image
            ? new[] { UserImageDir }
            : new[] { BundledDir, UserDir };

        foreach (var dir in dirs)
        {
            var p = Path.Combine(dir, m.FileName);
            if (File.Exists(p)) return p;
        }
        return null;
    }

    public static bool IsBundled(string path) =>
        path.StartsWith(BundledDir, StringComparison.OrdinalIgnoreCase);

    /// <summary>First chat model available offline.</summary>
    public static string? FirstInstalled()
    {
        foreach (var m in ChatModels)
        {
            var p = FindInstalled(m);
            if (p != null) return p;
        }
        try
        {
            if (Directory.Exists(BundledDir))
                return Directory.GetFiles(BundledDir, "*.gguf").OrderBy(f => f).FirstOrDefault();
            if (Directory.Exists(UserDir))
                return Directory.GetFiles(UserDir, "*.gguf").OrderBy(f => f).FirstOrDefault();
        }
        catch { /* unreadable folder */ }
        return null;
    }

    /// <summary>Recommend the best chat model for the current machine based on available RAM.</summary>
    public static ModelInfo RecommendBestChatModel()
    {
        long ramGb = GetTotalRamGb();
        // Prefer the largest model the machine can reasonably run (leave headroom for OS + app).
        // Prefer fully-public licenses (Apache / MIT) first to avoid gated-repo 401s.
        var candidates = ChatModels
            .Where(m => m.MinRamGb <= Math.Max(4, ramGb - 2))
            .OrderByDescending(m => m.License.Contains("Apache", StringComparison.OrdinalIgnoreCase)
                                 || m.License.Contains("MIT", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
            .ThenByDescending(m => m.MinRamGb)
            .ThenByDescending(m => m.ApproxBytes)
            .ToList();

        return candidates.FirstOrDefault() ?? ChatModels.First();
    }

    public static long GetTotalRamGb()
    {
        try
        {
            // Windows: use GlobalMemoryStatusEx via P/Invoke for accurate physical RAM
            var status = new MEMORYSTATUSEX();
            status.dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();
            if (GlobalMemoryStatusEx(ref status))
            {
                return (long)(status.ullTotalPhys / (1024.0 * 1024 * 1024));
            }
        }
        catch { /* fall through */ }

        // Fallback: GC.GetGCMemoryInfo is not total physical; use a conservative 8 GB assumption
        return 8;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    public static string FormatBytes(long bytes) =>
        bytes >= 1_000_000_000 ? $"{bytes / 1e9:F1} GB" : bytes >= 1_000_000 ? $"{bytes / 1e6:F0} MB" : $"{bytes} B";
}
