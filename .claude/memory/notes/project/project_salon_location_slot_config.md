---
name: salon-location-slot-config
description: "Salon Location bổ sung SlotDuration/BufferTime/MaxCapacity + TimeSlot Capacity/BookedCount/Manual override + auto-generate ranges (2026-05-20)"
metadata:
  node_type: memory
  type: project
  originSessionId: a89ec214-fd79-45a5-a94c-5c74851065ce
---

# Salon Beauty - Location slot config + TimeSlot capacity/booked + auto-gen

## Status (2026-05-20)
Hoàn thành:
- Entity `SalonBeautyLocation` thêm `SlotDuration` (default 60), `BufferTime` (default 0), `MaxCapacityPerSlot` (default 1)
- Entity `SalonBeautyTimeSlot` thêm `Capacity` (default 1), `BookedCount` (default 0), `IsManualOverride` (default false)
- EF FluentAPI `MultiTenancyDbContextModelCreatingExtensionsSalonBeauty.cs` map column + default
- Migration `20260520100420_Add_LocationSlotConfig_TimeSlotCapacity` (3 cột Location + 3 cột TimeSlot)
- DTOs:
  - `CreateSalonBeautyLocationDto`/`UpdateSalonBeautyLocationDto` thêm 3 trường + `[Range]` validation
  - `SalonBeautyLocationDto` + `SalonBeautyLocationLookupDto` (lookup gồm cả OpenTime/CloseTime/SlotDuration/BufferTime/MaxCapacityPerSlot — UI cần đọc cấu hình lúc auto-gen ranges)
  - `TimeRangeDto` thêm `Capacity?` (nullable, fallback = MaxCapacityPerSlot khi BE build slot)
  - `SalonBeautyTimeSlotDto` thêm `Capacity`, `BookedCount`, `CapacityText` ("0/1"), `IsManualOverride`
- AppService:
  - `SalonBeautyLocationAppService` thêm `NormalizeAndValidate` BR-01..06, lookup trả full config
  - `SalonBeautyTimeSlotAppService` thêm `GenerateRangesByLocationAsync(locationId)` (auto-gen) + `GetStylistLookupAsync(locationId?)` (cascade), `ValidateRangesAgainstLocation` (range không vượt OpenTime/CloseTime, capacity ≤ MaxCapacityPerSlot), `BuildSlots` set Capacity từ TimeRangeDto.Capacity ?? location.MaxCapacityPerSlot, `UpdateStatusAsync` set `IsManualOverride = true`
- Web Pages:
  - `SalonBeautyLocations/Create+Edit+Detail` thêm 3 input + 1 row Detail
  - `SalonBeautyTimeSlots/Create+Edit Modal` thêm nút "Tự sinh khung giờ" + cột capacity per range + hint config Location
  - `index.js`: cascade location→stylist, auto-generate (gọi `slotService.generateRangesByLocation`), capacity per range với hint "0/cap" + clamp ≤ MaxCapacityPerSlot
  - `calendar.js`: filter stylist dùng `slotService.getStylistLookup(locationId)` thay cho `stylistService.getList`
- Localization vi.json: thêm key `SalonBeautyLocations:SlotDuration|SlotDurationHelp|SlotDurationInvalid|SlotDurationTooLarge|BufferTime|BufferTimeHelp|BufferTimeInvalid|MaxCapacityPerSlot|MaxCapacityPerSlotHelp|MaxCapacityInvalid` + `SalonBeautyTimeSlots:Capacity|BookedCount|CapacityExceedsLocation|RangeOutsideLocation|AutoGenerate|AutoGenerateHint` + `Minutes`

**Why:** Theo SRS section IV.2.0 (Location Management) và IV.2.4 (Time Slot Calendar). Cần config động SlotDuration/BufferTime/MaxCapacity ở cấp cơ sở để auto-generate khung giờ khi tạo lịch, đồng thời track Capacity vs BookedCount để tự chuyển trạng thái FULL khi đầy.
**How to apply:**
- Khi tạo TimeSlot từ UI, nên bấm "Tự sinh khung giờ theo cơ sở" để fill ranges, sau đó admin chỉnh capacity nếu khác max.
- Khi BE build slot, Capacity từ `TimeRangeDto.Capacity ?? location.MaxCapacityPerSlot`, clamp ≤ location.MaxCapacityPerSlot.
- Khi book khách: increment BookedCount; nếu `BookedCount >= Capacity` thì set Status=Full (chưa implement auto, cần làm khi BookingService Create slot lookup).
- Khi admin đổi status thủ công trên Calendar → set `IsManualOverride = true`, không cho recalculate đè trong tương lai.

