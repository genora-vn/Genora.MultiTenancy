---
name: proorder-customer-soft-reference
description: "ProOrder.CustomerId là soft reference (không FK), dùng chung cho golf (AppCustomers) và salon (AppSalonBeautyCustomers); service tự lookup theo entry point"
metadata: 
  node_type: memory
  type: project
  originSessionId: b0157f35-01a5-463a-9e01-3b5eb1102803
---

# ProOrder.CustomerId = soft reference (no FK)

**Ngày:** 2026-05-25

## Vấn đề
Proshop là module dùng chung. Tạo đơn từ Mini App Salon (`MiniApp.SalonBeauty`) gửi lên `CustomerId` thuộc `AppSalonBeautyCustomers` → vi phạm `FK_AppProOrders_AppCustomers_CustomerId` → `SqlException`.

## Giải pháp
Bỏ FK cứng, giữ `CustomerId` (Guid?) làm "loose" pointer + index để query.

### Thay đổi
1. `Domain/DomainModels/AppProOrders/ProOrder.cs`
   - Xóa `public virtual Customer? Customer { get; set; }` + `using ...AppCustomers`
   - Giữ `CustomerId` (Guid?), comment giải thích là soft reference
2. `EntityFrameworkCore/MultiTenancyDbContextModelCreatingExtensionsProshop.cs`
   - Xóa block `b.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId)`
   - Thêm `b.HasIndex(x => x.CustomerId).HasDatabaseName("IX_AppProOrders_CustomerId")` để giữ index cũ
3. Migration `20260525102341_Drop_FK_AppProOrders_AppCustomers_CustomerId`
   - `Up`: chỉ `DropForeignKey FK_AppProOrders_AppCustomers_CustomerId` (không drop column/index)
   - `Down`: re-add FK

## Why
- Proshop dùng chung 2 domain khách hàng: `AppCustomers` (golf) và `AppSalonBeautyCustomers` (salon).
- FK cứng chỉ neo được 1 bảng → không thể chia sẻ cross-domain.
- Pattern tương đồng đã dùng cho `ProOrderItem.ItemId = null` để né FK cross-tenant ([[project_miniapp_itemid_null_pattern]]).

## How to apply
- `AppProOrderService` (CMS - golf): vẫn lookup `Customer` qua `_customerRepository.FindAsync(order.CustomerId.Value)` như cũ. Khi `CustomerId` không tồn tại trong `AppCustomers` → `FindAsync` trả null, không throw — flow CMS golf chỉ truy cập sau khi check `customer != null`.
- `MiniAppProOrderService` (Mini App Golf + Salon): chỉ lưu `CustomerId` thẳng từ input, không lookup customer (đã hiện masked phone từ `order.CustomerPhone`).
- Khi cần phân biệt golf vs salon trong tương lai có thể thêm `CustomerSourceType` (byte) — hiện chưa cần vì entry point đã biết domain.
- KHÔNG khôi phục `HasOne(x => x.Customer)` — sẽ break Salon flow.

## File map
- Domain: `DomainModels/AppProOrders/ProOrder.cs`
- EF: `EntityFrameworkCore/MultiTenancyDbContextModelCreatingExtensionsProshop.cs`
- Migration: `Migrations/20260525102341_Drop_FK_AppProOrders_AppCustomers_CustomerId.cs`

## Related
- [[project_miniapp_itemid_null_pattern]] — pattern tương tự cho `ProOrderItem.ItemId`
- [[project_multitenant_db_routing]] — IMultiTenant + cross-tenant FK gotcha
- [[project_miniapp_proorder_item_lookup]] — service tự lookup item bằng name fallback
