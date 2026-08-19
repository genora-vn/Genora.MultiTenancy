---
name: project-salon-booking-ui-v2
description: "Salon Beauty Booking UI v2 — sửa lỗi + cải tiến trang danh sách, modal Create/Edit/Detail (2026-05-21)"
metadata: 
  node_type: memory
  type: project
  originSessionId: ca135e2f-e2a6-4077-9ba0-7f5ae3cdc925
---

# Salon Beauty Booking UI v2 (2026-05-21)

## Enum BookingStatus thay đổi
- Thêm `Processing = 2`, shift `Completed = 3`, `Cancelled = 4`
- Luồng: New(0) → Confirmed(1) → Processing(2) → Completed(3), Cancelled(4)
- Không cần migration (enum byte không tạo schema change)

## Trang danh sách (Index)
- Button Refresh: reset tất cả filter + reload
- Filter responsive: col-xl/lg/md/col layout
- Modal Status: 4 options (New/Confirmed/Processing/Completed)
- Cột Thanh toán: status-badge
- Cột Phương thức thanh toán: badge mới (Cash=1/BankTransfer=2/Card=3)
- Fix lỗi Mã booking: thiếu `"` trong href render
- index.js: STATUS_PROCESSING=2, STATUS_COMPLETED=3, STATUS_CANCELLED=4

## Modal Cập nhật thanh toán
- Chưa thanh toán (0): disable payment method cards
- Partial/Paid/Refunded (1/2/3): enable payment method, bắt buộc chọn
- Payment method values: Cash=1, BankTransfer=2, Card=3 (không phải 0/1/2)

## Modal Thêm mới (CreateModal)
- Layout: Location+Stylist (6/6), Ngày hẹn+Khung giờ (6/6)
- Button "Thêm mới" khách hàng đã có trong UI nhưng chưa implement logic mở modal Customer

## Modal Cập nhật (EditModal)
- Layout: Location+Stylist (6/6), Ngày hẹn+Khung giờ (6/6)
- Status: đổi từ dropdown thành radio buttons, đẩy xuống dưới cùng
- 4 trạng thái: New/Confirmed/Processing/Completed (không có Cancelled — có modal hủy riêng)

## Trang chi tiết (Detail)
- Progress track: 4 steps (Đã đặt → Đã xác nhận → Đang thực hiện → Hoàn thành)
- Modal Status: 4 options
- statusClass thêm `sb-status-processing`

## TODO còn thiếu
- Button Thêm khách hàng trong CreateModal: chưa implement mở modal SalonBeautyCustomers/CreateModal
- Giảm BookedCount khi hủy booking (xem [[project_salon_timeslot_peakhour_booking_timeslot]])

**Why:** Yêu cầu cải tiến UX + thêm trạng thái Đã xác nhận vào luồng booking
**How to apply:** Khi extend status enum thêm case mới: cập nhật AppService (GetBookingStatusText, GetStatusColor, IsValidNextStatus), index.js (constants + normalizeStatus), Index.cshtml (modal radio), Detail.cshtml (modal radio + progress), vi.json
