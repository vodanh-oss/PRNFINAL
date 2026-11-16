# Implementation Plan: Smart Study Assistant (WPF .NET 8)

**Branch**: `001-smart-study-assistant` | **Date**: 2026-11-16 | **Spec**: [spec.md](./spec.md)

---

## 1. Summary
Xây dựng giải pháp desktop WPF chuyên nghiệp trên nền tảng .NET 8 theo kiến trúc Clean Architecture & MVVM (sử dụng `CommunityToolkit.Mvvm`). Hệ thống kết nối đa nền tảng AI (Gemini, OpenAI, Claude, Groq/Ollama) với kho lưu trữ vector cục bộ phục vụ cơ chế RAG trích xuất thông tin kèm trích dẫn chính xác từ tài liệu học tập (PDF, DOCX, TXT). Tích hợp đồng bộ bộ công cụ học tập sinh viên: Pomodoro Timer, Spaced Repetition Flashcards, Course Progress Checklist và Study Notes.

---

## 2. Technical Context

- **Runtime & Target**: .NET 8.0 Windows (`net8.0-windows`)
- **UI Framework**: Windows Presentation Foundation (WPF) with XAML
- **MVVM Framework**: `CommunityToolkit.Mvvm` (8.3+)
- **Dependency Injection**: `Microsoft.Extensions.DependencyInjection`
- **Database / Local Storage**: SQLite (`Microsoft.Data.Sqlite`) with lightweight repository pattern
- **Security**: Windows DPAPI (`System.Security.Cryptography.ProtectedData`) for local key encryption
- **Document Extractors**: `PdfPig` (trích xuất PDF theo trang/slide), `DocumentFormat.OpenXml` / ZipArchive (DOCX), System.IO (TXT)
- **Networking**: `System.Net.Http.HttpClient` with `IAsyncEnumerable<string>` server-sent event (SSE) streaming
- **Testing**: `xUnit` + `FluentAssertions`

---

## 3. Constitution Alignment

- [x] **Strict MVVM**: ViewModels không chứa tham chiếu UI; mọi logic điều khiển thông qua Data Binding và RelayCommands.
- [x] **IAiProvider Contract**: Kiến trúc Provider mở rộng độc lập, dễ dàng bổ sung provider mới mà không sửa đổi tầng UI.
- [x] **Local-First & DPAPI**: API Key lưu trữ an toàn trong máy người dùng; tài liệu và vector chunks nằm hoàn toàn ở máy sinh viên.
- [x] **Transparent Citations**: Chunk lưu giữ số trang / slide, response hiển thị rõ nguồn tham khảo và xem được trích đoạn.
- [x] **Anti-Slop UI/UX**: Thiết kế Fluent phẳng hiện đại, tương phản chuẩn WCAG AA, bảng màu dịu mắt, chuyển cảnh mượt mà.
- [x] **TDD Verification**: Bộ unit test tự động cho Chunker, Cosine Similarity, DPAPI và Pomodoro Timer.

---

## 4. Architecture & Project Layout

