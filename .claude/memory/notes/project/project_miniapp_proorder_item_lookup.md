---
name: MiniApp ProOrder — lookup ProItem để enrich items trong response
description: MiniAppProOrderService cần inject ProItem + ProCategory repos để trả về imageUrl, categoryName, isActive, isAvailable, sortOrder
type: project
---

`MiniAppProOrderService` sử dụng pattern `ItemId = null` (cross-tenant), nên phải lookup `ProItem` bằng `ItemName` fallback. Service inject:
- `IRepository<ProItem, Guid> _proItemRepository`
- `IRepository<ProCategory, Guid> _proCategoryRepository`
- `IConfiguration _configuration` (cho `ImageHelper.NormalizeThumb`)

**BuildItemDictAsync pattern:**
1. Collect `ItemId` có value → query `ProItem` by Id
2. Collect `ItemName` → query `ProItem` by Name (fallback)
3. Merge, dedup by Id
4. Dict key: `id.ToString()` hoặc `"name:{ItemName}"` (prefix tránh collision)

**ResolveItem:** Try by Id → fallback by Name (case-insensitive).

**Why:** `ToData()` cũ không inject repo nên các field `imageUrl`, `categoryName`, `isActive`, `isAvailable`, `sortOrder` luôn null trong response `/api/mini-app/get-pro-orders`.

**How to apply:** Mọi MiniApp service nào cần enrich OrderItem với catalog data đều dùng pattern này. Tham chiếu `MiniAppFnbOrderService.GetListAsync` (dùng FnbItem) và `MiniAppProOrderService.GetListAsync` (dùng ProItem) như blueprint.
