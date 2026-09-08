# ACTIVE CONTEXT — Việc đang làm dở

> File này mô tả bối cảnh đang hoạt động của phiên làm việc gần nhất.
> Cập nhật ở CUỐI mỗi phiên (xem [handover/HANDOFF.md](handover/HANDOFF.md)).

## Cập nhật gần nhất
- **Ngày:** 2026-09-08
- **Nhánh:** `feature/dev-hoalinh-25years` (đã merge dev + hotfix/20260826)
- **Việc mới nhất (2026-09-08b) — Rà soát UX toàn module hl25 Admin:** (1) **Localize enum dropdown** (CampaignStatus/GiftStatus/Gender/AgeGroup/SharePlatform/SpinTurnSource/RewardStatus) — thêm key `Enum:Hl25*:*` + `Hl25Field:*` + `Hl25Common:*` vi/en; các modal (Campaign/Participant/Gift) + Frames.js + Wheel.js dùng localization thay hardcode. (2) **Ảnh upload local HOẶC dán URL** ở Gift Create/Edit modal (file input tự upload qua `uploadGiftImageByFile` → điền `ImageUrl` + preview; vẫn cho dán URL trực tiếp). (3) **Fix flatpickr modal Campaign** (Create+Edit): thêm `static:true`+`allowInput:true`+`minuteIncrement:1` → calendar hoạt động trong Bootstrap modal (trước bị focus-trap chặn, chỉ tăng/giảm giờ). (4) **API GetWheel mapping quà:** `slotImageUrl`/`label` ưu tiên ô, fallback ảnh/tên **quà đã gán**; thêm `giftId`/`giftName`/`giftDescription`/`isGift`; full URL cho slot/background/pointer image. Build HttpApi+Web 0 errors. File Postman Get Wheel cập nhật. CHƯA commit.
- **Việc trước đó (2026-09-08) — Trang Cấu hình chương trình (/Hl25/Settings):** đổi menu "Cài đặt Mini App" → **"Cấu hình chương trình"** (title+subtitle+localization vi/en `Hl25Settings:*`); thêm 3 field `IntroductionHtml` (Summernote), `Format`, `GiftDeliveryTime` vào `Hl25AppConfig` (+DTO Admin/MiniApp + `GetConfigAsync` + DbContext ext); **migration MỚI `20260908054652_AddHl25ProgramInfoFields`** (3 AddColumn — DB đã apply P0-P7). Bỏ group "Tích hợp Zalo" khỏi trang. Layout UX (nút Lưu sticky, card-header, form gọn) + toolbar Summernote đầy đủ (font/size/color/table/hr). Config API trả 3 field mới. **Fix drift snapshot/Fluent API** (sót Tvc*, WishMessage 500→250) để migration sạch. Build EF+Web 0 errors. File Postman config API cập nhật. **CHƯA commit + CHƯA `database update`.**

