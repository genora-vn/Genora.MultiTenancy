---
name: salon-timeslot-ui-polish
description: "Salon TimeSlot UI polish — capacity-hint current/max, BS5-safe close modal, FullCalendar v6 nested text color (2026-05-21)"
metadata: 
  node_type: memory
  type: project
  originSessionId: f5d12a66-b7a5-4b6f-be2a-569ceedf31bf
---

# Salon Beauty TimeSlot — UI Polish (2026-05-21)

Ba chỉnh sửa nhỏ nhưng có pattern dùng lại cho các module khác.

## 1. capacity-hint hiển thị `current/max` (không phải `booked/cap`)

**Why:** PO yêu cầu hint phản ánh giá trị admin nhập (clamp ≤ MaxCapacityPerSlot của Location), số sau dấu `/` luôn = `MaxCapacityPerSlot` cứng từ config Location, không đổi theo input.

**How to apply:**
- Tách 2 span: `<span class="timeslot-capacity-current">{cap}</span>/<span class="timeslot-capacity-max">{max}</span>`
- `.timeslot-capacity-current` cập nhật mỗi lần bấm `+/-` hoặc input change
- `.timeslot-capacity-max` set 1 lần lúc render `buildRangeRow(...)`, không đổi
- Khi `MaxCapacityPerSlot === 1`: thêm `disabled` vào nút `+/-`, `readonly` vào input → user không tăng giảm được
- Handler `+/-` thêm guard `if ($(this).is(':disabled')) return;`

Files: `Pages/SalonBeautyTimeSlots/CreateModal.cshtml`, `EditModal.cshtml`.

## 2. Auto-close modal sau khi save thành công (BS5-safe)

**Why:** `$form.closest('.modal').modal('hide')` không hoạt động khi project nâng lên Bootstrap 5 (jQuery plugin `.modal()` không còn được Bootstrap 5 native cung cấp; ABP có thể inject jQuery plugin nhưng không đáng tin nếu setup khác). Thông báo success hiện nhưng modal không đóng.

**How to apply (pattern chuẩn — đã dùng trong `SalonBeautyBookings/index.js`):**
```js
var modalEl = $form.closest('.modal')[0];
if (modalEl && window.bootstrap && bootstrap.Modal) {
    bootstrap.Modal.getOrCreateInstance(modalEl).hide();
} else if (modalEl && $.fn.modal) {
    $(modalEl).modal('hide');   // fallback BS4 / jQuery plugin
}
```
Đặt sau `abp.notify.success(...)` và trước `salonTimeSlotReload()`.

Files: cùng 2 modal trên.

## 3. FullCalendar v6 — text mờ ở week/day view

**Why:** FullCalendar v6 render text qua nested DOM `.fc-event-main / .fc-event-main-frame / .fc-event-title / .fc-event-time`, không phải trực tiếp trên `.fc-event`. CSS cũ chỉ set `color` ở `.timeslot-event.on/.off/.full` nên text trong week/day (timeGrid view) bị inherit color mặc định của FullCalendar (xám/trắng nhạt) → khó đọc trên nền pastel. Month view dùng dotgrid (`.fc-event` flat) nên không bị.

**How to apply:**
```css
.timeslot-event .fc-event-main,
.timeslot-event .fc-event-main-frame,
.timeslot-event .fc-event-title,
.timeslot-event .fc-event-title-container,
.timeslot-event .fc-event-time {
    color: inherit !important;
    font-weight: 600 !important;
}
.timeslot-event.on .fc-event-main,
.timeslot-event.on .fc-event-title,
.timeslot-event.on .fc-event-time { color: #155724 !important; }
.timeslot-event.off ... { color: #6c757d !important; }
.timeslot-event.full ... { color: #721c24 !important; }
```

**Lưu ý chung cho FullCalendar v6:**
- Đừng chỉ set màu ở `.fc-event` — luôn target `.fc-event-main + .fc-event-title + .fc-event-time` cho timeGrid view.
- Dùng `!important` vì FC inject inline style theo eventColor/textColor.

File: `wwwroot/pages/salon/timeslot-shared.css`.

## Related
- [[project_salon_timeslot_modal_fix]] (pattern gốc render server-side + inline script)
- [[project_salon_location_slot_config]] (nguồn MaxCapacityPerSlot)
- [[project_salon_booking_ui]] (pattern hideModal BS5-safe)
