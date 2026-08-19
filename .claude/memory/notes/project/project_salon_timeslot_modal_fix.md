---
name: salon-timeslot-modal-fix
description: "TimeSlot modal — server-side render Location + inline script trong modal cshtml (pattern Booking), flatpickr d/m/Y, default location đầu tiên + cascade stylist, range theo location config, capacity +/- clamp 1..MaxCapacityPerSlot"
metadata: 
  node_type: memory
  type: project
  originSessionId: 88fa79db-60dc-4351-a0c8-343fbd07678f
---

Fix modal Thêm/Cập nhật Lịch làm việc (`SalonBeautyTimeSlots`).

**Why:** lần đầu dùng external `index.js` + `shown.bs.modal` event → dropdown Location/Stylist rỗng, flatpickr không bind, ranges không init. Nguyên nhân: ABP `ModalManager` inject HTML qua AJAX, timing event không đáng tin; `genora.multiTenancy.appServices.*` proxy đôi khi chưa load khi event fire. Pattern `SalonBeautyBookings` (server-side render dropdown + inline script trong modal cshtml) chạy ngay khi modal mount → ổn định hơn.

**How to apply (pattern chuẩn cho modal Salon Beauty có cascade Location→Stylist):**

1. **Server-side render Location dropdown** trong `CreateModal.cshtml.cs` / `EditModal.cshtml.cs`:
   - Inject `ISalonBeautyLocationAppService`, gọi `GetLookupAsync()` trong `OnGetAsync`.
   - Build `List<SelectListItem> LocationItems` với option đầu tiên `Selected=true` (default first location).
   - Expose toàn bộ `List<SalonBeautyLocationLookupDto> Locations` để serialize ra JSON cho client cache config (open/close/slotDuration/bufferTime/maxCapacityPerSlot).
   - Edit modal: gọi thêm `slotAppService.GetByStylistAsync(stylistId)` để pre-fill ranges/dates/mask/note.

2. **Modal cshtml** dùng `<select asp-items="Model.LocationItems">` (ASP.NET tự render `<option selected>`). Date input là `<input type="text" value="@Model.DefaultFromDate.ToString("dd/MM/yyyy")">`. Embed config qua `data-locations='@Html.Raw(JsonSerializer.Serialize(...))'` ở `.timeslot-modal` root.

3. **Inline `<script>` ngay dưới form** (không dùng external file):
   - Đọc `$modalRoot.data('locations')` và `data('ranges')` (jQuery tự parse JSON).
   - Init `flatpickr` với `{dateFormat:'d/m/Y', allowInput:true}` ngay lập tức (DOM đã ready khi script chạy).
   - Init dropdown Stylist qua `slotService.getStylistLookup(initialLocId)` ngay (Location đã có default).
   - Cascade: `$('#...LocationSelect').on('change', ...)` → reload Stylist + reset ranges theo config Location mới (Create), hoặc reset ranges theo Location mới (Edit).
   - Inline script chạy mỗi lần modal mount (do ABP load lại HTML), nên không cần worry về duplicate listener.

4. **Range default & Add Range:**
   - Default 1 dòng: `start=OpenTime`, `end=min(OpenTime+SlotDuration, CloseTime)`, capacity=1.
   - Add Range: `start = last_end + buffer_time`, `end = min(start + slot_duration, close)`. Nếu `start≥close` hoặc `end-start<5p` → notify `RangeOutsideLocation`.

5. **Capacity ±:** nút `.timeslot-cap-inc/dec` clamp [1..MaxCapacityPerSlot], hint `bookedCount/<span class=timeslot-capacity-max>cap</span>` cập nhật theo từng tương tác.

6. **Reload list sau save:** index.js expose `window.salonTimeSlotReload = () => dataTable.ajax.reload(null, false)`; inline script gọi sau khi save thành công + đóng modal qua `$form.closest('.modal').modal('hide')`. Cũng đăng ký `createModal.onResult` / `editModal.onResult` làm fallback.

7. **Index.cshtml:** thêm `<link>` flatpickr CSS + `<script>` flatpickr CDN trong `@section styles`/`scripts`. Filter date inputs đổi `type="date"` → `type="text"` + flatpickr.

Files: `Pages/SalonBeautyTimeSlots/{Index.cshtml, CreateModal.cshtml, CreateModal.cshtml.cs, EditModal.cshtml, EditModal.cshtml.cs, index.js}`, `wwwroot/pages/salon/timeslot-shared.css` (thêm `.timeslot-cap-btn`).

Liên quan: [[project_salon_location_slot_config]] (nguồn config SlotDuration/BufferTime/MaxCapacityPerSlot), [[project_salon_booking_ui]] (pattern gốc — render Location server-side + inline script trong modal).
