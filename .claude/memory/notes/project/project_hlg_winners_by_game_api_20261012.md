# HLG — Winners API đổi từ EventId sang GameId (chỉ game đã kết thúc) — 2026-10-12

- **Git status updated 2026-10-14:** commit `fe096ef` is merged into `dev` at `779367f`; no migration; deployment/UAT not verified.
- **Task file:** `.claude/tasks/20261012_hlg_winners_by_game.md`.

## Vấn đề
API `GET /api/mini-app/hlg/ranking/events/{id}/winners` trước đây lấy danh sách trúng thưởng theo **chiến dịch** (`HlgRankingEvent.Id`). Nhưng việc trao giải chạy theo **từng trò chơi** (1 chiến dịch có nhiều game, mỗi game có mốc kết thúc riêng). Cần lấy winners theo **game đã kết thúc**, game chưa kết thúc → rỗng.

## Làm rõ mapping entity ↔ bảng (nhãn trong task bị ĐẢO tên so với code)
Đối chiếu theo cột (nguồn chân lý là code):

| Khái niệm | Bảng THỰC (code) | Entity | ID ví dụ |
|---|---|---|---|
| Chiến dịch | `AppHlgRankingEvents` | `HlgRankingEvent` | `F6356FD2…` |
| **Cấu hình trò chơi** (Name/Status/StartAt/EndAt) | **`AppHlgGames`** | **`HlgGame`** | `72AE3886…` (Tuần 1), `C439A8F3…` (Tuần 2) |
| Map chiến dịch↔game (EventId/GameId/DisplayOrder) | `AppHlgRankingEventGames` | `HlgRankingEventGame` | `27AAA848…` |
| Người trúng | `AppHlgRankingWinners` | `HlgRankingWinner` (`GameId` FK → `HlgGame`) | — |

→ ID user truyền (`72AE3886…`) chính là `HlgGame.Id`; `HlgRankingWinner.GameId` cũng trỏ về `HlgGame`. Nên đổi param sang gameId + filter `w.GameId == gameId` là đúng mô hình.

## Thay đổi code
- `src/.../Application.Contracts/AppDtos/Hlg/ContentDtos.cs`: đổi `IHlgContentAppService.GetWinnersAsync(Guid eventId)` → `GetWinnersAsync(Guid gameId)` + XML doc.
- `src/.../Application/AppServices/Hlg/HlgContentAppService.cs`: `GetWinnersAsync(Guid gameId)` nay:
  1. Tìm `HlgGame` theo `gameId` + đúng tenant (`CurrentTenant.Id`); không có → trả rỗng.
  2. Gate "đã kết thúc": `game.Status == HlgGameStatus.Ended` **HOẶC** `game.EndAt < Clock.Now`; chưa kết thúc → rỗng.
  3. Lọc `HlgRankingWinner` theo `w.GameId == gameId && w.IsActive`, join `Customer`, order theo `Rank`, normalize avatar URL.
- **Route/Controller giữ nguyên:** `HoaLinhGamificationContentController.Winners(Guid id)` → `GetWinnersAsync(id)`. Path `ranking/events/{id}/winners` KHÔNG đổi (user mong muốn CURL giữ path cũ, chỉ đổi giá trị `{id}` sang game id). Gateway route cũng giữ nguyên.

## Định nghĩa "đã kết thúc"
Giữ `Status == Ended OR EndAt < Now` (plan mục 6, user approve không yêu cầu đổi). Lưu ý nhỏ: Admin publish winner (`HlgWinnerAdminAppService.ValidateAsync`) chỉ cho công bố khi `EndAt < Now`; nên khi game chưa qua EndAt sẽ không có winner active → list rỗng dù gate có nhánh Status. Enum `HlgGameStatus`: Upcoming=1, Ongoing=2, Ended=3.

## Enum/data mẫu (ngày hiện tại 12/10/2026)
- Tuần 1 `72AE3886` (Status=3 Ended) → có data.
- Tuần 2 `C439A8F3` (Status=2 Ongoing, EndAt 17/10 tương lai) → hiện trả **rỗng**; có data khi đã kết thúc.
- Tuần 3 `433F248A` / Tuần 4 `ADBA3E71` (Upcoming) → rỗng.
- Nếu FE còn truyền **event id** `F6356FD2` (như log hiện tại) → không khớp `HlgGame` → trả rỗng (không lỗi).

## Verify
- `dotnet build` test project: **0 errors** (357 warnings pre-existing).
- `dotnet test --filter Hlg`: **134/134 PASS** (gồm 2 test mới trong `HlgDesignContentTests.cs`: `Winners_Are_Returned_Only_For_Ended_Game_Of_Current_Tenant` 5 case + `Winners_Are_Scoped_To_The_Requested_Game_Not_The_Whole_Event`).
- Compile impact: caller duy nhất là controller; chỉ rename param (cùng kiểu Guid); không JS proxy; không vỡ compile.

## CURL test (FE Zalo mini-app)
```
# Tuần 1 — ĐÃ kết thúc → trả danh sách winners
curl --location 'https://spirited-whooping-uncouth.ngrok-free.dev/api/mini-app/hlg/ranking/events/72AE3886-B2C3-1AE4-6627-3A23C04F7688/winners'

# Tuần 2 — khi đã kết thúc → trả winners; chưa kết thúc → data rỗng
curl --location 'https://spirited-whooping-uncouth.ngrok-free.dev/api/mini-app/hlg/ranking/events/C439A8F3-81AD-E549-6832-3A24279F9C04/winners'

# Tuần 3/4 — CHƯA kết thúc → data rỗng
curl --location 'https://spirited-whooping-uncouth.ngrok-free.dev/api/mini-app/hlg/ranking/events/433F248A-761B-D1C4-1712-3A2427A043F5/winners'
curl --location 'https://spirited-whooping-uncouth.ngrok-free.dev/api/mini-app/hlg/ranking/events/ADBA3E71-7B77-246B-E300-3A2427A0EE46/winners'
```
Response envelope `HlgApiResult<List<RankingWinnerDto>>`: `{ "error":0, "message":..., "data":[ { prizeId, rank, userId, displayName, avatarUrl, score, isCurrentUser } ] }`. Game chưa kết thúc → `data: []`.

## Còn lại
- User commit + test local → deploy staging → FE ghép truyền **game id** (thay vì event id).
