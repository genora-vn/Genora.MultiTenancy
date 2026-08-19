---
name: project-hoalinh-ui-update-june25
description: Update UI Hoa Linh — Dashboard badges, detail modals, page size, Loyalty redesign, API Logs date filter+delete, new API endpoints (Brands, ProductGroups, OrderHeaders, MasterOrderStatus)
metadata:
  type: project
---

## Hoa Linh UI Update (2026-06-25)

### Backend — Thêm API endpoints mới:
- GetBrandsAsync, GetBrandDetailAsync, GetProductsByBrandAsync
- GetProductGroupsAsync, GetProductGroupDetailAsync
- GetOrderHeadersAsync, GetOrderHeaderDetailAsync
- GetOrderHeaderZaloAsync, GetOrderDetailZaloAsync
- GetMasterOrderStatusAsync
- DeleteApiLogsAsync (xóa log theo bộ lọc)
- GetApiLogsAsync thêm params dateFrom/dateTo

### DTOs mới (HlExtraDtos.cs):
- HlBrandDto, HlOrderHeaderDto, HlMasterOrderStatusDto, HlProductGroupDto

### UI Updates:

**Dashboard:**
- Thu hẹp gap (g-3), border-radius cards
- Status badges 7 màu: Khởi tạo=info, Đang xử lý=warning, Hoàn thành=success, Đã thanh toán=primary, Đã hủy=danger, Từ chối=dark, Đã trả hàng=secondary
- KPI dùng getOrderHeaders thay getOrders

**Products:**
- Click row mở modal chi tiết (ProductDetailModal)
- Select page size (10/20/50/100)
- Cursor pointer + hover highlight

**Customers:**
- Click row mở modal chi tiết 2 cột (info DN + info KD)
- Select page size

**Orders:**
- Click row mở modal chi tiết (order info + items table)
- Status badges 7 màu
- Select page size

**Loyalty (redesign hoàn toàn):**
- Bỏ gradient card khó đọc
- Header card nền xám nhạt với tên KH + badge hạng TV
- 4 stat cards (Điểm, Doanh số, Điểm lên hạng, Hạng tiếp)
- Info card 2 cột (Kênh/Nhóm/NV Sales + NPP/GKHL)
- Badge hạng theo gradient: Vàng=gold, Bạc=purple, Kim Cương=violet

**API Logs:**
- Filter ngày (flatpickr from/to)
- Nút "Xóa theo bộ lọc" with confirm dialog
- Select page size
- DataType options: Customer/Product/ProductGroup/Brand/Order/Saleman/Campaign/MasterData

**Why:** Hoàn thiện UI theo feedback. Detail modals giúp xem thông tin không cần mở page mới.
**How to apply:** Pattern modal: click row.hl-clickable → gọi service detail → render HTML → bootstrap.Modal.show()

[[project-hoalinh-phase3-complete]] [[feedback_hl_dual_permission_and_json_parse]]
