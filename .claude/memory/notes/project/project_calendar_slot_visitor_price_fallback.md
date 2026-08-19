---
name: CalendarSlot — Visitor price fallback khi user chưa đăng nhập
description: Khi customerId null hoặc CustomerType chưa xác định, phải lấy giá từ VIS row, không dùng .Max() của toàn bộ slotPrices
type: project
originSessionId: 5f94524f-c322-4fcf-8e8e-519f0aff4a55
---
Trong `MiniAppCalendarSlotService`, khi tính `CustomerTypePrice` cho một slot:

- Nếu user **đã đăng nhập + có CustomerType** → lấy giá theo CustomerTypeId của họ
- Nếu user **chưa đăng nhập** hoặc **chưa gán CustomerType** → fallback lookup row VIS cụ thể

**Fix đã áp dụng (commit 1efb24a):**
```csharp
// Sai (trước): lấy giá max của tất cả rows — trả về giá cao nhất, không phải giá Visitor
myPrice = slotPrices.Select(p => PriceByHoleHelper.GetPriceByNumberHoles(p, input.NumberHoles))
                    .DefaultIfEmpty(0m).Max();

// Đúng (sau): lookup đúng row VIS
var visRow = visCustomerType != null
    ? slotPrices.FirstOrDefault(p => p.CustomerTypeId == visCustomerType.Id)
    : null;
myPrice = visRow != null
    ? PriceByHoleHelper.GetPriceByNumberHoles(visRow, input.NumberHoles)
    : 0m;
```

**Why:** `.Max()` trả về giá cao nhất trong danh sách, có thể là giá Premium hay Weekend rate — không phải giá mặc định cho khách vãng lai.

**How to apply:** Luôn lookup explicit theo `visCustomerType.Id` khi cần giá fallback cho user ẩn danh. `visCustomerType` được resolve trước trong method từ CustomerType repo với Code = "VIS".
