---
name: Order status update modal — disable submit khi chưa đổi trạng thái
description: Pattern hidden input "Current..." + script so sánh string để enable nút submit chỉ khi user chọn trạng thái khác
type: project
originSessionId: 03f1f8f0-a727-4f28-8cd7-f688971b0a28
---
Áp dụng cho cả 4 modal: AppFnbOrders/AppProOrders × ServiceStatus/PaymentStatus.

Markup:
```cshtml
<input type="hidden" name="CurrentServiceStatus" value="@Model.Input.ServiceStatus" />
<!-- radio group asp-for="Input.ServiceStatus" với value="@item" (enum name) -->
```

Script:
```js
var currentStatus = (form.querySelector('input[name="CurrentServiceStatus"]').value) || '';
function updateSubmitState() {
    var selected = form.querySelector('input[name="Input.ServiceStatus"]:checked');
    submitBtn.disabled = !selected || String(selected.value) !== String(currentStatus);
}
```

**Why:** Trước đây dùng `parseInt` ở cả 2 phía, nhưng `asp-for` với enum render `value="Created"` (string) còn hidden lại render `(int)1` → mismatch, nút luôn enable. So sánh dạng string + cùng output `@Model.Input.ServiceStatus` (không cast) cho cả 2 vế là an toàn.

**How to apply:** Khi thêm modal "đổi trạng thái" mới, copy pattern hidden + script. Đừng cast `(int)` cho hidden nếu radio đang dùng enum name.
