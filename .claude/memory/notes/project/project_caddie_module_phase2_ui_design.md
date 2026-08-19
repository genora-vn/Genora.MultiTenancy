---
name: project-caddie-module-phase2-ui-design
description: "Phase 2 Caddie UI design specs from screenshots — list, create/edit modal, detail, schedule calendar, review modal"
metadata: 
  node_type: memory
  type: project
  originSessionId: c7f0aab2-9051-4ec9-be74-7c4ad7d1b062
---

## Caddie Module — Phase 2 UI Design Specs

### 1. Caddie List (caddy-list.png)
**Page Title:** "Quản lý danh sách Caddy"
**Subtitle:** "Quản lý thông tin, trạng thái và hiệu suất làm việc của đội ngũ Caddy."
**Buttons:** "+ Thêm mới Caddy"
**Columns:**
- Thao tác (dropdown: Xem chi tiết, Sửa, Xem lịch làm việc, Xóa)
- Mã Caddy (CAD-001 format)
- Tên Caddy (avatar + name)
- Ngày vào làm (dd/MM/yyyy)
- Ngoại ngữ (badges: Tiếng Anh, Tiếng Việt, Tiếng Hàn...)
- Ngày được KH Booking (dd/MM/yyyy)
- Đánh giá sao (star rating display)
- Trạng thái (toggle switch inline)
**Pagination:** "Hiển thị 4 trên 124 Caddy"

### 2. Add/Edit Caddie Modal (add-caddy.png)
**Title:** "Thêm Caddy Mới" / "Chỉnh sửa Caddy"
**Subtitle:** "Nhập thông tin chi tiết để khởi tạo hồ sơ nhân viên"
**Fields:**
- Avatar upload (tròn, tối đa 2MB)
- Mã Caddy (CD-XXX, tự động) — readonly
- Tên Caddy (text)
- Số điện thoại (text)
- Giới tính (select: Nam/Nữ)
- Chiều cao (CM) (number)
- Giọng nói / Vùng miền (multi-select: Miền Trung, Miền Nam, Miền Bắc)
- Ngoại ngữ (multi-select: Tiếng Việt, Tiếng Anh, Tiếng Hàn...)
- Thời gian vào làm (date picker)
- Hiển thị trên App (select: Có/Không)
- Trạng thái hoạt động (select: Được hoạt động/Ngừng)
- Ghi chú nội bộ (textarea)
**Buttons:** "Hủy", "Lưu thông tin"

### 3. Caddie Detail Page (caddy-detail-with-caddy-booking-tab.png)
**Layout:** 2 columns
**Left sidebar:**
- Avatar lớn + badge "ĐANG HOẠT ĐỘNG"
- Tên caddie + Mã caddie
- Rating (4.8 ★ - 150 đánh giá)
- SĐT (masked: 091 882 1xxx)
- Thông tin chi tiết: Giới tính, Chiều cao, Giọng nói (badges), Ngoại ngữ (badges), Thời gian vào làm (+ tính năm KN), Kỹ năng & Ghi chú (quote block)
**Right top card:** "Ngày được khách hàng đặt lịch" — ngày + giờ + tên khách + button "Xem chi tiết booking"
**Right tabs:**
- Tab "Lịch sử đặt caddy": Table (Ngày & Giờ đặt, Tên Golfer, Ngày & Giờ chơi, Trạng thái thanh toán, Đánh giá)
- Tab "Đánh giá khách hàng": Table (Ngày & Giờ chơi, Khách hàng, Thời gian đánh giá, Đánh giá, Nhận xét)
**Edit button:** FAB bottom-right

### 4. Caddie Schedule Calendar (caddy-schedule.png)
**Filters:** Khu vực sân (dropdown), Nhóm Caddy (dropdown), Trạng thái (dropdown)
**View toggle:** Ngày / Tuần / Tháng
**Legend:** Trống lịch (xanh), Đang phục vụ (vàng/cam), Nghỉ (xám)
**Calendar grid:** Tuần view — mỗi ngày hiển thị cards caddie:
- Tên caddie + Mã (C-xxx)
- Giờ làm (08:00 - 14:00)
- Ghi chú (Nghỉ phép năm, Vòng Đặc biệt, Tăng ca tối...)
- Số holes (18 Hố)
**Button:** "+ Xếp lịch mới"

### 5. Review Detail Modal (caddy-review.png)
**Title:** "Đánh giá chi tiết Caddy"
**Subtitle:** "Thông tin phản hồi từ khách hàng sau buổi chơi"
**Sections:**
- Thông tin đặt chỗ: Mã đặt chỗ (#BK-1234), Ngày chơi (20/10/2023 - 07:30)
- Khách hàng: Avatar + Tên + Hạng (Thành viên Diamond)
- Đánh giá kỹ năng: Grid 2 cols (Đọc line ★★★★☆, Thái độ ★★★★☆, Tư vấn gậy ★★★★★, Hiểu biết địa hình ★★★★☆)
- Nhận xét từ khách hàng: Quote block
**Button:** "Đóng cửa sổ"

### Menu structure (theo design):
- Quản lý Caddies (group)
  - Danh sách Caddy (/AppCaddies)
  - Quản lý chuyên môn - Skills (/AppCaddieSkills)
  - Cấu hình ngôn ngữ - Languages (/AppLanguages)
  - Lịch làm việc Caddy (/AppCaddieSchedules)
  - Lịch sử đặt Caddy (/AppCaddieBookings)
  - Lịch sử đánh giá Caddy (/AppCaddieRatings)

**Why:** Design specs cho Phase 2 UI implementation
**How to apply:** Implement theo thứ tự: DTOs → AppService → Pages (List → Modal → Detail → Schedule → Review)

[[project_caddie_module_phase1_complete]] [[project_caddie_module_srs]]
