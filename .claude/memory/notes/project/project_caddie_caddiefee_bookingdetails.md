---
name: project_caddie_caddiefee_bookingdetails
description: GolfCourse CaddieFee field + AppCaddieBookingDetail entity (multi-caddy booking) + Languages API
metadata: 
  node_type: memory
  type: project
  originSessionId: 42c60f84-6497-4468-9a7a-4a2842937bc4
---

## Caddie Module — CaddieFee + BookingDetails + Languages API (2026-06-09)

### 1. GolfCourse.CaddieFee
- Entity: `decimal? CaddieFee` trên `GolfCourse.cs`
- EF: `b.Property(x => x.CaddieFee).HasColumnType("decimal(18,2)");`
- DTOs: thêm vào `AppGolfCourseDto`, `CreateUpdateAppGolfCourseDto`, `GolfCourseListData` (MiniApp)
- UI: input trong group "Tiện ích & dịch vụ" trên CreateModal + EditModal
- AutoMapper: convention-based (cùng tên property)

### 2. AppCaddieBookingDetail (multi-caddy per booking)
- Entity: `AppCaddieBookingDetail` (Id, CaddieBookingId, CaddieId, ScheduleId, Status, Note)
- Table: `AppCaddieBookingDetails`
- FK: CaddieBookingId → AppCaddieBookings (Cascade), CaddieId → AppCaddies (Restrict), ScheduleId → AppCaddieSchedules (Restrict)
- DTO: `MiniAppCreateCaddieBookingDto.CaddieIds` (List<Guid>?, optional)
- Logic: nếu CaddieIds có data → validate + lock schedule cho mỗi caddy; nếu chỉ CaddieId → backward compat tạo 1 detail
- `AppCaddieBooking.CaddieId` giữ nguyên (primary caddy)

### 3. Languages API
- `GET /api/mini-app/caddie/languages`
- Response: `MiniAppCaddieLanguagesResponse : ZaloBaseResponse` + `Data: List<MiniAppLanguageDto>`
- Logic: query `AppLanguages` where Status=1, order by SortOrder

### Migration
- `AddCaddieFeeAndBookingDetails` — adds CaddieFee column + AppCaddieBookingDetails table

**Why:** Mở rộng tính năng đặt caddy cho phép book nhiều caddy; CaddieFee để mini app hiển thị giá; Languages cho filter/search theo ngôn ngữ.
**How to apply:** `_bookingDetailRepo.InsertAsync` per caddy trong loop; backward compat giữ CaddieId trên booking.

### 4. Caddie Payment APIs (prepare-order + check-transaction)
- Interface: `IMiniAppCaddiePaymentAppService` trong `IPaymentAppServices.cs`
- Input: `PrepareCaddieBookingInput` (CaddieBookingId, PaymentMethod)
- Service: `MiniAppCaddiePaymentAppService` — clone pattern từ FnbPayment
- Logic PrepareOrder: lookup booking → get CaddieFee từ GolfCourse → sign MAC → VietQR nếu BankTransfer
- Logic CheckTransaction: parse BookingCode từ orderId → check PaymentStatus
- orderId format: `{BookingCode}_{unixTimestamp}` (VD: CB-20260604-FDB7_1743638400)
- Endpoints: `POST /api/mini-app/caddie/prepare-order`, `GET /api/mini-app/caddie/check-transaction/{orderId}`

### 5. Refactor AppCaddieBooking — bỏ CaddieId/ScheduleId, thêm TotalCaddieFee/PaymentMethod
- Entity bỏ: `CaddieId`, `ScheduleId`, navigation `Caddie`, `Schedule`
- Entity thêm: `TotalCaddieFee` (decimal 18,2), `PaymentMethod` (byte: 0=COD, 1=Online, 2=BankTransfer)
- EF: bỏ FK + index Caddie/Schedule; thêm HasDefaultValue cho trường mới
- DTO: `CaddieBookingDto` bỏ ScheduleId, thêm TotalCaddieFee/PaymentMethod/PaymentMethodText
- `CaddieBookingAppService`: lấy caddie info qua `_bookingDetailRepo`; `ChangeCaddyAsync` update detail
- `MiniAppCreateCaddieBookingDto`: dùng `Caddies: List<MiniAppBookingCaddieItemDto>` (caddieId + note)
- `MiniAppCaddieAppService`: CreateBooking/GetHistory/CreateRating đều qua details
- `CaddieAppService`: lastBookingDate via details in-memory join
- Migration: `UpdateCaddieBookingRemoveCaddieIdAddPayment`
- UI Detail: hiển thị TotalCaddieFee.ToString("N0")đ + PaymentMethodText

### 6. Fix overallRating in recentReviews (GetCaddieDetailAsync)
- Compute avg từ `reviewDetails.Average(d => d.Score)` thay vì `r.OverallRating`
- `MiniAppCaddieReviewDto.OverallRating` đổi int → decimal

### 7. API Booking History — thêm CaddieCode, CaddieRatingAvg, TotalCaddieFee, PaymentMethod
- `MiniAppCaddieBookingHistoryDto` thêm: CaddieCode, CaddieRatingAvg, TotalCaddieFee, PaymentMethod
- Service query caddie includes CaddieCode + RatingAvg

### 8. API Booking Detail — GET /api/mini-app/caddie/booking/{id}
- Response: `MiniAppCaddieBookingDetailResponse : ZaloBaseResponse` + `Data: MiniAppCaddieBookingDetailDto`
- Data includes: booking info, customer info, golf course (name + address), caddies array
- `MiniAppBookingCaddieDetailDto`: CaddieId, CaddieName, CaddieCode, CaddieAvatar, RatingAvg, Phone, Gender, Note
- Inject `_golfCourseRepo` vào MiniAppCaddieAppService

### 9. CaddieSchedule — Upsert logic + Delete range + Delete single
- `CreateAsync` upsert key: `(CaddieId + WorkDate + ShiftCode + StartTime)` — cho phép nhiều khung giờ/ca
- Ví dụ: Ca sáng 06:00-09:00 + 09:00-12:00 = 2 records cùng ShiftCode=1 khác StartTime
- `DeleteAsync`: check booking trước khi xóa
- `DeleteRangeAsync`: xóa hàng loạt, skip ca có booking
- UI: "Xóa lịch" button + modal; "Xóa ca" button trong detail modal
- Excel import: hoạt động đúng với multi-slot per shift
- Notification fix: `abp.message.warn/error` (modal) thay `abp.notify` (toast tự đóng); delay reload 1.5s

### 10. Optimization: Rating Recalculation Background Job
- `RecalculateCaddieRatingJob` — fix ObjectDisposedException: thêm `IUnitOfWorkManager` + `[UnitOfWork]` + `_unitOfWorkManager.Begin(requiresNew: true)`
- Args thêm `TenantId` cho multi-tenant support
- Job tự tạo scope mới → không bị disposed DbContext

### 11. Schedule Template (Save + Apply pattern tuần) — UI buttons
- Button "Lưu Template" (outline-warning) → modal chọn Caddie + tên template → save tuần hiện tại
- Button "Áp dụng Template" (outline-info) → modal chọn Caddie + ngày bắt đầu tuần → generate lịch
- Cả 2 button nằm sau "Xóa lịch" trên toolbar
- JS dùng `window.__caddieItems` (server-rendered) để populate select

### 12. Booking History Export Excel — UI button
- Button "Xuất Excel" (outline-success) nằm ở header row trang Lịch sử Đặt Caddy
- Link: `/api/app/caddie-schedule-excel/export-bookings` (download trực tiếp)

[[project_caddie_fixes_june05_batch2]]
