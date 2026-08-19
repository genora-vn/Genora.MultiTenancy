# ACTIVE CONTEXT — Việc đang làm dở

> File này mô tả bối cảnh đang hoạt động của phiên làm việc gần nhất.
> Cập nhật ở CUỐI mỗi phiên (xem [handover/HANDOFF.md](handover/HANDOFF.md)).

## Cập nhật gần nhất
- **Ngày:** 2026-08-18
- **Việc vừa làm:** Chuẩn hóa toàn bộ project memory vào `.claude/` trong repo (migrate 108 note từ user-level).

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
