# Feature Specification: Smart Study Assistant (WPF .NET 8)

**Feature Branch**: `001-smart-study-assistant`  
**Created**: 2026-11-16  
**Status**: Ready for Planning  
**Target Architecture**: WPF (.NET 8.0, MVVM via CommunityToolkit.Mvvm, SQLite, DPAPI)

---

## 1. Executive Summary & Objective
Smart Study Assistant là ứng dụng desktop hiện đại dành cho sinh viên, tích hợp công nghệ AI đa nền tảng (OpenAI, Gemini, Claude, Groq/Ollama) kết hợp công nghệ RAG (Retrieval-Augmented Generation) tìm kiếm nội bộ từ tài liệu bài giảng (PDF, DOCX, TXT) và bộ công cụ tăng năng suất học tập (Pomodoro, Flashcards, Checklist môn học, Ghi chú).

---

## 2. User Scenarios & Prioritized User Stories

### User Story 1 - Cấu hình & Hoán chuyển Nền tảng AI Đa kênh (Priority: P1)
Sinh viên có thể cấu hình API Key của nhiều nhà cung cấp AI (Google Gemini, OpenAI, Anthropic Claude, Groq, Ollama cục bộ) và chuyển đổi linh hoạt ngay trên thanh điều khiển hoặc màn hình Settings mà không làm mất lịch sử hội thoại.
- **Acceptance Scenario 1.1**: Sinh viên mở tab Settings, chọn Gemini, nhập API Key, nhấn "Kiểm tra kết nối". Hệ thống gửi ping kiểm tra và hiển thị trạng thái hợp lệ màu xanh.
- **Acceptance Scenario 1.2**: API Key được mã hóa tự động bằng Windows DPAPI trước khi lưu vào SQLite/AppConfig.
- **Acceptance Scenario 1.3**: Trong màn hình Chat, sinh viên đổi từ "Gemini 1.5 Flash" sang "Claude 3.5 Sonnet", câu hỏi tiếp theo được gửi tới Claude trong khi ngữ cảnh trước đó vẫn giữ nguyên.

### User Story 2 - RAG: Tra cứu & Chat trên Slide/Đề cương Bài giảng (Priority: P1)
Sinh viên kéo thả slide bài giảng (PDF), đề cương (DOCX, TXT) vào ứng dụng. AI lập chỉ mục nội dung, trả lời câu hỏi dựa trên tài liệu và trích dẫn số trang/slide cụ thể.
- **Acceptance Scenario 2.1**: Sinh viên kéo-thả file `HeDieuHanh_Chuong3.pdf` vào vùng Document Manager. Ứng dụng hiển thị tiến trình phân trang và chunking (ví dụ: "Đã xử lý 45 slide, tạo 120 chunks").
- **Acceptance Scenario 2.2**: Sinh viên hỏi: *"Thuật toán lập lịch Round Robin hoạt động thế nào theo bài giảng?"*. AI trả lời chính xác kèm citation badge: `[Slide 18 - HeDieuHanh_Chuong3.pdf]`.
- **Acceptance Scenario 2.3**: Khi click vào citation badge, một popup nhỏ hiển thị đoạn trích nguyên văn tương ứng từ slide đó.
- **Acceptance Scenario 2.4**: Khi không có kết nối embedding API, hệ thống hỗ trợ cơ chế Hybrid BM25/Cosine fallback để sinh viên vẫn tra cứu tài liệu được ngay lập tức.

### User Story 3 - Trợ lý AI Đa năng (Giải đáp, Tóm tắt, Dịch thuật) (Priority: P2)
Sinh viên tương tác với chatbot AI có hỗ trợ streaming, hiển thị định dạng Markdown, công thức toán và code block.
- **Acceptance Scenario 3.1**: Sinh viên gõ câu hỏi hoặc chọn prompt mẫu (Tóm tắt bài học, Dịch thuật Anh-Việt, Giải thích khái niệm).
- **Acceptance Scenario 3.2**: Phản hồi được stream mượt mà từng từ kèm nút "Sao chép" và "Tạo thành Ghi chú".
- **Acceptance Scenario 3.3**: Quản lý nhiều phiên chat (Session): tạo mới, đổi tên, xóa, tìm kiếm trong lịch sử chat.

