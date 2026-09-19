# HANDOFF — Bàn giao giữa các phiên làm việc

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
