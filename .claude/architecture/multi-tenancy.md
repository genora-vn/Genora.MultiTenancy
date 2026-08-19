# Architecture — Multi-tenancy & DB Routing

> Nguồn: `project_multitenant_db_routing.md`. Đây là quy tắc kiến trúc CỐT LÕI.

## Cơ chế routing của ABP
ABP quyết định dùng database nào **hoàn toàn dựa vào entity có implement `IMultiTenant` hay không**:
- Entity có `IMultiTenant` + `TenantId` → route tới **tenant DB** (vd `MontgomerieLinks`).
- Entity **không có** `IMultiTenant` → route tới **host DB** (`GenoraMultiTenancy`).

Một số tenant dùng **separate database** (không dùng chung host DB). Ví dụ: tenant
`montgo-staging.genora.vn` dùng DB riêng `MontgomerieLinks`.

## Sự cố kinh điển
`FnbOrder` (có `IMultiTenant`) insert vào tenant DB, nhưng `FnbOrderItem` (thiếu `IMultiTenant`)
insert vào host DB → **FK `FK_AppFnbOrderItems_AppFnbOrders_OrderId` fail** vì 2 bảng nằm ở 2 DB khác nhau.

## QUY TẮC BẮT BUỘC
> **Mọi child entity của một aggregate root có `IMultiTenant` đều PHẢI implement `IMultiTenant`.**

Đặc biệt quan trọng khi hệ thống dùng separate DB per tenant. Thiếu là ABP route child sang host DB → FK fail.

### Cách áp dụng khi tạo entity mới
1. Nếu entity là child của aggregate root multi-tenant → thêm `IMultiTenant` + `TenantId` (nullable).
2. Tạo migration thêm cột `TenantId` (backfill từ parent nếu đã có dữ liệu).
3. Trong service, khi tạo child: explicit `TenantId = _currentTenant.Id`.
4. `b.ConfigureByConvention()` trong EF config tự xử lý phần còn lại.

## Entities đã xác nhận có IMultiTenant
`FnbOrder`, `FnbOrderItem`, `ProOrder`, `ProOrderItem`, `FnbOrderActivity`, `ProOrderActivity`,
`SalonBeautyBooking`, `SalonBeautyBookingService` (fix 2026-05-19, migration
`Add_TenantId_To_SalonBeautyBookingServices` có backfill từ parent Booking).

## Liên quan
- Cross-tenant FK: MiniApp set `ItemId=null` + lookup by name để tránh FK cross-tenant (`project_miniapp_itemid_null_pattern`).
- ProOrder.CustomerId soft reference dùng chung golf + salon (`project_proorder_customer_soft_reference`).
