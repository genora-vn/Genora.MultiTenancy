---
name: BookingNewRequest email — TotalAmount từ sum(PricePerPlayer)
description: CreateFromMiniAppAsync phải tính TotalAmount cho email từ savedPlayers, không dùng booking.TotalAmount (flat PricePerGolfer × N)
type: project
originSessionId: 03f1f8f0-a727-4f28-8cd7-f688971b0a28
---
Trong `MiniAppBookingAppService.CreateFromMiniAppAsync`, trước khi build `BookingNewRequestEmailModelDto`:

```csharp
var savedPlayers = await _playerRepo.GetListAsync(x => x.BookingId == booking.Id);
var emailTotalAmount = savedPlayers.Sum(p => p.PricePerPlayer ?? 0m);
if (emailTotalAmount <= 0m) emailTotalAmount = booking.TotalAmount; // fallback
```

Dùng `emailTotalAmount` cho cả `TotalAmount` và `TotalAmountText` trong model gửi email.

**Why:** `booking.TotalAmount` được set ở line 144 là `input.PricePerGolfer * input.NumberOfGolfers` (flat). Khi booking mix Member/MemberGuest/Visitor, giá mỗi golfer khác nhau và lưu trong `AppBookingPlayer.PricePerPlayer` — nếu nhân flat sẽ sai tổng. Pattern này đã áp dụng trong `BuildDetailDtoAsync` (xem `project_booking_total_amount_pattern.md`).

**How to apply:** Mọi chỗ tính tổng tiền booking (email, invoice, báo cáo) phải sum từ `AppBookingPlayers.PricePerPlayer`, không dùng trực tiếp `Booking.TotalAmount`. Template `BookingChangeRequest.tpl` hiện dùng `booking.TotalAmount` nhưng may mắn đúng do update flow đang flat tương đương — nếu sau này update cũng mix loại khách thì phải fix tương tự.
