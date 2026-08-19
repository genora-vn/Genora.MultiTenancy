# Architecture — Module Salon Beauty

> Nguồn: bản implementation đầy đủ tại `../memory/modules/salon-beauty/` (migrate đợt trước) +
> các note `project_salon_*.md` trong `../memory/notes/project/`.

## Tổng quan
- Schema riêng **"Salon"** (tách khỏi `dbo` của golf) để cô lập dữ liệu.
- 8 entity, 6 AppService, dual permission Host/Tenant + feature gate `SalonBeautyFeatures.Management`.
- Aggregate root: `SalonBeautyBooking`, child `SalonBeautyBookingService` (cả hai có `IMultiTenant`).

## Entities chính
- `SalonBeautyCustomer`, `SalonBeautyServiceCategory`, `SalonBeautyService`, `SalonBeautyStylist`.
- `SalonBeautyBooking` (state machine NEW→CONFIRMED→COMPLETED, nhánh CANCELLED).
- `SalonBeautyBookingService` (snapshot dịch vụ).
- `SalonBeautyCustomerLoyaltyBalance` + `SalonBeautyCustomerLoyaltyTransaction` (ledger).

## UI & tính năng
- **Stylist:** Index/Create/Edit, inline toggle `IsShowOnApp`, avatar upload base64, badge role/level.
- **Booking:** Index/Create/Edit/Detail, dual permission, API `/api/app/salon-beauty/bookings/*`. History + change stylist (validate cùng Location).
- **Location + TimeSlot:** CRUD, group-by-stylist, FullCalendar (On/Full/Off), capacity + PeakHour (=3 đỏ), `TimeSlotId` driving WorkDate/Time/Location; migrations 20260520052000, 20260520100420.
- **Customer Detail redesign:** KPI cards, tier NEW/REGULAR/VIP/DIAMOND, purchase history + deposit ledger.
- **Deposit + Loyalty:** DEP{date}{seq}, 2-step approval ACID (`_uowManager.Begin`), ledger, ExchangeRate per-tenant.

## MiniApp
- Payment endpoints clone Pro/Fnb; `SalonBeautyPaymentMethod`; orderId=`{BookingCode}_{ts}`.
- Location/TimeSlot/Stylist filter (business-establishments + tee-times từ DB, Stylist filter theo LocationId).
- ZBS: enqueue BookingCreated + UpdateStatus=Completed; ServiceReview.

## Lưu ý
- Booking MARS fix: insert parent trước, child qua repo (tránh IdentityConflict). Xem `../RULES.md`.
- Chi tiết implementation gốc: `../memory/modules/salon-beauty/final-implementation.md` + `implementation-progress.md`.
