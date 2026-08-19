---
name: Disabled select pattern — luôn kèm hidden input để POST
description: HTML <select disabled> không gửi value khi submit; phải có hidden input cùng name để bind model server-side
type: feedback
originSessionId: 03f1f8f0-a727-4f28-8cd7-f688971b0a28
---
Khi cần render dropdown chỉ-xem nhưng vẫn POST giá trị về server (ví dụ Customer.CustomerSource ở Create/Edit modal):

```cshtml
<select asp-for="Customer.CustomerSource"
        asp-items="Model.CustomerSourceItems"
        class="form-select readonly-gray"
        disabled="disabled"></select>
<input type="hidden" asp-for="Customer.CustomerSource" />
```

**Why:** `disabled` HTML element không serialize trong form post → server nhận `null` và Required validator báo lỗi hoặc giá trị bị reset. Hidden input cùng `name` đảm bảo value vẫn được gửi.

**How to apply:** Dùng cho mọi field "show only, không cho sửa" mà vẫn cần round-trip. Ngoài ra server-side (PageModel OnPostAsync) nên override lại field này từ DB hoặc business rule, không tin client (vì user có thể chỉnh hidden bằng devtools).
