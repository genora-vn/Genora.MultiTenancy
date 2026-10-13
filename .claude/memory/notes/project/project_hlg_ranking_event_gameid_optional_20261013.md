# HLG — /ranking/event thêm query gameId (optional) — 2026-10-13

- **Branch:** `hotfix/20261012` (tiếp tục). **Loại:** Bổ sung nhỏ (follow-up của task ranking-by-game cùng ngày). **Không migration/deploy/commit.**
- **Task file:** `.claude/tasks/20261013_hlg_ranking_event_gameid_optional.md`.

## Vấn đề
FE mở "Bảng xếp hạng" từ chi tiết 1 game cụ thể (vd Tuần 2 đã kết thúc) kèm `gameId`, nhưng `GET /ranking/event` không nhận `gameId` → luôn trả game HIỆN HÀNH (vd Tuần 3 ongoing) → header (tên game/thời gian/trạng thái) lệch với BXH + winners bên dưới.

## Thay đổi code (3 file + test)
1. `Application.Contracts/AppDtos/Hlg/IHlgRankingAppService.cs` — `GetCurrentEventAsync(Guid? gameId = null, CancellationToken ct = default)`.
2. `Application/AppServices/Hlg/HlgRankingAppService.cs`:
   - `GetCurrentEventAsync`: nếu có `gameId` → load `HlgGame` theo `(gameId, tenant)`; không có → `null`; có → tìm event sở hữu qua `ResolveEventForGameAsync` → `MapEvent(owningEvent, game)`. Nếu `gameId==null` → giữ nguyên hành vi cũ (event active → game hiện hành ongoing/ended-latest → null).
   - Thêm `ResolveEventForGameAsync(gameId)`: ưu tiên map `HlgRankingEventGame.GameId==gameId → EventId`, fallback `HlgRankingEvent.GameId==gameId` (legacy); null nếu game mồ côi.
   - `MapEvent(HlgRankingEvent? e, HlgGame game)` chuyển sang **nullable-safe**: event null → `Id=Guid.Empty`, `Title=""`, `StartAt/EndAt` fallback default, `EventStartAt/EventEndAt=default`.
3. `HttpApi/Controllers/HoaLinhGamificationController.cs` — `GetRankingEvent([FromQuery] Guid? gameId, CancellationToken ct)` → `GetCurrentEventAsync(gameId, ct)`.
4. Test `HlgDesignContentTests.cs` — thêm 3 test (ended-in-event / unknown→null / orphan→event trống). Caller cũ `GetCurrentEventAsync()` giữ nguyên nhờ default param.

## Quyết định (user xác nhận)
1. `gameId` không tồn tại/sai tenant → `data: null`.
2. `gameId` game mồ côi (không thuộc event) → trả field game, field event để trống.
3. Không ràng buộc game phải thuộc event đang active khi truyền `gameId` (xem được cả game đã kết thúc).

## Backward compatibility
- `gameId` optional; không truyền → output **y hệt** hành vi cũ. `RankingEventDto` **không đổi shape** (các field gameId/gameName/gameStatus/startAt/endAt/eventStartAt/eventEndAt đã thêm ở task ranking-by-game trước đó). FE cũ không ảnh hưởng. Gateway route `/ranking/event` không đổi (query param không ảnh hưởng routing).

## Đồng bộ ngữ nghĩa
`gameId` của `/ranking/event`, `/ranking/entries?gameId=`, `/ranking/events/{gameId}/winners` cùng là `HlgGame.Id` → header + BXH + winners cùng nói về 1 game.

## Verify
- Build test project + HttpApi: **0 errors**.
- `dotnet test --filter Hlg`: **141/141 PASS** (+3 case mới). (Lưu ý trong lúc dev: lần đầu test orphan fail NRE do `MapEvent` chưa nullable; đã sửa `MapEvent(HlgRankingEvent? e, ...)` → pass.)
- Compile impact: `GetCurrentEventAsync` đổi chữ ký (thêm param default) → caller duy nhất là controller (đã sửa) + test (dùng default, không cần sửa). DTO không đổi.

## CURL test
```
# Không truyền → game hiện hành (giữ nguyên)
curl --location 'https://.../api/mini-app/hlg/ranking/event'
# Truyền gameId game đã kết thúc → header đúng game đó
curl --location 'https://.../api/mini-app/hlg/ranking/event?gameId=c439a8f3-81ad-e549-6832-3a24279f9c04'
```

## Còn lại
User commit + test local → deploy staging → FE truyền `gameId` khi mở BXH từ chi tiết game để header khớp BXH/winners.
