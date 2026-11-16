using System.Text.Json;
using Microsoft.Data.SqlClient;
using SmartStudyAssistant.Core.Models;

namespace SmartStudyAssistant.Core.Services.Database;

public class SqlServerDatabaseService : IDatabaseService
{
    public const string DefaultConnectionString = "Server=localhost;Database=SmartStudyDb;Integrated Security=True;TrustServerCertificate=True;";
    public const string MasterConnectionString = "Server=localhost;Database=master;Integrated Security=True;TrustServerCertificate=True;";

    public string ConnectionString { get; set; } = DefaultConnectionString;

    public SqlServerDatabaseService(string? connectionString = null)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            ConnectionString = connectionString;
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            await conn.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task InitializeAsync()
    {
        // 1. Ensure SmartStudyDb database exists
        try
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString);
            var targetDb = builder.InitialCatalog;

            builder.InitialCatalog = "master";
            using (var masterConn = new SqlConnection(builder.ConnectionString))
            {
                await masterConn.OpenAsync();
                using var checkCmd = masterConn.CreateCommand();
                checkCmd.CommandText = $"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '{targetDb}') CREATE DATABASE [{targetDb}];";
                await checkCmd.ExecuteNonQueryAsync();
            }
        }
        catch
        {
            // Fall through if permissions on master are restricted
        }

        // 2. Ensure tables exist in target database
        using (var conn = new SqlConnection(ConnectionString))
        {
            await conn.OpenAsync();

            var schemaSql = """
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Documents')
                CREATE TABLE Documents (
                    Id NVARCHAR(50) PRIMARY KEY,
                    FileName NVARCHAR(255) NOT NULL,
                    FilePath NVARCHAR(500) NOT NULL,
                    FileType NVARCHAR(50) NOT NULL,
                    TotalPages INT DEFAULT 1,
                    TotalChunks INT DEFAULT 0,
                    UploadedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
                    IsSelectedForRag BIT DEFAULT 1
                );

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DocumentChunks')
                CREATE TABLE DocumentChunks (
                    Id NVARCHAR(50) PRIMARY KEY,
                    DocumentId NVARCHAR(50) NOT NULL FOREIGN KEY REFERENCES Documents(Id) ON DELETE CASCADE,
                    DocumentName NVARCHAR(255) NOT NULL,
                    ChunkIndex INT NOT NULL,
                    PageOrSlideNumber INT NOT NULL,
                    TextContent NVARCHAR(MAX) NOT NULL,
                    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
                );

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Flashcards')
                CREATE TABLE Flashcards (
                    Id NVARCHAR(50) PRIMARY KEY,
                    SubjectName NVARCHAR(100) NOT NULL DEFAULT N'Chung',
                    Front NVARCHAR(MAX) NOT NULL,
                    Back NVARCHAR(MAX) NOT NULL,
                    EaseLevel INT DEFAULT 3,
                    ReviewCount INT DEFAULT 0,
                    NextReviewDate DATETIME2 DEFAULT SYSUTCDATETIME(),
                    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
                );

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatSessions')
                CREATE TABLE ChatSessions (
                    Id NVARCHAR(50) PRIMARY KEY,
                    Title NVARCHAR(255) NOT NULL,
                    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
                    PreferredProvider INT DEFAULT 0
                );

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatMessages')
                CREATE TABLE ChatMessages (
                    Id NVARCHAR(50) PRIMARY KEY,
                    SessionId NVARCHAR(50) NOT NULL FOREIGN KEY REFERENCES ChatSessions(Id) ON DELETE CASCADE,
                    Role NVARCHAR(50) NOT NULL,
                    Content NVARCHAR(MAX) NOT NULL,
                    CitationsJson NVARCHAR(MAX) NULL,
                    Timestamp DATETIME2 DEFAULT SYSUTCDATETIME()
                );

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AppSettings')
                CREATE TABLE AppSettings (
                    SettingKey NVARCHAR(100) PRIMARY KEY,
                    SettingValue NVARCHAR(MAX) NOT NULL
                );
            """;

            using var cmd = conn.CreateCommand();
            cmd.CommandText = schemaSql;
            await cmd.ExecuteNonQueryAsync();
        }

        // 3. Seed sample data if empty
        var docs = await GetDocumentsAsync();
        if (docs.Count == 0)
        {
            await SeedSampleDataAsync(force: false);
        }
    }

    public async Task SeedSampleDataAsync(bool force = false)
    {
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        if (force)
        {
            using var clearCmd = conn.CreateCommand();
            clearCmd.CommandText = "DELETE FROM DocumentChunks; DELETE FROM Documents; DELETE FROM Flashcards; DELETE FROM ChatMessages; DELETE FROM ChatSessions;";
            await clearCmd.ExecuteNonQueryAsync();
        }

        // Seed Document & Chunks
        var sampleDocId = "sample-prn-guide";
        var doc = new DocumentItem
        {
            Id = sampleDocId,
            FileName = "DeCuong_KienTruc_DotNet_Va_AI.txt",
            FilePath = "C:\\SmartStudy\\DeCuong_KienTruc_DotNet_Va_AI.txt",
            FileType = "TXT",
            TotalPages = 4,
            TotalChunks = 4,
            FileSizeBytes = 2048,
            IsSelectedForRag = true
        };
        await SaveDocumentAsync(doc);

        var chunks = new List<DocumentChunk>
        {
            new()
            {
                Id = "c1",
                DocumentId = sampleDocId,
                DocumentName = doc.FileName,
                ChunkIndex = 1,
                PageOrSlideNumber = 1,
                TextContent = "Tổng quan Kiến trúc Ứng dụng .NET 8 & WPF: Phân chia rõ ràng thành Presentation Layer (WPF MVVM), Business Logic Layer (Services), và Data Access Layer (SQL Server). Tích hợp Inversion of Control (IoC) thông qua Microsoft Dependency Injection."
            },
            new()
            {
                Id = "c2",
                DocumentId = sampleDocId,
                DocumentName = doc.FileName,
                ChunkIndex = 2,
                PageOrSlideNumber = 2,
                TextContent = "Mô hình MVVM (Model-View-ViewModel) trong C#: Phân tách triệt để giữa giao diện XAML và logic điều khiển. ViewModel kế thừa ObservableObject, hỗ trợ Data Binding và RelayCommand tự động cập nhật UI."
            },
            new()
            {
                Id = "c3",
                DocumentId = sampleDocId,
                DocumentName = doc.FileName,
                ChunkIndex = 3,
                PageOrSlideNumber = 3,
                TextContent = "Kiến trúc RAG (Retrieval-Augmented Generation): Tìm kiếm đoạn văn bản tương đồng (TF-IDF Vector) trong slide bài giảng và đưa vào ngữ cảnh prompt để AI trả lời có trích dẫn số trang/slide rõ ràng."
            },
            new()
            {
                Id = "c4",
                DocumentId = sampleDocId,
                DocumentName = doc.FileName,
                ChunkIndex = 4,
                PageOrSlideNumber = 4,
                TextContent = "Phương pháp ôn tập lặp lại ngắt quãng (Spaced Repetition Flashcards): Ôn tập thẻ theo chu kỳ 1 ngày, 3 ngày, 7 ngày giúp củng cố trí nhớ dài hạn trước kỳ thi môn PRN."
            }
        };
        await SaveChunksAsync(chunks);

        // Seed Flashcards by Subject Groups
        var cards = new List<Flashcard>
        {
            new()
            {
                Id = Guid.NewGuid().ToString("N"),
                SubjectName = "PRN211",
                Front = "Mô hình MVVM gồm những thành phần nào?",
                Back = "Model (Dữ liệu/Nghiệp vụ), View (Giao diện XAML), ViewModel (Cầu nối xử lý Data Binding và Commands).",
                EaseLevel = 3,
                ReviewCount = 1,
                NextReviewDate = DateTime.UtcNow.AddDays(3)
            },
            new()
            {
                Id = Guid.NewGuid().ToString("N"),
                SubjectName = "PRN211",
                Front = "ICommand và RelayCommand dùng để làm gì?",
                Back = "Dùng để liên kết sự kiện người dùng (như nhấn nút) từ giao diện XAML vào hàm xử lý trong ViewModel mà không cần viết code-behind.",
                EaseLevel = 3,
                ReviewCount = 0,
                NextReviewDate = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid().ToString("N"),
                SubjectName = "PRN211",
                Front = "INotifyPropertyChanged có vai trò gì trong WPF?",
                Back = "Báo hiệu cho giao diện XAML biết thuộc tính dữ liệu vừa thay đổi để UI tự động cập nhật hiển thị mới nhất.",
                EaseLevel = 5,
                ReviewCount = 2,
                NextReviewDate = DateTime.UtcNow.AddDays(7)
            },
            new()
            {
                Id = Guid.NewGuid().ToString("N"),
                SubjectName = "AI301",
                Front = "RAG (Retrieval-Augmented Generation) là gì?",
                Back = "Kỹ thuật kết hợp giữa truy xuất tài liệu nội bộ (Search) và mô hình ngôn ngữ lớn (LLM) để AI trả lời chính xác kèm dẫn chứng.",
                EaseLevel = 3,
                ReviewCount = 1,
                NextReviewDate = DateTime.UtcNow.AddDays(3)
            },
            new()
            {
                Id = Guid.NewGuid().ToString("N"),
                SubjectName = "AI301",
                Front = "Cosine Similarity được dùng để làm gì trong RAG?",
                Back = "Đo lường góc giữa 2 vector văn bản để xác định mức độ tương đồng ngữ nghĩa giữa câu hỏi của sinh viên và đoạn slide.",
                EaseLevel = 3,
                ReviewCount = 0,
                NextReviewDate = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid().ToString("N"),
                SubjectName = "Chung",
                Front = "Quy tắc 20/80 (Pareto) trong ôn thi là gì?",
                Back = "80% điểm số trong bài thi thường tập trung vào 20% kiến thức cốt lõi nhất của môn học.",
                EaseLevel = 5,
                ReviewCount = 2,
                NextReviewDate = DateTime.UtcNow.AddDays(7)
            }
        };

        foreach (var c in cards)
        {
            await SaveFlashcardAsync(c);
        }
    }

    #region Documents & Chunks
    public async Task<List<DocumentItem>> GetDocumentsAsync()
    {
        var list = new List<DocumentItem>();
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, FileName, FilePath, FileType, TotalPages, TotalChunks, UploadedAt, IsSelectedForRag FROM Documents ORDER BY UploadedAt DESC;";
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
                UploadedAt = reader.GetDateTime(6),
                IsSelectedForRag = reader.GetBoolean(7)
            });
        }
        return list;
    }

    public async Task SaveDocumentAsync(DocumentItem doc)
    {
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            IF EXISTS (SELECT 1 FROM Documents WHERE Id = @Id)
                UPDATE Documents SET
                    FileName = @FileName, FilePath = @FilePath, FileType = @FileType,
                    TotalPages = @TotalPages, TotalChunks = @TotalChunks, IsSelectedForRag = @IsSelectedForRag
                WHERE Id = @Id
            ELSE
                INSERT INTO Documents (Id, FileName, FilePath, FileType, TotalPages, TotalChunks, UploadedAt, IsSelectedForRag)
                VALUES (@Id, @FileName, @FilePath, @FileType, @TotalPages, @TotalChunks, @UploadedAt, @IsSelectedForRag);
        """;
        cmd.Parameters.AddWithValue("@Id", doc.Id);
        cmd.Parameters.AddWithValue("@FileName", doc.FileName);
        cmd.Parameters.AddWithValue("@FilePath", doc.FilePath);
        cmd.Parameters.AddWithValue("@FileType", doc.FileType);
        cmd.Parameters.AddWithValue("@TotalPages", doc.TotalPages);
        cmd.Parameters.AddWithValue("@TotalChunks", doc.TotalChunks);
        cmd.Parameters.AddWithValue("@UploadedAt", doc.UploadedAt);
        cmd.Parameters.AddWithValue("@IsSelectedForRag", doc.IsSelectedForRag);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteDocumentAsync(string documentId)
    {
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Documents WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", documentId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task SaveChunksAsync(IEnumerable<DocumentChunk> chunks)
    {
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();
        using var trans = conn.BeginTransaction();

        try
        {
            foreach (var chunk in chunks)
            {
                using var cmd = conn.CreateCommand();
                cmd.Transaction = trans;
                cmd.CommandText = """
                    IF EXISTS (SELECT 1 FROM DocumentChunks WHERE Id = @Id)
                        UPDATE DocumentChunks SET TextContent = @TextContent WHERE Id = @Id
                    ELSE
                        INSERT INTO DocumentChunks (Id, DocumentId, DocumentName, ChunkIndex, PageOrSlideNumber, TextContent, CreatedAt)
                        VALUES (@Id, @DocumentId, @DocumentName, @ChunkIndex, @PageOrSlideNumber, @TextContent, @CreatedAt);
                """;
                cmd.Parameters.AddWithValue("@Id", chunk.Id);
                cmd.Parameters.AddWithValue("@DocumentId", chunk.DocumentId);
                cmd.Parameters.AddWithValue("@DocumentName", chunk.DocumentName);
                cmd.Parameters.AddWithValue("@ChunkIndex", chunk.ChunkIndex);
                cmd.Parameters.AddWithValue("@PageOrSlideNumber", chunk.PageOrSlideNumber);
                cmd.Parameters.AddWithValue("@TextContent", chunk.TextContent);
                cmd.Parameters.AddWithValue("@CreatedAt", chunk.CreatedAt);
                await cmd.ExecuteNonQueryAsync();
            }
            await trans.CommitAsync();
        }
        catch
        {
            await trans.RollbackAsync();
            throw;
        }
    }

    public async Task<List<DocumentChunk>> GetChunksByDocumentIdAsync(string documentId)
    {
        var list = new List<DocumentChunk>();
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, DocumentId, DocumentName, ChunkIndex, PageOrSlideNumber, TextContent, CreatedAt FROM DocumentChunks WHERE DocumentId = @DocId ORDER BY ChunkIndex ASC;";
        cmd.Parameters.AddWithValue("@DocId", documentId);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new DocumentChunk
            {
                Id = reader.GetString(0),
                DocumentId = reader.GetString(1),
                DocumentName = reader.GetString(2),
                ChunkIndex = reader.GetInt32(3),
                PageOrSlideNumber = reader.GetInt32(4),
                TextContent = reader.GetString(5),
                CreatedAt = reader.GetDateTime(6)
            });
        }
        return list;
    }

    public async Task<List<DocumentChunk>> GetAllSelectedChunksAsync()
    {
        var list = new List<DocumentChunk>();
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT c.Id, c.DocumentId, c.DocumentName, c.ChunkIndex, c.PageOrSlideNumber, c.TextContent, c.CreatedAt
            FROM DocumentChunks c
            INNER JOIN Documents d ON c.DocumentId = d.Id
            WHERE d.IsSelectedForRag = 1
            ORDER BY c.DocumentName ASC, c.ChunkIndex ASC;
        """;
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new DocumentChunk
            {
                Id = reader.GetString(0),
                DocumentId = reader.GetString(1),
                DocumentName = reader.GetString(2),
                ChunkIndex = reader.GetInt32(3),
                PageOrSlideNumber = reader.GetInt32(4),
                TextContent = reader.GetString(5),
                CreatedAt = reader.GetDateTime(6)
            });
        }
        return list;
    }
    #endregion

    #region Flashcards
    public async Task<List<Flashcard>> GetFlashcardsAsync(string? subject = null)
    {
        var list = new List<Flashcard>();
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        if (string.IsNullOrWhiteSpace(subject) || subject == "Tất cả")
        {
            cmd.CommandText = "SELECT Id, SubjectName, Front, Back, EaseLevel, ReviewCount, NextReviewDate, CreatedAt FROM Flashcards ORDER BY SubjectName ASC, CreatedAt ASC;";
        }
        else
        {
            cmd.CommandText = "SELECT Id, SubjectName, Front, Back, EaseLevel, ReviewCount, NextReviewDate, CreatedAt FROM Flashcards WHERE SubjectName = @Subject ORDER BY CreatedAt ASC;";
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
                EaseLevel = reader.GetInt32(4),
                ReviewCount = reader.GetInt32(5),
                NextReviewDate = reader.GetDateTime(6),
                CreatedAt = reader.GetDateTime(7)
            });
        }
        return list;
    }

    public async Task SaveFlashcardAsync(Flashcard flashcard)
    {
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            IF EXISTS (SELECT 1 FROM Flashcards WHERE Id = @Id)
                UPDATE Flashcards SET
                    SubjectName = @SubjectName, Front = @Front, Back = @Back,
                    EaseLevel = @EaseLevel, ReviewCount = @ReviewCount, NextReviewDate = @NextReviewDate
                WHERE Id = @Id
            ELSE
                INSERT INTO Flashcards (Id, SubjectName, Front, Back, EaseLevel, ReviewCount, NextReviewDate, CreatedAt)
                VALUES (@Id, @SubjectName, @Front, @Back, @EaseLevel, @ReviewCount, @NextReviewDate, @CreatedAt);
        """;
        cmd.Parameters.AddWithValue("@Id", flashcard.Id);
        cmd.Parameters.AddWithValue("@SubjectName", flashcard.SubjectName);
        cmd.Parameters.AddWithValue("@Front", flashcard.Front);
        cmd.Parameters.AddWithValue("@Back", flashcard.Back);
        cmd.Parameters.AddWithValue("@EaseLevel", flashcard.EaseLevel);
        cmd.Parameters.AddWithValue("@ReviewCount", flashcard.ReviewCount);
        cmd.Parameters.AddWithValue("@NextReviewDate", flashcard.NextReviewDate);
        cmd.Parameters.AddWithValue("@CreatedAt", flashcard.CreatedAt);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteFlashcardAsync(string id)
    {
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Flashcards WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync();
    }
    #endregion

    #region Chat
    public async Task<List<ChatSession>> GetSessionsAsync()
    {
        var list = new List<ChatSession>();
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Title, CreatedAt, PreferredProvider FROM ChatSessions ORDER BY CreatedAt DESC;";
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new ChatSession
            {
                Id = reader.GetString(0),
                Title = reader.GetString(1),
                CreatedAt = reader.GetDateTime(2),
                PreferredProvider = (AiProviderType)reader.GetInt32(3)
            });
        }
        return list;
    }

    public async Task<ChatSession?> GetSessionByIdAsync(string id)
    {
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Title, CreatedAt, PreferredProvider FROM ChatSessions WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ChatSession
            {
                Id = reader.GetString(0),
                Title = reader.GetString(1),
                CreatedAt = reader.GetDateTime(2),
                PreferredProvider = (AiProviderType)reader.GetInt32(3)
            };
        }
        return null;
    }

    public async Task SaveSessionAsync(ChatSession session)
    {
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            IF EXISTS (SELECT 1 FROM ChatSessions WHERE Id = @Id)
                UPDATE ChatSessions SET Title = @Title, PreferredProvider = @PreferredProvider WHERE Id = @Id
            ELSE
                INSERT INTO ChatSessions (Id, Title, CreatedAt, PreferredProvider)
                VALUES (@Id, @Title, @CreatedAt, @PreferredProvider);
        """;
        cmd.Parameters.AddWithValue("@Id", session.Id);
        cmd.Parameters.AddWithValue("@Title", session.Title);
        cmd.Parameters.AddWithValue("@CreatedAt", session.CreatedAt);
        cmd.Parameters.AddWithValue("@PreferredProvider", (int)session.PreferredProvider);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteSessionAsync(string id)
    {
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM ChatSessions WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(string sessionId)
    {
        var list = new List<ChatMessage>();
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, SessionId, Role, Content, CitationsJson, Timestamp FROM ChatMessages WHERE SessionId = @SessionId ORDER BY Timestamp ASC;";
        cmd.Parameters.AddWithValue("@SessionId", sessionId);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var citJson = reader.IsDBNull(4) ? null : reader.GetString(4);
            var citList = !string.IsNullOrEmpty(citJson)
                ? JsonSerializer.Deserialize<List<CitationItem>>(citJson) ?? new()
                : new List<CitationItem>();

            list.Add(new ChatMessage
            {
                Id = reader.GetString(0),
                SessionId = reader.GetString(1),
                Role = reader.GetString(2),
                Content = reader.GetString(3),
                Citations = citList,
                Timestamp = reader.GetDateTime(5)
            });
        }
        return list;
    }

    public async Task SaveMessageAsync(ChatMessage message)
    {
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO ChatMessages (Id, SessionId, Role, Content, CitationsJson, Timestamp)
            VALUES (@Id, @SessionId, @Role, @Content, @CitationsJson, @Timestamp);
        """;
        cmd.Parameters.AddWithValue("@Id", message.Id);
        cmd.Parameters.AddWithValue("@SessionId", message.SessionId);
        cmd.Parameters.AddWithValue("@Role", message.Role);
        cmd.Parameters.AddWithValue("@Content", message.Content);
        cmd.Parameters.AddWithValue("@CitationsJson", message.Citations.Count > 0 ? JsonSerializer.Serialize(message.Citations) : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Timestamp", message.Timestamp);
        await cmd.ExecuteNonQueryAsync();
    }
    #endregion

    #region AppSettings
    public async Task<string?> GetSettingAsync(string key)
    {
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT SettingValue FROM AppSettings WHERE SettingKey = @Key;";
        cmd.Parameters.AddWithValue("@Key", key);
        var val = await cmd.ExecuteScalarAsync();
        return val as string;
    }

    public async Task SaveSettingAsync(string key, string value)
    {
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            IF EXISTS (SELECT 1 FROM AppSettings WHERE SettingKey = @Key)
                UPDATE AppSettings SET SettingValue = @Value WHERE SettingKey = @Key
            ELSE
                INSERT INTO AppSettings (SettingKey, SettingValue) VALUES (@Key, @Value);
        """;
        cmd.Parameters.AddWithValue("@Key", key);
        cmd.Parameters.AddWithValue("@Value", value);
        await cmd.ExecuteNonQueryAsync();
    }
    #endregion
}
