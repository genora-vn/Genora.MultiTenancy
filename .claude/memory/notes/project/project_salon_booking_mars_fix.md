---
name: project-salon-booking-mars-fix
description: "SalonBeautyBooking MiniApp + Admin — fix EF tracking + FK conflict bằng MARS pattern (insert parent trước, child sau) và set TenantId thủ công"
metadata: 
  node_type: memory
  type: project
  originSessionId: a76ee1f3-5825-4922-ac30-1cd18179d441
---

Cả `MiniAppSalonBeautyBookingAppService.CreateMiniAppAsync` và `SalonBeautyBookingAppService.CreateAsync` đã từng dính bug khi insert nhiều `SalonBeautyBookingService`.

**Nguyên nhân tổng hợp:**
1. Cascade insert qua `booking.BookingServices.Add(...)` rồi `_bookingRepository.InsertAsync(booking, autoSave:true)` → EF tracking conflict / IdentityConflict khi nhiều child cùng `Guid.Empty`.
2. `SalonBeautyBookingService` ban đầu thiếu `IMultiTenant` → tenant dùng separate DB (vd `AmiHairSalon`) bị ABP route child sang host DB `GenoraMultiTenancy` → FK 547 `FK_AppSalonBeautyBookingServices_AppSalonBeautyBookings_BookingId` (booking ở tenant DB, child cố lookup ở host DB).

**Fix chuẩn (cả MiniApp lẫn admin):**
1. `_bookingRepository.InsertAsync(booking, autoSave: true)` — insert parent trước.
2. Loop `_bookingServiceRepository.InsertAsync(new SalonBeautyBookingService {..., TenantId = CurrentTenant.Id}, autoSave: true)` — insert từng child riêng, set TenantId tường minh để ABP route đúng tenant DB.
3. `SalonBeautyBookingService` implement `IMultiTenant` + cột `TenantId` (migration `20260519083657_Add_TenantId_To_SalonBeautyBookingServices`, có backfill từ parent Booking).

**Why:** Tenant `AmiHair` ở staging dùng DB `AmiHairSalon`. Trên local host DB không tách nên test bằng host account vẫn pass. Chỉ vỡ khi chạy với tenant + separate DB. Liên quan [[project_multitenant_db_routing]] và [[feedback_mars_autosave_pattern]].

**How to apply:** Mọi entity có child collection trong Salon Beauty module phải:
- Implement `IMultiTenant` trên child entity (kèm migration TenantId).
- Dùng pattern insert riêng từng child qua repository, không dùng navigation property cascade.
- Set TenantId thủ công khi tạo child instance trong AppService.
