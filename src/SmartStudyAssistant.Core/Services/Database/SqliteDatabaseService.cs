using System.Text.Json;
using Microsoft.Data.Sqlite;
using SmartStudyAssistant.Core.Models;

namespace SmartStudyAssistant.Core.Services.Database;

public class SqliteDatabaseService : IDatabaseService
{
    private readonly string _connectionString;
    private bool _isInitialized;

    public string DatabasePath { get; }

    public SqliteDatabaseService(string? customDbPath = null)
    {
        string dbPath;
        if (!string.IsNullOrEmpty(customDbPath))
        {
            dbPath = customDbPath;
        }
        else
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var folder = Path.Combine(appData, "SmartStudyAssistant");
            Directory.CreateDirectory(folder);
            dbPath = Path.Combine(folder, "study_assistant.db");
        }

        DatabasePath = dbPath;

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        var sql = """
            CREATE TABLE IF NOT EXISTS ChatSessions (
                Id TEXT PRIMARY KEY,
                Title TEXT,
                CreatedAt TEXT,
                LastModifiedAt TEXT,
                PreferredProvider INTEGER,
                IsRagEnabled INTEGER
            );

            CREATE TABLE IF NOT EXISTS ChatMessages (
                Id TEXT PRIMARY KEY,
                SessionId TEXT,
                Role TEXT,
                Content TEXT,
                Timestamp TEXT,
                CitationsJson TEXT,
                ProviderUsed TEXT,
                ModelUsed TEXT,
                FOREIGN KEY(SessionId) REFERENCES ChatSessions(Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS Documents (
                Id TEXT PRIMARY KEY,
                FileName TEXT,
                FilePath TEXT,
                FileType TEXT,
                TotalPages INTEGER,
                TotalChunks INTEGER,
                FileSizeBytes INTEGER,
                UploadedAt TEXT,
                IsSelectedForRag INTEGER
            );

            CREATE TABLE IF NOT EXISTS DocumentChunks (
                Id TEXT PRIMARY KEY,
                DocumentId TEXT,
                DocumentName TEXT,
                ChunkIndex INTEGER,
                PageOrSlideNumber INTEGER,
                TextContent TEXT,
                EmbeddingJson TEXT,
                CreatedAt TEXT,
                FOREIGN KEY(DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS Flashcards (
                Id TEXT PRIMARY KEY,
                SubjectName TEXT,
                Front TEXT,
                Back TEXT,
                Hint TEXT,
                Tags TEXT,
                ReviewCount INTEGER,
                EaseLevel INTEGER,
                NextReviewDate TEXT,
                CreatedAt TEXT
            );

            CREATE TABLE IF NOT EXISTS Subjects (
                Id TEXT PRIMARY KEY,
                Name TEXT,
                Code TEXT,
                TargetGrade TEXT,
                ExamDate TEXT,
                ChecklistJson TEXT
            );

            CREATE TABLE IF NOT EXISTS PomodoroStats (
                Id TEXT PRIMARY KEY,
                SessionDate TEXT,
                CompletedSessions INTEGER,
                TotalFocusMinutes INTEGER
            );

            CREATE TABLE IF NOT EXISTS StudyNotes (
                Id TEXT PRIMARY KEY,
                SubjectName TEXT,
                Title TEXT,
                Content TEXT,
                Tags TEXT,
                CreatedAt TEXT,
                UpdatedAt TEXT
            );
        """;

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();

