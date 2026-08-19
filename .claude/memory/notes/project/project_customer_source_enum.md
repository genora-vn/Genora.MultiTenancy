---
name: CustomerSource enum + nguồn khách hàng
description: Enum CustomerSource (4 giá trị) + quy ước gán nguồn theo từng entry point khi tạo Customer
type: project
originSessionId: 03f1f8f0-a727-4f28-8cd7-f688971b0a28
---
`Genora.MultiTenancy.Domain.Shared/Enums/CustomerSource.cs`:
```
ZaloMiniApp = 1   // khách tự đăng ký qua Mini App
Manual      = 2   // CMS user tạo thủ công
Extent      = 3   // Import / đồng bộ từ hệ thống khác (Excel, sync...)
Other       = 4   // dự phòng
```

Mỗi entry point bắt buộc gán đúng nguồn (override DTO ở server-side):
- `AppCustomerService.CreateAsync` → set `entity.CustomerSource = Manual` ngay sau ObjectMapper, không tin DTO từ form.
- `AppCustomerService.ImportExcelAsync` → DTO `CustomerSource = Extent`.
- `MiniAppCustomerAppService.UpsertFromMiniAppAsync` → set `customer.CustomerSource = ZaloMiniApp` cả nhánh insert và update.

**Why:** Nguồn phải khớp với entry point để filter, audit, và khoá field PhoneNumber khi đến từ Mini App. Nếu để DTO quyết định thì user CMS có thể đổi ngược lại Manual qua devtools.

**How to apply:** Khi thêm entry point mới (sync khác, API khác), gán nguồn tương ứng trên entity sau Map; không tạo nguồn mới trừ khi nghiệp vụ thực sự cần phân loại.
