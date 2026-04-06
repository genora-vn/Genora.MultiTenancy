# Project Memory: Genora.MultiTenancy

## Current Working State
- **Modules Active:** `AppCalendarSlots`, `AppZaloAuths`, `AppPayments` (Booking + FnB).
- **Last Task Completed:** Zalo Checkout SDK V1 — COD & BankTransfer cho cả Booking (đặt sân) và FnbOrder (đặt món). Đã build pass, API test thành công.

## Important Logic Decisions
- `AppZaloAuths`: Zalo Access Token để định danh Tenant.
- `AppCalendarSlotPrices`: Giá = khung giờ × loại member × số hố.
- `AppPayments`: Bảo mật callback bằng HMAC-SHA256 verify MAC + overallMac — **không dùng JWT** cho callback/notify endpoint.
- `FnbOrder.PaymentStatus` tách biệt với `FnbOrder.ServiceStatus` — thanh toán và phục vụ là 2 luồng độc lập.

## Things to Remember
- `AppNews` DB đã ổn định — không cần quét lại Domain layer.
- Helpers dùng chung: `Genora.MultiTenancy.Application/Helpers/`.
- `PaymentMethod` enum (Domain.Shared): `COD=0, Online=1, BankTransfer=2`.
- `BookingStatus` enum: `Processing=0, Confirmed=1, Paid=2, Completed=3, CancelledRefund=4, CancelledNoRefund=5`.
- `FnbPaymentStatus` enum: `Unpaid=1, Paid=2, Failed=3`.
- `FnbServiceStatus` enum: `Created=1, Preparing=2, Delivering=3, Served=4, Cancelled=5`.
- `PaymentOrderStatus` enum (Domain.Shared): `Pending=0, Success=1, Failed=2, Cancelled=3`.
- **OrderCode prefix để phân loại đơn:** `KH` = Booking | `FNB` = FnbOrder — dùng trong Callback/Notify để auto-detect.

---

## Zalo Checkout SDK V1 — Thông tin cấu hình (không cần đọc lại)

| Key | Value |
|-----|-------|
| App ID (MiniAppId) | `3536128005136318197` |
| Private Key | `1e35c8118e7a5e21b0dc3d9b46d2f4f2` |
| Security Method | HMAC SHA256 |
| Callback URL (Staging) | `https://montgo-staging.genora.vn/api/payment/callback` |
| Callback URL (Production) | `https://montgo.genora.vn/api/payment/callback` |
| Notify URL (Staging) | `https://montgo-staging.genora.vn/api/payment/notify` |
| Notify URL (Production) | `https://montgo.genora.vn/api/payment/notify` |

### MAC Formulas
```
# createOrder MAC (Backend → Mini App)
HMAC-SHA256(privateKey, "{appId}|{orderId}|{amount}")

# Callback/Notify MAC verify (Zalo → Backend)
HMAC-SHA256(privateKey, "{appId}|{orderId}|{transId}|{amount}|{description}|{resultCode}|{message}")

# overallMac verify
HMAC-SHA256(privateKey, tất cả fields sắp xếp key theo từ điển tăng dần, nối "key=value&key=value")
```

### ABP Setting Keys (per-tenant, đăng ký trong ZaloPaymentSettingDefinitionProvider)
```
Genora.Zalo.MiniAppId           ← AppId dùng chung với ZNS (ZaloSettingNames.MiniAppId)
Genora.Payment.Zalo.PrivateKey  ← encrypted, chỉ save khi user nhập mới
Genora.Payment.Bank.BankName
Genora.Payment.Bank.AccountNumber
Genora.Payment.Bank.AccountOwner
Genora.Payment.Bank.Branch
```

---

## Payment Module — Map file theo layer

