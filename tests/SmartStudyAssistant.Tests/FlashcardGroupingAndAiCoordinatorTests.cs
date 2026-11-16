using FluentAssertions;
using SmartStudyAssistant.Core.Models;
using SmartStudyAssistant.Core.Services.Ai;
using SmartStudyAssistant.Core.Services.Database;
using SmartStudyAssistant.Core.Services.Rag;
using SmartStudyAssistant.Core.Services.Security;
using SmartStudyAssistant.Wpf.ViewModels;
using Xunit;

namespace SmartStudyAssistant.Tests;

public class FakeSecureStorage : ISecureStorage
{
    private readonly Dictionary<string, string> _store = new();
    public string Encrypt(string plainText) => plainText;
    public string Decrypt(string cipherText) => cipherText;
    public void SaveSecret(string key, string secret) => _store[key] = secret;
    public string? GetSecret(string key) => _store.TryGetValue(key, out var val) ? val : null;
    public void DeleteSecret(string key) => _store.Remove(key);
}

public class FakeDatabaseService : IDatabaseService
{
    public string DatabasePath => "fake.db";
    public List<Flashcard> Flashcards { get; set; } = new();

    public Task InitializeAsync() => Task.CompletedTask;
    public Task<List<DocumentItem>> GetDocumentsAsync() => Task.FromResult(new List<DocumentItem>());
    public Task SaveDocumentAsync(DocumentItem doc) => Task.CompletedTask;
    public Task DeleteDocumentAsync(string documentId) => Task.CompletedTask;
    public Task SaveChunksAsync(IEnumerable<DocumentChunk> chunks) => Task.CompletedTask;
    public Task<List<DocumentChunk>> GetChunksByDocumentIdAsync(string documentId) => Task.FromResult(new List<DocumentChunk>());
    public Task<List<DocumentChunk>> GetAllSelectedChunksAsync() => Task.FromResult(new List<DocumentChunk>());
    public Task<List<ChatSession>> GetSessionsAsync() => Task.FromResult(new List<ChatSession>());
    public Task<ChatSession?> GetSessionByIdAsync(string id) => Task.FromResult<ChatSession?>(null);
    public Task SaveSessionAsync(ChatSession session) => Task.CompletedTask;
    public Task DeleteSessionAsync(string id) => Task.CompletedTask;
    public Task<List<ChatMessage>> GetMessagesAsync(string sessionId) => Task.FromResult(new List<ChatMessage>());
    public Task SaveMessageAsync(ChatMessage message) => Task.CompletedTask;
    public Task<List<Flashcard>> GetFlashcardsAsync(string? subject = null)
    {
        if (string.IsNullOrWhiteSpace(subject))
            return Task.FromResult(Flashcards.ToList());
        return Task.FromResult(Flashcards.Where(f => f.SubjectName.Equals(subject, StringComparison.OrdinalIgnoreCase)).ToList());
    }
    public Task SaveFlashcardAsync(Flashcard flashcard)
    {
        var existing = Flashcards.FirstOrDefault(f => f.Id == flashcard.Id);
        if (existing != null) Flashcards.Remove(existing);
        Flashcards.Add(flashcard);
        return Task.CompletedTask;
    }
    public Task DeleteFlashcardAsync(string id)
    {
        Flashcards.RemoveAll(f => f.Id == id);
        return Task.CompletedTask;
    }
    public Task<List<SubjectItem>> GetSubjectsAsync() => Task.FromResult(new List<SubjectItem>());
    public Task SaveSubjectAsync(SubjectItem subject) => Task.CompletedTask;
    public Task DeleteSubjectAsync(string id) => Task.CompletedTask;
    public Task<PomodoroStats> GetTodayPomodoroStatsAsync() => Task.FromResult(new PomodoroStats());
    public Task RecordPomodoroSessionAsync(int focusMinutes) => Task.CompletedTask;
    public Task<List<StudyNote>> GetNotesAsync() => Task.FromResult(new List<StudyNote>());
    public Task SaveNoteAsync(StudyNote note) => Task.CompletedTask;
    public Task DeleteNoteAsync(string id) => Task.CompletedTask;
    public Task SeedSampleDataAsync(bool force = false) => Task.CompletedTask;
}

public class FlashcardGroupingAndAiCoordinatorTests
{
    [Fact]
    public void AiCoordinator_WithoutKeysOrOllama_HasNoActiveEmbeddingProvider()
    {
        var fakeStorage = new FakeSecureStorage();
        var coordinator = new AiServiceCoordinator(fakeStorage);

        coordinator.HasActiveEmbeddingProvider().Should().BeFalse();
    }

    [Fact]
    public void VectorRagEngine_WithoutEmbeddings_ReturnsRankedLexicalMatches()
    {
        var engine = new VectorRagEngine();
        var chunks = new List<DocumentChunk>
        {
            new() { Id = "1", DocumentName = "Slide1.pdf", PageOrSlideNumber = 1, TextContent = "Kiến trúc MVVM phân tách Model View ViewModel" },
            new() { Id = "2", DocumentName = "Slide2.pdf", PageOrSlideNumber = 2, TextContent = "Độ phức tạp thuật toán QuickSort là O(n log n)" },
            new() { Id = "3", DocumentName = "Slide3.pdf", PageOrSlideNumber = 3, TextContent = "Dependency Injection ServiceCollection trong WPF C#" }
        };

        var results = engine.SearchSimilarChunks("MVVM là gì", chunks, topK: 2, queryEmbedding: null);

        results.Should().NotBeEmpty();
        results[0].PageOrSlideNumber.Should().Be(1);
        results[0].Snippet.Should().Contain("MVVM");
    }

    [Fact]
    public async Task FlashcardsViewModel_Grouping_FiltersCorrectly()
    {
        var fakeStorage = new FakeSecureStorage();
        var coordinator = new AiServiceCoordinator(fakeStorage);

        var fakeDb = new FakeDatabaseService
        {
            Flashcards = new List<Flashcard>
            {
                new() { Id = "1", SubjectName = "PRN211", Front = "Q1", Back = "A1" },
                new() { Id = "2", SubjectName = "PRN211", Front = "Q2", Back = "A2" },
                new() { Id = "3", SubjectName = "AI301", Front = "Q3", Back = "A3" },
                new() { Id = "4", SubjectName = "Chung", Front = "Q4", Back = "A4" }
            }
        };

        var vm = new FlashcardsViewModel(fakeDb, coordinator);
        await vm.LoadFlashcardsAsync();

        // 4 groups: Tất cả (4), AI301 (1), Chung (1), PRN211 (2)
        vm.GroupItems.Should().HaveCount(4);

        var allGroup = vm.GroupItems.First(g => g.GroupName == "Tất cả");
        allGroup.CardCount.Should().Be(4);

        var prnGroup = vm.GroupItems.First(g => g.GroupName == "PRN211");
        prnGroup.CardCount.Should().Be(2);

        // Select PRN211 group
        vm.SelectGroup("PRN211");
        vm.Flashcards.Should().HaveCount(2);
        vm.TotalCardsCount.Should().Be(2);
        vm.DisplayCardNumber.Should().Be(1);
        vm.CurrentCard?.SubjectName.Should().Be("PRN211");
        vm.ProgressPercentage.Should().Be(50.0); // 1 / 2 = 50%
    }
}
