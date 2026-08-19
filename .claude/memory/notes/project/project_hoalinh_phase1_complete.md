---
name: project-hoalinh-phase1-complete
description: "Phase 1 Foundation hoàn thành — 4 enums, 4 entities, EF config, migration, feature, permissions, menu, localization"
metadata: 
  node_type: memory
  type: project
  originSessionId: dcf06de4-47b3-4551-89dd-8fabec86f6de
---

## Hoa Linh Phase 1 Complete — Foundation (2026-06-23)

### Enums (Domain.Shared/Enums/):
- HlOrderDeliveryStatus: PendingConfirmation=1, Processing=2, Delivering=3, Completed=4, Cancelled=5
- HlOrderPaymentStatus: Unpaid=1, Paid=2, Debt=3
- HlGiftExchangeStatus: Pending=1, Approved=2, Rejected=3, Completed=4
- HlPaymentMethod: Cash=1, BankTransfer=2

### Entities (Domain/DomainModels/):
- AppHlOrders/HlOrder.cs — FullAuditedAggregateRoot, IMultiTenant, tham khảo ProOrder
- AppHlOrders/HlOrderItem.cs — Entity<Guid>, IMultiTenant, FK to HlOrder (cascade)
- AppHlGiftExchanges/HlGiftExchange.cs — FullAuditedAggregateRoot, IMultiTenant
- AppHlApiLogs/HlApiLog.cs — CreationAuditedEntity<Guid>, IMultiTenant

### Schema: HL (tất cả bảng nằm trong schema HL)

### Feature:
- AppHoaLinhFeatures.Management ("HoaLinh.Management") — toggle boolean
- File: Application.Contracts/Features/AppHoaLinhFeatures/

### Permissions (trong MultiTenancyPermissions.cs):
- AppHlProducts (read-only), HostAppHlProducts
- AppHlCustomers (read-only), HostAppHlCustomers
- AppHlOrders (CRUD), HostAppHlOrders (CRUD)
- AppHlLoyalty (read-only), HostAppHlLoyalty
- AppHlGiftExchange (CRUD), HostAppHlGiftExchange (CRUD)
- AppHlDashboard, HostAppHlDashboard
- AppHlApiLogs, HostAppHlApiLogs

### Menu: Group "Hoa Linh" (order: 50, icon: fa-leaf)
- Dashboard, Sản phẩm, Khách hàng, Đơn hàng, Loyalty, Đổi quà, Nhật ký API

### Migration: 20260623112209_Add_HoaLinh_Module

### EF Config: MultiTenancyDbContextModelCreatingExtensionsHoaLinh.cs
- Called via builder.ConfigureHoaLinhModule() in OnModelCreating

**Why:** Foundation cho toàn bộ module Hoa Linh. Mọi phase tiếp theo build trên nền này.
**How to apply:** Khi thêm entity mới cho HL, thêm vào file EF config này. Permission check dùng dual pattern (Tenant RequireFeatures + Host không cần).

[[project-hoalinh-brd-overview]] [[project-hoalinh-data-integration-pattern]]
