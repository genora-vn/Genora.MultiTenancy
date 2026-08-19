---
name: salon-beauty-miniapp-payment-endpoints
description: Bổ sung 2 API payment/prepare-order + payment/check-transaction vào SalonBeautyMiniAppController qua MiniAppSalonBeautyPaymentAppService — pattern tương tự Booking/FnB/Pro nhưng dùng SalonBeautyPaymentMethod (Cash=1/BankTransfer=2/Card=3)
metadata: 
  node_type: memory
  type: project
  originSessionId: b0157f35-01a5-463a-9e01-3b5eb1102803
---

# Salon Beauty MiniApp Payment Endpoints (2026-05-26)

## Endpoint mới
`SalonBeautyMiniAppController` (`/api/mini-app/salon-beauty`):
- `POST payment/prepare-order` body `PrepareSalonBeautyBookingInput { BookingId, PaymentMethod }` → `PrepareOrderResult` (AppId/OrderId/Amount/Mac/BankInfo)
- `GET  payment/check-transaction/{orderId}` → `CheckTransactionResult { Status, IsPaid, Message }`

## Service
`MiniAppSalonBeautyPaymentAppService : IMiniAppSalonBeautyPaymentAppService` (Application/AppServices/AppPayments) — clone từ `MiniAppProPaymentAppService`/`MiniAppFnbPaymentAppService` nhưng:
- Repo: `IRepository<SalonBeautyBooking, Guid>` (không phải `Booking` golf hay `FnbOrder`)
- Status check: `SalonBeautyPaymentStatus.Paid` + `SalonBeautyBookingStatus.Cancelled`
- Throw `BookingNotFound` / `BookingAlreadyPaid` / `BookingCancelled` / `PaymentNotConfigured` (reuse `AppPaymentErrorCodes`)
- `orderId = {BookingCode}_{unixTimestamp}`, ExtractOrderCode = phần trước `_` cuối
- PaymentMethod dùng `SalonBeautyPaymentMethod` (enum riêng — Cash=1/BankTransfer=2/Card=3), KHÔNG dùng `PaymentMethod` chung (COD/BankTransfer/Online).
- BankInfo + VietQR chỉ build khi `BankTransfer`. Setting bank + AppId/PrivateKey lấy từ `ZaloPaymentSettingNames` (per-tenant) — tái dùng cấu hình ZaloZns.
- `SalonBeautyPaymentStatus.Refunded` → map sang `PaymentOrderStatus.Cancelled`.

## Why
Salon Beauty mini app cần wire Zalo Checkout SDK (`createOrder` → `prepare-order`, poll status → `check-transaction`) cho 2 hình thức Cash (tại quầy) + BankTransfer (chuyển khoản). Pattern Booking/FnB/Pro đã có sẵn → reuse code thay vì viết flow mới. Salon dùng entity riêng (`AppSalonBeautyBookings` schema `Salon`) + enum payment method riêng → cần service riêng, không gộp được.

## How to apply
- Mini App Salon flow:
  1. User chọn slot → `POST /api/mini-app/salon-beauty/bookings` tạo booking (status `New`, paymentStatus `Unpaid`).
  2. User chọn phương thức → `POST /api/mini-app/salon-beauty/payment/prepare-order` với `BookingId` + `PaymentMethod`.
  3. Mini App nhận `PrepareOrderResult` → gọi Zalo Checkout SDK `createOrder({appId, orderId, amount, desc, mac, ...})`.
  4. Sau khi SDK trả → poll `GET /api/mini-app/salon-beauty/payment/check-transaction/{orderId}` để hiển thị status.
- Nếu `BankTransfer` → `PrepareOrderResult.BankInfo` chứa QR + deeplink để Mini App hiển thị.
- Khi cấu hình Zalo callback server: prefix orderId của Salon là `{BookingCode}_...` — BookingCode salon tự sinh (không trùng prefix golf "KH", FnB "FNB", Pro "PRO"). Khi route callback nên check theo BookingCode/repo, không nên check prefix string.
- KHÔNG tự cộng `Cash` vào enum `PaymentMethod` chung — Salon đã có `SalonBeautyPaymentMethod` riêng, mapping về `PaymentMethodName` qua helper trong service.

## File map
- App.Contracts:
  - `AppDtos/AppPayments/PrepareSalonBeautyBookingInput.cs` (mới)
  - `AppDtos/AppPayments/IPaymentAppServices.cs` (thêm `IMiniAppSalonBeautyPaymentAppService`)
- Application: `AppServices/AppPayments/MiniAppSalonBeautyPaymentAppService.cs` (mới)
- HttpApi: `Controllers/SalonBeautyMiniAppController.cs` (inject `_paymentService` + 2 endpoint mới)

## Related
- [[project_payment_toggles_and_news_edit_lazy_load]] — settings bật/tắt 2 phương thức (IsPayAtCounterEnabled / IsPayBankTransferEnabled) — Salon cũng nên check 2 cờ này khi hiện UI lựa chọn (chưa làm ở backend, frontend tự đọc qua calendar slot APIs cũ; nếu Salon cần riêng thì follow-up)
- [[project_payment_configuration]] — pattern cấu hình thanh toán (Bank QR per-tenant)
- [[project_proorder_customer_soft_reference]] — pattern Salon dùng entity riêng tách FK với golf
