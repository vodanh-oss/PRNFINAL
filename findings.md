# Technical Findings & Architectural Decisions

## 1. Single Simple SQLite Database (`study_assistant.db`)
- **Vị trí**: Lưu trữ tại `%LOCALAPPDATA%\SmartStudyAssistant\study_assistant.db`.
- **Ưu điểm**:
  - Không cần thiết lập máy chủ CSDL phức tạp (như SQL Server hay Docker PostgreSQL).
  - Tự động tạo thư mục và tệp DB khi chạy lần đầu.
  - Hỗ trợ đầy đủ Cascade Delete giữa Documents -> DocumentChunks và ChatSessions -> ChatMessages.
  - Tích hợp tính năng Seeding tự động: Khi CSDL mới tinh, tự động nạp 1 đề cương bài giảng (4 slide), 2 môn học kèm checklist, 5 thẻ flashcards, và 1 ghi chú ôn thi.
  - Cung cấp nút trực tiếp trên UI để mở thư mục chứa DB trong Windows Explorer.

## 2. RAG Engine Tiện Lợi & Khả Năng Hoạt Động Offline (Resilient RAG)
- Trích xuất tài liệu:
  - PDF: Sử dụng `UglyToad.PdfPig` đọc từng trang/slide độc lập để giữ nguyên ngữ cảnh `PageOrSlideNumber`.
  - DOCX: Sử dụng `DocumentFormat.OpenXml` duyệt các đoạn văn (`Paragraph`).
  - TXT: Đọc theo luồng đoạn văn bản.
- Tìm kiếm & Khớp dữ liệu:
  - Khi có mạng & API Key: Sử dụng Cosine Similarity trên dense vectors tạo từ AI Embeddings.
  - Khi offline hoặc không có API Key: Tự động fallback sang thuật toán **Lexical TF-IDF Vector Cosine Similarity**, đảm bảo 100% tính năng tra cứu slide vẫn hoạt động mượt mà.
  - Trích dẫn: Tự động đính kèm `[Slide X - TenFile.pdf]` vào phản hồi của AI. Người dùng bấm vào huy hiệu để xem nguyên văn đoạn trích.

## 3. Quản Lý Đa Nền Tảng AI & Bảo Mật Khóa API
- Hỗ trợ 4 loại AI: Google Gemini, OpenAI, Anthropic Claude, và Groq/Ollama.
- Bảo mật: Tuyệt đối không lưu plain-text API key. Sử dụng Windows DPAPI (`ProtectedData.Protect` với `DataProtectionScope.CurrentUser`), mã hóa trực tiếp theo tài khoản người dùng Windows.
- Chuyển đổi nhanh (Hot-Swap) trực tiếp từ thanh điều hướng bên trái mà không cần khởi động lại ứng dụng.

## 4. Thiết Kế Giao Diện Tối Giản, Chuyên Nghiệp (Impeccable & UI/UX Pro Max)
- Bảng màu: Nền Slate Dark (`#0F172A`), Surface (`#1E293B`), Border (`#334155`), Primary Blue (`#3B82F6`), Success Green (`#10B981`).
- Loại bỏ hoàn toàn AI Slop: Không sử dụng các dải màu tím/hồng ngẫu nhiên, không hiệu ứng lóa mắt gây mất tập trung khi ôn thi.
- Thao tác 1-chạm: Mọi tính năng cốt lõi (Gửi câu hỏi, Lật thẻ flashcard, Bắt đầu Pomodoro, Tick checklist, Đổi AI) đều thực hiện bằng 1 cú click chuột.
