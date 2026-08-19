---
name: ABP Tenant/Host dual permission pattern trong ApplicationService
description: Cách handle service cần phục vụ cả Host admin và Tenant admin với 2 permission khác nhau, không dùng [Authorize] cứng
type: feedback
---

Không dùng `[Authorize("MultiTenancy.AppXxx.Default")]` cứng trên class-level khi service cần phục vụ cả Host và Tenant. Thay vào đó inject `ICurrentTenant` và tự map permission.

**Why:** `[Authorize(TenantPermission)]` cứng → Host admin (không có TenantPermission) bị 403 dù đã được cấp `HostAppXxx.Default`. Codebase đã có base class `FeatureProtectedCrudAppService` làm điều này tự động qua `MapPermissionForSide()`.

**How to apply:** Với service custom (không dùng CrudAppService):
```csharp
[Authorize]
public class AppXxxService : ApplicationService, IAppXxxService
{
    private readonly ICurrentTenant _currentTenant;

    private string P(string tenantPermission)
    {
        if (_currentTenant.IsAvailable) return tenantPermission;
        const string tenantRoot = MultiTenancyPermissions.AppXxx.Default;
        const string hostRoot   = MultiTenancyPermissions.HostAppXxx.Default;
        if (tenantPermission.StartsWith(tenantRoot))
            return hostRoot + tenantPermission.Substring(tenantRoot.Length);
        return hostRoot;
    }

    private async Task CheckAsync(string tenantPermission)
        => await AuthorizationService.CheckAsync(P(tenantPermission));
}
```

Hoặc với CrudAppService: kế thừa `FeatureProtectedCrudAppService` và override `TenantDefaultPermission` + `HostDefaultPermission`.
