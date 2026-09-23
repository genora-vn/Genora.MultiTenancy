# ACTIVE CONTEXT — Việc đang làm dở

## HLG staging schema + menu recovery — 2026-09-23 (mới nhất)

- Branch `feature/dev-hoalinh-gamification`, starting HEAD `22a126b`. Host `GenoraMultiTenancy` had 0 HLG objects but five baseline HLG migrations in history. Guarded repair SQL backed up/cleared those rows and EF replayed five baseline + `AddHlgDesignContent`: verified 17 HLG tables, 6 history rows, 5 backup rows, `PharmacyCode` present. `dotnet ef database update --no-build` now reports up to date. Direct EF build and Web `HlgMenuValidation` build 0 errors; 17 HLG Web tests pass. Default `dotnet ef database update` inside this sandbox cannot read user NuGet.Config, so use a prior build plus `--no-build` here; normal external shell is unaffected.
- Tenant `HoaLinhMienNam` had 13 baseline HLG tables/5 history rows. Explicit `--connection` applied `AddHlgDesignContent`; now verified 17 HLG tables/6 history rows/`PharmacyCode` present. `Hlg.Management=True` and tenant admin root grants. Plain EF CLI still targets host.
- Host admin root HLG grants exist. Both staging Web sites return 404 for HLG Admin JS, while known HL25 JS returns 200; host HLG routes 404. Source menu/pages were added in `20b9e1c` after permissions. Web Release publish prepared at ignored `artifacts/hlg-web-staging-20260923` with DLL/web.config/HLG JS verified. **Menu issue requires deployment of that current Web package to both IIS staging sites**; no live deployment/authenticated UAT done here. Current source menu condition is correct for both sides. [Runbook](../docs/HLG_STAGING_RECOVERY_20260923.md) · [note](memory/notes/project/project_hlg_staging_schema_menu_recovery_20260923.md).
- Preserve unrelated Web log deletion/new log. HLG repair SQL and preflight migration change are uncommitted in this workspace.

## HLG verification follow-up — 2026-09-19 (mới nhất)

- Branch `feature/nghiadt-hoalinh-gamification`, HEAD `d4f67f9485d156da5e52640a5cad31446511647d`; corrective implementation đã nằm trong HEAD. Appsettings/log changes là pre-existing/unrelated và được giữ nguyên.
- Fix security/validation: shipping-address yêu cầu query `phone`, chỉ nhận session đã finish thuộc customer/tenant; URL validator áp dụng thêm cho category/reward/question image và từng product image-list URL. Route/envelope giữ nguyên; FE phải gửi `phone`.
- Verification phiên này: Web build PASS 0 errors (52 warnings); Application46/46, Domain3/3, Web17/17, JS11/11; EF no pending model changes.
- Browser UAT BLOCKED: Computer Use `apps=[]`, `browsers=[]`; create `iab` trả `Browser is not available: iab`. Không start Web, không DB write/live proxy/authenticated CRUD.
- Migration `20260919112304_AddHlgDesignContent` vẫn NOT APPLIED. Ba UNKNOWN giữ nguyên. Chi tiết: [note](memory/notes/project/project_hlg_verification_followup_20260919.md).

## HLG corrective design audit — 2026-09-19 (mới nhất)

