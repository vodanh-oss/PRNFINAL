using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using SmartStudyAssistant.Core.Models;

namespace SmartStudyAssistant.Core.Services.Ai;

public class OpenAiCompatibleProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private AiProviderConfig _config;

    public AiProviderType ProviderType => _config.ProviderType;
    public AiProviderConfig Config => _config;

    public OpenAiCompatibleProvider(AiProviderConfig config, HttpClient? httpClient = null)
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
        ValidateConfig();

        var url = $"{_config.BaseUrl.TrimEnd('/')}/chat/completions";
        var payload = BuildPayload(history, systemPrompt, stream: false);

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        SetAuthHeaders(request);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, ct);
        var responseString = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Lỗi gọi {_config.DisplayName} ({response.StatusCode}): {responseString}");
        }

        using var doc = JsonDocument.Parse(responseString);
        if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
        {
            var message = choices[0].GetProperty("message");
            if (message.TryGetProperty("content", out var contentElem))
            {
                return contentElem.GetString() ?? string.Empty;
            }
        }

        return string.Empty;
    }

    public async IAsyncEnumerable<string> StreamMessageAsync(IEnumerable<ChatMessage> history, string? systemPrompt = null, [EnumeratorCancellation] CancellationToken ct = default)
    {
        ValidateConfig();

        var url = $"{_config.BaseUrl.TrimEnd('/')}/chat/completions";
        var payload = BuildPayload(history, systemPrompt, stream: true);

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        SetAuthHeaders(request);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Lỗi {_config.DisplayName} Stream ({response.StatusCode}): {err}");
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
                if (json == "[DONE]") break;

                string? chunk = null;
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                    {
                        var delta = choices[0].GetProperty("delta");
                        if (delta.TryGetProperty("content", out var contentElem))
                        {
                            chunk = contentElem.GetString();
                        }
                    }
                }
                catch
                {
                    // Ignore partial json lines
                }

                if (!string.IsNullOrEmpty(chunk))
                {
                    yield return chunk;
                }
            }
        }
    }

    public async Task<float[]?> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        try
        {
            var url = $"{_config.BaseUrl.TrimEnd('/')}/embeddings";
            var model = _config.ProviderType == AiProviderType.OpenAi ? "text-embedding-3-small" : "nomic-embed-text";
            var payload = new { model, input = text };

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            SetAuthHeaders(request);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return null;

            var responseString = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(responseString);
            if (doc.RootElement.TryGetProperty("data", out var data) && data.GetArrayLength() > 0)
            {
                var embeddingElem = data[0].GetProperty("embedding");
                var list = new List<float>();
                foreach (var item in embeddingElem.EnumerateArray())
                {
                    list.Add(item.GetSingle());
                }
                return list.ToArray();
            }
        }
        catch
        {
            // Fallback gracefully
        }

        return null;
    }

    public async Task<(bool Success, string Message)> TestConnectionAsync(CancellationToken ct = default)
    {
        try
        {
            var testHistory = new[] { new ChatMessage { Role = "user", Content = "Ping. Trả lời 'OK' ngắn gọn." } };
            var reply = await SendMessageAsync(testHistory, ct: ct);
            return (true, $"Kết nối {_config.DisplayName} thành công! Phản hồi: {reply.Trim()}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private void ValidateConfig()
    {
        if (_config.ProviderType != AiProviderType.Ollama && string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            throw new InvalidOperationException($"Chưa cấu hình API Key cho {_config.DisplayName}. Vui lòng mở Cài đặt.");
        }
    }

    private void SetAuthHeaders(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);
        }
    }

    private object BuildPayload(IEnumerable<ChatMessage> history, string? systemPrompt, bool stream)
    {
        var messages = new List<object>();

        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            messages.Add(new { role = "system", content = systemPrompt });
        }

        foreach (var msg in history)
        {
            messages.Add(new { role = msg.Role, content = msg.Content });
        }

        var model = string.IsNullOrWhiteSpace(_config.ModelName) ? "gpt-4o-mini" : _config.ModelName;

        return new
        {
            model,
            messages,
            stream,
            temperature = 0.5
        };
    }
}
