# Hoa Linh Sales — Lịch sử nhận quà voucherType=2 — 2026-10-01

> Follow-up cùng ngày: [Host/local support](project_hl_gift_receipts_host_fix_20261001.md) đã bỏ TenantRequired, nullable scope + SQL IS NULL + unique Host index. Migration tiếp theo `20261001025429_AddHlGiftReceiptHostUniqueness`; 63 Sales+12 Web+4 EF tests pass, có local HTTP TestServer. Đọc follow-up trước các mốc kiểm thử ban đầu bên dưới.

## Trạng thái

Đã hoàn tất source, migration/SQL, admin page, FE contract/cURL, automated tests. **Chưa apply migration, bật feature/grant role, deploy, UAT browser/DMS/DB thật.** Không thực hiện POST tới domain production. User yêu cầu tiếp tục task dang dở về lịch sử nhận quà Sales, không phải mở lại task HL25 pacing hay HLG.

Tài liệu bàn giao đầy đủ: [HOALINH_GIFT_RECEIPTS_API_20261001.md](../../../../docs/HOALINH_GIFT_RECEIPTS_API_20261001.md).

## Business/data

- Schema HL, entity `DomainModels/AppHlGiftReceipts/HlGiftReceipt`: CreationAuditedAggregateRoot + IMultiTenant; snapshot khách hàng/địa chỉ chi nhánh, SĐT, campaign/kỳ/ngày, voucher/type/value/quantity, xác nhận/status, tier/doanh số/điểm, nhân viên/NPP, source/note/audit.
- Unique entitlement `(TenantId,CustCode,CampaignCode,CampaignPeriod,VoucherCode)`; period null upstream → 0. **Không** đưa phone vào key. Hai quà QT34 và QTHOA cùng GIFT25NAM được nhận độc lập. Retry cùng key trả phiếu cũ.
- Status Confirmed=1 + DTO `isConfirmed=true`: xác nhận yêu cầu nhận quà, **không phải xác nhận giao hàng thực tế**. Không có endpoint sửa/xóa/nhận lại.
- DMS xác minh phone→branch (`GetCustomerDetailAsync`) rồi custCode→campaign/voucher (`GetCampaignDetailAsync`). Không tin tên/địa chỉ/số lượng từ client. Chỉ voucherType2, VoucherValue phải nguyên dương → Quantity.
- Ngày campaign là snapshot báo cáo, **không coi endDate là hạn nhận quà**: preserve pattern Sales type1 vẫn claim sau kỳ tích lũy, DMS còn entitlement là nguồn quyết định. Không thêm quy tắc expiry user chưa yêu cầu. Kiểm tra start<=end.
- Retry đã tồn tại không phụ thuộc campaign còn trong DMS, nhưng vẫn xác minh phone thuộc branch.
- Giữ nguyên API type1, điểm/tiền, UrBox, HL25 inventory, gateway, TenantAutoMigrateMiddleware.

## Code map

- Contracts: `Application.Contracts/AppDtos/HoaLinh/HlGiftReceiptDtos.cs`: input POST/history/admin filter, receipt DTO, mini/admin interfaces.
- Application: `AppServices/HoaLinh/MiniAppHlGiftReceiptService.cs` + `HlGiftReceiptAdminAppService.cs` + `HlGiftReceiptQuery.cs`. No EF reference; AsyncExecuter. Admin manual host/tenant permission, feature check, explicit tenant predicates even when test repository unfiltered.
- EF: `HoaLinh/EfCoreHlGiftReceiptRepository.cs` custom `IHlGiftReceiptRepository`; requires active transaction, parameterized SELECT `UPDLOCK,HOLDLOCK`, recheck before insert. New transactional requiresNew UOW spans recheck+insert+commit. Unique index prevents duplicates.
- **DI gotcha tested/fixed:** custom EF repo requires `ITransientDependency` for conventional registration of its custom interface; AddRepository in DbContext registration also covers generic repo. Regression test checks actual AddAssemblyOf registration. Without marker interface test failed; fixed and 3/3 EF tests pass.
- DbContext + HoaLinh model config: new DbSet/table, unique entitlement index, two read indexes, decimal18,2, tinyint status.
- `HttpApi/Controllers/HoaLinhGiftReceiptMiniAppController.cs`: GET/POST `/api/mini-app/hl/gift-receipts`, same HlApiResult envelope. `HlSalesExcelController` adds `gift-receipts` action. Uses current Sales anonymous phone model; DMS branch relationship check is not token authentication.
- Feature `HoaLinh.GiftReceipts` child of Management, default false. Tenant permission root + Export both RequireFeatures Management+GiftReceipts. Dual roots `MultiTenancy.AppHlGiftReceipts` / `MultiTenancy.HostAppHlGiftReceipts`. Host features bypass but permission and current tenant scope retained.
- Menu and Razor page `/HoaLinh/GiftReceipts`: list/paging/filter/detail; export only with Export grant. Proxy name verified with installed ABP generator: `genora.multiTenancy.appServices.hoaLinh.hlGiftReceiptAdmin` (`getList/get`). Uses existing sales.js for date filters/download; new JS escapes snapshot values and ignores stale list responses.
- Excel 28 columns, full filtered result independent of paging, inclusive last day, text preserves leading zeros/prevents formulas. Number format `#,##0` avoids previous Sales trailing-dot regression; underlying decimal values preserved.
- VI/EN: 46 feature/menu/permission/UI keys per language, JSON parse verified.

