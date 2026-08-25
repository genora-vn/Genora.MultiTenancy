# LOAD CONTEXT — Quy trình khởi động phiên làm việc mới

> Đọc file này ĐẦU TIÊN khi bắt đầu một phiên làm việc mới trên repo Genora.MultiTenancy
> (đặc biệt trên máy mới hoặc khi bàn giao cho thành viên khác).

## Bước 1 — Thứ tự đọc file context (bắt buộc)
Đọc theo thứ tự sau để tái tạo bối cảnh:

1. **`CLAUDE.md`** (root repo) — điều hướng, code map, coding rules cơ bản.
2. **`.claude/RULES.md`** — coding conventions + lessons learned (18 quy tắc). Đọc kỹ để không lặp lỗi cũ.
3. **`.claude/PROJECT_STATE.md`** — trạng thái từng module (Golf, Salon, Caddie, Hoa Linh, Docs).
4. **`.claude/ACTIVE_CONTEXT.md`** — việc đang làm dở của phiên gần nhất.
5. **`.claude/TASK_LOG.md`** — nhật ký các mốc gần đây (để hiểu lịch sử).
6. **`.claude/architecture/`** — khi cần chi tiết kiến trúc của module đang làm.
7. **`.claude/MEMORY.md`** — index để tra note chi tiết trong `memory/notes/`.

## Bước 2 — Khôi phục trạng thái dự án
1. `git status` + `git log --oneline -10` — xem thay đổi chưa commit và commit gần nhất.
2. `dotnet build` — xác nhận build sạch trước khi bắt đầu.
3. Kiểm tra migration mới nhất (xem TASK_LOG mục "Migration"): `dotnet ef migrations list`.
4. Nếu chạy host: `dotnet run --project src/Genora.MultiTenancy.Web`.

## Bước 3 — Xác định task đang làm dở
1. Đọc `ACTIVE_CONTEXT.md` mục "Việc vừa làm" **VÀ mục "⛔ Task tạm dừng (parked branches)"**.
2. **Kiểm tra parked branches:** memory trong `.claude/` là branch-local, nên task đang tạm dừng trên
   feature branch CHƯA merge sẽ KHÔNG hiện trên `dev`. Chạy `git branch -a` + đọc bảng parked ở
   `ACTIVE_CONTEXT.md`. Muốn xem chi tiết một task parked: `git switch <branch>` rồi đọc
   `.claude/handover/HANDOFF.md` + `.claude/architecture/` của nhánh đó.
3. Đối chiếu với `git status` (file đang sửa) và `git log` (commit dở).
4. Tìm note mới nhất trong `memory/notes/project/` KHÔNG có hậu tố `complete`.
5. Grep `TODO`/`FIXME` trong module liên quan.
6. Nếu vẫn không rõ → hỏi người bàn giao (xem `handover/HANDOFF.md`).

## Bước 4 — Trong lúc làm việc
- Tuân thủ `RULES.md`. Khi phát hiện quy tắc/lesson mới → thêm note vào `memory/notes/feedback/` và cập nhật `RULES.md` + `MEMORY.md`.
- Khi hoàn thành tính năng → thêm note vào `memory/notes/project/` + 1 dòng vào `TASK_LOG.md`.

## Bước 5 — Bàn giao cuối phiên (BẮT BUỘC)
Theo `feedback_memory_update_before_handoff`: TRƯỚC khi kết thúc/chuyển task:
1. Cập nhật `ACTIVE_CONTEXT.md` (việc vừa làm, việc còn dở).
2. Thêm dòng vào `TASK_LOG.md`.
3. Cập nhật `PROJECT_STATE.md` nếu đóng một mốc lớn.
4. Ghi note chi tiết vào `memory/notes/` + cập nhật `MEMORY.md` index.
5. Điền `handover/HANDOFF.md` nếu bàn giao cho người khác.

## Tái tạo trên máy mới
Toàn bộ context nằm trong `.claude/` (đã version-control). Chỉ cần `git clone` là có đủ.
Kho user-level (`~\.claude\...`) chỉ là backup, KHÔNG cần cho người mới.