- Đã inspect trực quan31/31 trang PDF ở cả2 lượt;60 screen/component/state;54 nhóm dữ liệu.34 nhóm CMS/cần làm rõ:31 COVERED,0 PARTIAL,0 MISSING,3 UNKNOWN. Coverage source không phải nghiệm thu browser.
- Đã bổ sung Ngành hàng→Nhãn hàng→Sản phẩm, nội dung/FAQ/media/video/related/CTA; CMS Home; game banner/badge; ranking theo game/cơ cấu giải/công bố người trúng; fulfillment; Users read-only chi tiết; PharmacyCode riêng HLG, retailer3, progress/game-history API. Giữ API Mini cũ.
- Migration mới `20260919112304_AddHlgDesignContent` + SQL idempotent:4 bảng/6 cột nullable HLG; review additive; EF no pending model changes; **chưa apply DB**.
- Web build0 errors;42 Application +3 Domain +17 Web +11 JS tests pass.13 proxy names xác minh bằng generator ABP đang cài; chưa chạy lại live proxy HTTP trong phiên corrective.
- Browser UAT BLOCKED: browser tool báo không có browser, list=[]; chưa có thao tác tenant CRUD. Không suy ra DB kết nối lỗi. Ba UNKNOWN: vòng quay; tự chọn/trao giải và xử lý hòa; thời điểm cấp/trừ quà. Công bố winner thủ công không tự tạo đơn giao quà.
- Bước tiếp: review/apply migration đúng workflow multi-database, cấu hình catalog thật/quyền Content và chạy UAT tenant. Xem [audit](docs/HLG_FULL_DESIGN_AUDIT_20260919.md), [note](memory/notes/project/project_hlg_corrective_design_audit_20260919.md).


> File này mô tả bối cảnh đang hoạt động của phiên làm việc gần nhất.
> Cập nhật ở CUỐI mỗi phiên (xem [handover/HANDOFF.md](handover/HANDOFF.md)).

## Cập nhật gần nhất

### HLG — PDF design audit + runtime rules (2026-09-19)
- Đã text-extract, render và kiểm tra trực quan đủ 31/31 trang `HLG_FIGMA_ALL_SCREENS.pdf`; proxy runtime xác nhận là `genora.multiTenancy.appServices.hlg.admin.*`.
- Không migration: game bị chặn server-side nếu không `Ongoing` hoặc ngoài `StartAt`–`EndAt`; profile tính `accuracyPercent` từ completed sessions; ranking chỉ trả event active trong thời gian hiệu lực; profile contract thêm optional `VgaCode` vốn đã có trên `Customer`.
- Solution build, 13 HLG Application tests và 7 HLG JS tests pass. Browser UAT tenant thật BLOCKED vì không có tenant DB/feature/permission credentials.
- PDF gap thật còn lại: FAQ/sản phẩm liên quan có cấu trúc, cơ cấu giải thưởng/danh sách trúng thưởng. Source chỉ có content tự do và chưa có model prize/winner; không tự tạo schema khi chưa có quy tắc cấp/trao giải.

### HLG — Admin Razor UI 5 nhóm (2026-09-18)
- Đã triển khai Rewards, Knowledge (Category+Product), Ranking, Games+Questions, Users read-only; thêm service còn thiếu, menu order 49, VI/EN, feature/dual permission cho API và pages.
- CorrectKey chỉ tải qua editor yêu cầu Games.Edit; không có trong list/get DTO hoặc mini-app response. Câu hỏi + options lưu transaction; chặn thay đổi khi game đã có phiên chơi.
- Web build 0 errors; 13 Application + 7 JS tests pass. Chưa UAT browser/DB thật, chưa deploy. Không migration mới; giữ appsettings/log changes của user.
- [Chi tiết](memory/notes/project/project_hlg_admin_razor_ui_20260918.md) · [UAT](docs/HLG_ADMIN_UAT_20260918.md).

### IIS staging — ARR / YARP / guard configuration repair (2026-09-21)
- Đã kiểm tra log/cấu hình/2ảnh và source mới `e4ec431`: lỗi chính key trùng giữa2tenant, bật2guard, custom gateway.Staging.json không được nạp; ảnh còn DLL cũ. TestHL25trênhostnameHLG đang bypassYARP qua rulefallbackcũ.
- Source báo lỗi validation theo field không lộsecret; thêm mẫuARR chặn wronghost, script sinhcấuhình/keyriêng khớpGateway+ABP và runbook `docs/tenant-gateway/iis/README.md`.
- Build/publishPASS;60gateway+42WebtestsPASS; generatorPowerShell5 PASSStaging/Production. Khôngdeploy/IISruntimeUAT/loadtest hoặc migration. Cầnbindingloopback5088/8868 vàAppPool/ARRflags thực tế; Port user gửi có2dòng làIP nên chưa rõ.
- Next: theo runbook mới, generateconfig/freshpublish/reviewbindings rồi smoke200/403/404/429 và đo tải trướcproduction. Giữcácsửauser vềtenantresolver. [Note](memory/notes/project/project_gateway_iis_staging_fix_20260921.md).

