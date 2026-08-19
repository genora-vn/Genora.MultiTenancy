---
name: ABP DataTables — custom ajax khi service trả List<T> thay vì PagedResultDto
description: abp.libs.datatables.createAjax chỉ hoạt động khi service trả {items:[], totalCount:N}. Nếu service trả List<T> (array thuần) phải dùng custom ajax function
type: feedback
---

`abp.libs.datatables.createAjax(service.getList)` chỉ parse được response dạng `{items: [], totalCount: N}` (ABP paged format). Nếu service trả `List<T>` (array thuần), DataTables sẽ bị stuck "Đang xử lý..." mãi vì không parse được.

**Why:** Service `GetListAsync()` không dùng `PagedResultDto` mà trả `List<T>` trực tiếp — phù hợp với dữ liệu nhỏ không cần phân trang server-side.

**How to apply:** Dùng custom `ajax` function thay vì `createAjax`:
```javascript
function loadData(requestData, callback) {
    service.getList()
        .then(function (result) {
            var rows = Array.isArray(result) ? result : (result.items || []);
            callback({
                recordsTotal: rows.length,
                recordsFiltered: rows.length,
                data: rows
            });
        })
        .catch(function () {
            callback({ recordsTotal: 0, recordsFiltered: 0, data: [] });
        });
}

// Trong DataTable config:
ajax: loadData,
serverSide: false,
```

Áp dụng cho bất kỳ service nào trả array thuần (không phải ABP paged response).
