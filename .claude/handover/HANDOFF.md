# HANDOFF — Bàn giao giữa các phiên làm việc

## HLG staging schema/menu — 2026-09-23 (current handoff)

- Host `GenoraMultiTenancy` had zero HLG objects with five HLG baseline migration rows. Guarded SQL repair backed them up (`dbo.HlgMigrationHistoryRepair_20260923`) and removed just those five history rows; EF replayed the baseline and `AddHlgDesignContent`. Verified 17 HLG tables, six migration rows and `PharmacyCode`; a second `dotnet ef database update --no-build` was no-op.
- Tenant `HoaLinhMienNam`: after explicit `--connection` migration, 17 HLG tables/six HLG history rows/`PharmacyCode` present. `Hlg.Management=True`, admin roots granted. Host repair SQL was not run on tenant.
- Both staging IIS hosts serve old Web: `/Pages/Hlg/admin.js` and HLG routes 404, while HL25 assets return 200. Source menu/pages exist. Local Web Release artifact `artifacts/hlg-web-staging-20260923` is ready (DLL/web.config/HLG JS verified). Deploy complete package to both sites, preserve each site's staging config, then run authenticated host/tenant smoke per [runbook](../../docs/HLG_STAGING_RECOVERY_20260923.md). No IIS deployment or signed-in UAT done here.
- EF and Web builds pass, 17 HLG Web tests pass. `dotnet ef database update` with its implicit build is blocked by sandbox access to user NuGet.Config; explicit build followed by `--no-build` worked. Preserve unrelated Web log changes. [Detailed note](../memory/notes/project/project_hlg_staging_schema_menu_recovery_20260923.md).

## HLG verification follow-up — 2026-09-19 (mới nhất)

- Branch `feature/nghiadt-hoalinh-gamification`; HEAD `d4f67f9485d156da5e52640a5cad31446511647d`. Corrective implementation đã commit trong HEAD, follow-up fixes chưa commit.
- Đã fix: `POST games/sessions/{sessionId}/shipping-address` yêu cầu query `phone`, chỉ chấp nhận session đã finish thuộc customer/tenant; Admin image URL validation đã phủ category/reward/question/product legacy list.
- Phiên này thực chạy: Web build PASS 0 errors/52 warnings; Application46, Domain3, Web17, JS11 đều pass; EF pending model check clean.
- Browser UAT BLOCKED: `apps=[]`, `browsers=[]`, create iab báo `Browser is not available: iab`. Không start host, không live proxy/tenant CRUD/DB write.
- Migration `20260919112304_AddHlgDesignContent` NOT APPLIED. Ba UNKNOWN giữ nguyên. Unrelated appsettings/log work phải tiếp tục được bảo toàn.
- Next: FE bổ sung query `phone` cho shipping-address; review/apply migration đúng target/workflow sau khi được xác nhận; UAT authenticated tenant khi có browser; SQL concurrency test cho prize capacity.
- Chi tiết: [follow-up note](../memory/notes/project/project_hlg_verification_followup_20260919.md).

## HLG corrective design audit — 2026-09-19 (mới nhất)

- Đã inspect trực quan31/31 trang PDF ở cả2 lượt;60 screen/component/state;54 nhóm dữ liệu.34 nhóm CMS/cần làm rõ:31 COVERED,0 PARTIAL,0 MISSING,3 UNKNOWN. Coverage source không phải nghiệm thu browser.
- Đã bổ sung Ngành hàng→Nhãn hàng→Sản phẩm, nội dung/FAQ/media/video/related/CTA; CMS Home; game banner/badge; ranking theo game/cơ cấu giải/công bố người trúng; fulfillment; Users read-only chi tiết; PharmacyCode riêng HLG, retailer3, progress/game-history API. Giữ API Mini cũ.
- Migration mới `20260919112304_AddHlgDesignContent` + SQL idempotent:4 bảng/6 cột nullable HLG; review additive; EF no pending model changes; **chưa apply DB**.
- Web build0 errors;42 Application +3 Domain +17 Web +11 JS tests pass.13 proxy names xác minh bằng generator ABP đang cài; chưa chạy lại live proxy HTTP trong phiên corrective.
- Browser UAT BLOCKED: browser tool báo không có browser, list=[]; chưa có thao tác tenant CRUD. Không suy ra DB kết nối lỗi. Ba UNKNOWN: vòng quay; tự chọn/trao giải và xử lý hòa; thời điểm cấp/trừ quà. Công bố winner thủ công không tự tạo đơn giao quà.
- Bước tiếp: review/apply migration đúng workflow multi-database, cấu hình catalog thật/quyền Content và chạy UAT tenant. Xem [audit](../docs/HLG_FULL_DESIGN_AUDIT_20260919.md), [note](../memory/notes/project/project_hlg_corrective_design_audit_20260919.md).


