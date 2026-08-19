---
name: Action column slim + inline status update buttons
description: AppFnbOrders/AppProOrders Index — nút Cập nhật service/payment đẩy về cột Status (sau badge); dropdown action chỉ giữ View/Detail/Quick-status/Cancel
type: project
originSessionId: 03f1f8f0-a727-4f28-8cd7-f688971b0a28
---
Hai trang `AppFnbOrders/Index.js` và `AppProOrders/index.js`:

- Cột `ServiceStatus` và `PaymentStatus` render badge + một button nhỏ ngay sau:
  ```js
  function renderUpdateButton(id, type, title) {
      if (!canEdit() || !id) return '';
      return ' <button type="button" class="btn btn-sm btn-outline-secondary" '
          + 'data-fnb-update-type="'+type+'" data-fnb-id="'+id+'" '
          + 'title="'+title+'"><i class="fa fa-pen"></i></button>';
  }
  ```
  Render funcs lấy thêm tham số `row` (DataTables truyền `(data, type, row)`) để có `row.id`.

- Dropdown `rowAction.items` chỉ còn: View (modal), ViewDetail (page), Cập-nhật-trạng-thái-nhanh (one-click next status), CancelOrder. Hai entry "UpdateServiceStatus"/"UpdatePaymentStatus" mở modal đã bị xoá khỏi dropdown.

**Why:** Cột Action quá đông (6 mục) gây UX khó scan. Action update status thuộc về cột status tương ứng — đỡ phải mở dropdown khi nhân sự chỉ muốn đổi 1 trạng thái.

**How to apply:** Khi thêm trạng thái khác (vd. KitchenStatus), tiếp tục cùng pattern: render button inline trong cột tương ứng + giữ dropdown mảnh.