### Gateway — đổi tên project dùng chung (2026-09-21)
- Tên hiện tại: `src/Genora.MultiTenancy.Gateway` và `test/Genora.MultiTenancy.Gateway.Tests`; csproj, namespace, assembly, solution, launch profile và IIS `web.config` đã đồng bộ. Runbook dùng đường dẫn mới.
- Restore/build và42 gateway tests PASS; publish local PASS, xác minh DLL/IIS entry point mới và không đóng gói DLL gateway cũ. Giữ cấu hình/quota/route/guard; không migration hay deploy.
- Chưa thay Git index của user. Bước tiếp theo vẫn là cấu hình IIS origin/secret rồi triển khai và test staging theo `docs/tenant-gateway/README.md`. [Chi tiết](memory/notes/project/project_gateway_rename_20260921.md).

### Multi-tenant YARP — HL25 500 / HLG 300, staging preparation (2026-09-21)
- Source mở rộng gateway cũ theo exact host + configured TenantId. Alias/modules cùng tenant chung quota; tenant khác độc lập. Generic opt-in ABP guard pin tenant/key/path; legacy mode còn hỗ trợ nhưng không trộn cấu hình.
- Staging/prod public domains/GUID đã điền trong `docs/tenant-gateway/*.example.json`; Host quản trị nằm ngoài gateway. Còn thiếu IIS origin staging và secret; không suy diễn origin từ public URL/production8868.
- Fix regression YARP default OriginalHost transform xóa Host canonical. Gateway/Web builds PASS,42 gateway +40 Web +12 Node tests PASS. Không DB migration/businessAPI changes. Không deploy/browserUAT/k6load/SQLcapacitytest, k6 chưa có.
- Next: theo `docs/tenant-gateway/README.md`, điền cấu hình và deploy staging, smoke/direct-origin403/lowquota429, rồi đo tải250→500 HL25 và300 HLG riêng/kết hợp. Quota local1process, cần giữ1worker/tránhrecycleoverlap. [Chi tiết](memory/notes/project/project_multi_tenant_yarp_gateway_20260921.md).

### HL25 — YARP gateway 500 RPS: source ready, deployment pending (2026-09-20)
- Branch `hotfix/20260920`, baseline `060df7e`. Separate YARP gateway with one shared500RPS quota for public HL25 APIs. Keep `https://duocpham-hoalinh.genora.vn`; preserve Admin/static routing and unrelated Ocelot routes.
- Runtime tenant GUID: `209567fc-4850-44e9-11c8-3a23c58d15a4`. Origin IP:8868 responded HTTP200; HTTPS handshake failed. Loopback example requires YARP on the ABP machine; no shared key over public HTTP.
- Gateway/Web builds PASS;19 gateway +24 Web HL25 tests and7 Node load-script tests PASS. ABP adds an opt-in guard (default off). No migration/business API change, deployment or real SQL load test. Cache/quota remain process-local.
- Runbook: `docs/hl25-gateway/README.md`; scripts: `tests/load/hl25-gateway/`. Remaining: ingress fallback, placement/binding, secrets/proxy trust, IIS/UAT and measured250→500RPS. Existing request-time auto-migration remains a risk. [Details](memory/notes/project/project_hl25_yarp_gateway_20260920.md).

