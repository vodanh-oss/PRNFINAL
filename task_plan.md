# Smart Study Assistant - Task Plan

## Goal
Phát triển ứng dụng Desktop WPF (.NET 8) hoàn chỉnh cho "Trợ lý Ôn thi & Học tập Sinh viên" (Smart Study Assistant) tích hợp AI Chat đa nhà cung cấp (Gemini, OpenAI, Claude, Groq/Ollama), RAG trên tài liệu PDF/DOCX/TXT với trích dẫn slide/trang chính xác, và bộ công cụ tăng năng suất (Pomodoro, Flashcards SRS, Checklist tiến độ môn học, Ghi chú).

## Phases & Status

- [x] **Phase 1: Setup & Solution Infrastructure**
  - [x] T001: Khởi tạo .NET Solution & các Project (`SmartStudyAssistant.sln`)
  - [x] T002: Cài đặt NuGet dependencies (`CommunityToolkit.Mvvm`, `PdfPig`, `OpenXml`, `Sqlite`, `xunit`)
- [x] **Phase 2: Core Domain & Infrastructure**
  - [x] T003: Domain Models (`ChatMessage`, `DocumentItem`, `Flashcard`, `SubjectItem`, etc.)
  - [x] T004: DPAPI Secure Storage (`DpapiSecureStorage.cs`)
  - [x] T005: SQLite Local Database Service (`SqliteDatabaseService.cs`)
  - [x] T006: Document Parsers (`PdfDocumentParser`, `DocxDocumentParser`, `TxtDocumentParser`)
  - [x] T007: Text Chunker (preserve page/slide metadata)
  - [x] T008: Vector RAG Engine (Cosine Similarity & Lexical Fallback)
- [x] **Phase 3: Multi-Provider AI Engine (P1 MVP)**
  - [x] T009: `IAiProvider` contract
  - [x] T010: `GeminiAiProvider` (Gemini 1.5 Flash/Pro + Embeddings)
  - [x] T011: `OpenAiCompatibleProvider` (OpenAI, Groq, Ollama)
  - [x] T012: `ClaudeAiProvider` (Anthropic Messages API)
  - [x] T013: `AiServiceCoordinator` (Hot-swapping & Provider persistence)
  - [x] T014: `SettingsViewModel` & `SettingsView` (Test Connection & DPAPI Save)
- [x] **Phase 4: Document RAG Engine & Lecture Citations (P1)**
  - [x] T015: `DocumentsViewModel` & `DocumentsView` (Drag-drop & chunking progress)
  - [x] T016: RAG Context & Citation Injector (`[Slide X - TenFile.pdf]`)
  - [x] T017: Citation Preview Modal / Flyout
- [x] **Phase 5: AI Chat & Study Assistant (P2)**
  - [x] T018: `ChatViewModel` & `ChatView` (Streaming SSE & Session Management)
  - [x] T019: Markdown & Prompt Template Chips
- [x] **Phase 6: Student Productivity Suite (P2)**
  - [x] T020: `PomodoroViewModel` & `PomodoroView` (25/5/15 timing & sound alerts)
  - [x] T021: `FlashcardsViewModel` & `FlashcardsView` (Flip card, SRS & AI generation)
  - [x] T022: `ProgressViewModel` & `ProgressView` (Subject checklist & progress bars)
  - [x] T023: `StudyNote` Integration (Save chat messages to notebook)
- [x] **Phase 7: Shell & UI/UX Polish (Impeccable & UI/UX Pro Max)**
  - [x] T024: `MainWindow.xaml` & `MainViewModel` (Fluent Sidebar Navigation)
  - [x] T025: Resource Dictionaries (Colors, Typography, Controls)
- [x] **Phase 8: Automated Verification (Superpowers TDD)**
  - [x] T026: Unit Test Suite (`TextChunkerTests`, `VectorSimilarityTests`, `PomodoroTimerTests`, `SecurityStorageTests`)
  - [x] T027: Test Execution & Verification (10/10 tests passed)
