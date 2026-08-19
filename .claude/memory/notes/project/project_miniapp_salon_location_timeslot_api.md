---
name: project-miniapp-salon-location-timeslot-api
description: MiniApp APIs cho Location (business-establishments) và TimeSlot (tee-times) thay hardcode bằng dữ liệu thật từ DB
metadata: 
  node_type: memory
  type: project
  originSessionId: 61ad6b0b-b66d-4be7-ae42-3e5bee380a27
---

## MiniApp Salon Beauty — Location + TimeSlot + Stylist filter APIs

**Ngày:** 2026-05-21

### 1. API GET /api/mini-app/salon-beauty/business-establishments
- Service: `MiniAppSalonBeautyLocationAppService.GetListAsyncLocations()`
- Input DTO: `GetMiniAppLocationListInput` (Filter, IsActive, IsShowOnApp)
- Output DTO: `MiniAppSalonBeautyLocationDto` (Id, Name, Address, Phone, OpenTime, CloseTime, ImageUrl)
- Filter theo Name/Address/Phone, sort theo SortOrder + Name

### 2. API GET /api/mini-app/salon-beauty/tee-times
- Service: `MiniAppSalonBeautyTimeSlotAppService.GetListAsyncTimeSlots()`
- Input DTO: `GetMiniAppTimeSlotListInput` (LocationId, Date, StylistId)
- Output DTO: `MiniAppSalonBeautyTimeSlotDto` (TimeSlotId, WorkDate, StartTime, EndTime, Status, IsShowOnApp, BookedCount, Capacity)
- Filter: IsShowOnApp=true, Status != Off; sort theo WorkDate + StartTime

### 3. Stylist filter by LocationId
- `MiniAppSalonBeautyStylistAppService.GetListMiniAppAsync()` thêm filter `LocationId` từ `GetSalonBeautyListInput.LocationId`

### 4. Booking CreateMiniAppAsync
- Đã có sẵn xử lý TimeSlotId + LocationId trong `MiniAppSalonBeautyBookingAppService.CreateMiniAppAsync()`
- Khi có TimeSlotId: lấy WorkDate/StartTime/EndTime/LocationId từ slot, validate Off/Full/Capacity, tăng BookedCount

**Why:** Mini App cần lấy dữ liệu thật từ cấu hình Location/TimeSlot thay vì hardcode.

**How to apply:** Khi cần thêm API mới cho Mini App salon, follow pattern: Interface trong Application.Contracts → AppService trong Application → Route trong SalonBeautyMiniAppController.
