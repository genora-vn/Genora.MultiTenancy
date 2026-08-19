---
name: salon-stylist-booking-locationid
description: "Stylist + Booking gắn LocationId, đổi enum Role/Level, UI filter cơ sở (2026-05-20)"
metadata: 
  node_type: memory
  type: project
  originSessionId: a89ec214-fd79-45a5-a94c-5c74851065ce
---

# Salon Beauty - Thêm LocationId vào Stylist & Booking + đổi enum Role/Level

## Status (2026-05-20)
Hoàn thành:
- Entity `SalonBeautyStylist` + `SalonBeautyBooking` thêm `Guid? LocationId` + nav `Location`
- EF config FK + index `IX_..._TenantId_LocationId`, OnDelete=Restrict
- Migration: `20260520052000_Add_LocationId_To_StylistsAndBookings`
- Enum đổi:
  - `SalonBeautyStylistRole`: HairStylist=1, Shampoo=2, NailLashes=3, SkincareSpa=4, Other=5
  - `SalonBeautyStylistLevel`: Junior=1, Senior=2, Manager=3
- DTOs: thêm LocationId/LocationName cho Stylist DTOs, Booking DTOs (Detail/List/Calendar/Lookup), GetSalonBeautyListInput, GetSalonBeautyBookingListInput
- AppService: `SalonBeautyStylistAppService` + `SalonBeautyBookingAppService` inject `IRepository<SalonBeautyLocation, Guid>`, filter LocationId, BuildLocationMap, set entity.LocationId Create/Update
- AppService API: `GetCalendarEventsAsync(..., Guid? locationId)` và `GetStylistLookupAsync(Guid? locationId)` để cascade filter
- MiniApp: `MiniAppSalonBeautyBookingAppService.CreateMiniAppAsync` set `booking.LocationId = input.LocationId ?? stylist.LocationId`

**Why:** Cần phân biệt nhân viên/lịch đặt theo cơ sở vật lý khi salon có nhiều chi nhánh. Booking phải biết khách đến cơ sở nào để báo cáo doanh thu, lập lịch theo cơ sở.
**How to apply:** Khi list/filter Stylist hoặc Booking, luôn xem xét LocationId. Khi tạo booking từ MiniApp/CMS, fallback `stylist.LocationId` nếu input chưa có. Khi sửa enum Role/Level lookup phải dùng key `Enum:SalonBeautyStylistRole.HairStylist|Shampoo|NailLashes|SkincareSpa|Other` và `Enum:SalonBeautyStylistLevel.Junior|Senior|Manager`.

## UI Pattern: Cascade Location → Stylist
Index Booking + Calendar Booking đều có select `#BookingLocationFilter` / `#LocationFilterSelect` đứng trước Stylist. Khi đổi location, gọi `getStylistLookup(locationId)` rồi reload events:
```js
$('#BookingLocationFilter').on('change', function () {
    loadStylistFilter($(this).val());
    reloadAll(true);
});
```

CreateModal/EditModal Stylist Booking cũng có `#BookingLocationSelect` cascade xuống `#BookingStylistSelect`. Default = location đầu trong lookup.

Stylist Index có riêng `#SalonStylistLocationFilter` (không cascade), gửi `locationId` qua `buildListInput`.

## DetailModal Stylist
Render thêm 3 cell ở đầu grid: LocationName / Phone / Gender / Role / Level / Experience / TotalBooking / RatingAvg / IsShowOnApp / SortOrder.

## Detail Booking
Block "Thông tin đặt lịch" thêm row "Cơ sở" (icon `fa-building-o`) phía trên "Ngày giờ sử dụng".

## Localization mới
- `SalonBeautyLocation:PageTitle` (vi.json) — dùng làm label cho dropdown cơ sở ở Stylist + Booking pages
- `SalonBeautyStylists:AllLocations`, `SalonBeautyStylists:LocationPlaceholder` (vi.json + en.json)

## File Map
- Domain: `SalonBeautyStylist.cs`, `SalonBeautyBooking.cs` (+ LocationId, Location nav)
- Domain.Shared: `Enums/SalonBeautyStylistRole.cs`, `Enums/SalonBeautyStylistLevel.cs` (rewritten)
- EF: `MultiTenancyDbContextModelCreatingExtensionsSalonBeauty.cs` (+ FK config), Migration `20260520052000_*`
- App.Contracts: DTOs + IAppService trong `AppDtos/SalonBeauties/SalonBeautyStylists/` và `AppDtos/SalonBeauties/SalonBeautyBookings/`
- App: `SalonBeautyStylistAppService.cs`, `SalonBeautyBookingAppService.cs`, `MiniAppSalonBeautyBookingAppService.cs`
- Web: `SalonBeautyStylists/{Index,CreateModal,EditModal,DetailModal}.cshtml(.cs)` + `index.js`
- Web: `SalonBeautyBookings/{Index,CreateModal,EditModal,Detail,Calendar}.cshtml(.cs)` + `index.js` + `calendar.js`
- Web: `Pages/SalonBeautyBookings/IndexModel`, `CalendarModel` injects `ISalonBeautyLocationAppService` để cấp `LocationItems` cho dropdown server-rendered

## Verification
- `dotnet build src/Genora.MultiTenancy.Web/Genora.MultiTenancy.Web.csproj` → Build succeeded
- Migration build clean qua `dotnet ef migrations add`
- DB chưa apply migration — tenant cần chạy `dotnet ef database update -s ../Genora.MultiTenancy.Web` (host) hoặc tự deploy schema với multi-tenant

## Related
- [[project_salon_location_timeslot_ui]]
- [[project_salon_stylist_ui]]
- [[project_salon_booking_ui]]
- [[project_salon_booking_mars_fix]]
- [[project_salon_stylist_ui_updated]]
