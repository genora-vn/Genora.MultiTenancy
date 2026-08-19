---
name: MARS + autoSave pattern — insert parent trước, child sau riêng biệt
description: Với MARS enabled và separate tenant DB, cách duy nhất insert parent+child đúng là autoSave:true từng bước
type: feedback
---

Với connection string có `MultipleActiveResultSets=True` (MARS), không thể dùng `CurrentUnitOfWork.SaveChangesAsync()` một lần cho parent+child, vì EF không đảm bảo thứ tự insert.

**Đúng:** 2 bước riêng biệt với `autoSave: true`:
```csharp
await _orderRepository.InsertAsync(order, autoSave: true);         // commit order trước
await _orderItemRepository.InsertManyAsync(items, autoSave: true); // commit items sau
```

**Sai:** Cascade insert `order.Items.Add()` + `InsertAsync(order)` — tuy không báo lỗi nhưng với separate DB per tenant, items sẽ bị route sang DB sai (host DB) vì thiếu `IMultiTenant`.

**Sai:** `CurrentUnitOfWork.SaveChangesAsync()` một lần — EF Core không đảm bảo ORDER BY dependency khi có MARS, có thể insert items trước order → FK fail.

**Why:** MARS disables savepoints. Mỗi `autoSave: true` = một `SaveChanges` riêng, đảm bảo parent committed vào đúng DB trước khi child insert.

**How to apply:** Mọi service tạo order + items: luôn dùng pattern 2-bước `InsertAsync(order, autoSave:true)` → `InsertManyAsync(items, autoSave:true)`. Không dùng cascade insert qua navigation collection cho multi-tenant separate DB.
