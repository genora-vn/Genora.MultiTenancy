---
name: miniapp-booking-detail-policy-fields
description: "API GET /api/mini-app/get-bookings/{id} trả thêm PolicyTitle/CancellationPolicyHours[/Weekend]/CancellationPolicyContent từ AppPromotionPolicies theo (GolfCourseId, slot.PromotionTypeId)"
metadata: 
  node_type: memory
  type: project
  originSessionId: eccc6396-1889-4ab3-a51a-86af66f59b8e
---

# MiniApp Booking Detail trả thêm chính sách hoãn hủy

**Ngày:** 2026-05-22

## Endpoint
`GET /api/mini-app/get-bookings/{id}?customerId={cid}` → `MiniAppBookingDetailDto`

Trước đây chỉ trả price/player; chưa có chính sách hoãn hủy nên Mini App không hiển thị được nội dung policy theo loại ưu đãi của tee time.

## Thay đổi

`BookingDetailData` (DTO) thêm 4 trường:
- `PolicyTitle` (string?) — Tiêu đề chính sách
- `CancellationPolicyHours` (int?) — Giờ tối thiểu T2-T6
- `CancellationPolicyHoursWeekend` (int?) — Giờ tối thiểu T7+CN
- `CancellationPolicyContent` (string?) — Nội dung chi tiết

`MiniAppBookingAppService.GetMiniAppAsync(Guid id, Guid customerId)` lookup `AppPromotionPolicies` cùng pattern với `GetMiniAppAsync(GetMiniAppCalendarSlotDetailInput)` của `MiniAppCalendarSlotService`:

```csharp
var policy = await _promotionPolicyRepo.FirstOrDefaultAsync(x =>
    x.GolfCourseId == booking.GolfCourseId &&
    x.PromotionTypeId == calendar.PromotionTypeId);
```

Lookup chỉ chạy khi `dto.CalendarSlotId` có giá trị (đã có guard sẵn). `_promotionPolicyRepo` đã được inject vào service từ trước (dùng cho list endpoint).

## How to apply
- PromotionType của tee time được lưu ở `CalendarSlot.PromotionTypeId`. Booking lookup slot qua `CalendarSlotId` rồi dùng `slot.PromotionTypeId` + `booking.GolfCourseId` để tìm policy.
- Nếu không cấu hình policy cho cặp (GolfCourse, PromotionType) đó → 4 field giữ nguyên null. Mini App nên check null trước khi render.
- Best deal / Promotion / Normal là các bản ghi `AppPromotionTypes` (Code), không hard-code — Mini App chỉ cần đọc 4 field policy đã được trả về.

## File map
- App.Contracts: `AppDtos/AppBookings/MiniAppBookingDetailDto.cs`
- App: `AppServices/AppBookings/MiniAppBookingAppService.cs` (method `GetMiniAppAsync(Guid, Guid)`)

## Related
- [[project_promotion_policy_feature]] — entity AppPromotionPolicies + logic IsCancellationPolicy
- API tham chiếu: `MiniAppCalendarSlotService.GetMiniAppAsync(GetMiniAppCalendarSlotDetailInput)`
