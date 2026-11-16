# DESIGN.md - Design System & UI/UX Guidelines

## 1. Design Philosophy
- **Anti-Slop**: Không sử dụng các gradient màu tím/hồng neon ngẫu nhiên, không hiệu ứng phát sáng mờ ảo gây xao nhãng.
- **Content-Focused**: Mọi diện tích màn hình đều phục vụ mục đích đọc tài liệu, hỏi đáp và ghi nhớ bài học.
- **Fluent Design & Dark Surface**: Sử dụng bảng màu Slate/Zinc hiện đại, giảm mỏi mắt cho sinh viên khi ôn thi ban đêm.
- **Predictable Interactions**: Các nút bấm phẳng (flat), bo góc nhẹ (6-12px), chỉ dẫn trạng thái rõ ràng (Enabled/Disabled/Hover/Focus).

## 2. Color Palette & Design Tokens

```xaml
<!-- Background & Surface (Light Theme) -->
BackgroundBrush:       #F8FAFC  (Slate 50 - Nền sáng nhẹ dịu mắt, chống chói)
SurfaceBrush:          #FFFFFF  (Pure White - Khung sidebar, thẻ card nổi bật)
SurfaceAltBrush:       #F1F5F9  (Slate 100 - Ô nhập liệu, vùng thứ cấp)
SurfaceBorderBrush:    #E2E8F0  (Slate 200 - Đường viền phân cách tinh tế)

<!-- Brand & Accents -->
PrimaryBrush:          #2563EB  (Blue 600 - Nút chính, thương hiệu chuẩn)
PrimaryHoverBrush:     #1D4ED8  (Blue 700 - Trạng thái rê chuột)
AccentSuccessBrush:    #059669  (Emerald 600 - Thẻ dễ, hoàn thành môn, ping thành công)
AccentWarningBrush:    #D97706  (Amber 600 - Thẻ vừa, cảnh báo kết nối)
AccentDangerBrush:     #DC2626  (Red 600 - Thẻ khó, nút xóa tài liệu)

<!-- Typography Colors -->
TextPrimaryBrush:      #0F172A  (Slate 900 - Tiêu đề, chữ đen than sắc nét, tương phản > 12:1)
TextSecondaryBrush:    #475569  (Slate 600 - Phụ đề, nhãn mô tả)
TextMutedBrush:        #64748B  (Slate 500 - Timestamp, gợi ý)

<!-- RAG Citation Badges -->
CitationBadgeBrush:    #DBEAFE  (Blue 100 - Nền huy hiệu trích dẫn slide dịu mắt)
CitationBadgeTextBrush:#1D4ED8  (Blue 700 - Chữ huy hiệu trích dẫn slide rõ nét)
```

## 3. Typography
- **Font Family**: Segoe UI Variable, Segoe UI, sans-serif.
- **Monospace**: Consolas, Cascadia Code (dùng cho đoạn trích code, API key, đồng hồ đếm giờ Pomodoro).
- **Type Scale**:
  - `Header 1`: 20pt, SemiBold, Foreground `#F8FAFC`
  - `Header 2`: 16pt, SemiBold, Foreground `#F8FAFC`
  - `Header 3`: 13pt, Medium, Foreground `#94A3B8` (Caps)
  - `Body Text`: 13.5pt, Regular, Line-height thoải mái
  - `Caption`: 11pt, Regular, Foreground `#94A3B8`

## 4. UI Layout Hierarchy
1. **Sidebar Trái (Width: 240px)**:
   - Logo & Tên ứng dụng (SmartStudy).
   - 6 Tabs chuyển màn hình: Chat & RAG, Tài Liệu & Slide, Flashcards, Pomodoro, Tiến Độ, Cấu Hình.
   - Hộp điều khiển chuyển đổi nhanh nền tảng AI (Gemini, OpenAI, Claude, Groq/Ollama).
2. **Khu vực Nội dung Chính (Column 1)**:
   - Tiêu đề màn hình và phụ đề hướng dẫn.
   - Nội dung tương tác dạng thẻ Card phân tầng rõ rệt.
   - Luôn có chỉ báo trạng thái xử lý (ProgressBar, Status Text) khi làm việc với tệp lớn.

## 5. Accessibility (WCAG 2.1 AA)
- Độ tương phản chữ trắng `#F8FAFC` trên nền tối `#1E293B` đạt tỷ lệ `> 10:1` (vượt xa chuẩn AA yêu cầu 4.5:1).
- Mọi trường nhập liệu đều hỗ trợ phím tắt (`Ctrl + Enter` để gửi tin nhắn chat, phím bấm lật thẻ).
- Kích thước vùng bấm chuột (Touch/Click Target) tối thiểu 36x36px.
