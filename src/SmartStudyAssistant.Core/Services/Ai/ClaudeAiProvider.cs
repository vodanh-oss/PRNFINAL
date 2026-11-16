using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using SmartStudyAssistant.Core.Models;

namespace SmartStudyAssistant.Core.Services.Ai;

public class ClaudeAiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private AiProviderConfig _config;

    public AiProviderType ProviderType => AiProviderType.Claude;
    public AiProviderConfig Config => _config;

    public ClaudeAiProvider(AiProviderConfig config, HttpClient? httpClient = null)
    {
        _config = config;
        _httpClient = httpClient ?? new HttpClient();
    }

    public void UpdateConfig(AiProviderConfig config)
    {
        _config = config;
    }

    public async Task<string> SendMessageAsync(IEnumerable<ChatMessage> history, string? systemPrompt = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_config.ApiKey))
            throw new InvalidOperationException("Chưa cấu hình API Key cho Anthropic Claude. Vui lòng mở Cài đặt.");

        var url = $"{_config.BaseUrl.TrimEnd('/')}/messages";
        var payload = BuildPayload(history, systemPrompt, stream: false);

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-api-key", _config.ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, ct);
        var responseString = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Lỗi gọi Claude API ({response.StatusCode}): {responseString}");
        }

        using var doc = JsonDocument.Parse(responseString);
        if (doc.RootElement.TryGetProperty("content", out var contentArray) && contentArray.GetArrayLength() > 0)
        {
            var firstBlock = contentArray[0];
            if (firstBlock.TryGetProperty("text", out var textElem))
            {
                return textElem.GetString() ?? string.Empty;
            }
        }

        return string.Empty;
    }

    public async IAsyncEnumerable<string> StreamMessageAsync(IEnumerable<ChatMessage> history, string? systemPrompt = null, [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_config.ApiKey))
            throw new InvalidOperationException("Chưa cấu hình API Key cho Anthropic Claude.");

        var url = $"{_config.BaseUrl.TrimEnd('/')}/messages";
        var payload = BuildPayload(history, systemPrompt, stream: true);

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-api-key", _config.ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Lỗi Claude Stream ({response.StatusCode}): {err}");
        }

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.StartsWith("data: "))
            {
                var json = line.Substring(6).Trim();
                string? textChunk = null;

                try
                {
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("type", out var typeElem))
                    {
                        var type = typeElem.GetString();
                        if (type == "content_block_delta" && doc.RootElement.TryGetProperty("delta", out var delta))
                        {
                            if (delta.TryGetProperty("text", out var textElem))
                            {
                                textChunk = textElem.GetString();
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore partial json lines
                }

                if (!string.IsNullOrEmpty(textChunk))
                {
                    yield return textChunk;
                }
            }
        }
    }

    public Task<float[]?> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        // Claude does not provide a direct embedding endpoint
        return Task.FromResult<float[]?>(null);
    }

    public async Task<(bool Success, string Message)> TestConnectionAsync(CancellationToken ct = default)
    {
        try
        {
            var testHistory = new[] { new ChatMessage { Role = "user", Content = "Ping. Trả lời 'OK' ngắn gọn." } };
            var reply = await SendMessageAsync(testHistory, ct: ct);
            return (true, $"Kết nối Claude thành công! Phản hồi: {reply.Trim()}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private object BuildPayload(IEnumerable<ChatMessage> history, string? systemPrompt, bool stream)
    {
        var messages = new List<object>();

        foreach (var msg in history)
        {
            if (msg.Role == "system") continue;
            messages.Add(new { role = msg.Role, content = msg.Content });
        }

        if (messages.Count == 0)
        {
            messages.Add(new { role = "user", content = "Xin chào" });
        }

        var model = string.IsNullOrWhiteSpace(_config.ModelName) ? "claude-3-5-sonnet-20241022" : _config.ModelName;

        return new
        {
            model,
            messages,
            system = systemPrompt ?? string.Empty,
            max_tokens = 4096,
            stream
        };
    }
}
