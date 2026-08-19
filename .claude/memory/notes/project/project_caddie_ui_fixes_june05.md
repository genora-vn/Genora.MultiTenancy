---
name: project_caddie_ui_fixes_june05
description: "Caddie module UI fixes June 05 — star rating, booking history rating col, rating tab redesign, review detail modal redesign"
metadata: 
  node_type: memory
  type: project
  originSessionId: 42c60f84-6497-4468-9a7a-4a2842937bc4
---

## Caddie UI Fixes (2026-06-05)

### 1. Index — Star Rating Column Fix (`index.js`)
- Dùng `Math.floor(ratingAvg)` thay vì half-star logic để xác định số sao tô màu vàng
- Inline style `color:#f59e0b` cho filled, `color:#cbd5e1` cho empty (không dùng CSS class `caddie-stars` vì không apply đúng trong DataTable)
- Hiển thị số `4.1` bên cạnh sao nếu `ratingAvg > 0`

### 2. Booking History Tab — Thêm cột Đánh giá (`detail.js` + backend)
- `CaddieBookingDto` thêm `BookingRatingAvg` (decimal?)
- `CaddieBookingAppService.GetListAsync` inject `_ratingRepo` + `_ratingDetailRepo`, load ratings theo bookingIds, tính avg từ detail scores → map vào `BookingRatingAvg`
- JS: render `renderStars(data)` helper dùng `Math.floor`, hiển thị "Chưa đánh giá" nếu null

### 3. Rating Tab Redesign (`detail.js`)
- Bỏ cột button "Xem chi tiết"
- Thêm cột "Mã đánh giá" (bookingCode) với `<a class="rating-view-detail">` — click mở modal
- Cột "Đánh giá": dùng `renderStars(overallRating)` floor-based
- Thứ tự cột: Mã đánh giá | Ngày đánh giá | Khách hàng | Đánh giá | Nhận xét | Trạng thái

### 4. Review Detail Modal Redesign (`Detail.cshtml`)
- `modal-lg`, 2-column info cards (Booking info / Customer info) với border-bottom accent màu primary/secondary
- Skill ratings grid: `grid-template-columns: 1fr 1fr`, mỗi skill 1 row label + stars
- Comment section: italic, border-left accent
- Footer: nút "Đóng cửa sổ" với icon fa-check-circle
- JS populate: `#reviewBookingCode`, `#reviewBookingDate`, `#reviewCustomerName`, `#reviewCustomerAvatar`, `#reviewSkillRatings`, `#reviewComment`

### Pattern: `renderStars(avg)` helper
```js
function renderStars(avg, total) {
    total = total || 5;
    var filled = Math.floor(avg);
    var stars = '';
    for (var i = 1; i <= total; i++) {
        stars += i <= filled
            ? '<i class="fa fa-star" style="color:#f59e0b;font-size:13px;"></i>'
            : '<i class="fa fa-star" style="color:#cbd5e1;font-size:13px;"></i>';
    }
    return stars;
}
```

**Why:** Yêu cầu floor-based (4.1 → 4 sao), không half-star. CSS class không apply đúng trong DataTable cells.
**How to apply:** Tái dùng pattern này cho mọi star render trong Caddie module.
