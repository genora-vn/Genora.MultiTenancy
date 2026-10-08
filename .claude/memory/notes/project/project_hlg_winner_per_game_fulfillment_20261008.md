# HLG — Trao giải trúng thưởng theo game + Fulfillment + Menu reorder (2026-10-08)

> Phase 3 (P3) tiếp sau "HLG pharmacy registration 20261006". Chưa commit/deploy. Nhánh hiện tại:
> `feature/dev-hoalinh-gamification` (dựa trên `git log` gần nhất: `6fe42b8` merge nghiadt).

## Phạm vi đã làm (6 lô + 2 vòng fix bug + 1 vòng polish UI/menu)

### Lô 1 — Domain + EF + Migration `GameId` trên `HlgRankingWinner`
- `HlgRankingWinner.GameId` (nullable `Guid?`) — game (chặng) cụ thể được trao giải, null = winner cũ cấp sự kiện (legacy).
- EF config: FK tới `HlgGame` (`DeleteBehavior.NoAction`), unique index đổi từ `(TenantId,EventId,CustomerId)` sang `(TenantId,EventId,GameId,CustomerId)` filtered `IsDeleted=0`.
- Migration `20261007115931_AddHlgWinnerGameId`.

### Lô 2 — `IHlgRankingAppService.GetGameEntriesAsync`
- Thêm bên cạnh `GetEventEntriesAsync`: ranking/điểm tính riêng theo 1 game cụ thể trong sự kiện (dùng `BuildEntriesAsync` refactor nhận thêm `gameFilter`).

### Lô 3 — `HlgWinnerAdminAppService` theo game
- `ValidateAsync`: gate "chỉ trao sau khi GAME kết thúc" (`game.EndAt < Clock.Now`, thay cho gate theo Event); kiểm game thuộc event (qua `ev.GameId` hoặc bảng map `HlgRankingEventGame`); uniqueness đổi thành `(EventId, GameId, CustomerId)` — **1 người được trúng ở NHIỀU game khác nhau trong cùng sự kiện, chỉ chặn trùng trong CÙNG 1 game**.
- Score/Rank lấy qua `GetGameEntriesAsync` (không còn `GetEventEntriesAsync`).

### Lô 4 — Batch create + lookup game đã kết thúc
- `CreateHlgWinnersBatchInput` (EventId, GameId, PrizeId, CustomerIds, IsActive) + `CreateManyAsync` (all-or-nothing trong 1 transaction, trao 1 game + 1 giải cho nhiều người).
- `GetEndedEventGamesAsync(eventId)`: list game đã kết thúc để chọn trao giải. **Đã sửa 2 lần** (xem "Bug fix" dưới).

### Lô 5 — UI modal Winners
- `CreateModal`: chọn Event (ẩn, lấy từ `parentId`) → chọn Game đã kết thúc (dropdown server-side từ `GetEndedEventGamesAsync`) → chọn Prize (lookup theo event) → multi-select Players (select2) → `IsActive` → gọi `CreateManyAsync`.
- `EditModal`: thêm field GameId (dropdown game đã kết thúc của đúng event của winner đó).

### Lô 6 — Import Excel theo game
- Template thêm cột **GAME ĐÃ KẾT THÚC (*)** (vị trí 2, giữa Sự kiện và Giải thưởng) + sheet tham chiếu `DanhMucGame` (tên + ngày bắt đầu/kết thúc) + validation dropdown.
- Sheet `DanhMucGiai` thêm cột **TÊN QUÀ TẶNG** (join `Prize.RewardId → Reward.Name`) để BTC biết rõ quà cụ thể khi chọn giải.
- `ImportExcelAsync` parse thêm `GameId` từ cột mới, validate game tồn tại trước khi gọi `CreateWinnerAsync`.

## Bug fix sau khi user test thực tế (3 vòng)

