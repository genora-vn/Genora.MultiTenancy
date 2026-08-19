---
name: project-hoalinh-phase3-complete
description: "Phase 3 Admin Portal UI hoàn thành — 6 trang Read-only (Products, Customers, Orders, Loyalty, GiftExchanges, ApiLogs) + Dashboard + HlAdminAppService"
metadata: 
  node_type: memory
  type: project
  originSessionId: dcf06de4-47b3-4551-89dd-8fabec86f6de
---

## Hoa Linh Phase 3 Complete — Admin Portal UI (2026-06-23)

### AppService (Application layer):
- IHlAdminAppService — interface expose cho ABP auto-proxy (JS gọi service proxy)
- HlAdminAppService — wrap IHlApiClientService, [Authorize] permission check mỗi method

### Admin Pages (Web/Pages/HoaLinh/):

| Trang | URL | Dữ liệu | Ghi chú |
|-------|-----|----------|---------|
| Dashboard | /HoaLinh/Dashboard | API HL (KPI cards + recent data) | 4 KPI cards + 2 tables |
| Products | /HoaLinh/Products | API HL GetProducts | Search, paging, grid 8 cols |
| Customers | /HoaLinh/Customers | API HL GetCustomers | Search, paging, grid 9 cols |
| Orders | /HoaLinh/Orders | API HL GetOrders | Filter by customerCode, paging |
| Loyalty | /HoaLinh/Loyalty | API HL GetCustomerByPhone | Tra cứu SĐT → card điểm + info |
| GiftExchanges | /HoaLinh/GiftExchanges | DB Genora (placeholder) | Phase 4 sẽ implement CRUD |
| API Logs | /HoaLinh/ApiLogs | DB Genora (placeholder) | Phase tiếp theo impl read from DB |

### Pattern chung:
- Page model rỗng (OnGet empty) — ABP pattern
- JS gọi `genora.multiTenancy.appServices.hoaLinh.hlAdmin.{method}` (auto-proxy)
- Custom pagination (không dùng DataTables vì API HL trả paged response riêng)
- formatCurrency vi-VN, badge colors, responsive grid

### JS Service proxy path:
```
genora.multiTenancy.appServices.hoaLinh.hlAdmin.getProducts(page, limit, search)
genora.multiTenancy.appServices.hoaLinh.hlAdmin.getCustomers(page, limit, search)
genora.multiTenancy.appServices.hoaLinh.hlAdmin.getOrders(page, limit, customerCode)
genora.multiTenancy.appServices.hoaLinh.hlAdmin.getCustomerByPhone(phone)
genora.multiTenancy.appServices.hoaLinh.hlAdmin.getSalemans(page, limit)
genora.multiTenancy.appServices.hoaLinh.hlAdmin.getCampaigns(page, limit)
```

**Why:** Admin UI hoàn chỉnh cho việc xem dữ liệu từ DMS Hoa Linh. Tất cả read-only, call API real-time.
**How to apply:** Thêm page mới vào /HoaLinh/ folder, follow pattern: cshtml.cs rỗng + cshtml grid + index.js gọi hlAdmin service.

[[project-hoalinh-phase2-complete]] [[project-hoalinh-phase1-complete]]
