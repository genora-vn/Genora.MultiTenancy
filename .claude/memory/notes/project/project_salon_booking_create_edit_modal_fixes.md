---
name: salon-booking-create-edit-modal-fixes
description: "Fix CreateModal AddNewCustomerBtn + EditModal preselect Ngày hẹn/Khung giờ (2026-05-21)"
metadata: 
  node_type: memory
  type: project
  originSessionId: current
---

# Salon Beauty Booking Modal Fixes (2026-05-21)

## 1. CreateModal — Thêm mới khách hàng từ modal Booking

**Vấn đề:** Button "Thêm mới" (#AddNewCustomerBtn) không có event handler → không làm gì khi click.

**Giải pháp:**
- Dùng `abp.ModalManager('/SalonBeautyCustomers/CreateModal')` để mở modal tạo khách hàng trực tiếp từ trong CreateModal Booking
- Sau khi save thành công (`customerCreateModal.onResult`), tự động search tên khách vừa nhập qua `bookingService.getCustomerLookup(q)`
- Nếu chỉ 1 kết quả → auto-select luôn
- Nếu nhiều kết quả → hiện dropdown để user chọn
- Refactor `selectCustomer(c)` helper để tái sử dụng cho cả search dropdown và onResult callback

**Files:**
- `Pages/SalonBeautyBookings/CreateModal.cshtml` (inline script)

**Why:** Luồng tạo khách hàng nhanh từ modal Booking giúp user không phải thoát ra Index Customers → tạo → quay lại Booking. Pattern `abp.ModalManager` + `onResult` là cách chuẩn ABP để chain modals.

**How to apply:** Khi cần mở modal khác từ trong modal hiện tại, dùng `new abp.ModalManager(path)` + `onResult(callback)` thay vì tự viết AJAX. ABP tự quản lý lifecycle (open/close/refresh).

---

## 2. EditModal — Preselect Ngày hẹn và Khung giờ

**Vấn đề:**
- `loadAvailableDates()` trả về promise nhưng `loadLookups` callback không await xong trước khi gọi `loadAvailableSlots`
- `datePickerInstance.setDate(iso, false)` dùng `false` nên không trigger `onChange` → `loadAvailableSlots` không được gọi tự động
- Slot hiện tại của booking có thể đang `Full` → `GetAvailableSlotsAsync` không trả về → preselect thất bại

**Giải pháp:**
1. Chain promise đúng thứ tự: `loadAvailableDates()` → `$.when(datesPromise).always(...)` → `setDate` → `loadAvailableSlots`
2. Thêm `initialBookingIso` vào `availableDateSet` để flatpickr không block ngày cũ (booking cũ có thể trong quá khứ)
3. Thêm API `ISalonBeautyTimeSlotAppService.GetAsync(Guid id)` để fetch slot hiện tại nếu không có trong list available
4. Refactor `loadAvailableSlots` với helper `appendSlotOption` + `tryPreselect`:
   - Load available slots trước
   - Nếu `preselectSlotId` không có trong list → gọi `slotService.get(preselectSlotId)` riêng
   - Append slot đó vào dropdown với suffix "(hiện tại)"
   - Preselect

**Files:**
- `Pages/SalonBeautyBookings/EditModal.cshtml` (inline script)
- `Application.Contracts/AppDtos/SalonBeauties/SalonBeautyTimeSlots/ISalonBeautyTimeSlotAppService.cs` (thêm `GetAsync`)
- `Application/AppServices/SalonBeauties/SalonBeautyTimeSlotAppService.cs` (implement `GetAsync`)

**Why:** Booking cũ có thể gắn slot đang Full/Off → `GetAvailableSlotsAsync` filter ra → UI không hiển thị slot hiện tại → user không biết booking đang ở slot nào. Cần fetch riêng slot hiện tại để hiển thị dù không available.

**How to apply:**
- Khi preselect dropdown từ data cũ, luôn kiểm tra xem giá trị cũ có trong list mới không
- Nếu không có → fetch riêng giá trị cũ và append vào dropdown với label "(hiện tại)" để user biết đây là giá trị đang lưu
- Chain promise bằng `$.when(...).always(...)` hoặc `.then(...)` thay vì gọi tuần tự mà không await

---

## Pattern: ABP ModalManager chain

```js
var customerCreateModal = new abp.ModalManager('/SalonBeautyCustomers/CreateModal');

$('#AddNewCustomerBtn').on('click', function () {
    customerCreateModal.open();
});

customerCreateModal.onResult(function () {
    // Callback sau khi modal save thành công
    // Có thể refresh list, auto-select item vừa tạo, etc.
});
```

**Lưu ý:**
- `onResult` chỉ fire khi modal POST thành công (HTTP 200/204)
- Nếu cần pass data từ modal con về modal cha, dùng `onResult(function(result) { ... })` — `result` là response từ server
- Modal con tự đóng sau khi save thành công (ABP behavior)

---

## Related
- [[project_salon_booking_ui]]
- [[project_salon_timeslot_peakhour_booking_timeslot]]
- [[project_salon_location_timeslot_ui]]
