---
name: ABP Permission group — gom vào group có sẵn, không tạo group riêng
description: Cách tổ chức permission: thêm permission vào group đã có thay vì tạo group mới, và không dùng RequireFeatures trên permission nếu không cần feature gate
type: feedback
---

**Quy tắc tổ chức Permission trong dự án này:**

1. **Không tạo group permission riêng** cho từng tính năng nhỏ nếu nó thuộc về một nhóm lớn hơn. Ví dụ: `PaymentConfiguration` thuộc về "Cài đặt Mini App" → thêm vào group `appSettingGroup` / `appSettingGroupHost` đã có trong `MultiTenancyPermissionDefinitionProvider.cs`.

2. **Không dùng `RequireFeatures` trên permission** nếu tính năng không cần bật/tắt theo tenant. Dùng `RequireFeatures` chỉ khi cần feature gate thật sự (ví dụ: FnB, Proshop cần feature). `AppSettings`, `PaymentConfiguration` không cần feature gate → không có `RequireFeatures`.

3. **Menu check permission thẳng**, không cần check feature thêm:
```csharp
// Đúng:
var canSeePaymentConfigurations = await perms.IsGrantedAsync(MultiTenancyPermissions.AppPaymentConfigurations.Default);

// Sai (nếu không cần feature gate):
var canSeePaymentConfigurations =
    await feature.IsEnabledAsync(AppPaymentConfigurationFeatures.Management) &&
    await perms.IsGrantedAsync(...);
```

**Why:** Tạo group riêng → màn hình phân quyền bị chia nhỏ lộn xộn. `RequireFeatures` không cần thiết → lỗi 403 khi feature chưa bật dù user đã có quyền.