### Vòng 1 — User báo 2 bug + 2 yêu cầu bổ sung
1. **Dropdown "Trò chơi" trống dù game đã kết thúc.** Root cause thật (sau nhiều vòng điều tra): `GetEndedEventGamesAsync` không có logic "tự phục hồi" khi mapping game bị treo (trỏ tới game đã soft-delete) — đã thêm fallback coi là "không giới hạn game" nếu *toàn bộ* game trong mapping không còn tồn tại. **Nhưng fix này KHÔNG phải nguyên nhân chính** — xem Vòng 3.
2. **Dropdown "GIẢI THƯỞNG (*)" không chọn được trong file mẫu.** Nguyên nhân thật: khi thêm cột GAME ở Lô 6, cột Prize bị dịch từ B→C nhưng dòng data validation (`input.Range("B3:B1000")`) quên cập nhật, trỏ nhầm validation vào cột GAME. Đã sửa trỏ đúng cột C.
3. Bổ sung cột "Tên quà tặng" (đã làm ở Lô 6).
4. Phát hiện thêm (không có trong báo cáo gốc): `HlgWinnerAdminAppService` khi trao giải **không ghi vào `HlgRewardHistory`** → API `reward-history` (mini app) không bao giờ hiện quà trao qua Winners. Đã thêm `SyncRewardHistoryAsync`/`RemoveRewardHistoryAsync` (idempotent theo `WinnerId`), đồng bộ khi tạo/sửa/xóa/đổi `IsActive`. Thêm cột `HlgRewardHistory.WinnerId` (nullable) + migration `20261007152354_AddHlgRewardHistoryWinnerId`.

### Vòng 2 — User test tiếp, báo thêm 3 vấn đề
1. **`reward-history` trả `gameName=null`** cho winner trao qua Winners. Nguyên nhân: `SyncRewardHistoryAsync` không set `SessionId` (vì trao giải không gắn với 1 session cụ thể) — code cũ chỉ tra tên game qua `SessionId`. Đã sửa `GetRewardHistoryAsync` (`HlgProfileAppService`) tra tên game qua **2 đường**: `SessionId` (luồng tự đổi quà cũ) VÀ `WinnerId → HlgRankingWinner.GameId` (luồng Winners mới).
2. **`rewardName` hiện sai** — đang là tên GIẢI (`prize.Title`, VD "Giải tuần 1 Dạ Hương") thay vì tên QUÀ cụ thể (`reward.Name`, VD "Chai Dạ Hương..."). Sửa `SyncRewardHistoryAsync` dùng `reward.Name`.
3. **Dropdown "Trò chơi" VẪN trống** sau khi user set đúng `GameId` trên Event qua SQL trực tiếp. → Dẫn tới Vòng 3 (root cause thật).

### Vòng 3 — ROOT CAUSE THẬT của bug dropdown trống (quan trọng, lesson học được)
- Debug sâu qua nhiều bước (so sánh song song với `GetEndedCampaignGamesAsync` của Ranking export vẫn hoạt động đúng; verify SQL dữ liệu Game/EventGame mapping/Event đều đúng; loại trừ build/restart, tenant mismatch, timezone, model binding).
- **Root cause thật: KHÔNG phải lỗi code.** URL mở modal Create là `.../CreateModal?parentId=&brandId=` — **`parentId` RỖNG**. Lý do: user vào trang Winners **trực tiếp từ sidebar menu** (`/Hlg/Winners` không mang `?parentId=`), không qua action "Trao giải trúng thưởng" trên dòng sự kiện cụ thể ở trang Ranking (action đó mới tự mang đúng `parentId=eventId`).
- **Fix: ẩn 2 menu sidebar "Hlg.Prizes" và "Hlg.Winners"** khỏi `MultiTenancyMenuContributor.cs` — nhất quán với pattern đã có của trang Questions (không có menu riêng, chỉ truy cập qua action của Game cha). Lý do nghiệp vụ: Prize/Winner luôn thuộc 1 Event cụ thể, không có khái niệm "xem tất cả giải/tất cả winner của mọi sự kiện".
- **Bài học (nên note vào feedback/RULES):** khi 1 trang Admin phụ thuộc `parentId` (quan hệ cha-con bắt buộc), KHÔNG để menu sidebar riêng trỏ thẳng tới trang đó — chỉ cho truy cập qua row-action của entity cha. Nếu nghi ngờ bug "dữ liệu rỗng dù backend đúng", kiểm tra URL thực tế trình duyệt đang gọi (query string) TRƯỚC khi debug sâu backend.

