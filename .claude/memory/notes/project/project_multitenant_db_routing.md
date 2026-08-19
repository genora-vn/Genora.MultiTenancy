---
name: abp-multi-tenant-db-routing-imultitenant-b-t-bu-c-tr-n-child-entities
description: "FnbOrderItem/ProOrderItem thiếu IMultiTenant → ABP route sang host DB thay vì tenant DB, gây FK failure"
metadata: 
  node_type: memory
  type: project
  originSessionId: 895ac5b5-e08a-4833-aa5a-8b5420131df4
---

## Vấn đề gốc rễ đã xác nhận

Tenant `montgo-staging.genora.vn` dùng **database riêng** `MontgomerieLinks` (không phải `GenoraMultiTenancy` host DB).

ABP quyết định dùng DB nào hoàn toàn dựa vào entity có implement `IMultiTenant` không:
- Entity có `IMultiTenant` + `TenantId` → ABP route đến **tenant DB** (`MontgomerieLinks`)
- Entity **không có** `IMultiTenant` → ABP route đến **host DB** (`GenoraMultiTenancy`)

Khi `FnbOrder` (có `IMultiTenant`) được insert vào tenant DB, còn `FnbOrderItem` (không có `IMultiTenant`) bị insert vào host DB → **FK `FK_AppFnbOrderItems_AppFnbOrders_OrderId` fail** vì hai bảng nằm ở hai DB khác nhau.

## Fix đã áp dụng

1. Thêm `IMultiTenant` + `TenantId` vào `FnbOrderItem` và `ProOrderItem`
2. Tạo migration `Add_TenantId_To_OrderItems` (cột `TenantId` nullable)
3. Khi tạo items trong MiniApp service: explicit set `TenantId = _currentTenant.Id`
4. `b.ConfigureByConvention()` trong EF config tự động handle phần còn lại

## Quy tắc chung

**Mọi child entity của một aggregate root có `IMultiTenant` đều PHẢI implement `IMultiTenant`**, đặc biệt khi hệ thống dùng separate DB per tenant. Nếu không, ABP sẽ route child entity sang host DB.

Entities đã xác nhận có IMultiTenant: `FnbOrder`, `FnbOrderItem`, `ProOrder`, `ProOrderItem`, `FnbOrderActivity`, `ProOrderActivity`, `SalonBeautyBooking`, `SalonBeautyBookingService` (fix 2026-05-19, migration `Add_TenantId_To_SalonBeautyBookingServices` có backfill từ parent Booking).

**Why:** Separate DB per tenant — mỗi tenant (vd: MontgomerieLinks) có DB riêng, host DB là GenoraMultiTenancy. ABP routing dựa hoàn toàn vào IMultiTenant interface.

**How to apply:** Khi tạo bất kỳ entity mới nào là child của aggregate root multi-tenant, phải check ngay xem entity đó có `IMultiTenant` không. Nếu không → add vào và tạo migration.
