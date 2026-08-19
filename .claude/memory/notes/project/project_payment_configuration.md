---
name: PaymentConfiguration feature — full implementation
description: Toàn bộ pattern tạo entity PaymentConfiguration thay thế PaymentQr fields trên GolfCourse, gồm cả lỗi gặp phải và cách fix
type: project
---

## Fact: Đã hoàn thành tính năng PaymentConfiguration (thay thế GolfCourse.PaymentQr*)

Xóa 4 fields `PaymentQrText/BankCode/BankAccount/BankDisplay` khỏi `GolfCourse` entity và tạo entity riêng `PaymentConfiguration`.

### Các file đã tạo/sửa:
- **Domain:** `DomainModels/AppPaymentConfigurations/PaymentConfiguration.cs` — có TenantId, BankBin, AccountNumber, AccountName, MerchantId, ApiKey, Description, LogoUrl, IsActive, DisplayOrder
- **EF:** `MultiTenancyDbContextModelCreatingExtensions` — thêm `ConfigurePaymentModule()`, thêm `DbSet<PaymentConfiguration>`
- **Migration:** `20260407044728_Add_PaymentConfiguration_Remove_GolfCourse_PaymentQr`
- **DTOs:** `AppDtos/AppPaymentConfigurations/` — `PaymentConfigurationDto`, `CreateUpdatePaymentConfigurationDto`
- **Interface:** `AppServices/AppPaymentConfigurations/IAppPaymentConfigurationService.cs` — CRUD + `GetActiveAsync()`
- **Service:** `AppServices/AppPaymentConfigurations/AppPaymentConfigurationService.cs`
- **Permission:** Gom vào group `MiniAppSetting` / `MiniAppSettingHost` (không tạo group riêng)
- **Feature:** `AppPaymentConfigurationFeatures` — có nhưng **không dùng `RequireFeatures`** trên permission (xem bên dưới)
- **Pages:** `Web/Pages/AppPaymentConfigurations/` — Index, CreateModal, EditModal, PageModel, index.js
- **Menu:** Thêm vào nhóm "Cài đặt Mini App" (order: 5), check `canSeePaymentConfigurations` chỉ bằng permission (không check feature)
- **Bill pages cập nhật:** `FnbOrders/Kitchen/Detail.cshtml.cs`, `History.cshtml.cs`, `ProOrders/Board/Detail.cshtml.cs` — inject `IAppPaymentConfigurationService`, gọi `GetActiveAsync()` thay vì đọc `golfCourse.PaymentQr*`

**Why:** Tách PaymentQr ra entity riêng để dễ quản lý nhiều phương thức thanh toán theo thứ tự ưu tiên.

**How to apply:** Khi cần thêm payment provider mới, chỉ cần Insert record mới vào `PaymentConfiguration` table với `IsActive=true`, `DisplayOrder` phù hợp.
