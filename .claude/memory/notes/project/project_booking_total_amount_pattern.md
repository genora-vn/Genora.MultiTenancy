---
name: Booking TotalAmount tính từ AppBookingPlayers
description: totalAmount của booking phải sum PricePerPlayer từng người, không dùng booking.TotalAmount
type: project
originSessionId: 1d4a0f8f-d6a1-47b3-80bb-c4889c494f00
---
`BookingDetailData.TotalAmount` phải được tính lại từ `AppBookingPlayers.PricePerPlayer`, không map trực tiếp từ `booking.TotalAmount`.

```csharp
dto.TotalAmount = players.Sum(p => p.PricePerPlayer ?? 0m);
```

Lý do: Mỗi người chơi có thể có giá khác nhau (Member giá 900k, Member Guest giá 1.000.001), nên tổng thực tế phải cộng từng người. `booking.TotalAmount` lưu trong DB có thể là giá cũ tính theo `PricePerGolfer × numberOfGolfers` (đồng nhất), không phản ánh đúng thực tế.

**`MiniAppBookingPlayerInput`:** field tên là `PricePerGolfer` (không phải `PricePerPlayer`), service lưu vào `BookingPlayer.PricePerPlayer`. Player giữ đúng giá riêng, KHÔNG bị override bởi `booking.PricePerGolfer`.

**Why:** Booking Member dẫn đến nhóm có nhiều mức giá khác nhau trong cùng 1 booking.

**How to apply:** Mọi chỗ cần tổng tiền thực tế của 1 booking phải sum từ players, không tin vào `booking.TotalAmount`.
