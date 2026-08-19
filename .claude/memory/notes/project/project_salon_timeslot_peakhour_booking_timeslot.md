---
name: project-salon-timeslot-peakhour-booking-timeslot
description: Salon Beauty — enum PeakHour=3 cho TimeSlot + Booking gắn TimeSlotId driving WorkDate/StartTime/EndTime/LocationId
metadata: 
  node_type: memory
  type: project
  originSessionId: f5d12a66-b7a5-4b6f-be2a-569ceedf31bf
---

Salon Beauty hoàn thiện 2 luồng (5/2026):

**1. PeakHour cho TimeSlot**
- Enum `SalonBeautyTimeSlotStatus`: Off=0 (disable), On=1, Full=2 (disable), **PeakHour=3** (Mini App hiển thị đỏ nhưng vẫn đặt được)
- DTO `TimeRangeDto` thêm `IsPeakHour`; service `BuildSlots` map IsPeakHour→Status=PeakHour khi defaultStatus!=Off
- TimeSlot modal: toggle "Giờ cao điểm" mỗi range row (trước remove button); CSS `.timeslot-peak-wrap`/`.timeslot-event.peak`
- Calendar view legend + UpdateSlotStatusModal có button Peak Hour; calendar.js `statusKey/statusText` xử lý status=3

**2. Booking gắn TimeSlotId**
- `SalonBeautyBooking.TimeSlotId` (Guid?) + nav `TimeSlot`; FK Restrict; index TenantId+TimeSlotId
- Migration `20260521055553_Add_TimeSlotId_To_SalonBeautyBookings`
- API `GetAvailableDatesAsync(stylistId, fromDate, toDate, locationId?)` filter Status!=Off + IsShowOnApp + distinct WorkDate
- API `GetAvailableSlotsAsync(stylistId, workDate, locationId?)` filter Status not in (Off,Full) + IsShowOnApp + BookedCount<Capacity
- Booking modal CMS: cascade location→stylist→loadAvailableDates→flatpickr disable filter→loadAvailableSlots→dropdown; submit gửi TimeSlotId
- Edit modal: preselect initial date+slot từ booking hiện tại (mở rộng fromDate/toDate để bao initialBookingIso)
- AppService Create/Update: nếu có TimeSlotId → fetch slot, override BookingDate/StartTime/EndTime/LocationId từ slot, validate Off/Full/Capacity, tăng BookedCount + auto-flip Full khi đầy (nếu !IsManualOverride)
- Update: nếu đổi TimeSlotId → giảm BookedCount slot cũ + auto-revert Full→On (nếu !IsManualOverride), tăng slot mới
- MiniApp `CreateMiniAppAsync`: inject `_timeSlotRepository`, áp dụng cùng pattern; controller `[HttpPost("bookings")]` không cần đổi (DTO đã có TimeSlotId/LocationId/CustomerNote/Surcharge/Discount/Items)

**Why:** thay vì admin tự nhập StartTime/EndTime, gắn booking vào slot configured để (1) đảm bảo capacity tracking, (2) reuse policy stylist on/off của TimeSlot, (3) Mini App + CMS chia sẻ một nguồn truth duy nhất về availability.

**How to apply:**
- Tạo booking mới (CMS hoặc MiniApp): UI gửi TimeSlotId → backend tự lấy WorkDate/StartTime/EndTime/LocationId từ slot, không tin input từ client
- Hủy booking: TODO — cần giảm BookedCount slot tương ứng (chưa implement, [[feedback_signalr_try_catch]] style không fail flow)
- Khi extend status enum mới ngoài Off/On/Full/PeakHour: cập nhật cả `GetAvailableSlotsAsync` filter, `MapToDto.IsPeakHour` flag, calendar.js statusKey/statusText, CSS legend, vi.json `Enum:SalonBeautyTimeSlotStatus.{name}`

Liên quan: [[project_salon_booking_ui]], [[project_salon_location_slot_config]], [[project_salon_location_timeslot_ui]], [[project_salon_stylist_booking_locationid]]
