using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace KhelrunAI.Services;

/// <summary>Keyless web search via DuckDuckGo's HTML endpoint. Returns a compact text block for the model.</summary>
public sealed class WebSearchService
{
    private static readonly HttpClient Http = CreateClient();

    private static HttpClient CreateClient()
    {
        var c = new HttpClient { Timeout = TimeSpan.FromSeconds(12) };
        c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) KhelrunAI/1.0 (JTS)");
        return c;
    }

    public async Task<string?> SearchAsync(string query, int max = 5, CancellationToken ct = default)
    {
        try
        {
            var url = "https://html.duckduckgo.com/html/?q=" + Uri.EscapeDataString(query);
            var html = await Http.GetStringAsync(url, ct);

            var results = new List<(string Title, string Url, string Snippet)>();
            var blocks = Regex.Split(html, "<div class=\"result results_links");
            foreach (var b in blocks.Skip(1))
            {
                var titleM = Regex.Match(b, "class=\"result__a\"[^>]*>(.*?)</a>", RegexOptions.Singleline);
                var urlM = Regex.Match(b, "class=\"result__url\"[^>]*>(.*?)</a>", RegexOptions.Singleline);
                var snipM = Regex.Match(b, "class=\"result__snippet\"[^>]*>(.*?)</(?:a|td|div)", RegexOptions.Singleline);
                if (!titleM.Success) continue;

                string Clean(string s) => WebUtility.HtmlDecode(Regex.Replace(s, "<[^>]+>", "")).Trim();
                var title = Clean(titleM.Groups[1].Value);
                var link = urlM.Success ? Clean(urlM.Groups[1].Value) : "";
                var snip = snipM.Success ? Clean(snipM.Groups[1].Value) : "";
                if (title.Length == 0) continue;
                results.Add((title, link, snip));
                if (results.Count >= max) break;
            }

            if (results.Count == 0) return null;

            var sb = new StringBuilder();
            sb.AppendLine("Web search results:");
            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                sb.AppendLine($"{i + 1}. {r.Title}");
                if (r.Url.Length > 0) sb.AppendLine("   " + r.Url);
                if (r.Snippet.Length > 0) sb.AppendLine("   " + r.Snippet);
            }
            return sb.ToString();
        }
        catch
        {
            return null;
        }
    }
}