> Điền file này khi bàn giao cho người/phiên khác. Xem quy trình đầy đủ ở [../LOAD_CONTEXT.md](../LOAD_CONTEXT.md).

## Checklist bàn giao (người giao)
- [ ] Đã cập nhật `../ACTIVE_CONTEXT.md` (việc vừa làm + việc còn dở).
- [ ] Đã thêm dòng vào `../TASK_LOG.md`.
- [ ] Đã cập nhật `../PROJECT_STATE.md` nếu đóng mốc lớn.
- [ ] Đã ghi note chi tiết vào `../memory/notes/` + cập nhật `../MEMORY.md`.
- [ ] Build sạch (`dotnet build`) và commit/push code liên quan.
- [ ] Ghi rõ migration mới (nếu có) vào TASK_LOG.

## Checklist tiếp nhận (người nhận)
- [ ] Đọc theo thứ tự trong `../LOAD_CONTEXT.md`.
- [ ] `git pull` + `dotnet build` + kiểm tra migration.
- [ ] Xác định task dở theo `../ACTIVE_CONTEXT.md`.

---

## Bàn giao hiện tại

### HLG Admin 2026-09-18 — checkpoint mới nhất
Đủ 5 nhóm Razor UI + service/menu/quyền/VI-EN đã triển khai; Web build 0 errors, 13 Application + 7 JS tests pass. Chưa UAT/deploy/DB write. Tiếp theo restart Web và UAT tenant HLG theo checklist. [Note](../memory/notes/project/project_hlg_admin_razor_ui_20260918.md) · [UAT](../docs/HLG_ADMIN_UAT_20260918.md). Checkpoint 2026-08-21 bên dưới chỉ là lịch sử, đã được thay thế.

### HLG PDF design audit 2026-09-19
Đã render và kiểm tra trực quan đủ 31/31 trang PDF export. Proxy runtime xác nhận `genora.multiTenancy.appServices.hlg.admin.*`. Không migration: game chỉ bắt đầu khi `Ongoing` và trong lịch; profile tính accuracy từ completed sessions; ranking không trả event ngoài hiệu lực; `VgaCode` được thêm optional vào profile contracts. Solution build + 13 Application + 7 JS pass. Browser UAT BLOCKED vì chưa có tenant HLG/database/credentials. Còn gap cần quyết định business trước schema: FAQ/sản phẩm liên quan có cấu trúc, cơ cấu giải thưởng và danh sách trúng thưởng của ranking event.
### IIS staging repair — 2026-09-21
Sourcebaselinee4ec431; giữ3commitproxy/tenantresolvercủauser. Cácnguyênnhânstartup/routingđãxácđịnhvàsửa diagnostics/mẫutriểnkhai; dùng `docs/tenant-gateway/iis/README.md` cùngNew-DeploymentConfig.ps1 vàGet-IisInventory.ps1.60gateway/42WebtestsPASS,build/publishPASS, generatorPowerShell5PASS2env. ChưaIISruntime/cutover/loadtest; khôngxácnhậnproduction-ready. Cầnbindingloopback5088/8868,Host443rõhostname/ARRpreserveHost/AppPool/envđúngrồismoke200/403/404/429; keys mới sinh trênmáytriểnkhai, khôngcopysecrettronghộithoại. [Note](../memory/notes/project/project_gateway_iis_staging_fix_20260921.md).

