# HANDOFF — Bàn giao giữa các phiên làm việc

> Điền file này khi bàn giao cho người/phiên khác. Xem quy trình đầy đủ ở [../LOAD_CONTEXT.md](../LOAD_CONTEXT.md).

## Checklist bàn giao (người giao)
- [ ] Đã cập nhật `../ACTIVE_CONTEXT.md` (việc vừa làm + việc còn dở).
- [ ] Đã thêm dòng vào `../TASK_LOG.md`.
- [ ] Đã cập nhật `../PROJECT_STATE.md` nếu đóng mốc lớn.
- [ ] Đã ghi note chi tiết vào `../memory/notes/` + cập nhật `../MEMORY.md`.
- [ ] Build sạch (`dotnet build`) và commit/push code liên quan.
- [ ] Ghi rõ migration mới (nếu có) vào TASK_LOG.

## Checklist tiếp nhận (người nhận)
- [ ] Đọc theo thứ tự trong `../LOAD_CONTEXT.md`.
- [ ] `git pull` + `dotnet build` + kiểm tra migration.
- [ ] Xác định task dở theo `../ACTIVE_CONTEXT.md`.

---

## Bàn giao hiện tại

**Ngày:** 2026-08-18
**Người giao:** (phiên chuẩn hóa memory)

**Việc vừa hoàn thành:**
- Chuẩn hóa toàn bộ project memory vào `.claude/` (migrate 108 note từ user-level).
- Tạo cấu trúc curated: MEMORY / RULES / PROJECT_STATE / ACTIVE_CONTEXT / TASK_LOG / LOAD_CONTEXT + architecture/ + handover/.

**Việc còn dở / cần chú ý:**
- Chưa có: chưa phát hiện task code nào đang treo giữa chừng.
- Lưu ý: yêu cầu ban đầu có nhắc "mini app Hoa Linh Gamification" — tính năng này CHƯA được xây. Nếu cần, xem `../architecture/module-hoalinh.md` để hiểu bối cảnh loyalty/points trước khi bắt đầu.

**Điểm bảo mật cần xử lý:**
- `~\.claude\settings.json` (user-level) chứa `ANTHROPIC_AUTH_TOKEN` plaintext — nên rotate.
