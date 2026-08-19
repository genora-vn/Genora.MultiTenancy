---
name: ProOrder Items không load (WithDetailsAsync)
description: AppProOrderService.GetAsync dùng GetAsync thường không eager-load Items — phải dùng WithDetailsAsync
type: feedback
---

`_orderRepository.GetAsync(id)` trong ABP **không auto-include navigation properties**. Khi cần load `ProOrder.Items`, phải dùng:

```csharp
var query = await _orderRepository.WithDetailsAsync(o => o.Items);
var order = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id))
            ?? throw new EntityNotFoundException(typeof(ProOrder), id);
```

**Why:** `GetAsync` trả về entity trần — Items luôn là empty collection → Board/Detail hiển thị 0 sản phẩm.

**How to apply:** Bất cứ khi nào cần load navigation property của ProOrder (hoặc FnbOrder) phải dùng `WithDetailsAsync`, không dùng `GetAsync` thông thường.