## Vòng 4 — Yêu cầu bổ sung (không phải bug)
1. **API danh sách người trúng thưởng cho mini-app:** đã có sẵn, không cần code mới — `GET /api/mini-app/hlg/ranking/events/{id}/winners` (`HlgContentAppService.GetWinnersAsync`, filter `IsActive` = đã công bố). CURL đã gửi cho user.
2. **Trang Fulfillment (`Hlg/Fulfillment`):**
   - Thêm localization `Hlg:Address` (thiếu hoàn toàn trong vi.json/en.json).
   - `HlgFulfillmentAdminAppService.QueryAsync`: cột Người nhận/SĐT/Địa chỉ fallback về `Customer.FullName/PhoneNumber/Address` (`dbo.AppCustomers`) khi không có `HlgShippingAddress` riêng (đúng case winner trao qua Trao giải trúng thưởng, không qua luồng tự đổi quà có nhập địa chỉ riêng).
   - `EditModal.cshtml`: sửa dòng hiển thị ReceiverName/Phone/Address — lọc bỏ giá trị rỗng trước khi join bằng " · " (trước đây hiện dấu `· ·` thừa khi field null, nằm sát label "Trạng thái" nên user tưởng nhầm là lỗi ở label đó).
3. **Menu "Giao quà" (Fulfillment):**
   - Thêm mục menu riêng `Hlg.Fulfillment` (dùng chung quyền "Rewards", giống `PermissionGroup` của trang).
   - Bỏ button "Giao quà" (link `/Hlg/Fulfillment`) khỏi trang Rewards.
   - Bỏ link "Quà tặng" (link `/Hlg/Rewards`) khỏi trang Fulfillment (tránh trùng, vì giờ cả 2 đã có menu riêng).
4. **Sắp xếp lại thứ tự menu Hoa Linh Gamification** (`MultiTenancyMenuContributor.cs`): Ngành hàng → Nhãn hàng → Bài học/Sản phẩm → Sự kiện xếp hạng → Trò chơi → Quà tặng → Giao quà → Người chơi → (Nội dung trang chủ, giữ cuối, không có trong yêu cầu gốc).

## Verification
- Build Application + EntityFrameworkCore + Web: **0 lỗi CS** sau mỗi lô/fix (dùng `dotnet build <proj> --no-restore -clp:ErrorsOnly`, lọc MSB3027/MSB3021 copy-lock do host đang chạy — môi trường, không phải lỗi code).
- Test: **100/100 test HLG xanh** xuyên suốt toàn bộ 6 lô + 3 vòng fix (`dotnet test ... --filter "FullyQualifiedName~Hlg"`).
- User đã test thủ công trên host local (deploy qua `dotnet build` + restart process, KHÔNG qua IIS/staging) và xác nhận OK cho: trao giải thủ công, trao giải qua import, dropdown game, reward-history API, menu/Fulfillment UI.
- **CHƯA deploy/test trên staging** — user xác nhận ở cuối phase sẽ tự deploy lên staging và test lại toàn bộ (đây là lý do yêu cầu lưu memory).