### HL25 — cache read APIs + index (2026-09-20)
- Branch `hotfix/20260920`, baseline `c76c54b`. Thêm cache bộ nhớ 20 phút theo tenant cho config/campaigns/templates/gifts; khóa chống nạp trùng, invalidation sau commit Admin và khi quà vừa hết hàng. API contract/quy tắc quay giữ nguyên, tồn kho quay vẫn đọc DB.
- EF migration **20260920100056_AddHl25MiniAppReadIndexes** + SQL `docs/hl25_read_indexes_20260920.sql`: 3 index mới, explicit filter unique; bỏ qua DB thiếu bảng HL25/index cùng tên đã có. **Chưa apply DB**.
- Build 0 errors; **43 Application + 18 Domain + 6 EF + 14 Web = 81 .NET tests**, **3 JS tests** pass; EF model khớp snapshot. Test 7.000 lời gọi helper đồng thời mỗi nhóm/cold+expiry chỉ nạp một lần mỗi đợt.
- **Giới hạn:** cache/lock/invalidation chỉ trong một process. Chưa browser/API UAT, chưa benchmark HTTP/SQL 1.000 CCU. Nhiều worker/replica cần cache chung và invalidation/lock liên node. Appsettings/logs có sẵn giữ nguyên; không deploy/restart/commit.
- [Chi tiết và lệnh verify](memory/notes/project/project_hl25_cache_indexes_20260920.md).

### HL25 — staging AgeGroup schema repair (2026-09-18)
- Lỗi staging Invalid column name AgeGroup. Git xác nhận migration AddHl25Module cùng ID đã bị sửa BirthDate→AgeGroup in-place; DB chạy bản cũ không được nâng cấp.
- Đã thêm migration 20260918093000_EnsureHl25ParticipantAgeGroup: chỉ thêm AgeGroup nếu thiếu (tinyint NOT NULL, default Unknown=0), giữ BirthDate/dữ liệu cũ, bỏ qua cột đã đúng. Down giữ cột để tránh mất dữ liệu.
- EF build/script generation + no pending model changes đã kiểm tra; 4 SQL Server checks trên bảng tạm pass. SQL idempotent tại docs/hl25_agegroup_repair_20260918.sql. Chưa áp schema/data DB nghiệp vụ hoặc staging, anh xác nhận tenant HL25 dùng DuocPhamHoaLinh; DB trong cấu hình hiện tại đã có AgeGroup đúng kiểu. Migration bỏ qua DB không có bảng HL25 khi chạy toàn bộ tenant; chưa xác minh trực tiếp staging.
- Cần deploy migration mới, migrate đúng DB tenant HL25, restart host và test lại. [Chi tiết](memory/notes/project/project_hl25_staging_agegroup_migration_fix_20260918.md).

### Hoa Linh Sales — Excel money format (2026-09-18)
- User xác nhận tải Excel đã hoạt động. Đã đổi format tiền #,##0.## → #,##0 cho Giá trị (PointHistory), Số tiền (GiftExchanges), Thành tiền (Orders); bỏ dấu thập phân thừa cuối số, giữ numeric values.
- 14 Application tests pass, gồm assertions chuỗi hiển thị 600,000 / 500,000 / 900,000 trên file XLSX xuất thật qua service. Không migration/DB write; cần rebuild/restart host và xuất file mới.
- [Chi tiết](memory/notes/project/project_hl_sales_excel_money_format_fix_20260918.md).

### Hoa Linh Sales — sửa tải Excel (2026-09-18)
- Nhánh dev. Console TypeError getTenantIdCookie trên 3 trang Sales do sales.js gọi API không có trong ABP runtime; lỗi xảy ra trước fetch.
- Đã bỏ API/header tenant thủ công, giữ credentials same-origin gửi cookie và toàn bộ logic tải Excel theo bộ lọc.
- 12 JS tests pass (thêm 4 test chạy download thật thay vì mock); syntax/diff checks pass. Không backend/schema change hoặc DB write; chưa kiểm tra browser live.
- Cần serve JS mới và Ctrl+F5 ở 3 trang. [Chi tiết](memory/notes/project/project_hl_sales_excel_tenant_cookie_fix_20260918.md).

