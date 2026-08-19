---
name: project-hoalinh-ui-update-june25
description: "Update UI Hoa Linh — Dashboard badges, detail modals, page size, Loyalty redesign, API Logs date filter+delete, new API endpoints (Brands, ProductGroups, OrderHeaders, MasterOrderStatus)"
metadata: 
  node_type: memory
  type: project
  originSessionId: dcf06de4-47b3-4551-89dd-8fabec86f6de
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

**Dashboard:** gap thu hẹp, badges 7 màu, KPI dùng OrderHeaders
**Products:** click row → detail modal, page size select
**Customers:** click row → detail modal 2 cột, page size select
**Orders:** click row → detail modal (info + items table), badges, page size
**Loyalty:** redesign — header card xám + 4 stat cards + info card 2 cột + tier gradient badges
**API Logs:** filter ngày flatpickr, nút Xóa theo bộ lọc, page size

**Why:** Hoàn thiện UI theo feedback.
**How to apply:** Pattern modal: click row.hl-clickable → gọi service detail → render HTML → bootstrap.Modal.show()

[[project-hoalinh-phase3-complete]] [[feedback_hl_dual_permission_and_json_parse]]
