# Tasks: Smart Study Assistant (WPF .NET 8)

**Input**: Design documents from `/specs/001-smart-study-assistant/`  
**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md)  
**Tests**: Automated unit tests in `tests/SmartStudyAssistant.Tests/` (10/10 passed)

---

## Phase 1: Setup & Solution Infrastructure

- [x] **T001**: Khởi tạo cấu trúc Solution `SmartStudyAssistant.sln` với 3 project: `SmartStudyAssistant.Core`, `SmartStudyAssistant.Wpf`, và `SmartStudyAssistant.Tests`.
- [x] **T002**: Cài đặt các package NuGet cần thiết (`CommunityToolkit.Mvvm`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Data.Sqlite`, `PdfPig`, `DocumentFormat.OpenXml`, `xunit`, `FluentAssertions`).

---

## Phase 2: Foundational Architecture & Core Services (Blocking Prerequisites)

- [x] **T003** [P]: Xây dựng Domain Models trong `SmartStudyAssistant.Core/Models/` (`AiProviderConfig.cs`, `ChatMessage.cs`, `ChatSession.cs`, `DocumentItem.cs`, `DocumentChunk.cs`, `Flashcard.cs`, `SubjectItem.cs`, `PomodoroStats.cs`, `StudyNote.cs`).
- [x] **T004** [P]: Triển khai mã hóa bảo mật Windows DPAPI trong `SmartStudyAssistant.Core/Services/Security/` (`ISecureStorage.cs`, `DpapiSecureStorage.cs`).
- [x] **T005**: Xây dựng cơ sở dữ liệu SQLite cục bộ trong `SmartStudyAssistant.Core/Services/Database/` (`IDatabaseService.cs`, `SqliteDatabaseService.cs`).
- [x] **T006** [P]: Xây dựng bộ đọc tài liệu đa định dạng trong `SmartStudyAssistant.Core/Services/Parsers/` (`PdfDocumentParser.cs`, `DocxDocumentParser.cs`, `TxtDocumentParser.cs`).
- [x] **T007**: Xây dựng bộ phân đoạn văn bản thông minh `TextChunker.cs` bảo toàn số trang/slide và ngữ cảnh.
- [x] **T008**: Xây dựng Vector RAG Engine `VectorRagEngine.cs` (tính toán Cosine Similarity, rank Top-K chunks và hybrid fallback).

---

## Phase 3: User Story 1 - Cấu hình & Hoán chuyển Nền tảng AI Đa kênh (Priority: P1) 🎯 MVP Core

- [x] **T009**: Định nghĩa hợp đồng `IAiProvider.cs` (Completion, Streaming, Embeddings, Ping/Test).
- [x] **T010** [P] [US1]: Xây dựng `GeminiAiProvider.cs` hỗ trợ Gemini 1.5 Flash/Pro & text-embedding-004.
- [x] **T011** [P] [US1]: Xây dựng `OpenAiCompatibleProvider.cs` hỗ trợ OpenAI (GPT-4o), Groq (Llama 3.3) và Ollama localhost.
- [x] **T012** [P] [US1]: Xây dựng `ClaudeAiProvider.cs` hỗ trợ Anthropic Messages API (Claude 3.5 Sonnet).
- [x] **T013** [US1]: Xây dựng `AiServiceCoordinator.cs` cho phép hot-swap provider ngay trong phiên chat.
- [x] **T014** [US1]: Xây dựng `SettingsViewModel.cs` và `SettingsView.xaml` với nút Test Connection trực quan.

---

## Phase 4: User Story 2 - RAG: Tra cứu & Trích dẫn Bài giảng (Priority: P1)

- [x] **T015** [US2]: Xây dựng `DocumentsViewModel.cs` và `DocumentsView.xaml` hỗ trợ kéo-thả file (PDF, DOCX, TXT) và hiển thị thanh tiến trình nạp chunks.
- [x] **T016** [US2]: Tích hợp ngữ cảnh tài liệu vào Prompt AI kèm định dạng citation chuẩn `[Slide X - TenFile.pdf]`.
- [x] **T017** [US2]: Xây dựng modal/flyout trích dẫn tương tác cho phép sinh viên click xem nguyên văn slide khi được trích dẫn.

---

## Phase 5: User Story 3 - Trợ lý AI Chat Đa năng (Priority: P2)

- [x] **T018** [US3]: Xây dựng `ChatViewModel.cs` và `ChatView.xaml` với streaming text mượt mà, sidebar lịch sử hội thoại, và prompt mẫu (Dịch thuật, Tóm tắt, Giải thích).
- [x] **T019** [US3]: Hỗ trợ render định dạng Markdown, code block có syntax styling và nút copy nhanh.

---

## Phase 6: User Story 4 - Bộ công cụ Năng suất Sinh viên (Priority: P2)

- [x] **T020** [US4]: Xây dựng `PomodoroViewModel.cs` và `PomodoroView.xaml` (Đồng hồ đếm ngược 25/5/15, chuông báo, thống kê).
- [x] **T021** [US4]: Xây dựng `FlashcardsViewModel.cs` và `FlashcardsView.xaml` (Lật thẻ 2 mặt, đánh giá độ khó SRS, nút AI tạo flashcard tự động).
- [x] **T022** [US4]: Xây dựng `ProgressViewModel.cs` và `ProgressView.xaml` (Quản lý môn học, mục tiêu điểm, checklist chương học, progress bar).
- [x] **T023** [US4]: Tích hợp tính năng lưu ghi chú nhanh `StudyNote` từ tin nhắn chat sang sổ tay.

---

## Phase 7: UI/UX Hoàn thiện & Giao diện Chuyên nghiệp (Impeccable & UI/UX Pro Max)

- [x] **T024**: Xây dựng Shell chính `MainWindow.xaml` và `MainViewModel.cs` với sidebar điều hướng Fluent, hỗ trợ Dark/Light Theme.
- [x] **T025**: Hoàn thiện Resource Dictionaries (`Colors.xaml`, `Typography.xaml`, `Controls.xaml`) chuẩn typography, không AI slop, WCAG AA contrast.

---

## Phase 8: Kiểm thử Tự động & Nghiệm thu (Superpowers TDD)

- [x] **T026**: Viết bộ unit test `SmartStudyAssistant.Tests` (`TextChunkerTests`, `VectorSimilarityTests`, `PomodoroTimerTests`, `SecurityStorageTests`).
- [x] **T027**: Chạy kiểm thử toàn bộ `dotnet test` và xác nhận tất cả test pass 100%.
