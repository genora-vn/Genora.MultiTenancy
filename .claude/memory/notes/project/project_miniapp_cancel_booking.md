---
name: MiniApp Cancel Booking API
description: API huỷ booking từ Mini App — logic, files, DTOs, ZBS/Email templates, TODO
type: project
---

**Endpoint:** `POST /api/mini-app/cancel-booking/{id}` — `[AllowAnonymous]`

**Files thay đổi / tạo mới:**

| File | Thay đổi |
|------|----------|
| `MiniAppCancelBookingDto.cs` *(mới)* | Input: `CustomerId` (required) + `CancelReason?` (optional, max 500) |
| `IMiniAppBookingAppService.cs` | Thêm `CancelFromMiniAppAsync(Guid id, MiniAppCancelBookingDto input)` |
| `MiniAppBookingAppService.cs` | Implement `CancelFromMiniAppAsync` — full logic |
| `MiniAppController.cs` | `[HttpPost("cancel-booking/{id}")]` AllowAnonymous |

**Logic `CancelFromMiniAppAsync` (thứ tự bước):**
1. Load booking → 404 nếu không tìm thấy
2. Xác thực `booking.CustomerId == input.CustomerId` → 403 nếu không khớp
3. Guard: đã huỷ rồi → 400 | đã Completed → 400
4. `booking.Status = BookingStatus.CancelledRefund` → `UpdateAsync`
5. Gửi ZBS `"BookingCancelled"` — try/catch, không throw
6. Gửi Email `BookingCancelRequest` template — try/catch, không throw
7. Trả về `GetMiniAppAsync(id, customerId)` — response giống get detail

**ZBS TemplateData:**
```json
{ "customer_name": "...", "booking_code": "...", "tee_off_date": "dd/MM/yyyy", "tee_off_time": "HH:mm" }
```

**Email model:** `BookingCancelRequestEmailModelDto`
- `CancelRequesterName = "{FullName} (khách hàng)"`
- `CancelStatusText = "Huỷ hoàn tiền"`

**TODO Phase 2:**
- `Booking` entity không có field `InternalNote` → `CancelReason` nhận từ input nhưng **chưa persist vào DB**. Cần thêm `CancelNote` field + migration nếu muốn lưu lý do huỷ.
- Tích hợp ZaloPay / Momo / VNPay (Online = 1) cho cả Booking và FnB
- Push thông báo realtime (SignalR) khi callback thanh toán thành công cập nhật `PaymentStatus`
- Trang admin quản lý lịch sử giao dịch thanh toán

**Why:** Khách hàng cần huỷ booking trực tiếp từ Mini App mà không cần qua admin.

**How to apply:** Các API Mini App tương tự (không cần auth) đều đặt ở `MiniAppController`, guard bằng `CustomerId` thay vì JWT.
