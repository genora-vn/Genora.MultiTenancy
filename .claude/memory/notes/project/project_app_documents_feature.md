---
name: project-app-documents-feature
description: "Online user-guide site at /Documents — host-shared DB, Summernote editor, per-tenant gating by feature/permission"
metadata: 
  node_type: memory
  type: project
  originSessionId: b307db31-ef81-4d5c-910d-2d7c9b8ab49d
---

Trang `/Documents` là trung tâm hướng dẫn online cho hệ thống Genora.MultiTenancy.

**Why:** Hệ thống có rất nhiều module (Golf, Salon, Proshop, FnB, News, Loyalty...) bật/tắt theo từng tenant. Cần một nơi tập trung tài liệu sử dụng, Host edit online (không cần redeploy), mỗi tenant chỉ thấy hướng dẫn cho những tính năng họ được cấp.

**How to apply:** Khi cần thêm module mới hoặc tinh chỉnh docs:
- Entity host-shared (KHÔNG `IMultiTenant`): `AppDocumentSections` + `AppDocumentPages` (`Domain/DomainModels/AppDocuments/`).
- **Quan trọng:** vì entity host-shared nhưng dự án có thể có tenant với connection string riêng (DB tách rời), `DocumentReaderAppService` phải `using (_currentTenant.Change(null))` khi query để LUÔN hit host DB — nếu không tenant với separate DB sẽ thấy bảng rỗng.
- Mỗi section/page có 3 cột gating: `FeatureName`, `TenantPermissionName`, `HostPermissionName` (nullable). `DocumentReaderAppService` lọc bằng `IFeatureChecker` + `IPermissionChecker` theo `ICurrentTenant`. Empty section (mọi page bị filter) cũng ẩn.
- Permission: `AppDocuments.Default` (Tenant View only) + `HostAppDocuments.{Default,Create,Edit,Delete}` (CRUD chỉ Host). KHÔNG `RequireFeatures` — docs luôn available.
- Editor: Summernote (clone pattern AppNews), `onImageUpload` POST tới `/Documents/Manage/PageModal?handler=UploadImage` → `IManageImageService.UploadImageAsync(file, "host", "documents")`. Lazy-load `ContentHtml` qua handler `OnGetContentAsync` để modal mở nhanh.
- URL slug-based: `/Documents/{section}/{page}` (Razor route `@page "{section}/{page?}"`). Slug auto-generate từ Name qua `DocumentSlugifier.Slugify` (bỏ dấu vi-VN + dash) + `EnsureUnique` suffix `-2/-3`.
- Menu: top-level "Tài liệu hướng dẫn" (`fa fa-book`, order 5), đặt OUTSIDE if/else `tenant.IsAvailable` để cả Tenant + Host đều thấy. Auth ở PageModel `[Authorize]`.
- Seed `AppDocumentsDataSeedContributor` chạy 1 lần host-level (`if (context.TenantId != null) return;`) tạo 9 sections skeleton + 1 page placeholder/section. Chạy bằng `dotnet run --project src/Genora.MultiTenancy.DbMigrator` (Web không tự seed). KHÔNG dùng `_uowManager.Begin(requiresNew:true)` trong seeder — đã có outer UoW từ `MultiTenancyDbMigrationService`. Permission constants được hardcode string trong Domain (Domain không reference Application.Contracts) — phải sync nếu rename `MultiTenancyPermissions`. **Seeder phải UPSERT metadata** (FeatureName/TenantPermissionName/HostPermissionName) cho rows đã tồn tại — không chỉ skip — vì user có thể đã có DB seed lần trước với metadata thiếu sót, lần chạy sau cần update mà không ghi đè Name/Icon/Order/Content.
- **Feature gating mỗi section:** mỗi seed PHẢI có `FeatureName` tương ứng (`MiniAppSetting.Management`, `MiniAppGolfCourse.Management`, `SalonBeauty.Management`, `MiniAppProshop.Management`, `MiniAppFnb.Management`, `MiniAppBookings.Management`, `MiniAppMembershipTier.Management`, `MiniAppNews.Management`). Nếu để `FeatureName = null`, tenant sẽ thấy section đó dù không bật feature → user phàn nàn "thấy chuyên mục của tenant khác". `system-admin` cố tình không gate (luôn visible).
- DbSet: `AppDocumentSections`, `AppDocumentPages`. Unique index trên `Slug` (section) và `(SectionId, Slug)` (page). Migration: `20260528074728_Add_AppDocuments_Tables`.
- **JS pattern:** dự án KHÔNG generate JS proxy cho `appDtos.appDocuments.*` — Manage page dùng Razor PageModel handlers (`OnGetSectionsAsync`, `OnGetPagesAsync`, `OnPostDeleteSectionAsync`, `OnPostDeletePageAsync`) + `abp.ajax({ url: 'Documents/Manage?handler=...' })`. Pattern khớp AppZaloAuths/SalonBeauty.
- **AutoMapper:** PHẢI register profile `DocumentSection→DocumentSectionDto` + `DocumentPage→DocumentPageDto` trong `MultiTenancyApplicationAutoMapperProfile.cs`. `CrudAppService.GetAsync` dùng `ObjectMapper.Map`, không có profile sẽ throw `Missing type map configuration`.
- **Razor route — Index.cshtml duy nhất với CATCH-ALL:** Razor Pages KHÔNG support template `{section?}/{page?}` với 2 optional liên tiếp (route engine miss match → 404). GỘP cả landing + detail vào DUY NHẤT `Pages/Documents/Index.cshtml` với `@page "{*slug}"` (catch-all). PageModel parse `Slug` thủ công bằng `.Split('/')` thành `SectionSlug` + `PageSlug`. Index filename bị elide khi build route nên catch-all match `/Documents/{anything}`. Các route con (`/Documents/Manage/...`) vẫn thắng do route precedence (literal > catch-all). PageModel KHÔNG được đặt property tên `Page` (conflict với `PageModel.Page()` method) hay `Section`.
- **PageCount populate:** `GetAllAsync` phải GROUP BY pages theo SectionId rồi merge vào DTO; chỉ trả raw section list sẽ có `PageCount = 0`.

**Files chính:**
- Domain: `DomainModels/AppDocuments/{DocumentSection,DocumentPage}.cs`, `AppDocuments/AppDocumentsDataSeedContributor.cs`, `Domain.Shared/Enums/DocumentStatus.cs`
- Application: `AppServices/AppDocuments/{DocumentSection,DocumentPage,DocumentReader,DocumentMetadata}AppService.cs`, `Helpers/DocumentSlugifier.cs`
- Web: `Pages/Documents/{Index,View}.cshtml(.cs)`, `Pages/Documents/Manage/{Index,SectionModal,PageModal}.cshtml(.cs)`, `documents.css`, `Manage/{manage.css,manage.js}`

Liên quan: [[feedback-abp-dual-permission-pattern]], [[feedback-abp-permission-group-pattern]], [[email-template-fixes]] (Summernote pattern), [[project-payment-toggles-and-news-edit-lazy-load]] (lazy-load ContentHtml pattern).
