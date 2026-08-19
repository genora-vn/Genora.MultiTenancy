---
name: project_appcustomers_page_customertype_permission_leak
description: "Trang Quản lý khách hàng (AppCustomers) throw AbpAuthorizationException vì load dropdown qua service CRUD có [Authorize] của AppCustomerTypes"
metadata: 
  node_type: memory
  type: project
  originSessionId: ad3cfa3f-621a-4107-972e-d6114398a383
---

Bug staging tenant Hoa Linh Miền Nam (2026-07-20): đã phân quyền "Quản lý khách hàng" (AppCustomers) cho tenant nhưng vào trang vẫn `AbpAuthorizationException` (trace `AbpAuthorizationServiceExtensions.CheckAsync(authorizationService, policyName)`). Feature "Khách hàng" đã bật.

**Root cause:** Trang `Web/Pages/AppCustomers/{Index,CreateModal,EditModal}.cshtml.cs` inject `IAppCustomerTypeService` chỉ để `GetListAsync(...)` load dropdown loại khách hàng. Nhưng `AppCustomerTypeService` là `[Authorize]` + `FeatureProtectedCrudAppService` ràng permission `AppCustomerTypes.Default` + feature `AppCustomerTypes.Management` (KHÁC hoàn toàn AppCustomers). `CheckGetListPolicyAsync` → `CheckPolicyRequiredAsync` → `AuthorizationService.CheckAsync` ném exception khi tenant chưa được cấp quyền "Loại khách hàng". Tenant khác chạy được vì tình cờ có cả 2 quyền.

**Fix:** đổi 3 page sang inject `IMiniAppCustomerTypeService` (cùng namespace `Genora.MultiTenancy.AppDtos.AppCustomerTypes`, KHÔNG có `[Authorize]`, cùng chữ ký `GetListAsync(PagedAndSortedResultRequestDto) : PagedResultDto<AppCustomerTypeDto>`). Dropdown chỉ cần đọc list, không cần quyền CRUD. Không đụng permission grant, không ảnh hưởng tenant khác. `using` sẵn (cùng namespace).

**Bài học chung:** Razor Page chỉ cần đọc dữ liệu phụ (dropdown/lookup) KHÔNG được inject AppService CRUD `[Authorize]` của module khác — sẽ kéo theo yêu cầu permission+feature của module đó. Dùng service MiniApp/lookup không authorize. Pattern lỗi tương tự có thể còn ở các trang khác load lookup chéo module.

**Build:** lỗi copy DLL (MSB3027/MSB3021 file locked by Web PID) chỉ do Web đang chạy khóa output — KHÔNG phải lỗi compile. Verify compile sạch bằng `dotnet build ... -p:OutDir=<thư mục ngoài>` → Build succeeded 0 errors. Liên quan [[feedback_ef_migration_dll_lock]], [[feedback_permission_require_features]], [[feedback_abp_dual_permission_pattern]].
