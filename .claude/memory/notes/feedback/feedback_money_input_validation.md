---
name: Money input vi-VN — patch jQuery Validate trước khi validate
description: Field giá tiền format vi-VN (dấu chấm ngàn) bị jQuery Validate reject với giá trị >= 1.000 vì regex mặc định kỳ vọng dấu phẩy
type: feedback
---

jQuery Validate method `number` dùng regex kỳ vọng dấu phẩy là thousand separator. Locale `vi-VN` dùng dấu chấm (`1.000.000`) nên bất kỳ giá trị >= 1.000 đều bị reject — *không phải* giới hạn 999.999.

**Fix chuẩn:** Thêm `patchMoneyValidator()` trong `initMoney()` của modal:

```js
function patchMoneyValidator() {
    if (!$.validator) return;
    var origNumber = $.validator.methods.number;
    $.validator.methods.number = function (value, element) {
        return origNumber.call(this, normalizeMoney(value), element);
    };
    var origRange = $.validator.methods.range;
    $.validator.methods.range = function (value, element, param) {
        return origRange.call(this, normalizeMoney(value), element, param);
    };
}
```

Trong đó `normalizeMoney(raw)` = `String(raw).replace(/[^\d]/g, '')`.

**Why:** Lỗi xảy ra ở AppFnbItems và AppProItems CreateModal/EditModal khi nhập giá >= 1.000.000.

**How to apply:** Bất kỳ modal nào có field decimal/money format vi-VN đều cần patch này trong `initMoney()`. Patch override global `$.validator.methods` nhưng an toàn vì capture original trước.