        _isInitialized = true;
        await SeedSampleDataAsync(force: false);
    }

    private async Task EnsureInitializedAsync()
    {
        if (!_isInitialized) await InitializeAsync();
    }

    public async Task<List<ChatSession>> GetSessionsAsync()
    {
        await EnsureInitializedAsync();
        var list = new List<ChatSession>();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Title, CreatedAt, LastModifiedAt, PreferredProvider, IsRagEnabled FROM ChatSessions ORDER BY LastModifiedAt DESC;";
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new ChatSession
            {
                Id = reader.GetString(0),
                Title = reader.GetString(1),
                CreatedAt = DateTime.Parse(reader.GetString(2)),
                LastModifiedAt = DateTime.Parse(reader.GetString(3)),
                PreferredProvider = (AiProviderType)reader.GetInt32(4),
                IsRagEnabled = reader.GetInt32(5) == 1
            });
        }
        return list;
    }

    public async Task<ChatSession?> GetSessionByIdAsync(string id)
    {
        await EnsureInitializedAsync();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Title, CreatedAt, LastModifiedAt, PreferredProvider, IsRagEnabled FROM ChatSessions WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ChatSession
            {
                Id = reader.GetString(0),
                Title = reader.GetString(1),
                CreatedAt = DateTime.Parse(reader.GetString(2)),
                LastModifiedAt = DateTime.Parse(reader.GetString(3)),
                PreferredProvider = (AiProviderType)reader.GetInt32(4),
                IsRagEnabled = reader.GetInt32(5) == 1
            };
        }
        return null;
    }

    public async Task SaveSessionAsync(ChatSession session)
    {
        await EnsureInitializedAsync();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO ChatSessions (Id, Title, CreatedAt, LastModifiedAt, PreferredProvider, IsRagEnabled)
            VALUES (@Id, @Title, @CreatedAt, @LastModifiedAt, @PreferredProvider, @IsRagEnabled)
            ON CONFLICT(Id) DO UPDATE SET
                Title = excluded.Title,
                LastModifiedAt = excluded.LastModifiedAt,
                PreferredProvider = excluded.PreferredProvider,
                IsRagEnabled = excluded.IsRagEnabled;
        """;
        cmd.Parameters.AddWithValue("@Id", session.Id);
        cmd.Parameters.AddWithValue("@Title", session.Title);
        cmd.Parameters.AddWithValue("@CreatedAt", session.CreatedAt.ToString("o"));
        cmd.Parameters.AddWithValue("@LastModifiedAt", session.LastModifiedAt.ToString("o"));
        cmd.Parameters.AddWithValue("@PreferredProvider", (int)session.PreferredProvider);
        cmd.Parameters.AddWithValue("@IsRagEnabled", session.IsRagEnabled ? 1 : 0);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteSessionAsync(string id)
    {
        await EnsureInitializedAsync();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM ChatMessages WHERE SessionId = @Id; DELETE FROM ChatSessions WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(string sessionId)
    {
        await EnsureInitializedAsync();
        var list = new List<ChatMessage>();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, SessionId, Role, Content, Timestamp, CitationsJson, ProviderUsed, ModelUsed FROM ChatMessages WHERE SessionId = @SessionId ORDER BY Timestamp ASC;";
        cmd.Parameters.AddWithValue("@SessionId", sessionId);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var citationsJson = reader.IsDBNull(5) ? null : reader.GetString(5);
            var citations = !string.IsNullOrEmpty(citationsJson)
                ? JsonSerializer.Deserialize<List<CitationItem>>(citationsJson) ?? new()
                : new List<CitationItem>();

            list.Add(new ChatMessage
            {
                Id = reader.GetString(0),
                SessionId = reader.GetString(1),
                Role = reader.GetString(2),
                Content = reader.GetString(3),
                Timestamp = DateTime.Parse(reader.GetString(4)),
                Citations = citations,
                ProviderUsed = reader.IsDBNull(6) ? null : reader.GetString(6),
                ModelUsed = reader.IsDBNull(7) ? null : reader.GetString(7)
            });
        }
        return list;
    }

    public async Task SaveMessageAsync(ChatMessage message)
    {
        await EnsureInitializedAsync();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO ChatMessages (Id, SessionId, Role, Content, Timestamp, CitationsJson, ProviderUsed, ModelUsed)
            VALUES (@Id, @SessionId, @Role, @Content, @Timestamp, @CitationsJson, @ProviderUsed, @ModelUsed)
            ON CONFLICT(Id) DO UPDATE SET
                Content = excluded.Content,
                CitationsJson = excluded.CitationsJson;
        """;
        cmd.Parameters.AddWithValue("@Id", message.Id);
        cmd.Parameters.AddWithValue("@SessionId", message.SessionId);
        cmd.Parameters.AddWithValue("@Role", message.Role);
        cmd.Parameters.AddWithValue("@Content", message.Content);
        cmd.Parameters.AddWithValue("@Timestamp", message.Timestamp.ToString("o"));
        cmd.Parameters.AddWithValue("@CitationsJson", JsonSerializer.Serialize(message.Citations));
        cmd.Parameters.AddWithValue("@ProviderUsed", (object?)message.ProviderUsed ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ModelUsed", (object?)message.ModelUsed ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<DocumentItem>> GetDocumentsAsync()
    {
        await EnsureInitializedAsync();
        var list = new List<DocumentItem>();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, FileName, FilePath, FileType, TotalPages, TotalChunks, FileSizeBytes, UploadedAt, IsSelectedForRag FROM Documents ORDER BY UploadedAt DESC;";
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new DocumentItem
            {
                Id = reader.GetString(0),
                FileName = reader.GetString(1),
                FilePath = reader.GetString(2),
                FileType = reader.GetString(3),
                TotalPages = reader.GetInt32(4),
                TotalChunks = reader.GetInt32(5),
                FileSizeBytes = reader.GetInt64(6),
                UploadedAt = DateTime.Parse(reader.GetString(7)),
                IsSelectedForRag = reader.GetInt32(8) == 1
            });
        }
        return list;
    }

    public async Task SaveDocumentAsync(DocumentItem doc)
    {
        await EnsureInitializedAsync();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Documents (Id, FileName, FilePath, FileType, TotalPages, TotalChunks, FileSizeBytes, UploadedAt, IsSelectedForRag)
            VALUES (@Id, @FileName, @FilePath, @FileType, @TotalPages, @TotalChunks, @FileSizeBytes, @UploadedAt, @IsSelectedForRag)
            ON CONFLICT(Id) DO UPDATE SET
                TotalPages = excluded.TotalPages,
                TotalChunks = excluded.TotalChunks,
                IsSelectedForRag = excluded.IsSelectedForRag;
        """;
        cmd.Parameters.AddWithValue("@Id", doc.Id);
        cmd.Parameters.AddWithValue("@FileName", doc.FileName);
        cmd.Parameters.AddWithValue("@FilePath", doc.FilePath);
        cmd.Parameters.AddWithValue("@FileType", doc.FileType);
        cmd.Parameters.AddWithValue("@TotalPages", doc.TotalPages);
        cmd.Parameters.AddWithValue("@TotalChunks", doc.TotalChunks);
        cmd.Parameters.AddWithValue("@FileSizeBytes", doc.FileSizeBytes);
        cmd.Parameters.AddWithValue("@UploadedAt", doc.UploadedAt.ToString("o"));
        cmd.Parameters.AddWithValue("@IsSelectedForRag", doc.IsSelectedForRag ? 1 : 0);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteDocumentAsync(string documentId)
    {
        await EnsureInitializedAsync();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM DocumentChunks WHERE DocumentId = @Id; DELETE FROM Documents WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", documentId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task SaveChunksAsync(IEnumerable<DocumentChunk> chunks)
    {
        await EnsureInitializedAsync();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var transaction = conn.BeginTransaction();
        foreach (var chunk in chunks)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = """
                INSERT OR REPLACE INTO DocumentChunks (Id, DocumentId, DocumentName, ChunkIndex, PageOrSlideNumber, TextContent, EmbeddingJson, CreatedAt)
                VALUES (@Id, @DocumentId, @DocumentName, @ChunkIndex, @PageOrSlideNumber, @TextContent, @EmbeddingJson, @CreatedAt);
            """;
            cmd.Parameters.AddWithValue("@Id", chunk.Id);
            cmd.Parameters.AddWithValue("@DocumentId", chunk.DocumentId);
            cmd.Parameters.AddWithValue("@DocumentName", chunk.DocumentName);
            cmd.Parameters.AddWithValue("@ChunkIndex", chunk.ChunkIndex);
            cmd.Parameters.AddWithValue("@PageOrSlideNumber", chunk.PageOrSlideNumber);
            cmd.Parameters.AddWithValue("@TextContent", chunk.TextContent);
            cmd.Parameters.AddWithValue("@EmbeddingJson", chunk.Embedding != null ? JsonSerializer.Serialize(chunk.Embedding) : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedAt", chunk.CreatedAt.ToString("o"));
            await cmd.ExecuteNonQueryAsync();
        }
        transaction.Commit();
    }

    public async Task<List<DocumentChunk>> GetChunksByDocumentIdAsync(string documentId)
    {
        await EnsureInitializedAsync();
        var list = new List<DocumentChunk>();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, DocumentId, DocumentName, ChunkIndex, PageOrSlideNumber, TextContent, EmbeddingJson, CreatedAt FROM DocumentChunks WHERE DocumentId = @DocId ORDER BY ChunkIndex ASC;";
        cmd.Parameters.AddWithValue("@DocId", documentId);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            float[]? emb = null;
            if (!reader.IsDBNull(6))
            {
                var json = reader.GetString(6);
                emb = JsonSerializer.Deserialize<float[]>(json);
            }

            list.Add(new DocumentChunk
            {
                Id = reader.GetString(0),
                DocumentId = reader.GetString(1),
                DocumentName = reader.GetString(2),
                ChunkIndex = reader.GetInt32(3),
                PageOrSlideNumber = reader.GetInt32(4),
                TextContent = reader.GetString(5),
                Embedding = emb,
                CreatedAt = DateTime.Parse(reader.GetString(7))
            });
        }
        return list;
    }

    public async Task<List<DocumentChunk>> GetAllSelectedChunksAsync()
    {
        await EnsureInitializedAsync();
        var list = new List<DocumentChunk>();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT c.Id, c.DocumentId, c.DocumentName, c.ChunkIndex, c.PageOrSlideNumber, c.TextContent, c.EmbeddingJson, c.CreatedAt 
            FROM DocumentChunks c
            INNER JOIN Documents d ON c.DocumentId = d.Id
            WHERE d.IsSelectedForRag = 1;
        """;
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            float[]? emb = null;
            if (!reader.IsDBNull(6))
            {
                var json = reader.GetString(6);
                emb = JsonSerializer.Deserialize<float[]>(json);
            }

            list.Add(new DocumentChunk
            {
                Id = reader.GetString(0),
                DocumentId = reader.GetString(1),
                DocumentName = reader.GetString(2),
                ChunkIndex = reader.GetInt32(3),
                PageOrSlideNumber = reader.GetInt32(4),
                TextContent = reader.GetString(5),
                Embedding = emb,
                CreatedAt = DateTime.Parse(reader.GetString(7))
            });
        }
        return list;
    }

    public async Task<List<Flashcard>> GetFlashcardsAsync(string? subject = null)
    {
        await EnsureInitializedAsync();
        var list = new List<Flashcard>();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        if (string.IsNullOrEmpty(subject) || subject == "Tất cả")
        {
            cmd.CommandText = "SELECT Id, SubjectName, Front, Back, Hint, Tags, ReviewCount, EaseLevel, NextReviewDate, CreatedAt FROM Flashcards ORDER BY NextReviewDate ASC;";
        }
        else
        {
            cmd.CommandText = "SELECT Id, SubjectName, Front, Back, Hint, Tags, ReviewCount, EaseLevel, NextReviewDate, CreatedAt FROM Flashcards WHERE SubjectName = @Subject ORDER BY NextReviewDate ASC;";
            cmd.Parameters.AddWithValue("@Subject", subject);
        }

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Flashcard
            {
                Id = reader.GetString(0),
                SubjectName = reader.GetString(1),
                Front = reader.GetString(2),
                Back = reader.GetString(3),
                Hint = reader.IsDBNull(4) ? null : reader.GetString(4),
                Tags = reader.GetString(5),
                ReviewCount = reader.GetInt32(6),
                EaseLevel = reader.GetInt32(7),
                NextReviewDate = DateTime.Parse(reader.GetString(8)),
                CreatedAt = DateTime.Parse(reader.GetString(9))
            });
        }
        return list;
    }

    public async Task SaveFlashcardAsync(Flashcard flashcard)
    {
        await EnsureInitializedAsync();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Flashcards (Id, SubjectName, Front, Back, Hint, Tags, ReviewCount, EaseLevel, NextReviewDate, CreatedAt)
            VALUES (@Id, @SubjectName, @Front, @Back, @Hint, @Tags, @ReviewCount, @EaseLevel, @NextReviewDate, @CreatedAt)
            ON CONFLICT(Id) DO UPDATE SET
                SubjectName = excluded.SubjectName,
                Front = excluded.Front,
                Back = excluded.Back,
                Hint = excluded.Hint,
                Tags = excluded.Tags,
                ReviewCount = excluded.ReviewCount,
                EaseLevel = excluded.EaseLevel,
                NextReviewDate = excluded.NextReviewDate;
        """;
        cmd.Parameters.AddWithValue("@Id", flashcard.Id);
        cmd.Parameters.AddWithValue("@SubjectName", flashcard.SubjectName);
        cmd.Parameters.AddWithValue("@Front", flashcard.Front);
        cmd.Parameters.AddWithValue("@Back", flashcard.Back);
        cmd.Parameters.AddWithValue("@Hint", (object?)flashcard.Hint ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Tags", flashcard.Tags);
        cmd.Parameters.AddWithValue("@ReviewCount", flashcard.ReviewCount);
        cmd.Parameters.AddWithValue("@EaseLevel", flashcard.EaseLevel);
        cmd.Parameters.AddWithValue("@NextReviewDate", flashcard.NextReviewDate.ToString("o"));
        cmd.Parameters.AddWithValue("@CreatedAt", flashcard.CreatedAt.ToString("o"));
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteFlashcardAsync(string id)
    {
        await EnsureInitializedAsync();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Flashcards WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<SubjectItem>> GetSubjectsAsync()
    {
        await EnsureInitializedAsync();
        var list = new List<SubjectItem>();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Code, TargetGrade, ExamDate, ChecklistJson FROM Subjects ORDER BY Name ASC;";
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var checklistJson = reader.IsDBNull(5) ? null : reader.GetString(5);
            var checklist = !string.IsNullOrEmpty(checklistJson)
                ? JsonSerializer.Deserialize<List<ChecklistItem>>(checklistJson) ?? new()
                : new List<ChecklistItem>();

            list.Add(new SubjectItem
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
                Code = reader.GetString(2),
                TargetGrade = reader.GetString(3),
                ExamDate = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4)),
                Checklist = checklist
            });
        }
        return list;
    }

    public async Task SaveSubjectAsync(SubjectItem subject)
    {
        await EnsureInitializedAsync();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Subjects (Id, Name, Code, TargetGrade, ExamDate, ChecklistJson)
            VALUES (@Id, @Name, @Code, @TargetGrade, @ExamDate, @ChecklistJson)
            ON CONFLICT(Id) DO UPDATE SET
                Name = excluded.Name,
                Code = excluded.Code,
                TargetGrade = excluded.TargetGrade,
                ExamDate = excluded.ExamDate,
                ChecklistJson = excluded.ChecklistJson;
        """;
        cmd.Parameters.AddWithValue("@Id", subject.Id);
        cmd.Parameters.AddWithValue("@Name", subject.Name);
        cmd.Parameters.AddWithValue("@Code", subject.Code);
        cmd.Parameters.AddWithValue("@TargetGrade", subject.TargetGrade);
        cmd.Parameters.AddWithValue("@ExamDate", subject.ExamDate.HasValue ? subject.ExamDate.Value.ToString("o") : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ChecklistJson", JsonSerializer.Serialize(subject.Checklist));
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteSubjectAsync(string id)
    {
        await EnsureInitializedAsync();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Subjects WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<PomodoroStats> GetTodayPomodoroStatsAsync()
    {
        await EnsureInitializedAsync();
        var today = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, SessionDate, CompletedSessions, TotalFocusMinutes FROM PomodoroStats WHERE SessionDate = @Date;";
        cmd.Parameters.AddWithValue("@Date", today);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new PomodoroStats
            {
                Id = reader.GetString(0),
                SessionDate = DateTime.Parse(reader.GetString(1)),
                CompletedSessions = reader.GetInt32(2),
                TotalFocusMinutes = reader.GetInt32(3)
            };
        }

        return new PomodoroStats { SessionDate = DateTime.UtcNow.Date };
    }

    public async Task RecordPomodoroSessionAsync(int focusMinutes)
    {
        await EnsureInitializedAsync();
        var today = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO PomodoroStats (Id, SessionDate, CompletedSessions, TotalFocusMinutes)
            VALUES (@Id, @Date, 1, @Minutes)
            ON CONFLICT(Id) DO UPDATE SET
                CompletedSessions = CompletedSessions + 1,
                TotalFocusMinutes = TotalFocusMinutes + @Minutes;
        """;
        cmd.Parameters.AddWithValue("@Id", today);
        cmd.Parameters.AddWithValue("@Date", today);
        cmd.Parameters.AddWithValue("@Minutes", focusMinutes);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<StudyNote>> GetNotesAsync()
    {
        await EnsureInitializedAsync();
        var list = new List<StudyNote>();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, SubjectName, Title, Content, Tags, CreatedAt, UpdatedAt FROM StudyNotes ORDER BY UpdatedAt DESC;";
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new StudyNote
            {
                Id = reader.GetString(0),
                SubjectName = reader.GetString(1),
                Title = reader.GetString(2),
                Content = reader.GetString(3),
                Tags = reader.GetString(4),
                CreatedAt = DateTime.Parse(reader.GetString(5)),
                UpdatedAt = DateTime.Parse(reader.GetString(6))
            });
        }
        return list;
    }

    public async Task SaveNoteAsync(StudyNote note)
    {
        await EnsureInitializedAsync();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO StudyNotes (Id, SubjectName, Title, Content, Tags, CreatedAt, UpdatedAt)
            VALUES (@Id, @SubjectName, @Title, @Content, @Tags, @CreatedAt, @UpdatedAt)
            ON CONFLICT(Id) DO UPDATE SET
                SubjectName = excluded.SubjectName,
                Title = excluded.Title,
                Content = excluded.Content,
                Tags = excluded.Tags,
                UpdatedAt = excluded.UpdatedAt;
        """;
        cmd.Parameters.AddWithValue("@Id", note.Id);
        cmd.Parameters.AddWithValue("@SubjectName", note.SubjectName);
        cmd.Parameters.AddWithValue("@Title", note.Title);
        cmd.Parameters.AddWithValue("@Content", note.Content);
        cmd.Parameters.AddWithValue("@Tags", note.Tags);
        cmd.Parameters.AddWithValue("@CreatedAt", note.CreatedAt.ToString("o"));
        cmd.Parameters.AddWithValue("@UpdatedAt", note.UpdatedAt.ToString("o"));
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteNoteAsync(string id)
    {
        await EnsureInitializedAsync();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM StudyNotes WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task SeedSampleDataAsync(bool force = false)
    {
        // 1. Seed Sample Document & Chunks for immediate RAG testing
        var docs = await GetDocumentsAsync();
        if (docs.Count == 0 || force)
        {
            var sampleDocId = "sample-prn-guide";
            var sampleDoc = new DocumentItem
            {
                Id = sampleDocId,
                FileName = "DeCuong_KienTruc_DotNet_Va_AI.txt",
                FilePath = Path.Combine(Path.GetDirectoryName(DatabasePath) ?? "", "DeCuong_KienTruc_DotNet_Va_AI.txt"),
                FileType = "TXT",
                TotalPages = 4,
                TotalChunks = 4,
                FileSizeBytes = 2048,
                UploadedAt = DateTime.UtcNow,
                IsSelectedForRag = true
            };
            await SaveDocumentAsync(sampleDoc);

            var chunks = new List<DocumentChunk>
            {
                new()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    DocumentId = sampleDocId,
                    DocumentName = sampleDoc.FileName,
                    ChunkIndex = 1,
                    PageOrSlideNumber = 1,
                    TextContent = "Tổng quan Lập trình Ứng dụng .NET 8 & WPF: Khóa học trang bị cho sinh viên kỹ năng xây dựng ứng dụng máy tính hiện đại. Kiến trúc phân lớp chuẩn bao gồm: Presentation Layer (WPF MVVM), Business Logic Layer (Core Services), và Data Access Layer (SQLite). Các nguyên lý SOLID và Inversion of Control (IoC) thông qua Microsoft.Extensions.DependencyInjection.",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    DocumentId = sampleDocId,
                    DocumentName = sampleDoc.FileName,
                    ChunkIndex = 2,
                    PageOrSlideNumber = 2,
                    TextContent = "Mô hình MVVM (Model-View-ViewModel) trong C#: Phân tách triệt để giữa giao diện XAML và logic điều khiển. ViewModel kế thừa ObservableObject, triển khai INotifyPropertyChanged để tự động cập nhật dữ liệu khi thuộc tính thay đổi. ICommand và RelayCommand liên kết trực tiếp các sự kiện người dùng (Click, KeyPress) mà không cần viết code-behind rườm rà.",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    DocumentId = sampleDocId,
                    DocumentName = sampleDoc.FileName,
                    ChunkIndex = 3,
                    PageOrSlideNumber = 3,
                    TextContent = "Kiến trúc RAG (Retrieval-Augmented Generation) & Tìm kiếm ngữ nghĩa: RAG trích xuất các đoạn văn bản (chunks) từ slide bài giảng và tính toán độ tương đồng cosine similarity giữa câu hỏi và tài liệu. Mô hình AI nhận ngữ cảnh bài giảng và sinh câu trả lời chính xác, đính kèm số slide trích dẫn [Slide X - TênFile].",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    DocumentId = sampleDocId,
                    DocumentName = sampleDoc.FileName,
                    ChunkIndex = 4,
                    PageOrSlideNumber = 4,
                    TextContent = "Bảo mật thông tin & Tối ưu hóa học tập: API Key của các dịch vụ đám mây (Gemini, OpenAI, Claude) bắt buộc phải mã hóa qua Windows DPAPI (ProtectedData), lưu trong tệp nhị phân cục bộ. Phương pháp học tập Spaced Repetition (lặp lại ngắt quãng) kết hợp với kỹ thuật đếm giờ Pomodoro 25 phút giúp tăng 60% hiệu quả ghi nhớ trước kỳ thi.",
                    CreatedAt = DateTime.UtcNow
                }
            };
            await SaveChunksAsync(chunks);
        }

        // 2. Seed Sample Subjects & Checklists
        var subjects = await GetSubjectsAsync();
        if (subjects.Count == 0 || force)
        {
            var sub1 = new SubjectItem
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = "Lập trình C# & WPF Nâng Cao",
                Code = "PRN211",
                TargetGrade = "A+",
                ExamDate = DateTime.Now.AddDays(30),
                Checklist = new List<ChecklistItem>
                {
                    new() { Id = "1", Title = "Nắm vững Dependency Injection & ServiceCollection", IsCompleted = true },
                    new() { Id = "2", Title = "Thành thạo XAML Data Binding & IValueConverter", IsCompleted = true },
                    new() { Id = "3", Title = "Tích hợp SQLite & CRUD Operations", IsCompleted = true },
                    new() { Id = "4", Title = "Xử lý Async/Await và đa luồng Dispatcher", IsCompleted = false },
                    new() { Id = "5", Title = "Luyện đề thi thực hành cuối kỳ", IsCompleted = false }
                }
            };
            await SaveSubjectAsync(sub1);

            var sub2 = new SubjectItem
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = "Trí Tuệ Nhân Tạo & RAG",
                Code = "AI301",
                TargetGrade = "9.5",
                ExamDate = DateTime.Now.AddDays(45),
                Checklist = new List<ChecklistItem>
                {
                    new() { Id = "1", Title = "Hiểu cơ chế Embedding và Cosine Similarity", IsCompleted = true },
                    new() { Id = "2", Title = "Kỹ thuật Prompt Engineering và Citations", IsCompleted = true },
                    new() { Id = "3", Title = "Bảo mật khóa API với Windows DPAPI", IsCompleted = true },
                    new() { Id = "4", Title = "Tối ưu hóa chunking văn bản PDF/DOCX", IsCompleted = false }
                }
            };
            await SaveSubjectAsync(sub2);
        }

        // 3. Seed Sample Flashcards
        var flashcards = await GetFlashcardsAsync();
        if (flashcards.Count == 0 || force)
        {
            var cards = new List<Flashcard>
            {
                new()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    SubjectName = "PRN211",
                    Front = "Mô hình MVVM là gì và gồm những thành phần nào?",
                    Back = "Model: Dữ liệu & nghiệp vụ; View: Giao diện XAML; ViewModel: Cầu nối xử lý Data Binding và Commands. Giúp tách biệt hoàn toàn UI và Logic.",
                    ReviewCount = 2,
                    EaseLevel = 3,
                    NextReviewDate = DateTime.Today.AddDays(3),
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    SubjectName = "AI301",
                    Front = "RAG (Retrieval-Augmented Generation) giải quyết vấn đề gì của LLM?",
                    Back = "RAG giảm thiểu ảo giác (hallucination) bằng cách truy xuất dữ liệu từ tài liệu thực tế của người dùng, cung cấp câu trả lời có trích dẫn nguồn cụ thể.",
                    ReviewCount = 1,
                    EaseLevel = 3,
                    NextReviewDate = DateTime.Today.AddDays(2),
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    SubjectName = "AI301",
                    Front = "Công thức Cosine Similarity tính như thế nào?",
                    Back = "cos(θ) = (A · B) / (||A|| × ||B||). Đo góc giữa 2 vector từ -1 đến 1. Điểm số càng gần 1 chứng tỏ hai đoạn văn bản càng tương đồng ngữ nghĩa.",
                    ReviewCount = 0,
                    EaseLevel = 2,
                    NextReviewDate = DateTime.Today.AddDays(1),
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    SubjectName = "Chung",
                    Front = "Kỹ thuật Pomodoro chuẩn bao gồm các bước nào?",
                    Back = "25 phút tập trung cao độ (Work) -> 5 phút nghỉ ngắn (Short Break). Sau 4 chu kỳ lặp lại thì nghỉ dài 15 - 30 phút.",
                    ReviewCount = 3,
                    EaseLevel = 5,
                    NextReviewDate = DateTime.Today.AddDays(7),
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    SubjectName = "PRN211",
                    Front = "Windows DPAPI (Data Protection API) hoạt động ra sao?",
                    Back = "Sử dụng khóa bí mật gắn liền với tài khoản đăng nhập Windows để mã hóa dữ liệu. Không ai khác ngoài người dùng đó mở được tệp cấu hình.",
                    ReviewCount = 1,
                    EaseLevel = 4,
                    NextReviewDate = DateTime.Today.AddDays(4),
                    CreatedAt = DateTime.UtcNow
                }
            };
            foreach (var c in cards) await SaveFlashcardAsync(c);
        }

        // 4. Seed Sample Study Note
        var notes = await GetNotesAsync();
        if (notes.Count == 0 || force)
        {
            var note = new StudyNote
            {
                Id = Guid.NewGuid().ToString("N"),
                SubjectName = "PRN211",
                Title = "Ghi chú ôn thi kiến trúc WPF & RAG",
                Content = "Cần lưu ý:\n1. Khi cập nhật UI từ luồng nền (Background Task), bắt buộc dùng Dispatcher.\n2. Chunker văn bản cần giữ nguyên số trang/slide để trích dẫn chính xác.\n3. Mã hóa API Key bằng Windows DPAPI trước khi lưu xuống đĩa.",
                Tags = "WPF, RAG, Architecture",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await SaveNoteAsync(note);
        }
    }
}
