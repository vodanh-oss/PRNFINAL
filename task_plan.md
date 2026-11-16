# Task Plan: Smart Study Assistant (WPF + SQL Server SSMS + RAG + Multi-AI)

## Goal
Làm lại từ đầu (từ số 0) ứng dụng **Trợ lý Ôn thi & Học tập Sinh viên (Smart Study Assistant)** dựa trên các kinh nghiệm đã tích lũy:
1. **Đơn giản, ít code, không dùng code cao siêu**: Code C# và XAML sạch sẽ, trực quan, dễ hiểu, dễ giải thích cho giảng viên/hội đồng thi.
2. **Chia mục phù hợp, không gộp chung**:
   - 4 tab chuyên biệt: Chat & RAG, Quản lý Slide/Tài liệu, Thẻ Flashcards (phân chia nhóm môn học rõ rệt), Cấu hình (AI & SQL Server).
   - Flashcards tách riêng theo từng môn (`PRN211`, `AI301`, `Chung`...), có thanh chọn nhóm (Chips), bộ đếm và tiến độ riêng cho từng môn.
3. **Chạy trên 1 CSDL đơn giản với SQL Server Management Studio (SSMS)**:
   - Database: `SmartStudyDb` trên Microsoft SQL Server (`Server=localhost;Database=SmartStudyDb;Integrated Security=True;TrustServerCertificate=True;`).
   - Cung cấp file script `database_setup.sql` chuẩn để mở trong SSMS chạy F5.
   - Ứng dụng tự động kiểm tra và khởi tạo database, bảng, dữ liệu mẫu nếu chưa có.
4. **Hiệu suất cao, không treo đơ**:
   - RAG TF-IDF Vector siêu tốc (< 0.1s), lập chỉ mục slide tức thì, trích dẫn chuẩn `[Slide X - TênFile]`.
   - Giao diện Light Theme sáng sủa, thanh lịch, chống AI slop.

---

## Phases & Tasks

### Phase 1: Cơ sở Dữ liệu SQL Server (SSMS) & Cấu hình Đơn giản
- [ ] T1: Tạo script SQL `database_setup.sql` chuẩn SSMS (Tạo DB `SmartStudyDb`, các bảng Documents, Chunks, Flashcards, Sessions, Messages, Settings).
- [ ] T2: Chạy script khởi tạo `SmartStudyDb` trên SQL Server localhost và nạp dữ liệu mẫu ban đầu.
- [ ] T3: Triển khai `SqlServerDatabaseService.cs` bằng `Microsoft.Data.SqlClient` đơn giản, trực tiếp, ít code.

### Phase 2: Core Models, Document Parsers, RAG Engine & AI Coordinator
- [ ] T4: Xây dựng các Models tinh gọn (`DocumentItem`, `DocumentChunk`, `Flashcard`, `ChatSession`, `ChatMessage`, `CitationItem`).
- [ ] T5: Triển khai Document Parsers (`PdfParser`, `DocxParser`, `TxtParser`) và `TextChunker` phân trang/slide rõ ràng.
- [ ] T6: Triển khai `SimpleRagEngine` (TF-IDF Vector Cosine Similarity siêu tốc, không bị nghẽn mạng).
- [ ] T7: Triển khai `SimpleAiCoordinator` hỗ trợ 4 nền tảng (Gemini, OpenAI, Claude, Groq/Ollama) với logic gọi HTTP rõ ràng, đơn giản.

### Phase 3: Giao diện Người Dùng WPF (MVVM) Chia Mục Rõ Ràng & Sáng Sủa
- [ ] T8: Thiết lập Hệ thống Màu sắc & Resource Dictionary (Light Theme: Background `#F8FAFC`, Card `#FFFFFF`, Text `#0F172A`, Primary `#2563EB`).
- [ ] T9: Xây dựng `MainWindow.xaml` với Sidebar 4 tab gọn gàng, điều hướng bằng `ContentControl` DataTemplates.
- [ ] T10: Triển khai `ChatView.xaml` & `ChatViewModel.cs` (Chat hỏi đáp, trích dẫn Slide, chip câu hỏi mẫu).
- [ ] T11: Triển khai `DocumentsView.xaml` & `DocumentsViewModel.cs` (Kéo thả slide, lập chỉ mục tức thì, nút nạp bài giảng mẫu).
- [ ] T12: Triển khai `FlashcardsView.xaml` & `FlashcardsViewModel.cs` (Phân chia nhóm môn học rõ rệt, chip chọn nhóm, tiến độ & bộ đếm chuẩn xác, lật thẻ, chấm điểm Spaced Repetition, AI sinh thẻ).
- [ ] T13: Triển khai `SettingsView.xaml` & `SettingsViewModel.cs` (Cấu hình API Key, ping kiểm tra, cấu hình chuỗi kết nối SQL Server SSMS).

### Phase 4: Kiểm Thử Tự Động & Bàn Giao
- [ ] T14: Viết và chạy bộ Unit Tests xác thực RAG, Flashcard Grouping và SQL Server.
- [ ] T15: Cập nhật tài liệu hướng dẫn mở database trên SQL Server Management Studio (SSMS).
