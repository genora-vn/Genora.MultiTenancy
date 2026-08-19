---
name: project_caddie_fixes_june05_batch2
description: "Caddie Module fixes batch 2+3 — MiniApp API body, rating decimal, progress bar, avatars, reject modal"
metadata: 
  node_type: memory
  type: project
  originSessionId: 42c60f84-6497-4468-9a7a-4a2842937bc4
---

## Caddie Module Fixes Batch 2+3 (2026-06-05)

### 1. MiniApp APIs — bỏ request headers, truyền qua body
- `MiniAppCreateCaddieBookingDto` + `MiniAppCreateCaddieRatingDto` thêm `CustomerId`
- `MiniAppCaddieAppService` inject `IRepository<Customer>`, lookup FullName/PhoneNumber từ DB
- Controller bỏ toàn bộ X-Customer-* headers

### 2. Rating computed from skill details (decimal)
- DTO thêm `ComputedRating` (decimal) bên cạnh `OverallRating` (int)
- `GetListAsync` + `GetAsync` compute avg from details → map to both
- JS dùng `computedRating` cho cột Rating (hiển thị 2.5 thay vì 3)

### 3. Progress bar booking detail
- Status=1: 0%, Status=2: 50%, Status=3: 100%, Status=4: 50%
- `max-width:calc(100% - 120px)` ngăn tràn
- Thêm mốc "Đã hủy" thay "Đang phục vụ" khi status=4

### 4. Avatar display
- `AppCaddies/Detail` modal: `#reviewCustomerAvatar` + `#reviewCustomerInitials` show/hide
- `AppCaddieRatings/Index` modal: `css('display',...)` cho golfer/caddy avatar
- `AppCaddieRatings/Detail`: `onerror` fallback trên img

### 5. Reject modal — fix TypeError abp.message.prompt
- Thay bằng modal `#rejectReasonModalDetail` có textarea + confirm button

### 6. Thông báo + Spacing
- "Caddy {TênCaddy} không có lịch nào sắp tới"
- `mb-4` → `mb-3`, `g-4` → `g-3`

**Why:** OverallRating=5 luôn (user submit); phải compute từ details. `abp.message.prompt` không tồn tại.
**How to apply:** Dùng `ComputedRating` decimal; modal textarea cho reject.

### 9. MiniApp APIs — Avatar full URL (ImageHelper.NormalizeThumb)
- Inject `IConfiguration` vào `MiniAppCaddieAppService`
- Thêm `ResolveAvatarUrl(url)` wrapper gọi `ImageHelper.NormalizeThumb(_configuration, url)`
- Apply cho 4 APIs: `/caddie/available` (Avatar), `/caddie/{id}` (Avatar), `/caddie/booking` (CaddieAvatar), `/caddie/booking/history` (CaddieAvatar)
- Pattern: nếu path bắt đầu `/uploads` → prefix `App:AppUrl` config value

[[project_caddie_module_phase2_ui_redesign]]