- **Việc đang làm:** Cập nhật module hl25 theo **Figma FE mới (Delta 2026-09)** — đối chiếu thiết kế cập nhật với bản đã implement P0-P7.
  - 📌 **Delta chốt (2026-09-07):** (1) mỗi người **tối đa TRÚNG 1 lần** (lần 1 trúng→lần 2 ép trượt; lần 1 trượt→lần 2 random) — dùng `TotalGiftsWon` sẵn có, KHÔNG migration; (2) `Hl25SpinResultDto` thêm cờ FE (`CanShareForMoreTurn`/`EarnedCycles`/`TotalGiftsWon`/`HasWonBefore`) cho 3 màn kết quả; (3) **nhóm tuổi** (`Hl25AgeGroup` 18-25/26-35/36-44) thay `BirthDate` — sửa entity + migration in-place; (4) lời chúc `MaxWishLength` 500→250. Migration `20260825160252_AddHl25Module` **CHƯA apply** → sửa in-place.
  - 📄 **Chi tiết:** [`docs/HOALINH25_ADMIN_SCHEMA.md`](docs/HOALINH25_ADMIN_SCHEMA.md) mục 0 "Delta 2026-09".
  - ✅ **Đã commit `d20232b`:** 4 delta trên (trúng 1 lần/người, cờ FE, nhóm tuổi, lời chúc 250) + migration in-place + tài liệu/memory.
  - ✅ **P2 tinh giản mạnh (tiếp theo, chưa commit):** BỎ 5 field `LogoUrl`/`BannerUrl`/`TvcUrl`/`TvcHtml`/`GamePlayHtml` khỏi `Hl25AppConfig` (ảnh/nội dung cố định trong FE — lưu ý #4). Giữ `ProgramName`/`RulesHtml`(Thể lệ)/`StartTime`/`EndTime`/`Scope`/`OrganizerName`/`IsActive`. Đồng bộ entity+2 Admin DTO+MiniApp DTO/GetConfig+DbContext ext+Settings page(.cshtml/.cs/.js)+migration in-place (bỏ 5 cột). Build Web 0 errors.
  - **Đã rà soát (2026-09-07):** Vòng quay/Gift Admin OK (cơ cấu 5 loại quà nhập qua CRUD, không cần code). Report OK (TotalWins không đếm nhầm lượt ép trượt).
  - ✅ **Báo cáo phân bổ nhóm tuổi (P6) — XONG:** `GetAgeGroupStatsAsync` (query Participant group theo `AgeGroup`, lọc `JoinedTime`, đủ 4 nhóm kể cả 0 người + tỷ lệ %) + DTO `Hl25AgeGroupStatsDto`/`Row` + interface. UI Reports thêm card doughnut Chart.js + bảng + tfoot tổng. Build 0 errors.
  - ✅ **CURL API (T1) — đã gửi bộ đầy đủ 9 endpoint** theo thiết kế mới (AgeGroup thay BirthDate, cờ FE spin, config tinh giản) trong hội thoại.
  - ✅ **Bổ sung 7 MiniApp read API + chuẩn hóa mã lỗi (2026-09-07):** Frame (`GET frames/campaigns`, `GET frames/templates?campaignId=`, `GET me/frames?zaloUserId=`); Wheel (`GET gifts`, `GET me/spin-turns?zaloUserId=`, `GET me/spins?zaloUserId=`). DTO public riêng + `Hl25ErrorCodes` (Domain.Shared) gắn mã cho MỌI throw. File Postman `C:\Users\DPC\Downloads\hl25_curl.json` cập nhật 15 request + mô tả tiếng Việt có dấu + bảng mã lỗi. Build HttpApi 0 errors (Web build lỗi do app đang chạy khóa DLL — không phải lỗi code).
  - ✅ **Phản hồi FE (2026-09-07):** (1) Thêm `POST upload-image` (multipart/form-data field `file`, validate 5MB) trả `{ url }` full URL — FE thay mock `Hl25Api.uploadImage()`; (2) helper `ToFullUrl` (dùng `IHttpContextAccessor`) áp full URL cho MỌI ảnh trong response (templates/gifts/creations/spins/gifts) — fix `frames/templates` chỉ trả path; (3) **xác nhận mapping:** `ageGroup` CHỈ 4 giá trị (0=N/A,1=18-25,2=26-35,3=36-44) — FE dùng 6 giá trị SAI; `gender` 0=N/A,1=Nam,2=Nữ,3=Khác — FE thiếu Other=3. Thêm 3 mã lỗi upload. File Postman 16 request. Build HttpApi 0 errors.
  - **Chưa đụng:** Frame (P3), Report (P6 — trừ gợi ý nhóm tuổi). **CHƯA push** (SSH key môi trường chưa cấu hình). **CHƯA `dotnet ef database update`.**

### Việc cũ (P0-P7 module hl25 — nhánh `feature/hoalinh-25years` trước đây)
> Xây hệ thống Admin cho Zalo Mini App "Dược Phẩm Hoa Linh 25 Năm" (schema DB `hl25`).
  - ✅ **Thiết kế xong** (Bước 1-5): trích xuất UI Figma, thiết kế 10 entity schema `hl25`, xác định điểm tái sử dụng (Summernote / `IManageImageService` / Zalo OA-ZNS-Log), lập kế hoạch 8 Phase.
  - 📄 **Tài liệu kiến trúc:** [`docs/HOALINH25_ADMIN_SCHEMA.md`](docs/HOALINH25_ADMIN_SCHEMA.md).
  - ✅ **Quyết định nghiệp vụ đã chốt (2026-08-25):** (1) KHÔNG quản lý Points ở hl25 (thuộc gamification); (2) WheelConfig singleton/tenant; (3) trần 2 lượt quay/người — mỗi chu kỳ "Tạo thiệp → Chia sẻ thành công" = +1 lượt, tối đa 2 chu kỳ; (4) trao thưởng 2 bước (Won → Admin Delivered).
  - ✅ **P0 (Foundation) XONG:** 6 enum (`Enums/Hl25Enums.cs`) + `Hl25/Hl25Consts.cs` (MaxSpinTurnsPerUser=2, 5MB, phone regex); Feature `Hl25.Management` (`AppHl25Features` + provider); Permission dual 5 nhóm Tenant + 5 Host (`MultiTenancyPermissions` + provider block `MiniAppHl25`/`MiniAppHl25Host`); menu group `MenuGroup.Hl25` (order 51); localization vi/en. Build Web 0 errors.
  - ✅ **P1 (Entities + DB) XONG:** 10 entity (`Domain/DomainModels/AppHl25/`) + `MultiTenancyDbContextModelCreatingExtensionsHl25.cs` (`ConfigureHl25Module`) + 10 DbSet + gọi trong DbContext. Migration **`20260825160252_AddHl25Module`** (10 bảng, 23 index, 5 FK, schema `hl25`) — đã verify không rỗng. Build EF 0 errors. **CHƯA chạy `dotnet ef database update`.** (Đã commit `2ffacb7`.)
  - ✅ **P2 (Cài đặt Mini App) XONG — commit `a3b08cc`:** DTO `Hl25AppConfigDto`/`CreateUpdateHl25AppConfigDto` + `IHl25AppConfigAppService`; AppService singleton/tenant `Hl25AppConfigAppService` (GetAsync tự tạo mặc định / UpdateAsync / UploadAssetAsync có validate 5MB) + AutoMapper; trang `Web/Pages/Hl25/Settings` — form Summernote cho Thể lệ/Luật chơi/TVC, upload Logo/Banner (preview + chặn 5MB client), link `/AppZaloAuths` + `/AppZaloLogs`. (Kèm block menu `MenuGroup.Hl25` bị sót khỏi commit P0.)
  - ✅ **P4 (Vòng quay may mắn) XONG — commit `c1791fa`:** Backend 4 AppService: `Hl25GiftAppService` (CRUD kho quà + upload ảnh 5MB + tự set OutOfStock); `Hl25WheelConfigAppService` (singleton/tenant, get/update cấu hình + slots, validate tổng WinRate=100, MARS delete+insert slots); `Hl25SpinTurnLogAppService` (list read-only, join Participant); `Hl25SpinLogAppService` (list + `UpdateRewardStatusAsync` trao thưởng 2 bước Won→Delivered). AutoMapper đủ. Trang `Web/Pages/Hl25/Wheel` (4 tab) + Gift Create/Edit modals + `Wheel.js`.
  - ✅ **P3 (Quản lý Frame) XONG — commit `9243043`:** Backend 3 AppService: `Hl25FrameCampaignAppService` (CRUD chiến dịch + đếm TemplateCount); `Hl25FrameTemplateAppService` (CRUD mẫu frame + `UploadTemplateImageAsync` 5MB, lọc theo campaign); `Hl25FrameCreationAppService` (list read-only, join Participant+Campaign). AutoMapper đủ. Trang `Web/Pages/Hl25/Frames` (3 tab) + Campaign/Template Create/Edit modals + `Frames.js`.
  - ✅ **P5 (Quản lý Người dùng) XONG — commit `50ea1d7`:** `Hl25ParticipantAppService` (list filter theo tên/SĐT/FollowOA/Consent/khoảng ngày; `UpdateAsync` sửa thông tin + ghi mốc consent; `GrantSpinTurnAsync` cộng lượt thủ công source=AdminGrant, KHÔNG áp trần; `ExportExcelAsync`) + `Hl25ParticipantExcelExporter` (ClosedXML) + AutoMapper + controller `Hl25ParticipantExcelController`. Trang `Web/Pages/Hl25/Participants` + Edit/Grant modals + `Participants.js`.
  - ✅ **P6 (Báo cáo Thống kê) XONG — commit `a1a7d54`:** `Hl25ReportAppService` (3 method query AsyncExecuter, dual permission Reports): `GetFrameStatsAsync`, `GetWheelParticipationStatsAsync`, `GetWheelGiftStatsAsync` + DTO. Trang `Web/Pages/Hl25/Reports` (lọc khoảng ngày + Frame KPI+chart Chart.js line, Vòng quay KPI, bảng theo quà) + `Reports.js`.
  - ✅ **P7 (MiniApp API) XONG:** `HoaLinh25MiniAppController` (`api/mini-app/hl25`, `[AllowAnonymous]`, envelope `Hl25ApiResult<T>`) + `MiniAppHl25Service` (`[AllowAnonymous][RemoteService(false)][DisableValidation]`). Endpoints: `GET config`, `POST participants/register` (upsert theo ZaloUserId), `GET/PUT participants/me`, `POST frames` (tạo thiệp), `POST frames/share` (cộng lượt theo chu kỳ, trần 2, transaction), `GET wheel` (ẩn WinRate), `POST wheel/spin` (**ACID**: uowManager.Begin requiresNew+isTransactional, weighted random theo WinRate, trừ kho quà + giảm lượt + ghi SpinLog, autoSave:false + CompleteAsync), `GET me/gifts`. Build Application + HttpApi 0 errors. **CHƯA commit P7.**
  - 🎉 **MODULE HL25 HOÀN THÀNH toàn bộ P0-P7** (5 nhóm tính năng Admin + MiniApp API). Còn lại chỉ là: chạy `dotnet ef database update` khi deploy + FE ghép API.
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
