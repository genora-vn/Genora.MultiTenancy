---
name: CalendarSlot SlotAvailable init + reset pattern
description: Khi tạo mới slot gán SlotAvailable=MaxSlots; khi update/re-import phải trừ lại số golfer của booking active
type: project
originSessionId: 03f1f8f0-a727-4f28-8cd7-f688971b0a28
---
`CalendarSlot.SlotAvailable` là tồn kho chỗ trống, giảm khi booking được tạo và cộng lại khi booking bị hủy (xử lý ở `MiniAppBookingAppService`). Tại `AppCalendarSlotService` (file Application/AppServices/AppCalendarSlots/AppCalendarSlotService.cs):

- **Tạo mới** (CreateAsync thủ công + ImportExcelAsync nhánh pending/newCalendar): gán `SlotAvailable = input.MaxSlots` (hoặc `r.MaxSlots`).
- **Update** (UpdateAsync thủ công + ImportExcelAsync nhánh `existingCalendar != null`): reset theo công thức `MaxSlots − Σ NumberOfGolfers của booking active`. Dùng helper `ResolveSlotAvailableAsync(slotId, maxSlots)` đã thêm sẵn; query `_bookingRepository` với filter `Status != CancelledRefund && != CancelledNoRefund`.
- Đã inject thêm `IRepository<Booking, Guid> _bookingRepository` vào constructor (thứ tự tham số sau `specialDateRepository`, trước `dataFilter`).

**Why:** Reset cứng `SlotAvailable = MaxSlots` khi update sẽ cho phép over-booking nếu slot đã có booking đang active. Helper trừ lại để giữ tồn kho đúng.

**How to apply:** Bất kỳ flow nào chỉnh MaxSlots của CalendarSlot đã tồn tại đều phải đi qua `ResolveSlotAvailableAsync` thay vì gán trực tiếp. Nếu viết flow mới tạo CalendarSlot thì cứ `SlotAvailable = MaxSlots`. Enum hủy hiện tại: `BookingStatus.CancelledRefund=4`, `CancelledNoRefund=5`.

**Liên quan:** `AppCalendarExcelImporter` chỉ parse Excel → DTO, không tạo entity; entity tạo trong `ImportExcelAsync`. DTO `AppCalendarSlotExcelRowDto` không cần field SlotAvailable.
