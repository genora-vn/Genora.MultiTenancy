---
name: fnb-shared.js formatCurrency đã bao gồm ký hiệu "đ"
description: Hàm formatCurrency trong fnb-shared.js trả về chuỗi có "đ" ở cuối, không cần thêm thủ công ở nơi gọi
type: project
---

`window.genoraFnb.formatCurrency(value)` tại `wwwroot/pages/fnb/fnb-shared.js` trả về:
```js
number.toLocaleString('vi-VN') + 'đ'
// ví dụ: "525.000đ"
```

**Why:** Trước đây không có "đ", một số nơi tự thêm `<span class="vnd-symbol">đ</span>` sau khi gọi. Sau fix, không cần thêm nữa — sẽ bị duplicate.

**How to apply:** Khi render tiền trong DataTable column dùng `fnb.formatCurrency(data)` là đủ. Không append "đ" thêm. Các trang dùng raw `toLocaleString` riêng (như Kitchen, DetailModal Razor) thì vẫn tự thêm `đ` vì không đi qua hàm này.
