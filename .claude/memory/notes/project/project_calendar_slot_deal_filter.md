---
name: CalendarSlot — filter deal vs. tee time khác nhau
description: PromotionType != null (deal list) lấy cả slot tương lai; PromotionType == null (tee time now) chỉ lấy slot hôm nay còn giờ
type: project
originSessionId: 5f94524f-c322-4fcf-8e8e-519f0aff4a55
---
Trong `MiniAppCalendarSlotService.GetListMiniAppAsync`, logic filter thời gian phụ thuộc vào `input.PromotionType`:

**Khi `PromotionType != null`** (đang lấy danh sách deal/khuyến mãi):
```csharp
query = query.Where(x =>
    (x.ApplyDate.Date > DateTime.Now.Date) ||
    (x.ApplyDate.Date == DateTime.Now.Date && x.TimeTo >= DateTime.Now.TimeOfDay));
```
→ Bao gồm cả ngày hôm nay (còn giờ) **và** các ngày tương lai.

**Khi `PromotionType == null`** (danh sách tee time hiện tại):
```csharp
query = query.Where(x =>
    x.ApplyDate.Date == DateTime.Now.Date && x.TimeTo >= DateTime.Now.TimeOfDay);
```
→ Chỉ hôm nay, chỉ những slot chưa qua giờ.

**Why:** Deal list cần show các slot upcoming (người dùng book trước), còn tee time "now" chỉ hiện slot còn khả dụng trong ngày.

**How to apply:** Khi thêm filter mới hoặc refactor query CalendarSlot, kiểm tra 2 nhánh PromotionType riêng biệt — không gộp lại thành 1 điều kiện chung.
