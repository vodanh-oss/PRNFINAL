# Operational Guidelines & Installed Skills Protocol

Luôn ưu tiên và chủ động kích hoạt các skill và công cụ/agent đã được cài đặt trong hệ thống khi thực hiện bất kỳ nhiệm vụ nào:

## 1. Specification-Driven Development (Spec Kit)
- Áp dụng quy trình **Spec-Driven Development** (SDD) cho các tính năng mới hoặc thay đổi lớn trong dự án:
  - Bắt đầu với `/speckit-specify` để làm rõ yêu cầu chức năng trước khi code.
  - Sử dụng `/speckit-plan` để lên bản vẽ thiết kế kiến trúc và layout file.
  - Sử dụng `/speckit-tasks` để chia nhỏ thành danh sách task có thứ tự phụ thuộc.
  - Sử dụng `/speckit-implement` để thực thi từng task một cách có kiểm soát và chống lỗi hồi quy.
  - Tuân thủ hiến pháp dự án (`/speckit-constitution`).

## 2. Codebase Understanding & Architecture (Graphify)
- Khi tìm hiểu cấu trúc dự án, điều tra luồng dữ liệu, phân tích quan hệ giữa các module hoặc trả lời câu hỏi về mã nguồn:
  - Sử dụng `/graphify` để trích xuất AST và tạo đồ thị tri thức (Knowledge Graph).
  - Dùng `/graphify query "<câu hỏi>"` hoặc `/graphify path` để định vị chính xác mối liên hệ giữa các thành phần thay vì đọc thủ công rời rạc.

## 3. UI/UX & Frontend Excellence (Impeccable & UI/UX Pro Max)
- Khi xây dựng, chỉnh sửa hoặc đánh giá giao diện người dùng (Web, Mobile, App):
  - Áp dụng các nguyên tắc của **Impeccable** (`/impeccable`):
    - Khởi tạo context: `/impeccable init` (lưu vào `PRODUCT.md` và `DESIGN.md`).
    - Đánh giá chất lượng & UX: `/impeccable critique`, `/impeccable audit`.
    - Tinh chỉnh thẩm mỹ & triệt tiêu AI slop: `/impeccable polish`, `/impeccable bolder`, `/impeccable layout`, `/impeccable typeset`.
  - Kết hợp với **UI/UX Pro Max** để đảm bảo chuẩn accessibility (WCAG), hệ thống màu sắc và layout responsive.

## 4. Kỹ nghệ phần mềm & Kiểm thử (Superpowers & Everything Gemini CLI)
- Áp dụng quy trình **Test-Driven Development (TDD)** (Red -> Green -> Refactor) kết hợp `tdd-workflow`.
- Chạy vòng lặp kiểm tra chất lượng tự động với `verification-loop`.
- Rà soát bảo mật chặt chẽ (`security-review`): kiểm tra Secret, Token, input validation, SQL/Injection, quyền truy cập.
- Tuân thủ chuẩn mực code (`coding-standards`) và kiến trúc tầng (`backend-patterns`, `frontend-patterns`).
- Commit git có cấu trúc chuẩn Conventional Commits.

## 5. Quản lý tiến độ & Tracks (Conductor & Planning with Files)
- Quản lý lifecycle dự án theo từng track tính năng với **Conductor**:
  - `/conductor-setup`: Khởi tạo môi trường track context.
  - `/conductor-new-track`: Lập spec và plan chi tiết cho tính năng mới / bug fix.
  - `/conductor-implement`: Triển khai các task trong plan theo thứ tự phụ thuộc.
  - `/conductor-review` & `/conductor-status`: Đánh giá chất lượng bàn giao và theo dõi tiến độ tổng thể.
- Với các tác vụ phức tạp gồm nhiều bước, duy trì tracking rõ ràng qua `task_plan.md`, `findings.md`, `progress.md`.

## 6. Mở rộng Kỹ năng & Extensions (Skills Manager & Gemini CLI)
- Chủ động tra cứu, bổ sung hoặc cập nhật các skill chuyên biệt từ Skill Vault qua `antigravity-skills-manager` (`npx @rmyndharis/antigravity-skills`).
- Tận dụng hệ sinh thái extensions và MCP servers của Gemini CLI (`gemini extensions`) để tích hợp công cụ hỗ trợ công việc.
