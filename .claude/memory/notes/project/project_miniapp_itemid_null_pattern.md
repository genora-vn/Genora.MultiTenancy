---
name: FnbOrderItem/ProOrderItem — ItemId null + fallback by name
description: Mini App items phải set ItemId=null (tránh FK cross-tenant), lookup ảnh bằng ItemName fallback
type: project
---

## Vấn đề

`FnbOrderItem.ItemId` và `ProOrderItem.ItemId` là FK trỏ đến `AppFnbItems`/`AppProItems`. Trong Mini App (anonymous API), `ICurrentTenant` context không match với tenant session đầy đủ nên khi ABP apply tenant filter, FK check trên `AppFnbItems` có thể fail (`FK_AppFnbOrderItems_AppFnbItems_ItemId`).

## Fix

Khi tạo order item từ Mini App: **luôn set `ItemId = null`**, lưu tên món vào `ItemName`.

```csharp
var orderItem = new FnbOrderItem(...) {
    TenantId = _currentTenant.Id,
    ItemId = null,   // không set FK — tránh cross-tenant FK violation
    Note = ...
};
```

## Fallback lookup ảnh/thông tin

Mọi chỗ hiển thị ảnh sản phẩm phải có 2 tầng lookup:
1. Lookup theo `ItemId` (cho orders tạo từ staff dashboard)
2. Fallback lookup theo `ItemName` (cho orders từ Mini App với `ItemId = null`)

Pattern đã implement trong:
- `MiniAppFnbOrderService.GetListAsync/GetAsync`
- `FnbOrderRealtimeNotifier.BuildPayloadAsync`
- `MiniAppProOrderService.GetListAsync/GetAsync`
- `ProOrderRealtimeNotifier.BuildPayloadAsync`
- `AppProOrderService.GetAsync` (staff detail)
- `AppProOrderService.GetBoardAsync` (staff board)

**Why:** Separate DB per tenant — FK check chạy trong DB context của tenant, nhưng FnbItem có thể không tồn tại trong tenant DB nếu item được tạo trên host hay tenant khác.

**How to apply:** Bất kỳ Mini App service nào tạo order item → `ItemId = null` + `TenantId = _currentTenant.Id`. Bất kỳ query nào cần thông tin sản phẩm → phải có fallback by name.
