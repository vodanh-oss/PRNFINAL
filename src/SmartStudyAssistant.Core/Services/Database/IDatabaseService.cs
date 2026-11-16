using SmartStudyAssistant.Core.Models;

namespace SmartStudyAssistant.Core.Services.Database;

public interface IDatabaseService
{
    Task InitializeAsync();
    string DatabasePath { get; }
    Task SeedSampleDataAsync(bool force = false);
    
    // Sessions & Messages
    Task<List<ChatSession>> GetSessionsAsync();
    Task<ChatSession?> GetSessionByIdAsync(string id);
    Task SaveSessionAsync(ChatSession session);
    Task DeleteSessionAsync(string id);
    Task<List<ChatMessage>> GetMessagesAsync(string sessionId);
    Task SaveMessageAsync(ChatMessage message);

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

    // Subjects & Checklist
    Task<List<SubjectItem>> GetSubjectsAsync();
    Task SaveSubjectAsync(SubjectItem subject);
    Task DeleteSubjectAsync(string id);

    // Pomodoro
    Task<PomodoroStats> GetTodayPomodoroStatsAsync();
    Task RecordPomodoroSessionAsync(int focusMinutes);

    // Notes
    Task<List<StudyNote>> GetNotesAsync();
    Task SaveNoteAsync(StudyNote note);
    Task DeleteNoteAsync(string id);
}
