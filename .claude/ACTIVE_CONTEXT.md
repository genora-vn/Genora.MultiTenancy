# ACTIVE CONTEXT — Việc đang làm dở

> File này mô tả bối cảnh đang hoạt động của phiên làm việc gần nhất.
> Cập nhật ở CUỐI mỗi phiên (xem [handover/HANDOFF.md](handover/HANDOFF.md)).

## Cập nhật gần nhất

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

### HLG — Hoa Linh Gamification (đã merge từ feature/hoalinh-gamification)
- **Ngày:** 2026-08-21
- **TRẠNG THÁI: ⏸️ TẠM DỪNG** module Hoa Linh Gamification (HLG). Sẽ quay lại làm tiếp bộ Admin Razor UI.
- **Việc vừa làm:** Mini-app backend HLG hoàn tất Phase 0-6 (build sạch). Bắt đầu Phase 7 (Admin Razor UI): đã xong sample data seeder + permission provider + **Rewards admin CrudAppService** (`HlgRewardAdminAppService`, build 0 errors). Đang dở phần Razor Pages cho Rewards.
- **Trước đó (2026-08-18):** Chuẩn hóa toàn bộ project memory vào `.claude/` (migrate 108 note từ user-level).

## HLG — ĐIỂM DỪNG (nơi tiếp tục khi quay lại)
- **Việc kế tiếp NGAY:** Viết Razor Pages cho nhóm Rewards (Index.cshtml + Index.cshtml.cs + CreateModal + EditModal + index.js) theo pattern `Web/Pages/SalonBeautyStylists/*`. Backend Rewards admin đã sẵn sàng.
- **⚠️ CHẶN kỹ thuật cần giải quyết trước khi viết JS:** Đường dẫn JS proxy runtime của HLG admin service CHƯA xác minh. ABP sinh proxy động (không nằm trong wwwroot). Path dự kiến theo convention: `genora.multiTenancy.appServices.hlg.admin.hlgRewardAdmin` (suy từ namespace `AppDtos.Hlg.Admin`). PHẢI xác minh bằng cách mở `{{BASE_URL}}/Abp/ServiceProxyScript` khi chạy app, hoặc dùng `resolveService()` có throw lỗi rõ như pattern salon (`index.js:5-11`). Build KHÔNG bắt được lỗi path này.
- **Thứ tự làm admin UI (đã chốt):** Rewards (đơn giản nhất, làm mẫu) → Knowledge (Category+Product) → Ranking → Games+Questions (nested, phức tạp nhất, CorrectKey ẩn) → Users (read-only). Mỗi nhóm ~4 file, verify build từng nhóm.
- **Còn lại sau admin UI:** Menu contributor (`MultiTenancyMenuContributor.cs`, order 49, gate feature `Hlg.Management` + permission) + menu localization keys — làm SAU khi có pages (menu item phải trỏ page tồn tại).
- **Ghi chú:** HLG AppService map thủ công, KHÔNG dùng AutoMapper. Admin service dùng `FeatureProtectedCrudAppService` (bản 6-generic cho Create/Update DTO tách riêng) để sinh JS proxy.

## HLG — đã hoàn tất
- **Đã xong:** Phase 0 (hạ tầng), Phase 1 (Auth + Profile), Phase 2 (Knowledge base), Phase 3 (Games engine), Phase 4 (Rewards & Shipping), Phase 5 (Ranking), Phase 6 (Live-feed SignalR), Permission provider (`HlgManagement` + `HlgManagementHost`), sample data seeder (`HlgDataSeedContributor`). Xem `architecture/module-hlg.md` (6 quyết định nghiệp vụ + 3 quyết định kiến trúc AD-1/2/3).
- **Backend mini-app: HOÀN TẤT 100%** — ~24 endpoint theo contract, đã gửi CURL cho anh test (bỏ header `__tenant`).
- **Migration đã sinh:** `AddHlgModule` (schema HLG + HlgUserProfile), `AddHlgKnowledge` (3 bảng knowledge), `AddHlgGames` (5 bảng game), `AddHlgRewards` (3 bảng reward), `AddHlgRanking` (1 bảng ranking event). SQL script idempotent tại `Migrations/Scripts/`.
- **Files admin đã tạo (Rewards):** `Application.Contracts/AppDtos/Hlg/Admin/{HlgRewardAdminDtos,IHlgRewardAdminAppService}.cs` + `Application/AppServices/Hlg/Admin/HlgRewardAdminAppService.cs`. Localization keys cần bổ sung: `Hlg:RewardNameRequired`, `Hlg:RewardPointCostInvalid`, `Hlg:RewardTypeInvalid` (hiện fallback về key).
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

## Không có task treo được ghi nhận rõ ràng
Các note dạng `*_complete` cho thấy các phase lớn đã đóng. Không phát hiện file "in-progress"
nào ngoài `salon_beauty_implementation_progress.md` (bản cũ, đã được thay bằng `*_complete`).

## Cách xác định task đang làm dở (quy trình)
1. Sắp xếp note trong `memory/notes/project/` theo ngày (hậu tố `_juneXX`, `_roundXX`, `_YYYY_MM_DD`).
2. Note mới nhất KHÔNG có hậu tố `complete` → khả năng là việc dở.
3. Đối chiếu với `git log` và trạng thái build hiện tại.
4. Kiểm tra TODO/FIXME trong code các module tương ứng.

> Khi bắt đầu việc mới: cập nhật mục "Cập nhật gần nhất" và "Việc vừa làm" ở trên.
