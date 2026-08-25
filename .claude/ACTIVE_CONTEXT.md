# ACTIVE CONTEXT — Việc đang làm dở

> File này mô tả bối cảnh đang hoạt động của phiên làm việc gần nhất.
> Cập nhật ở CUỐI mỗi phiên (xem [handover/HANDOFF.md](handover/HANDOFF.md)).

## Cập nhật gần nhất
- **Ngày:** 2026-08-25
- **Nhánh:** `feature/hoalinh-25years`
- **Việc đang làm:** Xây hệ thống Admin cho Zalo Mini App "Dược Phẩm Hoa Linh 25 Năm" (schema DB `hl25`).
  - ✅ **Thiết kế xong** (Bước 1-5): trích xuất UI Figma, thiết kế 10 entity schema `hl25`, xác định điểm tái sử dụng (Summernote / `IManageImageService` / Zalo OA-ZNS-Log), lập kế hoạch 8 Phase.
  - 📄 **Tài liệu kiến trúc:** [`docs/HOALINH25_ADMIN_SCHEMA.md`](docs/HOALINH25_ADMIN_SCHEMA.md).
  - ✅ **Quyết định nghiệp vụ đã chốt (2026-08-25):** (1) KHÔNG quản lý Points ở hl25 (thuộc gamification); (2) WheelConfig singleton/tenant; (3) trần 2 lượt quay/người — mỗi chu kỳ "Tạo thiệp → Chia sẻ thành công" = +1 lượt, tối đa 2 chu kỳ; (4) trao thưởng 2 bước (Won → Admin Delivered).
  - ✅ **P0 (Foundation) XONG:** 6 enum (`Enums/Hl25Enums.cs`) + `Hl25/Hl25Consts.cs` (MaxSpinTurnsPerUser=2, 5MB, phone regex); Feature `Hl25.Management` (`AppHl25Features` + provider); Permission dual 5 nhóm Tenant + 5 Host (`MultiTenancyPermissions` + provider block `MiniAppHl25`/`MiniAppHl25Host`); menu group `MenuGroup.Hl25` (order 51); localization vi/en. Build Web 0 errors.
  - ✅ **P1 (Entities + DB) XONG:** 10 entity (`Domain/DomainModels/AppHl25/`) + `MultiTenancyDbContextModelCreatingExtensionsHl25.cs` (`ConfigureHl25Module`) + 10 DbSet + gọi trong DbContext. Migration **`20260825160252_AddHl25Module`** (10 bảng, 23 index, 5 FK, schema `hl25`) — đã verify không rỗng. Build EF 0 errors. **CHƯA chạy `dotnet ef database update`.** (Đã commit `2ffacb7`.)
  - ✅ **P2 (Cài đặt Mini App) XONG:** DTO `Hl25AppConfigDto`/`CreateUpdateHl25AppConfigDto` + `IHl25AppConfigAppService`; AppService singleton/tenant `Hl25AppConfigAppService` (GetAsync tự tạo mặc định / UpdateAsync / UploadAssetAsync có validate 5MB) + AutoMapper; trang `Web/Pages/Hl25/Settings` (cshtml+cshtml.cs+js) — form Summernote cho Thể lệ/Luật chơi/TVC, upload Logo/Banner (preview + chặn 5MB client), link tới `/AppZaloAuths` + `/AppZaloLogs`. Build Application + Web 0 errors. **CHƯA commit P2.**
  - ⏳ **Tiếp theo:** P4 (Vòng quay — lõi) → P3 (Frame) → P5 (Người dùng) → P6 (Báo cáo) → P7 (MiniApp API).
- **Việc vừa làm trước đó:** Đăng ký con trỏ parked-branch cho `feature/hoalinh-gamification` (commit `d33be8a` trên dev).

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