### DbMigrator — HLG host seeder failure (2026-09-18)
- Nhánh `dev`, baseline HEAD `b8d0c06`; Sales exports đã commit. Fix này chưa commit/deploy.
- Xác nhận SQL read-only: host GenoraMultiTenancy không có bảng HLG, nhưng history ghi đủ 5 migration HLG. Seeder host chạy luôn gây SQL 208 sau migrate host và chặn vòng lặp tenant.
- Đã sửa seeder HLG chỉ chạy tenant bật Hlg.Management, dùng tenant scope riêng; host và tenant không bật HLG không truy vấn bảng HLG. 3 Domain tests pass, DbMigrator build 0 errors.
- Chưa chạy lại DbMigrator trên DB thật hoặc sửa history/schema. Cần chạy lại sau rebuild; tenant bật HLG phải có đủ schema. Cảnh báo Salon/BonusAmount không thuộc lỗi dừng này.
- Chi tiết: [note HLG host seeding](memory/notes/project/project_hlg_host_seed_migration_failure_20260918.md).

### Hoa Linh Sales — Admin filters + Excel (2026-09-17)
- **Nhánh:** `feature/hoalinh-sales`, baseline HEAD `501c10c`; thay đổi lần này chưa commit/deploy.
- **Tên gọi:** Hoa Linh Sales = Hoa Linh cũ / Hoa Linh Gắn Kết, DB `HoaLinhMienNam`, schema `HL`; tách biệt HL25 và HLG.
- **Đã xong code:** sửa ISO/ngày VN + placeholder/validation PointHistory (cả 2 tab); thêm lọc ngày GiftExchanges; Excel theo toàn bộ bộ lọc cho PointHistory/GiftExchanges/Orders, đúng cột yêu cầu. Orders gộp Genora+DMS dùng chung query cho bảng/Excel, đọc đầy đủ trang DMS.
- **Kiểm tra:** Web build 0 errors, 14 Application + 8 JS tests pass; chưa UAT browser/DB/DMS thật. Không migration mới.
- **Còn lại:** rebuild/restart host và smoke-test JS proxy, lọc 17/09/2026–17/09/2026, tải Excel với tài khoản Host/Tenant và dữ liệu thật; xem hiệu năng DMS nhiều trang. Giữ nguyên thay đổi appsettings của user.
- **Chi tiết:** [Hoa Linh Sales filters/Excel](memory/notes/project/project_hl_sales_admin_filters_excel_20260917.md).

### Merge 2026-09-16 — Hoàn tất merge feature/hoalinh-gamification vào feature/dev-hoalinh-gamification
- **Ngày:** 2026-09-16
- **Nhánh:** `feature/dev-hoalinh-gamification` (HEAD `53e767f`)
- **Merge commit:** `53e767f` — merge `feature/hoalinh-gamification` (HEAD `b507697`) vào `feature/dev-hoalinh-gamification`
- **Conflicts đã fix (4 files):**
  - `.claude/ACTIVE_CONTEXT.md` — giữ cả HL25 + HLG sections
  - `MultiTenancyPermissions.cs` — giữ cả HL25 + HLG permission definitions (10 conflict sections)
  - `MultiTenancyPermissionDefinitionProvider.cs` — giữ cả HL25 + HLG permission providers
  - `MultiTenancyDbContext.cs` — giữ cả HL25 + HLG DbSet + OnModelCreating
- **Build:** ✅ Thành công, 0 errors
- **Trạng thái:** Nhánh `feature/dev-hoalinh-gamification` giờ chứa toàn bộ code HL25 (Hoa Linh 25 Năm) + HLG (Hoa Linh Gamification)

