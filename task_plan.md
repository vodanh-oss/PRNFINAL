# Task Plan: Smart Study Assistant (WPF App + RAG + Multi-AI + Simple SQLite DB)

## Goal
Tái cấu trúc và hoàn thiện toàn diện dự án **Trợ lý Ôn thi & Học tập Sinh viên (Smart Study Assistant)** theo tiêu chuẩn:
- **Đơn giản & Đầy đủ**: Không phức tạp hóa, không code thừa, đầy đủ toàn bộ chức năng theo yêu cầu.
- **Không màu mè, nhưng vẫn chuyên nghiệp**: Giao diện Fluent Dark cao cấp, chống AI slop, rõ ràng, dễ tiếp cận.
- **Thao tác người dùng cực kỳ dễ hiểu**: Tối đa 1-2 click cho mọi tác vụ, có hướng dẫn nhanh và dữ liệu mẫu nạp sẵn.
- **1 Cơ sở dữ liệu SQLite đơn giản**: `study_assistant.db` duy nhất, lưu trữ lịch sử chat, slide chunks, flashcards, môn học, pomodoro và ghi chú. Có nút mở thư mục DB trực tiếp.

---

## Phases & Tasks

### Phase 1: Chuẩn hóa Đặc tả (Spec-Kit) & Thiết kế UI/UX (Impeccable)
- [x] T101: Cập nhật Hiến pháp & Đặc tả chức năng (`specs/001-smart-study-assistant/spec.md`).
- [x] T102: Lập tài liệu thiết kế sản phẩm và hệ thống giao diện (`PRODUCT.md` & `DESIGN.md`) theo chuẩn Impeccable & UI/UX Pro Max.
- [x] T103: Chuẩn hóa bản vẽ kiến trúc kỹ thuật (`specs/001-smart-study-assistant/plan.md`) và danh mục công việc (`specs/001-smart-study-assistant/tasks.md`).

### Phase 2: Kiến trúc CSDL SQLite Đơn Giản & Dịch vụ Nền tảng (Core)
- [x] T104: Tinh gọn và tối ưu hóa `SqliteDatabaseService` (1 file `study_assistant.db`, tự động nạp dữ liệu mẫu ban đầu, API đơn giản, đường dẫn minh bạch).
- [x] T105: Đảm bảo bộ máy RAG (`TextChunker` + `VectorRagEngine`) trích xuất PDF/DOCX/TXT chính xác theo từng Slide/Trang, hỗ trợ cả vector online và TF-IDF offline fallback.
- [x] T106: Tinh giản bộ điều phối AI (`AiServiceCoordinator`) với 4 nền tảng (Gemini, OpenAI, Claude, Groq/Ollama) và bảo mật Windows DPAPI.

### Phase 3: Giao diện Người Dùng Chuyên Nghiệp, Thao Tác Dễ Hiểu (WPF MVVM)
- [x] T107: Màn hình Chat & RAG (`ChatView`): Bảng hướng dẫn bắt đầu nhanh, chip câu hỏi mẫu, huy hiệu trích dẫn `[Slide X - TenFile.pdf]`, modal xem trích đoạn slide bài giảng.
- [x] T108: Màn hình Tài Liệu & Slide (`DocumentsView`): Kéo-thả slide, nút nạp bài giảng mẫu 1-click, thanh tiến trình xử lý, danh sách chunks rõ ràng.
- [x] T109: Màn hình Flashcards (`FlashcardsView`): Lật thẻ mượt mà, nút lật thẻ rõ ràng, chấm điểm ôn tập ngắt quãng (1-3-7 ngày), AI sinh thẻ tự động.
- [x] T110: Màn hình Pomodoro (`PomodoroView`): Đồng hồ to rõ ràng, 3 chế độ (25p/5p/15p), 1 nút Bắt đầu/Tạm dừng, thống kê trong ngày.
- [x] T111: Màn hình Tiến Độ & Ghi Chú (`ProgressView`): Checklist môn học với thanh tiến độ % trực quan, sổ tay ghi chú lưu từ đoạn chat.
- [x] T112: Màn hình Cấu Hình Nền Tảng (`SettingsView`): Quản lý API Key, nút kiểm tra kết nối (Ping), thẻ quản lý SQLite DB với nút mở thư mục file DB.

### Phase 4: Kiểm Thử Tự Động (Superpowers TDD) & Đồ Thị Tri Thức (Graphify)
- [x] T113: Viết bộ unit tests hoàn chỉnh trong `SmartStudyAssistant.Tests` (Database, Chunker, Vector similarity, DPAPI security, Pomodoro timer).
- [x] T114: Chạy kiểm thử tự động đạt 100% pass (Verification Loop).
- [x] T115: Cập nhật Knowledge Graph với `graphify update` và xuất báo cáo.
- [x] T116: Lưu trữ mã nguồn chuẩn Git Conventional Commits.
