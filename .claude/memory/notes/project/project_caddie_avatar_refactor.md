---
name: project_caddie_avatar_refactor
description: "Caddie Module refactored — avatar upload file (not base64), FeatureProtectedCrudAppService, 15MB limit, bỏ Google Fonts, fix Missing policy name"
metadata: 
  node_type: memory
  type: project
  originSessionId: 81d5b313-b800-4559-ba7f-4e4acfa2a89a
---

Caddie Module đã refactor hoàn chỉnh (June 03, 2026):

1. **Avatar upload file thay base64**: DTO dùng `IRemoteStreamContent? AvatarFile` + `string? AvatarUrl`; page model convert `IFormFile` → `RemoteStreamContent`; JS chỉ preview (FileReader), file gửi qua `enctype="multipart/form-data"` với `name="AvatarFile"`
2. **CaddieAppService → FeatureProtectedCrudAppService**: base(caddieRepo, currentTenant, featureChecker); FeatureName = `CaddieFeatures.Management`; override GetListAsync/GetAsync/CreateAsync/UpdateAsync/DeleteAsync; **MUST set GetPolicyName/CreatePolicyName/UpdatePolicyName/DeletePolicyName** in constructor — thiếu sẽ gây "Missing policy name" exception
3. **5 sub-services (Booking/Language/Rating/Schedule/Skill)**: giữ `ApplicationService` base (do business logic phức tạp ko fit CrudAppService), thêm `IFeatureChecker` + `EnsureFeatureAsync()` pattern, giữ `P()` helper
4. **Avatar 15 MB**: `AVATAR_MAX_MB = 15`, `AllowedExtensions = [".jpg",".jpeg",".png",".webp"]`; validate trong `ValidateAvatarFile(IRemoteStreamContent)`; upload qua `_manageImageService.UploadImageAsync(file, tenantId, "caddies", exts)`; xóa cũ nếu startsWith("/uploads/")
5. **Bỏ Google Fonts**: xóa link Manrope+Inter trong Index.cshtml + Detail.cshtml + AppCaddieSchedules/Index.cshtml; CSS `caddie-shared.css` đổi sang font-weight: 700 thay font-family
6. **Localization**: thêm ~50 keys EN/VI cho Caddie UI (labels, placeholders, validation messages)

**Why:** Avatar base64 gây lỗi insert (string quá dài cho DB); service không đồng bộ pattern với các module khác; thiếu PolicyName gây AbpAuthorizationException.

**How to apply:** [[project_caddie_module_complete]], [[feedback_no_ef_in_application_layer]], [[feedback_abp_dual_permission_pattern]]
