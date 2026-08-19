---
name: UI Filter pattern chuẩn (FnbOrders style)
description: Pattern bộ lọc chuẩn cho trang Orders: label + input trong div.mb-3, auto-refresh, flatpickr date
type: project
---

Tất cả trang Orders (FnB và Proshop) phải theo pattern của `AppFnbOrders/Index.cshtml`:

**Cấu trúc filter row:**
```html
<abp-column size-md="_N">
    <div class="mb-3">
        <label class="form-label fnb-filter-label" for="InputId">@L["Key"]</label>
        <input id="InputId" class="form-control" type="text" />
    </div>
</abp-column>
```

**Toolbar chuẩn (trái sang phải):** Auto-refresh dropdown → Refresh → Export Excel → Board/Kitchen link

**Auto-refresh:** Dropdown `Off / 5s / 10s / 15s(default) / 30s`, dùng `setInterval` + `document.hidden` guard.

**Flatpickr:** Load từ CDN, init `$('.public-time-input').flatpickr({ dateFormat: 'Y-m-d' })`.

**Class wrapper:** `<div class="fnb-page">` bao toàn bộ card.

**Why:** Đảm bảo UX nhất quán giữa FnB và Proshop.

**How to apply:** Khi tạo/sửa trang Orders mới, tham chiếu `AppFnbOrders/Index.cshtml` làm chuẩn.