## Migration CHƯA apply (3 migration mới của phase này, cộng với 1 từ phase trước)
1. `20261006054856_AddHlgPharmacyRegistration` (phase trước, pharmacy registration).
2. `20261007060417_AddHlgRankingEventGame` (map Event↔Game nhiều chặng — đã có từ trước phase này, phase trước nữa).
3. `20261007115931_AddHlgWinnerGameId` — cột `GameId` trên `HlgRankingWinner` + unique index đổi.
4. `20261007152354_AddHlgRewardHistoryWinnerId` — cột `WinnerId` trên `HlgRewardHistory`.
- **QUAN TRỌNG khi deploy staging:** phải chạy `dotnet ef database update` (hoặc để `TenantAutoMigrateMiddleware` tự áp khi host restart) cho ĐÚNG thứ tự 4 migration trên, trên ĐÚNG DB tenant/host đang dùng cho HLG (xem cảnh báo connection string ở `ACTIVE_CONTEXT.md` — `DbMigrator/appsettings.json` có thể trỏ SQL Server chung).

## File chính đã sửa trong phase này
- `src/Genora.MultiTenancy.Domain/DomainModels/AppHlg/HlgRankingWinner.cs` (+GameId), `HlgRewardHistory.cs` (+WinnerId).
- `src/Genora.MultiTenancy.Application/AppServices/Hlg/Admin/HlgWinnerAdminAppService.cs` (toàn bộ logic Lô 3-6 + 2 vòng fix bug + reward-history sync).
- `src/Genora.MultiTenancy.Application/AppServices/Hlg/Admin/HlgRankingAdminAppService.cs` (defensive fix `GetCampaignGameIdsAsync` cho mapping treo, dùng chung pattern với Winners).
- `src/Genora.MultiTenancy.Application/AppServices/Hlg/HlgRankingAppService.cs` (`GetGameEntriesAsync`).
- `src/Genora.MultiTenancy.Application/AppServices/Hlg/HlgProfileAppService.cs` (`GetRewardHistoryAsync` — fix gameName qua WinnerId).
- `src/Genora.MultiTenancy.Application/AppServices/Hlg/Admin/HlgFulfillmentAdminAppService.cs` (fallback Customer cho Receiver/Phone/Address).
- `src/Genora.MultiTenancy.Application/AppServices/Hlg/Admin/HlgWinnerExcelTemplateGenerator.cs`/`HlgWinnerExcelImporter.cs` (cột Game + Tên quà tặng + fix validation lệch cột).
- `src/Genora.MultiTenancy.Web/Pages/Hlg/Winners/{CreateModal,EditModal}.cshtml(.cs)` (UI chọn game + multi-select players).
- `src/Genora.MultiTenancy.Web/Pages/Hlg/Fulfillment/{Index,EditModal}.cshtml` (localization + bỏ dấu thừa + bỏ link trùng).
- `src/Genora.MultiTenancy.Web/Pages/Hlg/Rewards/Index.cshtml` (bỏ button Giao quà).
- `src/Genora.MultiTenancy.Web/Menus/MultiTenancyMenuContributor.cs` (ẩn Prizes/Winners sidebar + thêm Fulfillment + reorder toàn bộ).
- `src/Genora.MultiTenancy.Domain.Shared/Localization/MultiTenancy/{vi,en}.json` (nhiều key mới: GameId liên quan Winners, Address, GameNotEnded/GameNotInEvent, RewardOutOfStock, WinnerImportGameInvalid, v.v.)
- `src/Genora.MultiTenancy.EntityFrameworkCore/Migrations/{20261007115931_AddHlgWinnerGameId,20261007152354_AddHlgRewardHistoryWinnerId}.*`

## Việc còn lại / chưa làm
- Deploy + migrate + UAT trên **staging** (user sẽ tự làm, cần theo dõi kết quả ở phiên sau).
- Chưa commit các thay đổi này (vẫn là working tree changes, xem `git status` — nhiều file M/A chưa add/commit).
- Chưa viết feedback note riêng cho bài học "menu sidebar không nên trỏ thẳng trang phụ thuộc parentId" — nên thêm vào `memory/notes/feedback/` + `RULES.md` ở phiên sau nếu có thời gian.
