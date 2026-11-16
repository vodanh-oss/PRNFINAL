-- ==============================================================================
-- SMART STUDY ASSISTANT - CƠ SỞ DỮ LIỆU SQL SERVER (SSMS)
-- Mở tệp này trong SQL Server Management Studio (SSMS) và nhấn [F5] (Execute)
-- ==============================================================================

-- 1. Tạo Database SmartStudyDb nếu chưa tồn tại
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SmartStudyDb')
BEGIN
    CREATE DATABASE SmartStudyDb;
    PRINT N'✓ Đã tạo thành công Database SmartStudyDb';
END
GO

USE SmartStudyDb;
GO

-- 2. Bảng Tài liệu & Slide bài giảng (Documents)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Documents')
BEGIN
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
    PRINT N'✓ Đã tạo bảng Documents';
END
GO

-- 3. Bảng Phân đoạn Slide (Document Chunks phục vụ RAG)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'DocumentChunks')
BEGIN
    CREATE TABLE DocumentChunks (
        Id NVARCHAR(50) PRIMARY KEY,
        DocumentId NVARCHAR(50) NOT NULL FOREIGN KEY REFERENCES Documents(Id) ON DELETE CASCADE,
        DocumentName NVARCHAR(255) NOT NULL,
        ChunkIndex INT NOT NULL,
        PageOrSlideNumber INT NOT NULL,
        TextContent NVARCHAR(MAX) NOT NULL,
        CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
    );
    PRINT N'✓ Đã tạo bảng DocumentChunks';
END
GO

-- 4. Bảng Thẻ Ghi Nhớ Ôn Tập (Flashcards chia theo từng môn học)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Flashcards')
BEGIN
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
    PRINT N'✓ Đã tạo bảng Flashcards';
END
GO

-- 5. Bảng Phiên Hỏi Đáp (Chat Sessions)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'ChatSessions')
BEGIN
    CREATE TABLE ChatSessions (
        Id NVARCHAR(50) PRIMARY KEY,
        Title NVARCHAR(255) NOT NULL,
        CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
        PreferredProvider INT DEFAULT 0
    );
    PRINT N'✓ Đã tạo bảng ChatSessions';
END
GO

-- 6. Bảng Tin Nhắn & Trích Dẫn RAG (Chat Messages)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'ChatMessages')
BEGIN
    CREATE TABLE ChatMessages (
        Id NVARCHAR(50) PRIMARY KEY,
        SessionId NVARCHAR(50) NOT NULL FOREIGN KEY REFERENCES ChatSessions(Id) ON DELETE CASCADE,
        Role NVARCHAR(50) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        CitationsJson NVARCHAR(MAX) NULL,
        Timestamp DATETIME2 DEFAULT SYSUTCDATETIME()
    );
    PRINT N'✓ Đã tạo bảng ChatMessages';
END
GO

-- 7. Bảng Cấu Hình Hệ Thống (AppSettings)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'AppSettings')
BEGIN
    CREATE TABLE AppSettings (
        SettingKey NVARCHAR(100) PRIMARY KEY,
        SettingValue NVARCHAR(MAX) NOT NULL
    );
    PRINT N'✓ Đã tạo bảng AppSettings';
END
GO

-- ==============================================================================
-- NẠP DỮ LIỆU MẪU BAN ĐẦU ĐỂ SINH VIÊN TRẢI NGHIỆM NGAY
-- ==============================================================================

