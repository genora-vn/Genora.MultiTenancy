---
name: ABP Permission definition phải gọi RequireFeatures cho Tenant
description: Khi định nghĩa permission cho Tenant, phải gọi RequireFeatures() trên cả root lẫn mọi child, nếu không feature bị tắt vẫn hiện trong phân quyền
type: feedback
---

Mỗi `PermissionDefinition` (root + các child Create/Edit/Delete) dành cho Tenant phải gọi `.RequireFeatures(FeatureName.Management)` — thiếu một level là permission đó vẫn xuất hiện trong UI phân quyền dù Host đã tắt feature cho tenant.

Menu contributor cũng phải check feature trước permission:
```csharp
var canSee = await feature.IsEnabledAsync(XxxFeatures.Management)
          && await perms.IsGrantedAsync(MultiTenancyPermissions.AppXxx.Default);
```

**Why:** Bug PaymentConfiguration — permission định nghĩa có comment "không ràng Feature", menu chỉ check permission. Kết quả tenant bị tắt tính năng vẫn thấy menu và group phân quyền.

**How to apply:** Khi tạo permission mới cho Tenant trong `MultiTenancyPermissionDefinitionProvider`, luôn `.RequireFeatures()` trên root và tất cả child. Khi thêm menu item trong `MultiTenancyMenuContributor`, luôn check `feature.IsEnabledAsync` trước `perms.IsGrantedAsync`.
