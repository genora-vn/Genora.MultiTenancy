---
name: AppCustomers EditModal — disable PhoneNumber khi nguồn ZaloMiniApp
description: PhoneNumber là khoá định danh giữa Mini App và CMS, không cho phép sửa khi customer đến từ ZaloMiniApp
type: project
originSessionId: 03f1f8f0-a727-4f28-8cd7-f688971b0a28
---
Trong `EditModal.cshtml`:
```cshtml
var isFromMiniApp = Model.Customer?.CustomerSource == CustomerSource.ZaloMiniApp;
...
<abp-input asp-for="Customer.PhoneNumber"
           readonly="@isFromMiniApp"
           class="@(isFromMiniApp ? "readonly-gray" : "")" />
```

**Why:** Khách Mini App được upsert theo phone (key tự nhiên). Nếu CMS sửa phone thì lần upsert kế tiếp sẽ tạo trùng record hoặc lệch FK với booking/order đã gắn customer cũ.

**How to apply:** Bất kỳ field nào là key đồng bộ với nguồn ngoài (phone, ZaloUserId...) phải readonly trong modal sửa khi nguồn = ZaloMiniApp; ZaloUserId luôn readonly bất kể nguồn.