-- Nạp tài liệu mẫu và chunks
IF NOT EXISTS (SELECT 1 FROM Documents WHERE Id = 'sample-prn-guide')
BEGIN
    INSERT INTO Documents (Id, FileName, FilePath, FileType, TotalPages, TotalChunks, UploadedAt, IsSelectedForRag)
    VALUES ('sample-prn-guide', N'DeCuong_KienTruc_DotNet_Va_AI.txt', N'C:\SmartStudy\DeCuong_KienTruc_DotNet_Va_AI.txt', 'TXT', 4, 4, SYSUTCDATETIME(), 1);

    INSERT INTO DocumentChunks (Id, DocumentId, DocumentName, ChunkIndex, PageOrSlideNumber, TextContent)
    VALUES 
    ('c1', 'sample-prn-guide', N'DeCuong_KienTruc_DotNet_Va_AI.txt', 1, 1, N'Tổng quan Kiến trúc Ứng dụng .NET 8 & WPF: Phân chia rõ ràng thành Presentation Layer (WPF MVVM), Business Logic Layer (Services), và Data Access Layer (SQL Server). Tích hợp Inversion of Control (IoC) thông qua Microsoft Dependency Injection.'),
    ('c2', 'sample-prn-guide', N'DeCuong_KienTruc_DotNet_Va_AI.txt', 2, 2, N'Mô hình MVVM (Model-View-ViewModel) trong C#: Phân tách triệt để giữa giao diện XAML và logic điều khiển. ViewModel kế thừa ObservableObject, hỗ trợ Data Binding và RelayCommand tự động cập nhật UI.'),
    ('c3', 'sample-prn-guide', N'DeCuong_KienTruc_DotNet_Va_AI.txt', 3, 3, N'Kiến trúc RAG (Retrieval-Augmented Generation): Tìm kiếm đoạn văn bản tương đồng (TF-IDF Vector) trong slide bài giảng và đưa vào ngữ cảnh prompt để AI trả lời có trích dẫn số trang/slide rõ ràng.'),
    ('c4', 'sample-prn-guide', N'DeCuong_KienTruc_DotNet_Va_AI.txt', 4, 4, N'Phương pháp ôn tập lặp lại ngắt quãng (Spaced Repetition Flashcards): Ôn tập thẻ theo chu kỳ 1 ngày, 3 ngày, 7 ngày giúp củng cố trí nhớ dài hạn trước kỳ thi môn PRN.');

    PRINT N'✓ Đã nạp tài liệu bài giảng mẫu & chunks RAG';
END
GO

-- Nạp thẻ Flashcards chia theo môn học
IF NOT EXISTS (SELECT 1 FROM Flashcards)
BEGIN
    INSERT INTO Flashcards (Id, SubjectName, Front, Back, EaseLevel, ReviewCount, NextReviewDate)
    VALUES
    (NEWID(), N'PRN211', N'Mô hình MVVM gồm những thành phần nào?', N'Model (Dữ liệu/Nghiệp vụ), View (Giao diện XAML), ViewModel (Cầu nối xử lý Data Binding và Commands).', 3, 1, DATEADD(day, 3, SYSUTCDATETIME())),
    (NEWID(), N'PRN211', N'ICommand và RelayCommand dùng để làm gì?', N'Dùng để liên kết sự kiện người dùng (như nhấn nút) từ giao diện XAML vào hàm xử lý trong ViewModel mà không cần viết code-behind.', 3, 0, SYSUTCDATETIME()),
    (NEWID(), N'PRN211', N'INotifyPropertyChanged có vai trò gì trong WPF?', N'Báo hiệu cho giao diện XAML biết thuộc tính dữ liệu vừa thay đổi để UI tự động cập nhật hiển thị mới nhất.', 5, 2, DATEADD(day, 7, SYSUTCDATETIME())),
    (NEWID(), N'AI301', N'RAG (Retrieval-Augmented Generation) là gì?', N'Kỹ thuật kết hợp giữa truy xuất tài liệu nội bộ (Search) và mô hình ngôn ngữ lớn (LLM) để AI trả lời chính xác kèm dẫn chứng.', 3, 1, DATEADD(day, 3, SYSUTCDATETIME())),
    (NEWID(), N'AI301', N'Cosine Similarity được dùng để làm gì trong RAG?', N'Đo lường góc giữa 2 vector văn bản để xác định mức độ tương đồng ngữ nghĩa giữa câu hỏi của sinh viên và đoạn slide.', 3, 0, SYSUTCDATETIME()),
    (NEWID(), N'Chung', N'Quy tắc 20/80 (Pareto) trong ôn thi là gì?', N'80% điểm số trong bài thi thường tập trung vào 20% kiến thức cốt lõi nhất của môn học.', 5, 2, DATEADD(day, 7, SYSUTCDATETIME()));

    PRINT N'✓ Đã nạp danh sách thẻ Flashcards mẫu theo nhóm môn học';
END
GO

PRINT N'==============================================================================';
PRINT N'✓ KHỞI TẠO CƠ SỞ DỮ LIỆU HOÀN TẤT! SẴN SÀNG KẾT NỐI TỪ ỨNG DỤNG WPF.';
PRINT N'==============================================================================';
GO
