# ACTIVE CONTEXT — Việc đang làm dở

> File này mô tả bối cảnh đang hoạt động của phiên làm việc gần nhất.
> Cập nhật ở CUỐI mỗi phiên (xem [handover/HANDOFF.md](handover/HANDOFF.md)).

## Cập nhật gần nhất
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
- **Việc runtime chưa chạy:** áp `AddHlgKnowledge` + `AddHlgGames` + `AddHlgRewards` + `AddHlgRanking`; tạo tenant "Hoa Linh Miền Nam Gamification" + bật feature `Hlg.Management`; re-seed host admin để nhận permission `HostAppHlg*`. Seeder host seed luôn (không cần feature); tenant cần bật feature.
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
