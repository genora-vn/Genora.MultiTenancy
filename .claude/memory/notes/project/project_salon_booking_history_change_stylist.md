---
name: project-salon-booking-history-change-stylist
description: Salon Beauty Booking — page Lịch sử thao tác (mirror Proshop) + chức năng Thay đổi stylist trên Detail
metadata: 
  node_type: memory
  type: project
  originSessionId: b0157f35-01a5-463a-9e01-3b5eb1102803
---

Salon Beauty Booking bổ sung 2 luồng UX song song với Proshop:

1. **Trang Lịch sử thao tác** — `/SalonBeautyBookings/History?id={bookingId}`
   - Razor: `Pages/SalonBeautyBookings/History.cshtml(.cs)`
   - Mirror y nguyên `AppProOrders/Board/History` (breadcrumb, metric cards, action type filter, paged table, pagination, export CSV).
   - Service: `ISalonBeautyBookingAppService.GetHistoryPageAsync(GetSalonBeautyBookingHistoryInput)` trả `SalonBeautyBookingHistoryPageDto` với `PagedActivities` (`SalonBeautyBookingHistoryItemDto`) + `ActionTypeOptions`.
   - DTOs: `Application.Contracts/AppDtos/SalonBeauties/SalonBeautyBookings/SalonBeautyBookingHistoryPageDto.cs`.
   - Action types được nhóm bằng helpers `ResolveActionTypeKey/Text/Class` trong AppService — parse Vietnamese keywords (`khởi tạo`, `đổi stylist`, `check-in`, `hủy`, `trạng thái`).
   - Style dùng class `booking-history-*` trong `wwwroot/pages/salon/booking-shared.css` (đã thêm sẵn các block topbar/breadcrumb/metric/filter/table/footer).
   - Nút "Xem thêm" trong khối **LỊCH SỬ THAO TÁC** trên `Detail.cshtml` redirect sang trang này.

2. **Chức năng Thay đổi stylist** trên `Detail.cshtml`
   - Endpoint: `ISalonBeautyBookingAppService.ChangeStylistAsync(Guid id, ChangeBookingStylistDto)` — DTO có `Guid StylistId` + `string? Note`.
   - Logic: validate stylist cùng `LocationId`, update `StylistId`, append internal note `"Đổi stylist: <old> → <new>"` (kèm `Note` tuỳ chọn).
   - UI: button `#BtnChangeStylist` chỉ active khi `CanEdit && !cancelled && !completed`. Modal `#ChangeStylistModal` (header w/ refresh icon, body: keyword search + grid `.sb-stylist-pick-list` (cards `.sb-stylist-pick is-current/is-selected/is-disabled`) + textarea note, footer: Hủy bỏ + Cập nhật).
   - JS: load qua `bookingService.getStylistLookup(currentLocationId)`; search filter in-memory; submit gọi `bookingService.changeStylist(bookingId, {stylistId, note})` rồi reload page.

**Why:** đồng bộ trải nghiệm admin Salon với Proshop (audit trail + đổi stylist khi khách yêu cầu trước giờ phục vụ); giữ stylist trong cùng location để đảm bảo lịch làm việc.

**How to apply:** khi cần thêm chức năng tương tự cho module khác (hoặc cần thay đổi luồng đổi stylist/staff), tái sử dụng pattern này — DTO + AppService method + Razor History page + nút trên Detail. Không tạo group permission riêng cho action — vẫn dùng `SalonBeautyBookings.Edit`.

Liên quan: [[project-salon-booking-ui]], [[project-salon-booking-create-edit-modal-fixes]], [[project-salon-stylist-booking-locationid]].
