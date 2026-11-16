using SmartStudyAssistant.Core.Models;

namespace SmartStudyAssistant.Core.Services.Database;

public interface IDatabaseService
{
    string ConnectionString { get; set; }
    Task InitializeAsync();
    Task<bool> TestConnectionAsync();
    Task SeedSampleDataAsync(bool force = false);

    // Documents & Chunks
    Task<List<DocumentItem>> GetDocumentsAsync();
    Task SaveDocumentAsync(DocumentItem doc);
    Task DeleteDocumentAsync(string documentId);
    Task SaveChunksAsync(IEnumerable<DocumentChunk> chunks);
    Task<List<DocumentChunk>> GetChunksByDocumentIdAsync(string documentId);
    Task<List<DocumentChunk>> GetAllSelectedChunksAsync();

    // Flashcards
    Task<List<Flashcard>> GetFlashcardsAsync(string? subject = null);
    Task SaveFlashcardAsync(Flashcard flashcard);
    Task DeleteFlashcardAsync(string id);

    // Chat
    Task<List<ChatSession>> GetSessionsAsync();
    Task<ChatSession?> GetSessionByIdAsync(string id);
    Task SaveSessionAsync(ChatSession session);
    Task DeleteSessionAsync(string id);
    Task<List<ChatMessage>> GetMessagesAsync(string sessionId);
    Task SaveMessageAsync(ChatMessage message);

    // AppSettings
    Task<string?> GetSettingAsync(string key);
    Task SaveSettingAsync(string key, string value);
}
