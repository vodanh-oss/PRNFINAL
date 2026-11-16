# Progress Tracking: Smart Study Assistant

## Current Status: Completed & Verified (All 16 Tasks T101 - T116 Done)

### Completed Tasks
- [x] **Fix Navigation Glitch**: Sửa lỗi không chuyển/thoát được màn hình Cấu hình. Nguyên nhân do 6 UserControl bị xếp chồng trong cùng ô Grid và gán DataContext trực tiếp làm hỏng binding Visibility. Đã chuyển đổi sang kiến trúc chuẩn `ContentControl Content="{Binding CurrentView}"` với DataTemplates, đảm bảo chuyển tab 100% mượt mà, dứt khoát.
- [x] **T101**: Cập nhật Hiến pháp & Đặc tả chức năng (`specs/001-smart-study-assistant/spec.md`).
- [x] **T102**: Lập tài liệu thiết kế sản phẩm và hệ thống giao diện (`PRODUCT.md` & `DESIGN.md`) theo chuẩn Impeccable & UI/UX Pro Max.
- [x] **T103**: Chuẩn hóa bản vẽ kiến trúc kỹ thuật (`specs/001-smart-study-assistant/plan.md`) và danh mục công việc (`specs/001-smart-study-assistant/tasks.md`).
- [x] **T104**: Quản lý duy nhất 1 tệp SQLite Database (`study_assistant.db`), hiển thị đường dẫn tệp minh bạch trên UI kèm nút mở thư mục file DB trực tiếp.
- [x] **T105**: Bộ máy RAG (`TextChunker` + `VectorRagEngine`) trích xuất PDF/DOCX/TXT chính xác theo từng Slide/Trang, hỗ trợ cả vector online và TF-IDF offline fallback.
- [x] **T106**: Bộ điều phối AI (`AiServiceCoordinator`) với 4 nền tảng (Gemini, OpenAI, Claude, Groq/Ollama) và bảo mật Windows DPAPI.
- [x] **T107**: Màn hình Chat & RAG (`ChatView`): Bảng hướng dẫn bắt đầu nhanh, chip câu hỏi mẫu, huy hiệu trích dẫn `[Slide X - TenFile.pdf]`, modal xem trích đoạn slide bài giảng.
- [x] **T108**: Màn hình Tài Liệu & Slide (`DocumentsView`): Kéo-thả slide, nút nạp bài giảng mẫu 1-click, thanh tiến trình xử lý, danh sách chunks rõ ràng.
- [x] **T109**: Màn hình Flashcards (`FlashcardsView`): Lật thẻ mượt mà, nút lật thẻ rõ ràng, chấm điểm ôn tập ngắt quãng (1-3-7 ngày), AI sinh thẻ tự động.
- [x] **T110**: Màn hình Pomodoro (`PomodoroView`): Đồng hồ to rõ ràng, 3 chế độ (25p/5p/15p), 1 nút Bắt đầu/Tạm dừng, thống kê trong ngày.
- [x] **T111**: Màn hình Tiến Độ & Ghi Chú (`ProgressView`): Checklist môn học với thanh tiến độ % trực quan, sổ tay ghi chú lưu từ đoạn chat.
- [x] **T112**: Màn hình Cấu Hình Nền Tảng (`SettingsView`): Quản lý API Key, nút kiểm tra kết nối (Ping), thẻ quản lý SQLite DB với nút mở thư mục file DB.
- [x] **T113**: Viết bộ unit tests hoàn chỉnh trong `SmartStudyAssistant.Tests` (`DatabaseServiceTests.cs`, `TextChunkerTests.cs`, `VectorSimilarityTests.cs`, `SecurityStorageTests.cs`, `PomodoroTimerTests.cs`).
- [x] **T114**: Chạy kiểm thử tự động đạt 100% pass (11/11 tests pass).
- [x] **T115**: Cập nhật Knowledge Graph với `graphify update` (1065 nodes, 1952 edges).
- [x] **T116**: Lưu trữ mã nguồn chuẩn Git Conventional Commits.
