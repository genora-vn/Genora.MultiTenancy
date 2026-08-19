---
name: ProPaymentStatus enum values
description: ProPaymentStatus enum: Unpaid=1, Paid=2, Failed=3 — không có Refunded
type: project
---

```csharp
public enum ProPaymentStatus : byte
{
    Unpaid = 1,  // Chưa thanh toán
    Paid   = 2,  // Đã thanh toán
    Failed = 3   // Thanh toán thất bại
}
```

Localization keys: `ProPaymentStatus:Unpaid`, `ProPaymentStatus:Paid`, `ProPaymentStatus:Failed`

**Why:** Trước đó đã dùng nhầm `ProPaymentStatus:Refunded` (không tồn tại) trong localization JSON và Index.cshtml → đã được sửa thành `Failed`.

**How to apply:** Khi dùng `ProPaymentStatus` trong switch/select, chỉ có 3 giá trị: Unpaid, Paid, Failed.
