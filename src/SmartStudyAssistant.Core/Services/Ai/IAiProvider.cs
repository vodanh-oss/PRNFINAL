using SmartStudyAssistant.Core.Models;

namespace SmartStudyAssistant.Core.Services.Ai;

public interface IAiProvider
{
    AiProviderType ProviderType { get; }
    AiProviderConfig Config { get; }
    void UpdateConfig(AiProviderConfig config);

    Task<string> SendMessageAsync(IEnumerable<ChatMessage> history, string? systemPrompt = null, CancellationToken ct = default);
    IAsyncEnumerable<string> StreamMessageAsync(IEnumerable<ChatMessage> history, string? systemPrompt = null, CancellationToken ct = default);
    Task<float[]?> GenerateEmbeddingAsync(string text, CancellationToken ct = default);
    Task<(bool Success, string Message)> TestConnectionAsync(CancellationToken ct = default);
}
