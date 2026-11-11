using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace KhelrunAI.Services;

/// <summary>Downloads a catalog model into the user's models folder with progress and cancellation.</summary>
public static class ModelDownloader
{
    private static readonly HttpClient Http = CreateClient();

    private static HttpClient CreateClient()
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10,
            AutomaticDecompression = DecompressionMethods.All
        };

        var c = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };

        // Browser-like headers greatly reduce 401 / bot-blocking from Hugging Face
        c.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
        c.DefaultRequestHeaders.Accept.ParseAdd("*/*");
        c.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
        c.DefaultRequestHeaders.Add("Accept-Encoding", "identity"); // avoid compressed body for large binary files

        return c;
    }

    /// <summary>
    /// Optional Hugging Face token. Set via environment variable HF_TOKEN or
    /// AppSettings (if you later expose it in the UI). Public models usually work without it.
    /// </summary>
    public static string? HfToken { get; set; } =
        Environment.GetEnvironmentVariable("HF_TOKEN")
        ?? Environment.GetEnvironmentVariable("HUGGING_FACE_HUB_TOKEN");

    public static async Task DownloadAsync(ModelInfo model, IProgress<(long Done, long Total)> progress, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(model.Url))
            throw new InvalidOperationException("This model has no download URL. Please import a local file instead.");

        var dir = model.Kind == ModelKind.Image ? ModelCatalog.UserImageDir : ModelCatalog.UserDir;
        Directory.CreateDirectory(dir);
        var dest = Path.Combine(dir, model.FileName);
        var temp = dest + ".part";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, model.Url);

            // Prefer the raw binary download path
            request.Headers.TryAddWithoutValidation("Accept", "application/octet-stream");

            if (!string.IsNullOrWhiteSpace(HfToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", HfToken.Trim());
            }

            using var resp = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            if (resp.StatusCode == HttpStatusCode.Unauthorized || resp.StatusCode == HttpStatusCode.Forbidden)
            {
                var msg =
                    $"Hugging Face returned {(int)resp.StatusCode} {resp.StatusCode}.\n\n" +
                    "This usually means:\n" +
                    "• The model is gated (needs a free Hugging Face account + token), or\n" +
                    "• Temporary rate-limiting / bot protection.\n\n" +
                    "Fixes:\n" +
                    "1. Open the Models panel and try a different model (Qwen models are public).\n" +
                    "2. Or create a free read token at https://huggingface.co/settings/tokens\n" +
                    "   then set the environment variable HF_TOKEN=hf_yourtoken before launching the app.\n" +
                    "3. Or simply download the .gguf file in your browser and use “Import local file…”.";
                throw new HttpRequestException(msg);
            }

            resp.EnsureSuccessStatusCode();

            long? expected = resp.Content.Headers.ContentLength;
            long total = expected ?? model.ApproxBytes;
            long written = 0;

            await using (var src = await resp.Content.ReadAsStreamAsync(ct))
            await using (var dst = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                var buffer = new byte[81920];
                var clock = Stopwatch.StartNew();
                long lastReport = 0;
                int n;
                while ((n = await src.ReadAsync(buffer, ct)) > 0)
                {
                    await dst.WriteAsync(buffer.AsMemory(0, n), ct);
                    written += n;
                    if (clock.ElapsedMilliseconds - lastReport >= 150)
                    {
                        lastReport = clock.ElapsedMilliseconds;
                        progress.Report((written, total));
                    }
                }
            }

            if (expected is long exp && written != exp)
                throw new IOException("The download was incomplete. Please try again.");

            progress.Report((written, total));
            File.Move(temp, dest, overwrite: true);
        }
        catch
        {
            try { if (File.Exists(temp)) File.Delete(temp); } catch { /* best effort */ }
            throw;
        }
    }
}
