---
name: feedback-salon-phone-regex-0-or-84
description: Salon Beauty — input số điện thoại Customer/Stylist phải accept cả prefix 0 và 84 vì DB lưu đầu 84
metadata: 
  node_type: memory
  type: feedback
  originSessionId: b0157f35-01a5-463a-9e01-3b5eb1102803
---

Form **SalonBeautyCustomers** (Create/Edit) và **SalonBeautyStylists** (Create/Edit) phải chấp nhận số điện thoại có:
- Bắt đầu bằng `0` (số nội địa, 10–11 chữ số tổng) — pattern `^0\d{9,10}$`, hoặc
- Bắt đầu bằng `84` (mã quốc gia, 11–12 chữ số tổng) — pattern `^84\d{9,10}$`.

Regex thống nhất: `^(0\d{9,10}|84\d{9,10})$`. Maxlength input = `13`. JS `normalizePhoneInput` strip về 13 ký tự số.

**Why:** DB Salon Beauty lưu phone với prefix `84` (do flow Mini App Zalo upsert customer dùng `84976687984`). Nếu form CMS chỉ validate `^0\d{9,10}$` thì khi Edit khách hàng/nhân viên hiện hữu, button **Lưu** bị disable vì phone hiện tại không khớp regex → không sửa được record.

**How to apply:**
- Khi tạo/sửa form Salon liên quan đến phone (kể cả `SalonBeautyLocations` nếu user yêu cầu sau): cập nhật **toàn bộ chuỗi validate** — thiếu một mắt xích là server vẫn reject:
  - DTO `Application.Contracts/AppDtos/SalonBeauties/...` — `[RegularExpression(@"^(0\d{9,10}|84\d{9,10})$", ...)]` (StringLength = 15).
  - AppService `Application/AppServices/SalonBeauties/...` — `Regex.IsMatch(phone, @"^(0\d{9,10}|84\d{9,10})$")` trong validate helper (SalonBeautyCustomerAppService line ~598, SalonBeautyStylistAppService line ~305).
  - Razor `<input pattern>` trong cshtml + JS validator (`isXxxFormValid`) + JS `normalizePhoneInput` (substring 13).
  - Server-side `ValidateStylistInput` PageModel (`Web/Pages/SalonBeautyStylists/CreateModal.cshtml.cs` + `EditModal.cshtml.cs`).
- Localization tương ứng (`*:PhoneInvalid`, `*:PhonePlaceholder`, `*:PhoneHint`) cập nhật cho cả vi/en.
- Không cần convert prefix về dạng chuẩn ở front-end — giữ nguyên giá trị user nhập, BE chấp nhận cả hai.
- **Lưu ý**: Customer validate qua `BusinessException("SalonBeautyCustomer:PhoneInvalid")`, Stylist validate qua `[RegularExpression]` của DTO + UserFriendlyException của AppService — phải đồng bộ regex ở **cả ba** nơi để tránh mismatch giữa BE/FE.

Liên quan: [[project-salon-booking-ui]], [[project-customer-phone-readonly-miniapp]].
