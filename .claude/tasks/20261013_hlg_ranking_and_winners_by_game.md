[HLG] Chỉnh API Bảng xếp hạng & Danh sách trúng thưởng theo từng Trò chơi (Game/Tuần)

BỐI CẢNH

Mỗi tuần thiết lập 1 Game (VD: "Thử tài tốc độ: Dạ Hương Tuần 1", "Tuần 2"...).

Bảng xếp hạng và Danh sách trúng thưởng phải TÁCH THEO TỪNG GAME, không gộp chung.

Hiện tại FE đang hiển thị sai vì:
- /ranking/entries không nhận gameId nên xếp hạng là toàn cục.
- /ranking/event trả gameId của tuần ĐẦU đã kết thúc (không order), nên luôn ra Tuần 1 dù Tuần 2 cũng đã kết thúc.

YÊU CẦU

1) GET /api/mini-app/hlg/ranking/entries

- Thêm query param: gameId (tùy chọn).
- Nếu truyền gameId: trả bảng xếp hạng CHỈ của game đó.
- Nếu KHÔNG truyền gameId: dùng "game hiện tại" (xem mục 3) làm mặc định.
- Giữ nguyên các param cũ (phone, top) và cấu trúc phản hồi trả về.

Ví dụ:
GET /api/mini-app/hlg/ranking/entries?phone=0976687984&top=50&gameId=72ae3886-b2c3-1ae4-6627-3a23c04f7688

2) GET /api/mini-app/hlg/ranking/event

- Sửa logic chọn game: KHÔNG được cố định tuần đầu.
- Ưu tiên game đang diễn ra (ongoing).
- Nếu không có game ongoing, lấy game ĐÃ KẾT THÚC gần nhất (ORDER BY endAt DESC).

Bổ sung dữ liệu game vào response:

gameId      : Id game hiện tại
gameName    : tên game (VD: "Thử tài tốc độ: Dạ Hương Tuần 2")
gameStatus  : ongoing | ended | upcoming
startAt     : thời gian bắt đầu của game
endAt       : thời gian kết thúc của game

(Giữ nguyên các field hiện có như id, title, description...)

Mục tiêu:
FE lấy đúng gameId hiện tại để gọi entries và winners, đồng thời hiển thị đúng tên tuần/game.

3) ĐỊNH NGHĨA "GAME HIỆN TẠI"

- Nếu có game ongoing:
  → Trả game ongoing.
  → Nếu có nhiều game ongoing thì lấy game có startAt mới nhất.

- Nếu không có game ongoing:
  → Trả game ended mới nhất theo endAt DESC.

- Nếu không có game nào:
  → Trả data null (event)
  → Trả [] (entries)

4) GET /api/mini-app/hlg/ranking/events/{gameId}/winners

- GIỮ NGUYÊN.
- Chỉ trả danh sách winner của đúng gameId.
- Chỉ trả dữ liệu khi game đã kết thúc.
- Nếu game chưa kết thúc thì trả:

Vui lòng xác nhận:

- Thêm các field trả thêm /ranking/event (gameId, gameName, gameStatus, startAt, endAt).
- /ranking/entries nhận tham số gameId (optional) và mặc định theo "game hiện tại".

---

STATUS: DONE (2026-10-13) — code + test xong, chờ user commit/test local/deploy staging.

ĐÃ XÁC NHẬN & TRIỂN KHAI:
1. /ranking/event: gameId=game hiện tại, startAt/endAt=thời gian GAME; thêm gameName/gameStatus; giữ event gốc ở eventStartAt/eventEndAt.
2. /ranking/entries: nhận gameId optional, mặc định theo "game hiện tại".
3. Chỉ còn upcoming/không có game → event=null, entries=[].
4. Nhánh: tiếp tục hotfix/20261012.

FILE SỬA: RankingDtos.cs, IHlgRankingAppService.cs, HlgRankingAppService.cs, HoaLinhGamificationController.cs, HlgDesignContentTests.cs, HlgRankingShareImageTests.cs.
VERIFY: build HttpApi + test project 0 errors; dotnet test --filter Hlg → 138/138 PASS.
KHÔNG migration/deploy/commit. Chi tiết: memory/notes/project/project_hlg_ranking_by_game_20261013.md.