### Application.Contracts — `AppDtos/AppPayments/`
| File | Nội dung |
|------|----------|
| `IPaymentAppServices.cs` | 5 interfaces: IMiniAppPaymentAppService, IMiniAppFnbPaymentAppService, IPaymentCallbackAppService, IPaymentNotifyAppService, IFnbOrderStatusAppService |
| `PrepareOrderInput.cs` | Input Booking: BookingId + PaymentMethod |
| `PrepareFnbOrderInput.cs` | Input FnB: FnbOrderId + PaymentMethod |
| `PrepareOrderResult.cs` | Output chung: AppId, OrderId, Amount, Mac, BankInfo? |
| `ZaloPaymentCallbackInput.cs` | Payload từ Zalo: data{} + mac + overallMac |
| `ZaloCallbackResponse.cs` | Response về Zalo: ReturnCode + ReturnMessage. **Cũng chứa `CheckTransactionResult`** |
| `FnbOrderStatusDtos.cs` | GetOrderStatusResult, UpdateFnbPaymentStatusInput, UpdateFnbPaymentStatusResult |

### Application — `AppServices/AppPayments/`
| File | Nội dung |
|------|----------|
| `ZaloPaymentSettingNames.cs` | Hằng số setting keys |
| `ZaloPaymentSettingDefinitionProvider.cs` | Đăng ký ABP Settings (PrivateKey encrypted) — đã thêm vào MultiTenancyApplicationModule |
| `ZaloMacHelper.cs` | `GenerateCreateOrderMac`, `VerifyCallbackMac`, `VerifyOverallMac` |
| `MiniAppPaymentAppService.cs` | Booking: PrepareOrder + CheckTransaction |
| `MiniAppFnbPaymentAppService.cs` | FnbOrder: PrepareOrder + CheckTransaction |
| `PaymentCallbackAppService.cs` | Unified callback: detect KH→Booking / FNB→FnbOrder, verify MAC, update status, ghi FnbOrderActivity |
| `PaymentNotifyAppService.cs` | Notify khi user chọn COD/BankTransfer: ghi nhận PaymentMethod, status vẫn Unpaid |
| `FnbOrderStatusAppService.cs` | GetOrderStatus + UpdatePaymentStatus [Authorize] (merchant xác nhận thủ công) |

### HttpApi — `Controllers/`
| File | Endpoints |
|------|-----------|
| `MiniAppController.cs` | POST `payment/prepare-order`, GET `payment/check-transaction/{id}`, POST `payment/fnb/prepare-order`, GET `payment/fnb/check-transaction/{id}` |
| `PaymentCallbackController.cs` | POST `callback`, POST `notify`, GET `order-status/{id}`, POST `update-payment-status` [Authorize] |

### Web — `Pages/UpgradeSettings/ZaloZns`
- Admin cấu hình Private Key (password field, không load lại), BankName, AccountNumber, AccountOwner, BankBranch.
- MiniAppId dùng chung field đã có trong section Zalo phía trên.

---

## API Endpoints — Đầy đủ

| # | Method | Route | Auth | Mô tả |
|---|--------|-------|------|-------|
| 1 | POST | `/api/mini-app/payment/prepare-order` | Anonymous | createOrder Booking |
| 2 | GET  | `/api/mini-app/payment/check-transaction/{orderId}` | Anonymous | checkTransaction Booking |
| 3 | POST | `/api/mini-app/payment/fnb/prepare-order` | Anonymous | createOrder FnB |
| 4 | GET  | `/api/mini-app/payment/fnb/check-transaction/{orderId}` | Anonymous | checkTransaction FnB |
| 5 | POST | `/api/payment/callback` | Anonymous (MAC verify) | Zalo gọi sau giao dịch |
| 6 | POST | `/api/payment/notify` | Anonymous (MAC verify) | Zalo gọi khi user xác nhận COD/BankTransfer |
| 7 | GET  | `/api/payment/order-status/{orderId}` | Anonymous | Truy vấn trạng thái FnB |
| 8 | POST | `/api/payment/update-payment-status` | **[Authorize]** | Merchant xác nhận đã nhận tiền |

---

## Lịch sử thay đổi

### [2026-04-03 v1] Implement Zalo Checkout SDK V1 — Booking (COD + BankTransfer)
- Tạo 10 files mới từ Domain.Shared → HttpApi.
- Đăng ký `ZaloPaymentSettingDefinitionProvider` vào `MultiTenancyApplicationModule`.
- Tích hợp UI cấu hình vào trang `UpgradeSettings/ZaloZns`.

