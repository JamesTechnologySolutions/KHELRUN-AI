using LLama;
using LLama.Common;
using LLama.Sampling;
using LLama.Transformers;

namespace KhelrunAI.Services;

/// <summary>Loads a GGUF model and streams chat replies, CPU only.</summary>
public sealed class LlmService : IDisposable
{
    private readonly AppSettings _settings;
    private ModelParams? _params;
    private LLamaWeights? _weights;
    private LLamaContext? _context;
    private ChatSession? _session;

    public LlmService(AppSettings settings) => _settings = settings;

    public bool IsLoaded => _session != null;

    public async Task LoadAsync(string ggufPath)
    {
        Dispose();
        await Task.Run(() =>
        {
            var p = new ModelParams(ggufPath)
            {
                ContextSize = (uint)Math.Max(512, _settings.ContextSize),
                GpuLayerCount = 0,
                Threads = _settings.Threads > 0 ? _settings.Threads : Math.Max(1, Environment.ProcessorCount / 2)
            };
            _params = p;
            _weights = LLamaWeights.LoadFromFile(p);
            ResetCore();
        });
    }

    /// <summary>Starts a fresh conversation (also picks up a changed system prompt).</summary>
    public void NewChat()
    {
        if (_params == null || _weights == null) return;
        ResetCore();
    }

    private void ResetCore()
    {
        // A fresh context starts with an empty memory, so no version-specific "clear cache" call is needed.
        _context?.Dispose();
        _context = _weights!.CreateContext(_params!);
        var executor = new InteractiveExecutor(_context);
        var history = new ChatHistory();
        var prompt = string.IsNullOrWhiteSpace(_settings.SystemPrompt) ? AppSettings.DefaultPrompt : _settings.SystemPrompt;
        history.AddMessage(AuthorRole.System, prompt);
        _session = new ChatSession(executor, history);
        _session.WithHistoryTransform(new PromptTemplateTransformer(_weights!, true));
    }

    public async IAsyncEnumerable<string> ChatAsync(
        string userText,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        if (_session == null) throw new InvalidOperationException("Load a model first.");

        // Temperature and max tokens are read per message, so changes apply immediately.
        var inference = new InferenceParams
        {
            MaxTokens = Math.Max(16, _settings.MaxTokens),
            AntiPrompts = new List<string> { "User:" },   // stop if the model starts writing the user's next turn
            SamplingPipeline = new DefaultSamplingPipeline { Temperature = (float)_settings.Temperature }
        };

        await foreach (var token in _session.ChatAsync(
            new ChatHistory.Message(AuthorRole.User, userText), inference, ct))
        {
            yield return token;
        }
    }

    public void Dispose()
    {
        _session = null;
        _context?.Dispose();
        _weights?.Dispose();
        _context = null;
        _weights = null;
        _params = null;
    }
}
