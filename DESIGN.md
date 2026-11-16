# DESIGN.md - Design System & UI/UX Guidelines

## 1. Design Philosophy
- **Anti-Slop**: Không sử dụng các gradient màu tím/hồng neon ngẫu nhiên, không hiệu ứng phát sáng mờ ảo gây xao nhãng.
- **Content-Focused**: Mọi diện tích màn hình đều phục vụ mục đích đọc tài liệu, hỏi đáp và ghi nhớ bài học.
- **Fluent Design & Dark Surface**: Sử dụng bảng màu Slate/Zinc hiện đại, giảm mỏi mắt cho sinh viên khi ôn thi ban đêm.
- **Predictable Interactions**: Các nút bấm phẳng (flat), bo góc nhẹ (6-12px), chỉ dẫn trạng thái rõ ràng (Enabled/Disabled/Hover/Focus).

## 2. Color Palette & Design Tokens

```xaml
<!-- Background & Surface -->
BackgroundBrush:       #0F172A  (Slate 900 - Nền chính cửa sổ)
SurfaceBrush:          #1E293B  (Slate 800 - Khung sidebar, thẻ card)
SurfaceAltBrush:       #334155  (Slate 700 - Ô nhập liệu, vùng thứ cấp)
SurfaceBorderBrush:    #475569  (Slate 600 - Đường viền phân cách)

<!-- Brand & Accents -->
PrimaryBrush:          #3B82F6  (Blue 500 - Nút chính, chỉ báo hoạt động)
PrimaryHoverBrush:     #60A5FA  (Blue 400 - Trạng thái rê chuột)
AccentSuccessBrush:    #10B981  (Emerald 500 - Thẻ dễ, hoàn thành môn, ping thành công)
AccentWarningBrush:    #F59E0B  (Amber 500 - Thẻ vừa, cảnh báo kết nối)
AccentDangerBrush:     #EF4444  (Rose 500 - Thẻ khó, nút xóa tài liệu)

<!-- Typography Colors -->
TextPrimaryBrush:      #F8FAFC  (Slate 50 - Tiêu đề, nội dung chính)
TextSecondaryBrush:    #94A3B8  (Slate 400 - Phụ đề, nhãn mô tả)
TextMutedBrush:        #64748B  (Slate 500 - Timestamp, gợi ý ẩn)

<!-- RAG Citation Badges -->
CitationBadgeBrush:    #1E3A8A  (Blue 900 - Nền huy hiệu trích dẫn slide)
CitationBadgeTextBrush:#93C5FD  (Blue 300 - Chữ huy hiệu trích dẫn slide)
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
