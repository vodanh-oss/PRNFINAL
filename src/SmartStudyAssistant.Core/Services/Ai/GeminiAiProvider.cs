using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using SmartStudyAssistant.Core.Models;

namespace SmartStudyAssistant.Core.Services.Ai;

public class GeminiAiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private AiProviderConfig _config;

    public AiProviderType ProviderType => AiProviderType.Gemini;
    public AiProviderConfig Config => _config;

    public GeminiAiProvider(AiProviderConfig config, HttpClient? httpClient = null)
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
            throw new InvalidOperationException("Chưa cấu hình API Key cho Google Gemini. Vui lòng mở Cài đặt để nhập Key.");

        var model = string.IsNullOrWhiteSpace(_config.ModelName) ? "gemini-1.5-flash" : _config.ModelName;
        var url = $"{_config.BaseUrl.TrimEnd('/')}/v1beta/models/{model}:generateContent?key={_config.ApiKey}";

        var requestBody = BuildGeminiPayload(history, systemPrompt);
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content, ct);
        var responseString = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Lỗi gọi Gemini API ({response.StatusCode}): {responseString}");
        }

        using var doc = JsonDocument.Parse(responseString);
        if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var firstCandidate = candidates[0];
            if (firstCandidate.TryGetProperty("content", out var contentElem) &&
                contentElem.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
            {
                var text = parts[0].GetProperty("text").GetString();
                return text ?? string.Empty;
            }
        }

        return string.Empty;
    }

    public async IAsyncEnumerable<string> StreamMessageAsync(IEnumerable<ChatMessage> history, string? systemPrompt = null, [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_config.ApiKey))
            throw new InvalidOperationException("Chưa cấu hình API Key cho Google Gemini.");

        var model = string.IsNullOrWhiteSpace(_config.ModelName) ? "gemini-1.5-flash" : _config.ModelName;
        var url = $"{_config.BaseUrl.TrimEnd('/')}/v1beta/models/{model}:streamGenerateContent?alt=sse&key={_config.ApiKey}";

        var requestBody = BuildGeminiPayload(history, systemPrompt);
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Lỗi Gemini Stream ({response.StatusCode}): {err}");
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

                string? chunkText = null;
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                    {
                        var parts = candidates[0].GetProperty("content").GetProperty("parts");
                        if (parts.GetArrayLength() > 0)
                        {
                            chunkText = parts[0].GetProperty("text").GetString();
                        }
                    }
                }
                catch
                {
                    // Ignore partial json parse errors
                }

                if (!string.IsNullOrEmpty(chunkText))
                {
                    yield return chunkText;
                }
            }
        }
    }

    public async Task<float[]?> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_config.ApiKey) || string.IsNullOrWhiteSpace(text))
            return null;

        try
        {
            var url = $"{_config.BaseUrl.TrimEnd('/')}/v1beta/models/text-embedding-004:embedContent?key={_config.ApiKey}";
            var payload = new
            {
                model = "models/text-embedding-004",
                content = new
                {
                    parts = new[] { new { text } }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content, ct);

            if (!response.IsSuccessStatusCode) return null;

            var responseString = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(responseString);
            if (doc.RootElement.TryGetProperty("embedding", out var embeddingElem) &&
                embeddingElem.TryGetProperty("values", out var valuesElem))
            {
                var list = new List<float>();
                foreach (var item in valuesElem.EnumerateArray())
                {
                    list.Add(item.GetSingle());
                }
                return list.ToArray();
            }
        }
        catch
        {
            // Fallback gracefully on embedding failure
        }

        return null;
    }

    public async Task<(bool Success, string Message)> TestConnectionAsync(CancellationToken ct = default)
    {
        try
        {
            var testHistory = new[] { new ChatMessage { Role = "user", Content = "Chào Gemini, hãy phản hồi 'OK' ngắn gọn." } };
            var reply = await SendMessageAsync(testHistory, ct: ct);
            return (true, $"Kết nối thành công! Phản hồi: {reply.Trim()}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private static object BuildGeminiPayload(IEnumerable<ChatMessage> history, string? systemPrompt)
    {
        var contents = new List<object>();

        foreach (var msg in history)
        {
            if (msg.Role == "system") continue;
            var geminiRole = msg.Role == "assistant" ? "model" : "user";
            contents.Add(new
            {
                role = geminiRole,
                parts = new[] { new { text = msg.Content } }
            });
        }

        if (contents.Count == 0)
        {
            contents.Add(new { role = "user", parts = new[] { new { text = "Xin chào" } } });
        }

        object? systemInstruction = null;
        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            systemInstruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            };
        }

        return new
        {
            contents,
            systemInstruction,
            generationConfig = new
            {
                temperature = 0.5,
                maxOutputTokens = 4096
            }
        };
    }
}
