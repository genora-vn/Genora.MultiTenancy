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

**Ngày:** 2026-08-21
**Trạng thái:** ⏸️ TẠM DỪNG module Hoa Linh Gamification (HLG). Sẽ quay lại làm tiếp bộ Admin Razor UI.

**Việc vừa hoàn thành:**
- Mini-app backend HLG: HOÀN TẤT 100% (Phase 0-6, ~24 endpoint theo contract, build sạch). Đã gửi CURL cho anh test (bỏ header `__tenant`).
- Sample data seeder (`HlgDataSeedContributor`) + permission provider (`HlgManagement`/`HlgManagementHost`).
- Bắt đầu Admin Razor UI: xong **Rewards admin CrudAppService** (`HlgRewardAdminAppService` + DTOs + interface, build 0 errors).

**Việc còn dở / nơi tiếp tục khi quay lại** (chi tiết đầy đủ ở `../ACTIVE_CONTEXT.md` mục "HLG — ĐIỂM DỪNG"):
- Việc kế tiếp NGAY: viết Razor Pages cho nhóm Rewards (Index + Create/Edit modal + index.js) theo pattern `Web/Pages/SalonBeautyStylists/*`.
- ⚠️ CHẶN: chưa xác minh đường dẫn JS proxy runtime của HLG admin service (dự kiến `genora.multiTenancy.appServices.hlg.admin.hlgRewardAdmin`). Cần mở `{{BASE_URL}}/Abp/ServiceProxyScript` khi chạy app để xác minh — build KHÔNG bắt được lỗi path này.
- Thứ tự làm admin UI: Rewards → Knowledge → Ranking → Games+Questions (nested, phức tạp nhất) → Users. Sau đó: Menu contributor (order 49) + menu localization keys.
- Bổ sung localization keys còn thiếu: `Hlg:RewardNameRequired`, `Hlg:RewardPointCostInvalid`, `Hlg:RewardTypeInvalid`.

**Việc runtime chưa chạy (không phải code):**
- Áp migration `AddHlgKnowledge` + `AddHlgGames` + `AddHlgRewards` + `AddHlgRanking` (review SQL script tại `Migrations/Scripts/` trước — DB config trỏ server dùng chung từ xa).
- Tạo tenant "Hoa Linh Miền Nam Gamification" + bật feature `Hlg.Management`; re-seed host admin để nhận permission `HostAppHlg*`.

**Điểm bảo mật cần xử lý:**
- `~\.claude\settings.json` (user-level) chứa `ANTHROPIC_AUTH_TOKEN` plaintext — nên rotate.
- `DbMigrator/appsettings.json` chứa mật khẩu `sa` + Seq API key plaintext (DB dùng chung từ xa 103.157.218.187).