### Gateway dùng chung — đổi tên project (2026-09-21)
Sử dụng `src/Genora.MultiTenancy.Gateway/Genora.MultiTenancy.Gateway.csproj` và `test/Genora.MultiTenancy.Gateway.Tests/Genora.MultiTenancy.Gateway.Tests.csproj` từ đây. Namespace/assembly/solution/launch profile/IIS web.config và lệnh trong runbook đã cập nhật.42 gateway tests PASS; gói publish local gọi đúng DLL mới, không chứa DLL gateway cũ. Giữ cấu hình nhiều tenant và compatibility HL25 legacy; không deploy/schema change. User đã staged công việc trước, Git index giữ nguyên; cần đưa cả rename vào commit sau review. [Note](../memory/notes/project/project_gateway_rename_20260921.md).

### Multi-tenant YARP — 2026-09-21
Đã mở rộng source gateway từ HL25 sang dictionary tenant/profile/hostname, HL25quota500 vàHLG300. GuardABP genericopt-in, fixedHost regression được kiểm thử. Staging/production examples điềnpublicdomain/GUID đã đối chiếu; secrets/backendaddress trống bắt buộc điền. Runbook `docs/tenant-gateway/README.md`, smoke script và dualtenant gatedk6 readscript. Gateway/Web buildsPASS;42gateway +40Web +12Node testsPASS. KhôngbusinessAPI/DBmigration, deploy/k6/SQLcapacityUAT; quota1process. Tiếp theo xác minhIISorigin staging/secret/ingressHostpreservation+Admin/staticfallback, deploystagingrồitest429/403 và tải250→500/300. Giữ appsettings/logs user và unrelatedcode. [Note](../memory/notes/project/project_multi_tenant_yarp_gateway_20260921.md).

### HL25 YARP gateway 500 RPS — 2026-09-20
Source ready on `hotfix/20260920` (baseline060df7e): standalone YARP2.3/.NET9, shared public HL25 quota500RPS, opt-in ABP guard, deployment examples and gated k6 scripts. Gateway/Web builds PASS;19 gateway +24 Web tests and7 Node tests PASS. Keep `duocpham-hoalinh.genora.vn`; verified tenant GUID209567fc-4850-44e9-11c8-3a23c58d15a4. Port8868 responded HTTP200, HTTPS failed. Next: review actual ingress/Ocelot version and Admin/static fallback, gateway placement/TLS/loopback, secrets/trusted proxies/one worker, then SQL-backed UAT/load tests. No deployment, migration or capacity certification. [Details](../memory/notes/project/project_hl25_yarp_gateway_20260920.md); runbook `docs/hl25-gateway/README.md`.

### HL25 cache/index 2026-09-20
Source trên `hotfix/20260920`: cache 4 API 20 phút theo tenant, single-process locking/invalidation sau commit. Migration `20260920100056_AddHl25MiniAppReadIndexes` và SQL đã chuẩn bị, chưa áp DB. Build + 81 .NET/3 JS tests pass, EF model khớp snapshot. Cần migrate/restart theo quy trình triển khai, smoke-test tenant/Admin và đo HTTP/SQL 1.000 CCU thực tế. Nhiều worker/replica cần cơ chế cache/lock/invalidation dùng chung; test 7.000 lời gọi helper không phải benchmark HTTP. Giữ appsettings/logs của user. [Chi tiết](../memory/notes/project/project_hl25_cache_indexes_20260920.md).

### HL25 staging AgeGroup schema repair 2026-09-18
Migration mới 20260918093000_EnsureHl25ParticipantAgeGroup và SQL idempotent đã chuẩn bị/kiểm tra EF; chưa apply staging. DB tenant HL25: DuocPhamHoaLinh. Migration bỏ qua DB không có bảng HL25 khi chạy toàn bộ tenant; migrate sau deploy rồi restart/test. Giữ BirthDate và dữ liệu hiện có. [Chi tiết](../memory/notes/project/project_hl25_staging_agegroup_migration_fix_20260918.md).

### Hoa Linh Sales Excel money format 2026-09-18
Download được user xác nhận hoạt động. Đã sửa 3 cột tiền dùng #,##0, 14 Application tests pass với formatted-output assertions. Rebuild/restart host rồi xuất file mới. [Note](../memory/notes/project/project_hl_sales_excel_money_format_fix_20260918.md).