### [2026-04-03 v2] Bổ sung Payment cho FnB (đặt món)
- Thêm 4 files mới: `PrepareFnbOrderInput`, `FnbOrderStatusDtos`, `MiniAppFnbPaymentAppService`, `PaymentNotifyAppService`, `FnbOrderStatusAppService`.
- Refactor `PaymentCallbackAppService`: unified handler cho cả Booking + FnbOrder qua prefix detection.
- Refactor `PaymentCallbackController`: thêm `/notify`, `/order-status/{id}`, `/update-payment-status`.
- Inject `IMiniAppFnbPaymentAppService` vào `MiniAppController`.

### [2026-04-03 v3] Bugfix — CheckTransactionResult.Status type mismatch
- **Lỗi:** `Cannot implicitly convert type 'string' to 'PaymentOrderStatus'`
- **Nguyên nhân:** `ZaloCallbackResponse.cs` (user sửa) đổi `CheckTransactionResult.Status` từ `string` sang `PaymentOrderStatus` enum.
- **Fix trong `MiniAppPaymentAppService.CheckTransactionAsync`:**
  - `"NotFound"` → `PaymentOrderStatus.Failed`
  - `isPaid ? "Success" : booking.Status.ToString()` → `PaymentOrderStatus.Success / .Pending / .Cancelled`
  - Bổ sung case `CancelledRefund/CancelledNoRefund` → `PaymentOrderStatus.Cancelled`.
- `MiniAppFnbPaymentAppService` đã đúng từ đầu — không cần sửa.

---

### [2026-04-03 v4] Bổ sung VietQR — QR code + deeplink cho BankTransfer

**Yêu cầu:** Trả thêm `qrCode`, `qrImageUrl`, `bankAppUrl` trong `bankInfo` khi `paymentMethod = BankTransfer`
để Mini App hiển thị QR và nút "Mở app ngân hàng".

**Files thay đổi:**
- `PrepareOrderResult.cs` — `BankInfoDto` thêm 3 field: `QrCode?`, `QrImageUrl?`, `BankAppUrl?`
- `VietQrBankCodeMap.cs` *(mới)* — static dictionary 40+ ngân hàng: tên → {Bin, ShortCode}
- `VietQrApiClient.cs` *(mới)* — gọi `api.vietqr.io/v2/generate` lấy EMVCo QR string; `BuildFallback()` khi API lỗi
- `MiniAppPaymentAppService.cs` — inject `VietQrApiClient`, populate QR fields sau khi load bankInfo
- `MiniAppFnbPaymentAppService.cs` — tương tự
- `MultiTenancyApplicationModule.cs` — `AddHttpClient("VietQR")` timeout 5s

**Response mới (BankTransfer):**
```json
"bankInfo": {
  "bankName": "TPBANK",
  "accountNumber": "040091011510",
  "accountOwner": "DANG VAN TAN",
  "branch": "Chi nhánh Hà Nội",
  "qrCode": "00020101021238560010A000000727...",
  "qrImageUrl": "https://img.vietqr.io/image/TPB-040091011510-qr_only.jpg?amount=2000000&addInfo=...",
  "bankAppUrl": "vietqr://pay?app=tpb&ba=040091011510&am=2000000&tn=Thanh+toan+dat+san&nn=DANG+VAN+TAN"
}
```

**Luồng QR generation:**
1. Load bankName từ Setting → `VietQrBankCodeMap.GetCode(bankName)` → {Bin, ShortCode}
2. Gọi `VietQrApiClient.GenerateAsync()` → EMVCo QR string từ `api.vietqr.io`
3. Nếu API lỗi/timeout (5s) → `BuildFallback()` → QrCode=null, nhưng QrImageUrl + BankAppUrl vẫn có
4. QrImageUrl = `https://img.vietqr.io/image/{ShortCode}-{AccountNo}-qr_only.jpg?...`
5. BankAppUrl = `vietqr://pay?app={shortCode}&ba={accountNo}&am={amount}&tn={addInfo}&nn={accountOwner}`