## Auto-generate Algorithm
```
current = location.OpenTime
slotDur = TimeSpan(location.SlotDuration min)
buffer  = TimeSpan(location.BufferTime min)
while current < closeTime:
    end = min(current + slotDur, closeTime)
    if (end - current) < 5min: break
    push range(current, end, capacity = MaxCapacityPerSlot)
    current = end + buffer
```
VD: open=09:00, close=18:00, slot=60, buffer=10 → [09:00-10:00, 10:10-11:10, ..., 17:50-18:00].

## API mới
- `POST /api/app/salon-beauty/time-slot/generate-ranges-by-location/{locationId}` → `List<TimeRangeDto>`
- `POST /api/app/salon-beauty/time-slot/get-stylist-lookup?locationId=...` → `List<SalonBeautyStylistLookupDto>`

## Hierarchy validation
`MaxCapacityPerSlot (Location) >= Capacity (TimeSlot) >= BookedCount (TimeSlot)`
- BE check `Capacity > MaxCapacityPerSlot` → throw `SalonBeautyTimeSlots:CapacityExceedsLocation`
- BE check Range trong khoảng OpenTime/CloseTime → throw `SalonBeautyTimeSlots:RangeOutsideLocation`
- Frontend clamp capacity input bằng `max=MaxCapacityPerSlot`

## File Map
- Domain: `SalonBeautyLocation.cs`, `SalonBeautyTimeSlot.cs`
- EF: `MultiTenancyDbContextModelCreatingExtensionsSalonBeauty.cs`, Migration `20260520100420_*`
- App.Contracts: `SalonBeautyLocations/{CreateDto,UpdateDto,Dto,ISalonBeautyLocationAppService}.cs` + `SalonBeautyTimeSlots/{CreateDto,UpdateDto,Dto,ISalonBeautyTimeSlotAppService}.cs`
- App: `SalonBeautyLocationAppService.cs`, `SalonBeautyTimeSlotAppService.cs`
- Web: `Pages/SalonBeautyLocations/{Create,Edit,Detail}Modal.cshtml(.cs)`, `Pages/SalonBeautyTimeSlots/{Create,Edit}Modal.cshtml`, `Pages/SalonBeautyTimeSlots/index.js`, `calendar.js`
- Web CSS: `wwwroot/pages/salon/timeslot-shared.css` (timeslot-capacity-wrap, timeslot-auto-generate-btn, timeslot-location-hint)
- Localization: `Domain.Shared/Localization/MultiTenancy/vi.json`

## Verification
- `dotnet build src/Genora.MultiTenancy.Web/Genora.MultiTenancy.Web.csproj` → Build succeeded (0 errors, warnings cũ)
- Migration build clean (cần kill Web process khi đang chạy mới sinh được Up/Down body — khi process lock dll, EF thấy snapshot stale → migration body rỗng, phải clean bin/obj của Domain + EF + Web rồi rebuild lại; xem [[feedback_ef_migration_dll_lock]] nếu hit case này lần nữa)
- DB cần chạy `dotnet ef database update -s ../Genora.MultiTenancy.Web` để apply 6 cột mới

## Bug fixes liên quan
- **TimeSlot Create modal: Location dropdown trống, Stylist dropdown trống**: nguyên nhân do `loadStylistsIntoForm` cũ gọi `stylistService.getList({...})` không kèm location → đã đổi sang `slotService.getStylistLookup(locationId)`. Cascade hoạt động khi đổi Location.

## Related
- [[project_salon_location_timeslot_ui]]
- [[project_salon_stylist_booking_locationid]]
- [[project_salon_stylist_ui]]
- [[project_salon_booking_ui]]
