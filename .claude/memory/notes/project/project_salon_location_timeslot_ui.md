---
name: salon-location-timeslot-ui
description: Salon Beauty Location + TimeSlot full CRUD + FullCalendar (UC-LC02 + 2.4) - hoàn thành 2026-05-19
metadata: 
  node_type: memory
  type: project
  originSessionId: 756fb1cc-f3c2-4e53-8bcc-7009c469f55d
---

# Salon Beauty - Location + TimeSlot Module

## Status (2026-05-19)
Hoàn thành đầy đủ:
- Entity SalonBeautyLocation + SalonBeautyTimeSlot + enum SalonBeautyTimeSlotStatus (Off/On/Full)
- EF config + migration `20260520015535_Add_SalonBeautyLocations_TimeSlots`
- Permissions (4 keys × 2 sides) đặt trong group `salonBeautyGroup` / `salonBeautyGroupHost` với `RequireFeatures(SalonBeauty.Management)` cho tenant
- DTOs + AppService (Location dùng FeatureProtectedCrudAppService, TimeSlot dùng ApplicationService thuần do logic group/replace)
- UI: 2 module dưới `Pages/SalonBeautyLocations` và `Pages/SalonBeautyTimeSlots`
- Menu: tên group "Cơ sở & Giờ hẹn" tái sử dụng `MenuGroup.SalonBeautyAndTeeTimes` (alias dynamic của GolfAndTeeTimes khi SalonBeauty active)
- Localization vi.json bổ sung đầy đủ key `SalonBeautyLocations:*`, `SalonBeautyTimeSlots:*`, `Day:Mon..Sun`, `Enum:SalonBeautyTimeSlotStatus.*`

**Why:** UC-LC02 (cơ sở salon) + 2.4 (lịch làm việc stylist) trong SALON_BEAUTY_SYSTEM_MANAGEMENT. Tách entity Location vì cần quản lý độc lập với booking.
**How to apply:** Khi mở rộng tính năng salon (vd: dịch vụ kèm cơ sở, slot booking lookup) thì import từ 2 module này, dùng `salonBeautyLocation.getLookup()` để lấy cơ sở, `salonBeautyTimeSlot.getCalendarEvents()` để xem khung giờ.

## Key Patterns

### Per-row TimeSlot vs. Header/Detail
TimeSlot lưu **flat row per (stylist, date, time-range)**, không có header. Lúc tạo, BE tự sinh nhiều row cho `(FromDate..ToDate) × Ranges × WeekdayMask`. Update theo stylist = delete-all-then-recreate trong scope stylist (xem `SalonBeautyTimeSlotAppService.UpdateByStylistAsync`).

### Group-by-stylist trong list
List page show 1 dòng/stylist với min/max date + min/max time + count, dùng GroupBy LINQ trên slot rồi enrich Stylist + Location qua dictionary để tránh N+1. Frontend không nhận row gốc — nhận `SalonBeautyTimeSlotGroupedDto`.

### Calendar (FullCalendar 6.1.11) flow
- Page `Calendar.cshtml` dùng FullCalendar locale 'vi', timeGridWeek default
- Sidebar filter (Location, Stylist, Status) → refetchEvents
- Click event → modal với 3 nút On/Full/Off → call `slotService.updateStatus(id, {status})`
- Service method `getCalendarEventsAsync` nhận FromDate/ToDate từ FullCalendar `info.startStr/endStr`

### CSS prefix `.location-*` và `.timeslot-*`
Riêng cho mỗi module, không dùng chung với `.stylist-*`. Files:
- `wwwroot/pages/salon/location-shared.css`
- `wwwroot/pages/salon/timeslot-shared.css`

## File Map

### Domain
- `Domain/DomainModels/AppSalonBeauty/SalonBeautyLocation/SalonBeautyLocation.cs`
- `Domain/DomainModels/AppSalonBeauty/SalonBeautyTimeSlot/SalonBeautyTimeSlot.cs`
- `Domain.Shared/Enums/SalonBeautyTimeSlotStatus.cs`

### EF
- `EntityFrameworkCore/MultiTenancyDbContextModelCreatingExtensionsSalonBeauty.cs` (+ Location/TimeSlot config, schema "Salon")
- `EntityFrameworkCore/MultiTenancyDbContext.cs` (+ DbSet)
- `EntityFrameworkCore/Migrations/20260520015535_Add_SalonBeautyLocations_TimeSlots.cs`

### Application
- `Application/AppServices/SalonBeauties/SalonBeautyLocationAppService.cs` (FeatureProtectedCrudAppService, dùng IManageImageService, subFolder "salon-locations")
- `Application/AppServices/SalonBeauties/SalonBeautyTimeSlotAppService.cs` (ApplicationService thuần)
- `Application/AppServices/SalonBeauties/SalonBeautyApplicationAutoMapperProfile.cs` (added Location + TimeSlot maps)

### Permissions + Localization
- `Application.Contracts/Permissions/MultiTenancyPermissions.cs` (+ 4 nhóm × 4 op)
- `Application.Contracts/Permissions/MultiTenancyPermissionDefinitionProvider.cs`

### UI
- `Web/Pages/SalonBeautyLocations/{Index,CreateModal,EditModal,DetailModal}.cshtml(.cs)` + `index.js`
- `Web/Pages/SalonBeautyTimeSlots/{Index,CreateModal,EditModal,Calendar}.cshtml(.cs)` + `index.js` + `calendar.js`
- `Web/wwwroot/pages/salon/{location,timeslot}-shared.css`

## Validation Rules
- Location: phone `^0\d{9,10}$`, OpenTime < CloseTime, IsShowOnApp requires IsActive
- TimeSlot: ranges sorted không trùng, FromDate ≤ ToDate, WeekdayMask 0-127 (0 = áp dụng tất cả 7 ngày)

## Related
- [[project_salon_stylist_ui]]
- [[project_salon_booking_ui]]
- [[feedback_abp_dual_permission_pattern]]
- [[feedback_abp_permission_group_pattern]]
- [[feedback_permission_require_features]]
