namespace SmartStudyAssistant.Core.Models;

public enum AiProviderType
{
    Gemini,
    OpenAi,
    Claude,
    Groq,
    Ollama
}

public class AiProviderConfig
{
    public AiProviderType ProviderType { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public bool IsDefault { get; set; }
    public bool SupportsEmbedding { get; set; }

    public static AiProviderConfig CreateDefault(AiProviderType type)
    {
        return type switch
        {
            AiProviderType.Gemini => new AiProviderConfig
            {
                ProviderType = AiProviderType.Gemini,
                DisplayName = "Google Gemini",
                ModelName = "gemini-1.5-flash",
                BaseUrl = "https://generativelanguage.googleapis.com",
                SupportsEmbedding = true,
                IsDefault = true
            },
            AiProviderType.OpenAi => new AiProviderConfig
            {
                ProviderType = AiProviderType.OpenAi,
                DisplayName = "OpenAI",
                ModelName = "gpt-4o-mini",
                BaseUrl = "https://api.openai.com/v1",
                SupportsEmbedding = true
            },
            AiProviderType.Claude => new AiProviderConfig
            {
                ProviderType = AiProviderType.Claude,
                DisplayName = "Anthropic Claude",
                ModelName = "claude-3-5-sonnet-20241022",
                BaseUrl = "https://api.anthropic.com/v1",
                SupportsEmbedding = false
            },
            AiProviderType.Groq => new AiProviderConfig
            {
                ProviderType = AiProviderType.Groq,
                DisplayName = "Groq (Llama 3.3)",
                ModelName = "llama-3.3-70b-versatile",
                BaseUrl = "https://api.groq.com/openai/v1",
                SupportsEmbedding = false
            },
            AiProviderType.Ollama => new AiProviderConfig
            {
                ProviderType = AiProviderType.Ollama,
                DisplayName = "Ollama (Cục bộ)",
                ModelName = "deepseek-r1:8b",
                BaseUrl = "http://localhost:11434/v1",
                SupportsEmbedding = true
            },
            _ => new AiProviderConfig { ProviderType = type, DisplayName = type.ToString() }
        };
    }
}
