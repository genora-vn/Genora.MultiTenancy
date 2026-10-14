# HLG — Ranking Event & Entries theo từng Game (tuần) — 2026-10-13

- **Git status updated 2026-10-14:** commit `fe096ef` is merged into `dev` at `779367f`; no migration; deployment/UAT not verified.
- **Task file:** `.claude/tasks/20261013_hlg_ranking_and_winners_by_game.md`.

## Vấn đề
Bảng xếp hạng (entries) & sự kiện (event) đang gộp toàn chiến dịch, không tách theo từng Game (tuần):
- `/ranking/entries` không nhận `gameId` → xếp hạng toàn cục (hoặc theo `ev.GameId` cố định).
- `/ranking/event` trả `ev.GameId` cố định (tuần đầu), không order → luôn ra Tuần 1 dù Tuần 2 đã/đang diễn ra.

## "Game hiện tại" (contract mục 3)
Ongoing ưu tiên (nhiều → `StartAt` mới nhất) → nếu không có, ended mới nhất theo `EndAt DESC` → nếu chỉ còn upcoming / không có game → `event=null`, `entries=[]`.
- `ComputeGameStatus(game, now)`: `ended` nếu `Status==Ended` HOẶC `EndAt<now`; `upcoming` nếu `StartAt>now`; còn lại `ongoing` (nhất quán với winners task 20261012).
- Nguồn game của event: bảng map `HlgRankingEventGame` (`AppHlgRankingEventGames`), fallback `ev.GameId` (legacy 1 game).

## Thay đổi code (4 file + test)
1. `Application.Contracts/AppDtos/Hlg/RankingDtos.cs` — `RankingEventDto` thêm `GameName`, `GameStatus`, `EventStartAt`, `EventEndAt`. **Đổi ngữ nghĩa:** `GameId`=game hiện tại; `StartAt`/`EndAt`=thời gian GAME hiện tại (fallback event nếu game không cấu hình). Giữ `Id`/`Title`/`Description` của event; thời gian event gốc lưu ở `EventStartAt`/`EventEndAt`.
2. `Application.Contracts/AppDtos/Hlg/IHlgRankingAppService.cs` — `GetEntriesAsync(string? phone=null, int top=50, Guid? gameId=null, CancellationToken ct=default)`.
3. `Application/AppServices/Hlg/HlgRankingAppService.cs`:
   - Inject thêm `IRepository<HlgRankingEventGame,Guid>` + `IRepository<HlgGame,Guid>`.
   - Thêm `GetEventGameIdsAsync(ev)`, `ComputeGameStatus(game,now)`, `ResolveCurrentGameAsync(ev)`.
   - `GetCurrentEventAsync`: event active → current game; không có current game → `null`. `MapEvent(event, game)` (non-static) map game + event cũ.
   - `GetEntriesAsync`: `effectiveGameId = gameId ?? ResolveCurrentGameAsync(ev)?.Id`; null → `[]`; `BuildEntriesAsync(ev,...,gameFilter)` giữ công thức cũ (chỉ phiên ĐẠT, cửa sổ `[ev.StartAt,ev.EndAt]`), nhất quán rank/score với winners.
4. `HttpApi/Controllers/HoaLinhGamificationController.cs` — `GetRankingEntries` thêm `[FromQuery] Guid? gameId`, truyền xuống service.
5. Test `test/.../Hlg/HlgDesignContentTests.cs` — cập nhật constructor cũ (2 repo mới) + 2 test mới; `HlgRankingShareImageTests.cs` cập nhật constructor.

## Không đụng
`/ranking/events/{gameId}/winners` (task 20261012), `/ranking/events` list, `/ranking/events/{id}/entries` (Controller2 theo eventId), `HlgContentAppService`, Gateway (path `/ranking/event` & `/ranking/entries` không đổi; thêm query param không ảnh hưởng route).

## Verify
- Build test project + HttpApi: **0 errors**.
- `dotnet test --filter Hlg`: **138/138 PASS** (+4 case mới): `Current_Event_Picks_Ongoing_Then_Latest_Ended_Else_Null` (ongoing/endedOnly/upcomingOnly) + `Entries_Default_To_Current_Game_And_Respect_Explicit_GameId`.
- Compile impact: `HlgRankingAppService` constructor đổi (thêm 2 repo) → đã cập nhật 2 test khởi tạo trực tiếp; DI runtime tự xử lý. `GetEntriesAsync` chỉ caller là controller (đã sửa). DTO thêm field — additive, không vỡ caller.

## CURL test (FE Zalo mini-app)
```
# Event hiện tại (ưu tiên ongoing): trả gameId/gameName/gameStatus/startAt/endAt(GAME) + eventStartAt/eventEndAt
curl --location 'https://.../api/mini-app/hlg/ranking/event'

# Entries mặc định theo game hiện tại
curl --location 'https://.../api/mini-app/hlg/ranking/entries?phone=0976687984&top=50'

# Entries theo game FE chọn (Tuần 1)
curl --location 'https://.../api/mini-app/hlg/ranking/entries?phone=0976687984&top=50&gameId=72ae3886-b2c3-1ae4-6627-3a23c04f7688'

# Winners theo game đã kết thúc (giữ nguyên — task 20261012)
curl --location 'https://.../api/mini-app/hlg/ranking/events/72AE3886-B2C3-1AE4-6627-3A23C04F7688/winners'
```

## Còn lại
User commit + test local → deploy staging → FE lấy `gameId` từ `/ranking/event` để gọi `/ranking/entries` + `/winners`, hiển thị `gameName`/`gameStatus`.