### User Story 4 - Bộ công cụ Năng suất Sinh viên (Pomodoro, Flashcards, Checklist) (Priority: P2)
Hỗ trợ quản lý thời gian và củng cố kiến thức ôn thi:
- **Acceptance Scenario 4.1 (Pomodoro)**: Bộ đếm giờ 25p học / 5p nghỉ với giao diện đồng hồ tối giản, đếm ngược chính xác, phát chuông thông báo và ghi nhận thống kê số phiên hoàn thành.
- **Acceptance Scenario 4.2 (Flashcards)**: Quản lý bộ thẻ theo môn học. Hỗ trợ lật thẻ (Front/Back), đánh giá mức độ ghi nhớ (Khó / Trung bình / Dễ). Tích hợp nút "Tạo Flashcard tự động từ tài liệu/chat bằng AI".
- **Acceptance Scenario 4.3 (Checklist Môn học)**: Danh sách môn học với các đầu mục cần ôn tập (Chương, bài tập, đề cương), thanh tiến độ phần trăm tự động cập nhật khi tích chọn hoàn thành.
- **Acceptance Scenario 4.4 (Ghi chú học tập)**: Trình soạn thảo ghi chú nhanh theo môn, hỗ trợ lưu trực tiếp các ý quan trọng từ chat RAG.

---

## 3. Detailed Functional Requirements

### AI Provider Engine
- **FR-001**: Hệ thống PHẢI định nghĩa interface `IAiProvider` với các phương thức `SendMessageAsync`, `StreamMessageAsync`, `GenerateEmbeddingAsync`, và `TestConnectionAsync`.
- **FR-002**: Hệ thống PHẢI cài đặt sẵn adapter cho:
  - `GeminiProvider`: Google Generative AI API (Gemini 1.5 Flash / Pro, text-embedding-004)
  - `OpenAiProvider`: OpenAI Chat Completion API (GPT-4o, GPT-4o-mini, text-embedding-3-small)
  - `ClaudeProvider`: Anthropic Messages API (Claude 3.5 Sonnet / Haiku)
  - `GroqOllamaProvider`: OpenAI-compatible endpoint (Groq Llama 3.3 70B, Ollama localhost:11434)
- **FR-003**: Hệ thống PHẢI mã hóa bảo mật toàn bộ API Key bằng Windows Data Protection API (`DPAPI`).

### Document & RAG Pipeline
- **FR-004**: Hỗ trợ đọc tài liệu PDF qua `PdfPig` (trích xuất text theo từng trang/slide), DOCX qua `DocumentFormat.OpenXml`, và file TXT UTF-8.
- **FR-005**: Phân đoạn tài liệu (Chunking) theo kích thước cấu hình (mặc định 600 ký tự, overlap 100 ký tự) gắn kèm metadata: `DocumentId`, `DocumentName`, `PageOrSlideNumber`, `ChunkIndex`.
- **FR-006**: Tính toán vector similarity theo Cosine Similarity giữa câu hỏi và danh sách chunk; trích xuất Top-K chunks liên quan nhất đưa vào System Prompt của AI.
- **FR-007**: Hệ thống PHẢI trích xuất nguồn trích dẫn rõ ràng trong câu trả lời và cho phép người dùng click để xem đoạn văn bản nguồn.

### User Interface & Experience
- **FR-008**: Giao diện WPF hiện đại, hỗ trợ Dark Mode và Light Mode, tuân thủ nguyên tắc Impeccable (loại bỏ gradient sến, sử dụng typography chuẩn Segoe UI / Inter, khoảng cách nhịp nhàng).
- **FR-009**: Sidebar điều hướng mượt mà giữa các phân hệ:
  1. `AI Chat & RAG` (Trò chuyện & Tra cứu)
  2. `Tài liệu môn học` (Document Library & Chunk Index)
  3. `Thẻ ghi nhớ (Flashcards)`
  4. `Đồng hồ Pomodoro`
  5. `Tiến độ & Checklist`
  6. `Cài đặt AI & Ứng dụng`
- **FR-010**: Toàn bộ thao tác I/O tài liệu và gọi API AI PHẢI chạy bất đồng bộ (`async/await`) kèm chỉ báo trạng thái (Spinner/Progress bar), không bao giờ gây đơ giao diện UI.

---

## 4. Edge Cases & Error Handling
- **EC-001 (Mất mạng / Lỗi API Key)**: Hiển thị banner thông báo lỗi thân thiện, gợi ý kiểm tra lại API Key hoặc đổi sang Ollama cục bộ.
- **EC-002 (File PDF scan dạng ảnh)**: Nếu PDF không có text layer (ảnh scan), hệ thống thông báo cho người dùng tài liệu cần OCR thay vì nạp chuỗi rỗng.
- **EC-003 (Tài liệu quá lớn)**: Xử lý chunking theo luồng batch bất đồng bộ, có nút hủy (CancellationToken) nếu sinh viên không muốn tiếp tục xử lý.
- **EC-004 (Không chọn tài liệu nào khi bật chế độ RAG)**: Tự động chuyển về chế độ chat thông thường và thông báo nhẹ "Chưa có tài liệu nào được chọn để tra cứu".
