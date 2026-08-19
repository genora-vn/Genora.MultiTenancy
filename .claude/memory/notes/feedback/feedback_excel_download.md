---
name: genora.excel.download dùng fetch+Blob không dùng window.location.href
description: window.location.href không trigger download — phải dùng fetch+Blob+<a>.click()
type: feedback
---

`window.location.href = url` chỉ điều hướng browser đến URL, **không trigger download file**. Với ABP `IRemoteStreamContent`, browser mở URL trống thay vì tải file.

**Fix đã áp dụng** trong `wwwroot/global-scripts.js`:

```js
fetch(finalUrl, { credentials: 'same-origin' })
  .then(r => r.blob())
  .then(blob => {
      var a = document.createElement('a');
      a.href = URL.createObjectURL(blob);
      a.download = fileName;  // từ Content-Disposition header
      a.click();
      URL.revokeObjectURL(a.href);
  });
```

**Bonus fixes trong cùng function:**
- Lọc bỏ query params rỗng (`""`, `null`, `undefined`) trước khi gửi → tránh DateTime binding lỗi
- `abp.ui.setBusy()` / `clearBusy()` khi chờ
- `abp.notify.error()` nếu server trả lỗi

**Why:** Tất cả trang dùng `genora.excel.download()` đều bị ảnh hưởng: ProCategories, ProItems, FnbCategories, FnbOrders, ProOrders.

**How to apply:** Không thay đổi gì ở call sites — fix đã ở helper. Khi thêm export Excel mới, gọi `genora.excel.download(url, getFilter())` như bình thường.