### HL25 — Hoa Linh 25 Năm (đã hoàn thành P0-P7 + Admin updates)
- **2026-09-14 — Follow-up UI/kho quà (mới nhất, chưa commit/deploy):** sửa enum dropdown bằng select/option vi/en tường minh; sửa treo lịch sử quay do `visible` nhận raw row nhưng code đọc data.record. Thêm **WheelImageUrl** riêng cho kho quà + modal Large hai ảnh (multipart upload hoặc URL) + tiền VNĐ; API wheel trả wheelImageUrl/giftImageUrl, slotImageUrl fallback. Migration **20260914111213_AddHl25GiftWheelImage** + SQL đã tạo, **chưa apply**. 17 Application + 14 Web + 3 JS tests pass, Web build OutDir riêng pass; host đang giữ DLL cũ, cần migrate/rebuild/restart. Chi tiết: [note UI/images](memory/notes/project/project_hl25_gift_images_ui_fixes_20260914.md).
- **2026-09-14 — Đã triển khai cập nhật Admin theo approach, chưa commit/deploy:** anh chốt **tạo thiệp nhận lượt đầu; chia sẻ nhận lượt thứ hai**. Đã sửa Domain/MiniApp, transaction tạo thiệp/AdminGrant; guard Won→Delivered idempotent; bảo toàn ID/ảnh cấu hình vòng quay; validate ngày/kho; báo cáo hết ngày cuối + tách lượt Admin; sửa code/message lỗi API. **32 tests pass**, build solution pass; EF không có model change, không tạo/apply migration. Chưa UAT browser/DB thật và tải đồng thời. Chi tiết/delta/checklist: [Admin update 14/09](docs/HOALINH25_ADMIN_UPDATE_20260914.md). Note context review bên dưới phản ánh baseline TRƯỚC sửa.
- **2026-09-14 — Khôi phục context, đối chiếu source/Git:** nhánh `feature/dev-hoalinh-25years`, HEAD `31d8e11`; các thay đổi 08/09 đã commit. **Source hiện tại cộng lượt ở cả CreateFrame và ShareFrame, chung trần EarnedCycles=2; AdminGrant không áp trần.** Các mô tả cũ "chỉ cộng sau khi chia sẻ" dưới đây là lịch sử. Đã kiểm tra danh sách migration offline, chưa xác minh DB đích. Chưa có yêu cầu cập nhật chức năng cụ thể; bước tiếp theo là lập bảng delta theo từng trang, ưu tiên Wheel/Participants và quy tắc cấp lượt/trao quà. Chi tiết: [note rà soát 2026-09-14](memory/notes/project/project_hl25_context_review_20260914.md).

### HLG — Hoa Linh Gamification
- Checkpoint tạm dừng 2026-08-21 đã được tiếp tục ngày 2026-09-18; Admin Razor UI đủ 5 nhóm đã triển khai và kiểm thử tự động.

## HLG — ĐIỂM DỪNG (hiện tại)
- Corrective design audit/source implementation đã thực hiện; không coi checkpoint Admin5 nhóm là đủ design.
- Chạy UAT tenant theo checklist sau khi review/apply migration AddHlgDesignContent; resolve3 UNKNOWN trong audit trước automatic game reward.
- Chi tiết, test counts, proxy evidence và blocker ở mục mới nhất đầu file và docs/HLG_FULL_DESIGN_AUDIT_20260919.md.

