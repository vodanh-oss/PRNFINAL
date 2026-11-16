using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartStudyAssistant.Core.Models;
using SmartStudyAssistant.Core.Services.Ai;
using SmartStudyAssistant.Core.Services.Database;
using SmartStudyAssistant.Core.Services.Rag;

namespace SmartStudyAssistant.Wpf.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;
    private readonly AiServiceCoordinator _aiCoordinator;
    private readonly IRagEngine _ragEngine;

    [ObservableProperty]
    private ObservableCollection<ChatSession> _sessions = new();

    [ObservableProperty]
    private ChatSession? _currentSession;

    [ObservableProperty]
    private ObservableCollection<ChatMessage> _messages = new();

    [ObservableProperty]
    private string _userInput = string.Empty;

    [ObservableProperty]
    private bool _isGenerating;

    [ObservableProperty]
    private bool _isRagEnabled = true;

    [ObservableProperty]
    private CitationItem? _inspectedCitation;

    [ObservableProperty]
    private bool _isCitationModalOpen;

    [ObservableProperty]
    private string _currentProviderName = string.Empty;

    public ChatViewModel(IDatabaseService databaseService, AiServiceCoordinator aiCoordinator, IRagEngine ragEngine)
    {
        _databaseService = databaseService;
        _aiCoordinator = aiCoordinator;
        _ragEngine = ragEngine;

        _aiCoordinator.ProviderChanged += (type) =>
        {
            CurrentProviderName = _aiCoordinator.GetCurrentProvider().Config.DisplayName;
        };

        CurrentProviderName = _aiCoordinator.GetCurrentProvider().Config.DisplayName;
    }

    public async Task InitializeAsync()
    {
        await LoadSessionsAsync();
    }

    [RelayCommand]
    public async Task LoadSessionsAsync()
    {
        var list = await _databaseService.GetSessionsAsync();
        Sessions = new ObservableCollection<ChatSession>(list);

        if (Sessions.Count == 0)
        {
            await CreateNewSessionAsync();
        }
        else
        {
            CurrentSession = Sessions[0];
        }
    }

    [RelayCommand]
    public async Task CreateNewSessionAsync()
    {
        var session = new ChatSession
        {
            Title = $"Ôn tập #{Sessions.Count + 1}",
            PreferredProvider = _aiCoordinator.CurrentProviderType,
            IsRagEnabled = IsRagEnabled
        };

        await _databaseService.SaveSessionAsync(session);
        Sessions.Insert(0, session);
        CurrentSession = session;
    }

    partial void OnCurrentSessionChanged(ChatSession? value)
    {
        if (value != null)
        {
            IsRagEnabled = value.IsRagEnabled;
            _ = LoadMessagesForSessionAsync(value.Id);
        }
        else
        {
            Messages.Clear();
        }
    }

    private async Task LoadMessagesForSessionAsync(string sessionId)
    {
        var msgs = await _databaseService.GetMessagesAsync(sessionId);
        Messages = new ObservableCollection<ChatMessage>(msgs);
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(UserInput) || IsGenerating || CurrentSession == null)
            return;

        var text = UserInput.Trim();
        UserInput = string.Empty;

        var userMsg = new ChatMessage
        {
            SessionId = CurrentSession.Id,
            Role = "user",
            Content = text,
            Timestamp = DateTime.UtcNow
        };

        Messages.Add(userMsg);
        await _databaseService.SaveMessageAsync(userMsg);

        // Update session title if first message
        if (Messages.Count == 1)
        {
            CurrentSession.Title = text.Length > 30 ? text.Substring(0, 30) + "..." : text;
            CurrentSession.LastModifiedAt = DateTime.UtcNow;
            await _databaseService.SaveSessionAsync(CurrentSession);
        }

        IsGenerating = true;

        var assistantMsg = new ChatMessage
        {
            SessionId = CurrentSession.Id,
            Role = "assistant",
            Content = string.Empty,
            Timestamp = DateTime.UtcNow,
            ProviderUsed = _aiCoordinator.GetCurrentProvider().Config.DisplayName,
            ModelUsed = _aiCoordinator.GetCurrentProvider().Config.ModelName
        };
        Messages.Add(assistantMsg);

        try
        {
            string? systemPrompt = null;
            List<CitationItem> retrievedCitations = new();

            // Perform RAG if enabled
            if (IsRagEnabled)
            {
                var candidateChunks = await _databaseService.GetAllSelectedChunksAsync();
                if (candidateChunks.Count > 0)
                {
                    // Generate embedding for query if available
                    var queryEmbedding = await _aiCoordinator.GenerateEmbeddingAsync(text);
                    retrievedCitations = _ragEngine.SearchSimilarChunks(text, candidateChunks, topK: 4, queryEmbedding: queryEmbedding);

                    if (retrievedCitations.Count > 0)
                    {
                        systemPrompt = _ragEngine.BuildRagSystemPrompt(retrievedCitations);
                        assistantMsg.Citations = retrievedCitations;
                    }
                }
            }

            var fullResponse = new StringBuilder();

            await foreach (var chunk in _aiCoordinator.StreamMessageAsync(Messages.Take(Messages.Count - 1), systemPrompt))
            {
                fullResponse.Append(chunk);
                assistantMsg.Content = fullResponse.ToString();
            }

            if (string.IsNullOrEmpty(assistantMsg.Content))
            {
                // Fallback to non-stream if stream returned empty
                assistantMsg.Content = await _aiCoordinator.SendMessageAsync(Messages.Take(Messages.Count - 1), systemPrompt);
            }

            await _databaseService.SaveMessageAsync(assistantMsg);
        }
        catch (Exception ex)
        {
            assistantMsg.Content = $"⚠️ Lỗi phản hồi: {ex.Message}\n\n*Gợi ý: Kiểm tra lại API Key trong mục Cài đặt hoặc thử chuyển sang nhà cung cấp AI khác.*";
            await _databaseService.SaveMessageAsync(assistantMsg);
        }
        finally
        {
            IsGenerating = false;
        }
    }

    [RelayCommand]
    private void UsePromptTemplate(string templateType)
    {
        UserInput = templateType switch
        {
            "summary" => "Tóm tắt các ý chính và sơ đồ kiến thức của tài liệu này một cách cô đọng, dễ nhớ nhất.",
            "explain" => "Giải thích chi tiết khái niệm này kèm ví dụ thực tế trực quan để tôi dễ dàng ôn thi:",
            "translate" => "Dịch đoạn văn bản học thuật sau sang tiếng Việt chuẩn chuyên ngành và giải thích các thuật ngữ khó:",
            "quiz" => "Dựa vào nội dung tài liệu, hãy tạo 5 câu hỏi trắc nghiệm có đáp án và giải thích chi tiết để tôi tự kiểm tra kiến thức.",
            "flashcards" => "Hãy trích xuất 5 thuật ngữ/định nghĩa quan trọng nhất từ nội dung này dưới định dạng Flashcard (Thuật ngữ - Định nghĩa).",
            _ => UserInput
        };
    }

    [RelayCommand]
    private void InspectCitation(CitationItem citation)
    {
        InspectedCitation = citation;
        IsCitationModalOpen = true;
    }

    [RelayCommand]
    private void CloseCitationModal()
    {
        IsCitationModalOpen = false;
        InspectedCitation = null;
    }

    [RelayCommand]
    private async Task DeleteSessionAsync(ChatSession session)
    {
        await _databaseService.DeleteSessionAsync(session.Id);
        Sessions.Remove(session);
        if (CurrentSession == session)
        {
            CurrentSession = Sessions.FirstOrDefault();
        }
    }

    [RelayCommand]
    private async Task SaveAsFlashcardAsync(ChatMessage msg)
    {
        var card = new Flashcard
        {
            Front = "Kiến thức trọng tâm từ Chat",
            Back = msg.Content,
            SubjectName = "Chung",
            NextReviewDate = DateTime.UtcNow
        };
        await _databaseService.SaveFlashcardAsync(card);
    }
}
