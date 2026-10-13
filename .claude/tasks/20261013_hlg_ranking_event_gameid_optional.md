[HLG] Bổ sung query gameId (optional) cho API /ranking/event

BỐI CẢNH
Màn Bảng xếp hạng hỗ trợ xem BXH theo từng game/tuần. Khi người dùng mở
"Bảng xếp hạng" từ trang chi tiết một Game cụ thể (vd "Dạ Hương Tuần 2" đã
kết thúc), FE điều hướng kèm gameId. Nhưng API /ranking/event hiện KHÔNG nhận
gameId nên luôn trả game hiện hành (vd "Tuần 3" đang diễn ra) kèm thời gian,
khiến phần header (tên game / thời gian / đếm ngược / trạng thái) hiển thị SAI
game so với BXH + danh sách trúng thưởng bên dưới.

YÊU CẦU

GET /api/mini-app/hlg/ranking/event
- Thêm query param: gameId (TÙY CHỌN).
- Nếu TRUYỀN gameId: trả đúng thông tin game đó:
    gameId, gameName, gameStatus (ongoing|ended|upcoming),
    startAt, endAt, title, description (và id event nếu có).
- Nếu KHÔNG truyền gameId: giữ hành vi hiện tại — trả game HIỆN HÀNH
  (ongoing); nếu không có ongoing thì trả game GẦN NHẤT (ORDER BY endAt DESC).
- Ví dụ:
    /api/mini-app/hlg/ranking/event
    /api/mini-app/hlg/ranking/event?gameId=c439a8f3-81ad-e549-6832-3a24279f9c04

LƯU Ý ĐỒNG BỘ
- gameId truyền vào /ranking/event phải khớp ngữ nghĩa với gameId của
  /ranking/entries?gameId=... và /ranking/events/{gameId}/winners để toàn bộ
  màn (header + BXH + người trúng thưởng) cùng nói về MỘT game.

Vui lòng xác nhận: tên các field trả về (gameId, gameName, gameStatus,
startAt, endAt) và việc gameId là optional.

---

STATUS: DONE (2026-10-13) — code + test xong, chờ user commit/test local/deploy staging.

ĐÃ XÁC NHẬN & TRIỂN KHAI:
1. gameId optional. Truyền → trả đúng game đó (gameId/gameName/gameStatus/startAt/endAt + title/description/id event + eventStartAt/eventEndAt). Không truyền → game hiện hành (giữ nguyên).
2. gameId không tồn tại/sai tenant → data: null.
3. gameId game mồ côi (không thuộc event) → trả field game, field event để trống (Id=Guid.Empty, Title="").
4. Không ràng buộc game phải thuộc event đang active (xem được cả game đã kết thúc).
5. Field trả về đã tồn tại sẵn trong RankingEventDto (thêm ở task ranking-by-game) → DTO không đổi shape, backward compatible.

FILE SỬA: IHlgRankingAppService.cs, HlgRankingAppService.cs (nhánh gameId + ResolveEventForGameAsync + MapEvent nullable-safe), HoaLinhGamificationController.cs (+[FromQuery] Guid? gameId), HlgDesignContentTests.cs (+3 test).
VERIFY: build HttpApi + test project 0 errors; dotnet test --filter Hlg → 141/141 PASS.
KHÔNG migration/deploy/commit. Chi tiết: memory/notes/project/project_hlg_ranking_event_gameid_optional_20261013.md.