## Migration

- `20260930171009_AddHlGiftReceipts` + Designer + snapshot; only CREATE TABLE HL.AppHlGiftReceipts and 3 indexes; no UPDATE/ALTER/drop of existing tables.
- SQL `Migrations/Scripts/AddHlGiftReceipts.sql` generated idempotently from `20260929091139_AddHlBlouseModule` to new migration; baseline/schema HL/history must already exist. **NOT APPLIED**.
- `dotnet ef migrations has-pending-model-changes --no-build`: no pending changes.
- EF default factory uses DbMigrator appsettings and typically targets host. Use controlled explicit DB selection for rollout; don't blindly run all-tenant migrator merely to enable one tenant.

## Verification actually run

- `dotnet test test/Genora.MultiTenancy.Application.Tests --no-restore --filter FullyQualifiedName~HoaLinhSales`: **59/59**, including 24 new receipt tests (two gifts/retry/locked recheck/normalization/different tenant and period/type/quantity/branch/DMS failure, feature, tenant isolation, paging, Excel filters and text/date/numeric values, permissions).
- Web tests filter HlGiftReceipt with custom OutputPath: **8/8** (page feature/host/tenant permissions, mini envelope success/error/history, ABP proxy naming).
- EF tests filter HlGiftReceiptModelTests: **3/3** (DI registration, SQL Server model unique entitlement + reporting indexes, additive migration only).
- Node `--test test/hl-sales-ui-regressions.cjs test/hl-blouse-config-ui-regressions.cjs test/hl-gift-receipts-ui-regressions.cjs`: **33/33** (6 new list/export filters, validation, stale responses, busy errors, XSS escape, pagination).
- Final Web build `dotnet build src/Genora.MultiTenancy.Web/Genora.MultiTenancy.Web.csproj --no-restore -p:OutputPath=D:/Genora/Projects/BE/GitHubs/Genora.MultiTenancy/artifacts/gift-receipts-validation/web/ -v:q -clp:ErrorsOnly`: **0 errors, 388 warnings**. Output isolated because VS/Web lock default bin. Do not stop user's app.
- One verification attempt ran parallel .NET builds sharing obj and failed with CS2012 locks; sequential rerun passes. **Run .NET build/test sequentially for this graph even with different OutputPath.**
- `git diff --check` clean; migration reviewed; localization JSON parses. No commits/deploy.
- Browser runtime discovery returned no available browser. No browser UAT, real DMS request, real SQL concurrency test, or migration application. LocalDB info for MSSQLLocalDB returned API error, so no claim of SQL execution. Mock locked recheck + model index assertions are not SQL concurrency/load tests.

## Next steps / resume

1. Review/apply additive migration on intended host/HL tenant DBs with normal controlled rollout; enable Management+GiftReceipts only intended tenant; assign root+Export grants, deploy updated Web/backend.
2. FE use doc cURLs: POST phoneNumber/custCode/campaignCode/campaignPeriod/voucherCode/note; history GET phoneNumber+custCode with paging. Match campaign+period+voucher to disable button. Existing campaigns contract unchanged.
3. UAT two gifts in same campaign, retry/reload/parallel calls, different phone same branch, wrong branch/type, empty/history pagination, admin filters/Excel, disabled tenant and role without Export.
4. History GET filters original confirming phone; another branch-associated phone may have empty history, but POST returns existing receipt rather than issue duplicate. This behavior documented explicitly.
5. Preserve unrelated user appsettings changes, deleted old Web logs, newly generated Sept28/29/30/Oct1 logs, uploads/hl-blouse. No changes made by this task to these files.

Optional future enhancements, **not implemented**: separate delivery/tracking workflow, explicit claim deadline, signed Zalo user authentication, pushing receipt confirmation back to DMS (no upstream endpoint provided).