**Lưu ý:** BankCodeMap dùng case-insensitive, tự bỏ tiền tố "ngân hàng/ngan hang". Nếu ngân hàng không map được → bỏ qua QR (bankInfo vẫn trả về, QR fields = null).

---

### [2026-04-03 v5] Bugfix — bankAppUrl đổi scheme vietqr:// để mở app ngân hàng từ Zalo Mini App

**Vấn đề:** `https://dl.vietqr.io/pay?...` chỉ mở browser, không mở được app ngân hàng trong Zalo Mini App.

**Fix:** Đổi `DeeplinkBase = "https://dl.vietqr.io/pay"` → `DeeplinkScheme = "vietqr://pay"` trong `VietQrApiClient.cs`.

**File thay đổi:** `VietQrApiClient.cs` — constant `DeeplinkScheme` + method `BuildDeeplink()`.

**Cấu trúc URL:**
```
vietqr://pay?app={shortCode}&ba={accountNo}&am={amount}&tn={addInfo}&nn={accountOwner}
```

| Param | Giá trị | Nguồn |
|-------|---------|-------|
| `app` | ShortCode viết thường (vd: `tpb`, `vcb`) | `VietQrBankCodeMap.ShortCode.ToLower()` |
| `ba`  | Số tài khoản | Setting `BankAccountNumber` |
| `am`  | Số tiền (VND, không dấu phẩy) | `TotalAmount` |
| `tn`  | Nội dung CK (max 50 ký tự, URL-encoded) | `description` của order |
| `nn`  | Tên chủ tài khoản (URL-encoded) | Setting `BankAccountOwner` |

**Áp dụng cho cả 2 endpoints:** `prepare-order` (Booking) và `fnb/prepare-order` (FnbOrder).

---

### [2026-04-03 v6] Bổ sung API huỷ booking từ Mini App

**Endpoint mới:** `POST /api/mini-app/cancel-booking/{id}`

**Files thay đổi / tạo mới:**
| File | Thay đổi |
|------|----------|
| `MiniAppCancelBookingDto.cs` *(mới)* | Input: `CustomerId` (required) + `CancelReason?` (optional, max 500) |
| `IMiniAppBookingAppService.cs` | Thêm `CancelFromMiniAppAsync(Guid id, MiniAppCancelBookingDto input)` |
| `MiniAppBookingAppService.cs` | Implement `CancelFromMiniAppAsync` — full logic |
| `MiniAppController.cs` | `[HttpPost("cancel-booking/{id}")]` AllowAnonymous |

**Logic `CancelFromMiniAppAsync`:**
1. Load booking → 404 nếu không tìm thấy
2. Xác thực `booking.CustomerId == input.CustomerId` → 403 nếu không khớp
3. Guard: đã huỷ rồi → 400 | đã Completed → 400
4. `booking.Status = BookingStatus.CancelledRefund` → `UpdateAsync`
5. Gửi ZBS `"BookingCancelled"` (try/catch, không throw)
6. Gửi Email `BookingCancelRequest` template (try/catch, không throw)
7. Trả về `GetMiniAppAsync(id, customerId)` — response giống get detail

**ZBS TemplateData:**
```json
{ "customer_name": "...", "booking_code": "...", "tee_off_date": "dd/MM/yyyy", "tee_off_time": "HH:mm" }
```

**Email model:** `BookingCancelRequestEmailModelDto` — `CancelRequesterName = "{FullName} (khách hàng)"`, `CancelStatusText = "Huỷ hoàn tiền"`.

**Lưu ý:** `Booking` entity không có field `InternalNote` → `CancelReason` nhận từ input nhưng chưa persist vào DB. TODO Phase 2: thêm `CancelNote` field + migration nếu cần lưu lý do huỷ.
- [ ] Tích hợp ZaloPay / Momo / VNPay (Online = 1) cho cả Booking và FnB.
- [ ] Push thông báo realtime (SignalR) khi callback thành công cập nhật PaymentStatus.
- [ ] Trang admin quản lý lịch sử giao dịch thanh toán.
