using SmartStudyAssistant.Core.Models;
using SmartStudyAssistant.Core.Services.Security;

namespace SmartStudyAssistant.Core.Services.Ai;

public class AiServiceCoordinator
{
    private readonly ISecureStorage _secureStorage;
    private readonly Dictionary<AiProviderType, IAiProvider> _providers = new();
    private AiProviderType _currentProviderType = AiProviderType.Gemini;

    public event Action<AiProviderType>? ProviderChanged;

    public AiProviderType CurrentProviderType
    {
        get => _currentProviderType;
        set
        {
            if (_currentProviderType != value)
            {
                _currentProviderType = value;
                ProviderChanged?.Invoke(_currentProviderType);
            }
        }
    }

    public AiServiceCoordinator(ISecureStorage secureStorage, HttpClient? httpClient = null)
    {
        _secureStorage = secureStorage;

        // Initialize default configs
        foreach (AiProviderType type in Enum.GetValues<AiProviderType>())
        {
            var config = AiProviderConfig.CreateDefault(type);
            var savedKey = _secureStorage.GetSecret($"ApiKey_{type}");
            if (!string.IsNullOrEmpty(savedKey))
            {
                config.ApiKey = savedKey;
            }

            var savedModel = _secureStorage.GetSecret($"Model_{type}");
            if (!string.IsNullOrEmpty(savedModel))
            {
                config.ModelName = savedModel;
            }

            IAiProvider provider = type switch
            {
                AiProviderType.Gemini => new GeminiAiProvider(config, httpClient),
                AiProviderType.Claude => new ClaudeAiProvider(config, httpClient),
                _ => new OpenAiCompatibleProvider(config, httpClient)
            };

            _providers[type] = provider;
        }

        var savedDefault = _secureStorage.GetSecret("DefaultProvider");
        if (Enum.TryParse<AiProviderType>(savedDefault, out var def))
        {
            _currentProviderType = def;
        }
    }

    public IAiProvider GetCurrentProvider()
    {
        return _providers[_currentProviderType];
    }

    public IAiProvider GetProvider(AiProviderType type)
    {
        return _providers[type];
    }

    public IEnumerable<IAiProvider> GetAllProviders() => _providers.Values;

    public void UpdateProviderConfig(AiProviderConfig config)
    {
        if (_providers.TryGetValue(config.ProviderType, out var provider))
        {
            provider.UpdateConfig(config);

            // Persist sensitive details securely
            if (!string.IsNullOrEmpty(config.ApiKey))
            {
                _secureStorage.SaveSecret($"ApiKey_{config.ProviderType}", config.ApiKey);
            }
            if (!string.IsNullOrEmpty(config.ModelName))
            {
                _secureStorage.SaveSecret($"Model_{config.ProviderType}", config.ModelName);
            }
            if (config.IsDefault)
            {
                _secureStorage.SaveSecret("DefaultProvider", config.ProviderType.ToString());
                CurrentProviderType = config.ProviderType;
            }
        }
    }

    public async Task<string> SendMessageAsync(IEnumerable<ChatMessage> history, string? systemPrompt = null, CancellationToken ct = default)
    {
        return await GetCurrentProvider().SendMessageAsync(history, systemPrompt, ct);
    }

    public IAsyncEnumerable<string> StreamMessageAsync(IEnumerable<ChatMessage> history, string? systemPrompt = null, CancellationToken ct = default)
    {
        return GetCurrentProvider().StreamMessageAsync(history, systemPrompt, ct);
    }

    public bool HasActiveEmbeddingProvider()
    {
        // 1. If Ollama is the selected provider, it may run locally
        if (_currentProviderType == AiProviderType.Ollama)
        {
            return true;
        }

        // 2. Current provider supports embedding and has an API key
        var current = GetCurrentProvider();
        if (current.Config.SupportsEmbedding && !string.IsNullOrWhiteSpace(current.Config.ApiKey))
        {
            return true;
        }

        // 3. Gemini has API key
        if (_providers.TryGetValue(AiProviderType.Gemini, out var gemini) &&
            !string.IsNullOrWhiteSpace(gemini.Config.ApiKey))
        {
            return true;
        }

        // 4. OpenAI has API key
        if (_providers.TryGetValue(AiProviderType.OpenAi, out var openAi) &&
            !string.IsNullOrWhiteSpace(openAi.Config.ApiKey))
        {
            return true;
        }

        return false;
    }

    public async Task<float[]?> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        // 1. Try current provider if it supports embedding and has an API key
        var current = GetCurrentProvider();
        if (current.Config.SupportsEmbedding && !string.IsNullOrWhiteSpace(current.Config.ApiKey))
        {
            try
            {
                var emb = await current.GenerateEmbeddingAsync(text, ct);
                if (emb != null) return emb;
            }
            catch
            {
                // Fall through to next available provider
            }
        }

        // 2. Try Gemini if configured with an API key
        if (_providers.TryGetValue(AiProviderType.Gemini, out var gemini) &&
            !string.IsNullOrWhiteSpace(gemini.Config.ApiKey))
        {
            try
            {
                var emb = await gemini.GenerateEmbeddingAsync(text, ct);
                if (emb != null) return emb;
            }
            catch
            {
                // Fall through
            }
        }

        // 3. Try OpenAI if configured with an API key
        if (_providers.TryGetValue(AiProviderType.OpenAi, out var openAi) &&
            !string.IsNullOrWhiteSpace(openAi.Config.ApiKey))
        {
            try
            {
                var emb = await openAi.GenerateEmbeddingAsync(text, ct);
                if (emb != null) return emb;
            }
            catch
            {
                // Fall through
            }
        }

        // 4. ONLY probe Ollama if it is the explicitly active provider to avoid TCP connection timeouts
        if (_currentProviderType == AiProviderType.Ollama && _providers.TryGetValue(AiProviderType.Ollama, out var ollama))
        {
            try
            {
                var emb = await ollama.GenerateEmbeddingAsync(text, ct);
                if (emb != null) return emb;
            }
            catch
            {
                // Local server unreachable
            }
        }

        return null;
    }
}