## HLG — checkpoint backend trước corrective audit (lịch sử, không phải design coverage hiện tại)
- **Đã xong:** Phase 0 (hạ tầng), Phase 1 (Auth + Profile), Phase 2 (Knowledge base), Phase 3 (Games engine), Phase 4 (Rewards & Shipping), Phase 5 (Ranking), Phase 6 (Live-feed SignalR), Permission provider (`HlgManagement` + `HlgManagementHost`), sample data seeder (`HlgDataSeedContributor`). Xem `architecture/module-hlg.md` (6 quyết định nghiệp vụ + 3 quyết định kiến trúc AD-1/2/3).
- **Backend mini-app: HOÀN TẤT 100%** — ~24 endpoint theo contract, đã gửi CURL cho anh test (bỏ header `__tenant`).
- **Migration đã sinh:** `AddHlgModule` (schema HLG + HlgUserProfile), `AddHlgKnowledge` (3 bảng knowledge), `AddHlgGames` (5 bảng game), `AddHlgRewards` (3 bảng reward), `AddHlgRanking` (1 bảng ranking event). SQL script idempotent tại `Migrations/Scripts/`.
- **Checkpoint cũ — files admin đầu tiên (Rewards):** `Application.Contracts/AppDtos/Hlg/Admin/{HlgRewardAdminDtos,IHlgRewardAdminAppService}.cs` + `Application/AppServices/Hlg/Admin/HlgRewardAdminAppService.cs`. Ba localization key Reward còn thiếu đã bổ sung VI/EN ngày 2026-09-18.
- **Việc runtime chưa chạy:** áp `AddHlgKnowledge` + `AddHlgGames` + `AddHlgRewards` + `AddHlgRanking`; tạo tenant "Hoa Linh Miền Nam Gamification" + bật feature `Hlg.Management`; re-seed host admin để nhận permission `HostAppHlg*`. Từ fix 2026-09-18: seeder mẫu HLG bỏ qua host; chỉ tenant bật Hlg.Management mới seed.
- **SignalR live-feed:** hub `/signalr-hubs/hlg-live-feed`, client `JoinGame(gameId)`, event `hlg.live-feed.activity`. Endpoint polling `games/{id}/live-feed` vẫn giữ làm fallback.
- **Điểm chưa nối dây:** `GameResult.reward` + `requiresShippingAddress` trong finish (cần mapping game↔reward); `accuracyPercent` trong profile/stats; tích hợp UrBox thật cho voucher.
- **Lưu ý (BD-2):** /answer chấm theo `HlgQuestion.CorrectKey` (bí mật, KHÔNG serialize ra client); /finish đối soát từ `HlgSessionAnswer`, bỏ qua totalScore client, log cảnh báo nếu lệch.
- **Lưu ý (AD-2):** điểm game cộng vào `Customer.BonusPoint` khi finish (1 lần); redeem quà trừ `Customer.BonusPoint` trong transaction ACID. An toàn vì tenant HLG có DB riêng.
- **Lưu ý (BD-3, BD-6):** redeem phân luồng theo customerType — voucher=Done; physical+consumer bắt buộc địa chỉ (status Shipping); physical+pharmacy=Pending.
- **Lưu ý permission (feedback_permission_require_features):** group Tenant "Hoa Linh Gamification" chỉ hiện trong modal phân quyền KHI feature `Hlg.Management` đã bật cho tenant. Host group hiện luôn.
- **Connection string cảnh báo:** `DbMigrator/appsettings.json` trỏ SQL Server dùng chung từ xa (103.157.218.187) + chứa mật khẩu sa plaintext. Migration chỉ nên áp sau khi review SQL script.

## Trạng thái các mốc gần đây (suy ra từ note mới nhất)
Theo mốc thời gian trên tên note, các đợt làm việc gần nhất tập trung vào:
- **Caddie multi-caddie** (mới nhất, migration 20260725062150): booking gắn nhiều Caddie vào từng golf player, phí Caddie cộng vào TotalAmount, API upsert/unassign.
- **Hoa Linh loyalty + UrBox + Zalo OA** (migration 20260709064009): điểm thưởng FIFO, worker hết hạn, gift exchange status enum mới, eVoucher.
- **Salon Beauty** UI polish + deposit/loyalty + MiniApp payment.

## Nhận định lịch sử về task treo (không áp dụng cho HLG corrective audit ở đầu file)
Các note dạng `*_complete` cho thấy các phase lớn đã đóng. Không phát hiện file "in-progress"
nào ngoài `salon_beauty_implementation_progress.md` (bản cũ, đã được thay bằng `*_complete`).

## Cách xác định task đang làm dở (quy trình)
1. Sắp xếp note trong `memory/notes/project/` theo ngày (hậu tố `_juneXX`, `_roundXX`, `_YYYY_MM_DD`).
2. Note mới nhất KHÔNG có hậu tố `complete` → khả năng là việc dở.
3. Đối chiếu với `git log` và trạng thái build hiện tại.
4. Kiểm tra TODO/FIXME trong code các module tương ứng.

> Khi bắt đầu việc mới: cập nhật mục "Cập nhật gần nhất" và "Việc vừa làm" ở trên.
