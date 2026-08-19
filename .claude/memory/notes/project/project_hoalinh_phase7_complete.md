---
name: project-hoalinh-phase7-complete
description: "Phase 7 Dashboard + Data-level auth — HlDataAccessService, Sales filter by dsr_code, AppSettings mapping user→DsrCode"
metadata: 
  node_type: memory
  type: project
  originSessionId: dcf06de4-47b3-4551-89dd-8fabec86f6de
---

## Hoa Linh Phase 7 Complete — Dashboard + Phân quyền dữ liệu (2026-06-24)

### Data-level Access:

**IHlDataAccessService + HlDataAccessService:**
- GetCurrentUserDsrCodeAsync() — check role admin (→ null = xem tất cả), lookup AppSettings key
- IsSalesRestrictedAsync() — xác định user có bị restrict theo dsr_code không

**Cơ chế mapping:**
- Lưu trong AppSettings bảng hiện có
- Key pattern: `HoaLinh.UserDsrCode.{userId}` = `"HL00019"` (mã Sales trên DMS)
- Admin: role "admin" → bypass filter, xem toàn bộ
- Sales: có mapping dsr_code → chỉ thấy KH có dsr_code trùng

**Áp dụng:**
- HlAdminAppService.GetCustomersAsync — sau khi gọi API HL, filter result.Data by dsrCode nếu là Sales
- Pattern: client-side filter (vì API HL không hỗ trợ filter by dsr_code riêng)

### HlSettingNames:
- `HoaLinh.UserDsrCode.{userId}` — mapping user → mã Sales
- Quản trị viên set mapping qua AppSettings CRUD (đã có sẵn)

### DI:
- AddScoped<IHlDataAccessService, HlDataAccessService>()

### Dashboard page:
- Đã tạo ở Phase 3: /HoaLinh/Dashboard
- 4 KPI cards gọi API HL real-time (Products, Customers, Orders, Salemans)
- 2 bảng dữ liệu gần đây (Recent Orders + Recent Products)

**Why:** Phân quyền data-level đảm bảo Sales chỉ xem được KH mình phụ trách, tuân thủ BRD section 5.13.
**How to apply:** Khi thêm user Sales mới, admin set AppSetting `HoaLinh.UserDsrCode.{userId}` = mã dsr_code tương ứng trên DMS HL.

[[project-hoalinh-phase5-complete]] [[project-hoalinh-brd-overview]]
