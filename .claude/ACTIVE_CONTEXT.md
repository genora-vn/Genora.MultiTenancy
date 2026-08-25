# ACTIVE CONTEXT — Việc đang làm dở

> File này mô tả bối cảnh đang hoạt động của phiên làm việc gần nhất.
> Cập nhật ở CUỐI mỗi phiên (xem [handover/HANDOFF.md](handover/HANDOFF.md)).

## Cập nhật gần nhất
- **Ngày:** 2026-08-18
- **Việc vừa làm:** Chuẩn hóa toàn bộ project memory vào `.claude/` trong repo (migrate 108 note từ user-level).

## ⛔ Task tạm dừng (parked branches) — CẦN BIẾT khi khởi động phiên
> Đây là các feature branch CHƯA merge vào `dev`, đang tạm dừng để ưu tiên việc khác.
> Memory chi tiết của mỗi task nằm TRÊN chính nhánh đó (single source of truth), KHÔNG copy về đây.
> Muốn xem chi tiết: `git switch <branch>` rồi đọc `.claude/handover/HANDOFF.md` + `.claude/architecture/`.

| Nhánh | HEAD commit | Trạng thái | Chi tiết ở nhánh đó |
|-------|-------------|-----------|---------------------|
| `feature/hoalinh-gamification` | `b507697` | ⏸️ Parked — Admin cho Mini App "Hoa Linh Gamification" (16 entity `AppHlg/*`, 5 migration `2026081x/2026082x`, `HoaLinhGamificationController`, SignalR `HlgLiveFeedHub/Notifier`). Đi sau `dev` 2 commit (`463aae4`, `fa3b41f`). | `handover/HANDOFF.md`, `architecture/module-hlg.md`, `PROJECT_STATE.md` |

> Task hiện hành (nhánh `feature/hoalinh-25years`): Admin cho Mini App "Dược Phẩm Hoa Linh 25 Năm" — độc lập với gamification, hầu như không đụng nhau.

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