```text
c:\PRNFINAL\
├── SmartStudyAssistant.sln
├── src/
│   ├── SmartStudyAssistant.Core/
│   │   ├── Models/
│   │   │   ├── AiProviderConfig.cs        # Cấu hình API key, endpoint, model
│   │   │   ├── ChatMessage.cs             # Lịch sử tin nhắn & citation metadata
│   │   │   ├── ChatSession.cs             # Phiên chat
│   │   │   ├── DocumentItem.cs            # Thông tin tài liệu đã nạp
│   │   │   ├── DocumentChunk.cs           # Phân đoạn văn bản & vector embedding
│   │   │   ├── Flashcard.cs               # Thẻ ghi nhớ (Front/Back/Ease/DueDate)
│   │   │   ├── SubjectItem.cs             # Môn học & Checklist tiến độ
│   │   │   ├── PomodoroStats.cs           # Thống kê thời gian tập trung
│   │   │   └── StudyNote.cs               # Ghi chú học tập
│   │   ├── Services/
│   │   │   ├── Ai/
│   │   │   │   ├── IAiProvider.cs         # Hợp đồng chung cho AI providers
│   │   │   │   ├── GeminiAiProvider.cs    # Google Gemini 1.5 Flash/Pro + Embeddings
│   │   │   │   ├── OpenAiCompatibleProvider.cs # OpenAI, Groq, Ollama
│   │   │   │   ├── ClaudeAiProvider.cs    # Anthropic Claude 3.5 Sonnet
│   │   │   │   └── AiServiceCoordinator.cs# Điều phối chuyển đổi provider
│   │   │   ├── Rag/
│   │   │   │   ├── IRagEngine.cs          # Hợp đồng tìm kiếm ngữ nghĩa
│   │   │   │   ├── VectorRagEngine.cs     # Cosine similarity + top-k retrieval
│   │   │   │   └── TextChunker.cs         # Cắt đoạn giữ nguyên ngữ cảnh trang/slide
│   │   │   ├── Parsers/
│   │   │   │   ├── IDocumentParser.cs
│   │   │   │   ├── PdfDocumentParser.cs
│   │   │   │   ├── DocxDocumentParser.cs
│   │   │   │   └── TxtDocumentParser.cs
│   │   │   ├── Security/
│   │   │   │   ├── ISecureStorage.cs
│   │   │   │   └── DpapiSecureStorage.cs  # Mã hóa Windows DPAPI
│   │   │   └── Database/
│   │   │       ├── IDatabaseService.cs
│   │   │       └── SqliteDatabaseService.cs
│   │   └── SmartStudyAssistant.Core.csproj
│   │
│   └── SmartStudyAssistant.Wpf/
│       ├── App.xaml / App.xaml.cs          # Composition Root & DI bootstrap
│       ├── ViewModels/
│       │   ├── MainViewModel.cs           # Điều hướng tab, trạng thái toàn cục
│       │   ├── ChatViewModel.cs           # Chat streaming, RAG toggle, Citations
│       │   ├── DocumentsViewModel.cs      # Kéo thả tài liệu, thanh tiến trình nạp
│       │   ├── FlashcardsViewModel.cs     # Quản lý thẻ, chế độ ôn luyện SRS
│       │   ├── PomodoroViewModel.cs       # Đồng hồ 25/5, đếm ngược, chuông
│       │   ├── ProgressViewModel.cs       # Môn học, mục tiêu điểm, checklist
│       │   └── SettingsViewModel.cs       # Quản lý API Key các nhà cung cấp AI
│       ├── Views/
│       │   ├── MainWindow.xaml            # Khung điều hướng & title bar
│       │   ├── ChatView.xaml              # Giao diện chat trực quan
│       │   ├── DocumentsView.xaml         # Kéo thả tài liệu & danh sách slide
│       │   ├── FlashcardsView.xaml        # Thẻ lật 3D hoặc animation lật 2 mặt
│       │   ├── PomodoroView.xaml          # Đồng hồ tròn tối giản
│       │   ├── ProgressView.xaml          # Checklist môn học & thanh phần trăm
│       │   └── SettingsView.xaml          # Cấu hình API key & kiểm tra kết nối
│       ├── Styles/
│       │   ├── Colors.xaml                # Bảng màu tương phản chuẩn WCAG
│       │   ├── Typography.xaml            # Font & cỡ chữ chuẩn
│       │   └── Controls.xaml              # Style cho Button, TextBox, Cards
│       └── SmartStudyAssistant.Wpf.csproj
│
└── tests/
    └── SmartStudyAssistant.Tests/
        ├── TextChunkerTests.cs            # Kiểm tra phân đoạn tài liệu & overlap
        ├── VectorSimilarityTests.cs       # Kiểm tra thuật toán Cosine Similarity
        ├── PomodoroTimerTests.cs          # Kiểm tra chuyển trạng thái Work/Break
        ├── SecurityStorageTests.cs        # Kiểm tra mã hóa/giải mã DPAPI
        └── SmartStudyAssistant.Tests.csproj
```
