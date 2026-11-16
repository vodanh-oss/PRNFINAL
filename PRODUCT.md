# PRODUCT.md - Smart Study Assistant (Trợ Lý Ôn Thi & Học Tập Sinh Viên)

## 1. Product Vision
Trợ lý Ôn thi & Học tập Sinh viên là ứng dụng máy tính (Desktop App) trên nền tảng .NET 8 WPF, được thiết kế chuyên biệt để giúp sinh viên đại học ôn tập hiệu quả trước các kỳ thi. Ứng dụng kết hợp sức mạnh của các mô hình AI tiên tiến (Gemini, OpenAI, Claude, Groq/Ollama) với kỹ thuật RAG (Retrieval-Augmented Generation) tra cứu trực tiếp từ slide bài giảng và đề cương nội bộ của môn học.

## 2. Target Audience
- Sinh viên các trường đại học (đặc biệt là sinh viên công nghệ thông tin, kỹ thuật, kinh tế).
- Giảng viên hoặc trợ giảng cần công cụ tóm tắt bài giảng và tạo bộ câu hỏi ôn tập, thẻ flashcard nhanh chóng.

## 3. Core Principles
1. **Đơn giản & Trực quan (Simplicity First)**: Không đòi hỏi người dùng phải cấu hình phức tạp. Mọi thao tác cốt lõi chỉ cần 1 đến 2 cú click.
2. **Không màu mè, Chuyên nghiệp (Distraction-Free)**: Giao diện chuẩn Fluent Dark Mode, tập trung cao độ vào việc đọc hiểu kiến thức, triệt tiêu mọi hiệu ứng màu mè lòe loẹt ("AI slop").
3. **Minh bạch & Dễ kiểm chứng (Verifiable Citations)**: Mọi câu trả lời của AI dựa trên bài giảng đều đính kèm chính xác số trang/slide `[Slide X - TenFile.pdf]`, cho phép người dùng click để đối chiếu trực tiếp văn bản gốc.
4. **Cơ sở dữ liệu Đơn lẻ (Single SQLite Database)**: Lưu trữ gọn gàng trong 1 tệp `study_assistant.db`, dễ sao lưu, di chuyển và kiểm tra.

## 4. Feature Matrix

### A. AI Chat & RAG
- Trò chuyện giải đáp kiến thức học thuật với AI.
- Kéo-thả tài liệu (PDF, DOCX, TXT) vào ứng dụng để lập chỉ mục.
- Tìm kiếm ngữ nghĩa bằng Cosine Similarity trên Vector Embeddings hoặc thuật toán TF-IDF khi offline.
- Tự động trích dẫn số trang/slide và hiển thị modal xem đoạn trích gốc.
- Mẫu câu lệnh 1-chạm: Tóm tắt bài học, Giải thích khái niệm, Dịch thuật học thuật, Tạo 5 câu trắc nghiệm, Tạo Flashcards.

### B. Hoán chuyển Nền tảng AI (Multi-AI Hot Swap)
- Hỗ trợ 4 nhà cung cấp: Google Gemini, OpenAI, Anthropic Claude, Groq & Ollama (Local LLM).
- Chuyển đổi nhà cung cấp tức thời ngay trên giao diện chính.
- Mã hóa an toàn khóa API bằng Windows DPAPI (`ProtectedData`).

### C. Công cụ Năng suất Học tập
- **Đồng hồ Pomodoro**: 25 phút học tập trung, 5 phút nghỉ ngắn, 15 phút nghỉ dài; thống kê số phiên hoàn thành trong ngày.
- **Thẻ Flashcards**: Ôn tập ngắt quãng (Spaced Repetition) với 3 mức độ đánh giá (Khó, Tốt, Dễ); hỗ trợ AI tự động sinh thẻ từ bài học.
- **Tiến độ Môn học & Checklist**: Theo dõi mục tiêu điểm số, tích hợp checklist đề cương tự động tính % tiến độ.
- **Sổ tay Ghi chú**: Lưu lại nhanh chóng các câu trả lời hay từ AI hoặc tự ghi chép.