### Hoa Linh Sales Excel runtime fix 2026-09-18
Shared sales.js downloader đã bỏ API tenant cookie không tồn tại; 12 JS regression tests pass. Serve JS mới rồi Ctrl+F5/smoke-test 3 nút xuất Excel. Không backend/schema/DB change. [Chi tiết](../memory/notes/project/project_hl_sales_excel_tenant_cookie_fix_20260918.md).

### DbMigrator HLG host seed fix 2026-09-18
Nhánh dev: đã sửa tenant-only HLG seeder, 3 Domain tests + DbMigrator build pass. Đã kiểm tra SQL read-only host: thiếu HLG schema dù history có 5 migrations. Chưa sửa DB/chạy lại toàn bộ migrate; bước tiếp theo chạy lại DbMigrator sau rebuild và theo dõi tenant. [Chi tiết](../memory/notes/project/project_hlg_host_seed_migration_failure_20260918.md).

### Cập nhật Hoa Linh Sales 2026-09-17
Nhánh `feature/hoalinh-sales`: đã sửa filters/Excel cho 3 trang Sales, Web build + 14 Application/8 JS tests pass; chưa commit/deploy hoặc UAT runtime. Không migration mới. Chi tiết và bước kiểm tra tiếp theo: [note](../memory/notes/project/project_hl_sales_admin_filters_excel_20260917.md). Điểm dừng HLG bên dưới vẫn giữ để tiếp tục khi có task tương ứng.

### Bàn giao HLG trước đó (2026-08-21)

**Ngày:** 2026-08-21
**Trạng thái:** ⏸️ TẠM DỪNG module Hoa Linh Gamification (HLG). Sẽ quay lại làm tiếp bộ Admin Razor UI.

**Việc vừa hoàn thành:**
- Mini-app backend HLG: HOÀN TẤT 100% (Phase 0-6, ~24 endpoint theo contract, build sạch). Đã gửi CURL cho anh test (bỏ header `__tenant`).
- Sample data seeder (`HlgDataSeedContributor`) + permission provider (`HlgManagement`/`HlgManagementHost`).
- Bắt đầu Admin Razor UI: xong **Rewards admin CrudAppService** (`HlgRewardAdminAppService` + DTOs + interface, build 0 errors).

**Việc còn dở / nơi tiếp tục khi quay lại** (chi tiết đầy đủ ở `../ACTIVE_CONTEXT.md` mục "HLG — ĐIỂM DỪNG"):
- Việc kế tiếp NGAY: viết Razor Pages cho nhóm Rewards (Index + Create/Edit modal + index.js) theo pattern `Web/Pages/SalonBeautyStylists/*`.
- ⚠️ CHẶN: chưa xác minh đường dẫn JS proxy runtime của HLG admin service (dự kiến `genora.multiTenancy.appServices.hlg.admin.hlgRewardAdmin`). Cần mở `{{BASE_URL}}/Abp/ServiceProxyScript` khi chạy app để xác minh — build KHÔNG bắt được lỗi path này.
- Thứ tự làm admin UI: Rewards → Knowledge → Ranking → Games+Questions (nested, phức tạp nhất) → Users. Sau đó: Menu contributor (order 49) + menu localization keys.
- Bổ sung localization keys còn thiếu: `Hlg:RewardNameRequired`, `Hlg:RewardPointCostInvalid`, `Hlg:RewardTypeInvalid`.

**Việc runtime chưa chạy (không phải code):**
- Áp migration `AddHlgKnowledge` + `AddHlgGames` + `AddHlgRewards` + `AddHlgRanking` (review SQL script tại `Migrations/Scripts/` trước — DB config trỏ server dùng chung từ xa).
- Tạo tenant "Hoa Linh Miền Nam Gamification" + bật feature `Hlg.Management`; re-seed host admin để nhận permission `HostAppHlg*`.

**Điểm bảo mật cần xử lý:**
- `~\.claude\settings.json` (user-level) chứa `ANTHROPIC_AUTH_TOKEN` plaintext — nên rotate.
- `DbMigrator/appsettings.json` chứa mật khẩu `sa` + Seq API key plaintext (DB dùng chung từ xa 103.157.218.187).
