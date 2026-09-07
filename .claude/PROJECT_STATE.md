# PROJECT STATE — Genora.MultiTenancy

> Trạng thái tổng hợp của từng module, tổng hợp từ các note `*_phase*_complete`, `*_progress`,
> `*_complete` trong `.claude/memory/notes/project/`. Cập nhật khi hoàn thành mốc lớn.

## Tổng quan
- **Framework:** ABP (DDD), multi-tenancy enabled.
- **Core modules:** Calendar Slots, Zalo Auths, Bookings, Golf Courses, News Services.
- **Feature modules đã build:** Golf core + MiniApp, Salon Beauty, Caddie, Hoa Linh, Documents site.

---

## Module: Golf Core & MiniApp — ĐANG VẬN HÀNH
- Đặt sân, pricing theo Member/Guest/Visitor, CalendarSlot (available init/reset, deal filter, visitor fallback).
- PaymentConfiguration entity riêng (thay `GolfCourse.PaymentQr*`), gom permission MiniAppSetting.
- Booking TotalAmount tính từ `AppBookingPlayers` (sum PricePerPlayer), không dùng `booking.TotalAmount`.
- PromotionPolicy entity riêng theo (GolfCourse, PromotionType); MiniApp Booking Detail hiển thị chính sách hoãn/hủy.
- VGA code: validate + recalc Member pricing, dedup 1 mã/1 người.
- MiniApp: cancel booking, notifier SignalR, ItemId=null tránh FK cross-tenant.
- Đã xử lý sự cố prod antiforgery SSL (proxy terminate TLS).
- Note: `project_calendar_slot_*`, `project_booking_*`, `project_payment_configuration`, `project_validate_vga_code_api`, `project_multitenant_db_routing`.

## Module: Salon Beauty — HOÀN THÀNH BACKEND, UI đã build
- Backend đầy đủ (chi tiết cũ ở `memory/modules/salon-beauty/`): 8 entity schema "Salon", 6 AppService, dual permission, feature gate, loyalty.
- UI: Stylist / Booking / Location / TimeSlot (capacity + peak hour), customer detail redesign, booking history + change stylist.
- Deposit + Loyalty config (DEP code, 2-step approval ACID, ledger, ExchangeRate per-tenant).
- MiniApp: payment endpoints (clone Pro/Fnb), location/timeslot/stylist filter, ZBS booking + service review.
- Note: `project_salon_*`, `memory/modules/salon-beauty/`.

## Module: Caddie — HOÀN THÀNH Phase 1-7
- SRS 5 module; DB design 10 tables; 9 enums, 9 entities, EF + migration.
- 6 AppService, 6 page group, MiniApp 6 endpoints, Excel import/export, Calendar 3 views.
- CaddieFee (`GolfCourse.CaddieFee`), multi-caddie per booking (`AppCaddieBookingDetail`), booking gắn vào golf players.
- Avatar refactor: bỏ base64 → `IRemoteStreamContent` qua ManageImageService (15MB).
- Migration mốc: 20260724091716 (link players), 20260725062150 (TotalCaddieFee).
- Note: `project_caddie_*`.

## Module: Hoa Linh (Dược phẩm) — HOÀN THÀNH Phase 1-7
- BRD: Mini App 8 module + Admin Portal 10 + API DMS sync.
- Data integration: 10 nhóm dữ liệu Pull/Push, prefix `AppHl`, SyncLog.
- Phase 1 foundation (4 enum, 4 entity schema HL, feature AllowHoaLinhModule, 7 permission pair, menu order 50).
- Phase 2 API client (`IHlApiClientService`, HttpClient "HoaLinhDms", X-API-Key, log `AppHlApiLogs`).
- Phase 3-5: Admin Portal UI (6 page + Dashboard), CRUD Orders/GiftExchanges, MiniApp 13 endpoints.
- Phase 7: Dashboard + data-level auth (`HlDataAccessService` user→dsr_code).
- Loyalty: `HlPointBatch` (FIFO +1 năm) + `HlPointTransaction` (sổ cái) + `Customer.BonusAmount`; `HlPointExpireWorker` mỗi giờ; migration 20260709064009.
- UrBox eVoucher: `IUrBoxService`, cartPayVoucher POST + Signature RSA-SHA256 (.NET9), redeem lưu `HlGiftExchange`.
- Zalo OA articles: news list + detail, cache per-tenant `IDistributedCache`.
- Note: `project_hoalinh_*`, `project_hl_*`, `project_urbox_integration`, `project_zalo_oa_articles`.

## Module: Documents site — HOÀN THÀNH
- Online docs `/Documents`: entity host-shared, FeatureName + Tenant/HostPermissionName, URL slug, seeder 11 section.
- Note: `project_app_documents_*`.

## Module: Hoa Linh 25 Năm (hl25) — ✅ HOÀN THÀNH P0-P7 + 🔄 DELTA 2026-09 (nhánh `feature/dev-hoalinh-25years`)
- Admin cho Zalo Mini App "Dược Phẩm Hoa Linh 25 Năm" (chương trình kỷ niệm 25 năm: tạo thiệp ghép ảnh + chia sẻ + vòng quay may mắn). Schema DB riêng `hl25`.
- **🔄 Delta 2026-09 (cập nhật theo Figma FE mới):** (1) mỗi người **tối đa TRÚNG 1 lần** (SpinAsync chặn `TotalGiftsWon>=1→NotWon`); (2) `Hl25SpinResultDto` thêm cờ FE cho 3 màn kết quả; (3) **nhóm tuổi** `Hl25AgeGroup` thay `BirthDate`; (4) `MaxWishLength` 500→250. Migration `20260825160252` CHƯA apply → sửa in-place. Chi tiết: `docs/HOALINH25_ADMIN_SCHEMA.md` mục 0. (commit `d20232b`)
- **🔄 P2 tinh giản mạnh (2026-09-07):** BỎ 5 field `LogoUrl`/`BannerUrl`/`TvcUrl`/`TvcHtml`/`GamePlayHtml` khỏi `Hl25AppConfig` (ảnh/nội dung cố định trong FE — lưu ý #4). Giữ `ProgramName`/`RulesHtml`/`StartTime`/`EndTime`/`Scope`/`OrganizerName`/`IsActive`. Đồng bộ entity+DTO+MiniApp+DbContext ext+Settings page+migration in-place. Build 0 errors. Đã rà soát Vòng quay/Gift + Report: OK, không cần sửa (cơ cấu 5 loại quà nhập qua CRUD).
- **Thiết kế (Bước 1-5):** UI Figma, 10 entity, 8 Phase plan. Tài liệu: `docs/HOALINH25_ADMIN_SCHEMA.md`.
- **Quyết định chốt:** bỏ Points (thuộc gamification), Wheel singleton/tenant, trần 2 lượt quay (mỗi chu kỳ "Tạo thiệp→Chia sẻ" = +1, tối đa 2), trao thưởng 2 bước.
- **Tái dùng:** Summernote (HTML editor), `IManageImageService` (upload ảnh, tự chặn 5MB), Zalo OA/ZNS/Log dùng chung.
- **✅ P0 (Foundation):** 6 enum (`Enums/Hl25Enums.cs`) + `Hl25/Hl25Consts.cs`; Feature `Hl25.Management`; Permission dual 5 nhóm Tenant+Host (group `MiniAppHl25`/`MiniAppHl25Host`); menu `MenuGroup.Hl25` (order 51); localization vi/en.
- **✅ P1 (Entities + DB):** 10 entity `Domain/DomainModels/AppHl25/` (`Hl25AppConfig`; Frame `Campaign/Template/Creation`; Wheel `Config/Slot/Gift/SpinTurnLog/SpinLog`; `Participant`) + `ConfigureHl25Module` + 10 DbSet. Migration **`20260825160252_AddHl25Module`** (10 bảng, 23 index, 5 FK). Build Web+EF 0 errors. **CHƯA `database update`.** (commit `2ffacb7`)
- **✅ P2 (Cài đặt Mini App) — commit `a3b08cc`:** DTO `Hl25AppConfigDto`/`CreateUpdateHl25AppConfigDto` + `IHl25AppConfigAppService`; `Hl25AppConfigAppService` singleton/tenant (GetAsync tự tạo mặc định / UpdateAsync / UploadAssetAsync validate 5MB) + AutoMapper; trang `Web/Pages/Hl25/Settings` (Summernote cho Thể lệ/Luật chơi/TVC, upload Logo/Banner, link `/AppZaloAuths`+`/AppZaloLogs`).
- **✅ P4 (Vòng quay may mắn) — commit `c1791fa`:** 4 AppService — `Hl25GiftAppService` (CRUD kho quà + upload 5MB + tự OutOfStock), `Hl25WheelConfigAppService` (singleton/tenant get/update cấu hình+slots, validate tổng WinRate=100), `Hl25SpinTurnLogAppService` (list read-only), `Hl25SpinLogAppService` (list + `UpdateRewardStatusAsync` 2 bước Won→Delivered) + AutoMapper. Trang `Web/Pages/Hl25/Wheel` 4 tab + Gift modals + `Wheel.js`.
- **✅ P3 (Quản lý Frame) — commit `9243043`:** 3 AppService — `Hl25FrameCampaignAppService` (CRUD chiến dịch + đếm TemplateCount), `Hl25FrameTemplateAppService` (CRUD mẫu frame + `UploadTemplateImageAsync` 5MB, lọc theo campaign), `Hl25FrameCreationAppService` (list read-only, join Participant+Campaign) + AutoMapper. Trang `Web/Pages/Hl25/Frames` 3 tab + Campaign/Template Create/Edit modals + `Frames.js`.
- **✅ P5 (Quản lý Người dùng) — commit `50ea1d7`:** `Hl25ParticipantAppService` (list filter tên/SĐT/FollowOA/Consent/khoảng ngày; `UpdateAsync`; `GrantSpinTurnAsync` cộng lượt thủ công source=AdminGrant, KHÔNG áp trần; `ExportExcelAsync`) + `Hl25ParticipantExcelExporter` (ClosedXML) + AutoMapper + controller `Hl25ParticipantExcelController` (`api/app/hl25-participant-excel/export`). Trang `Web/Pages/Hl25/Participants` (list + filter + Xuất Excel) + Edit modal + Grant spin-turn modal + `Participants.js`.
- **✅ P6 (Báo cáo Thống kê) — commit `a1a7d54`:** `Hl25ReportAppService` (3 method query AsyncExecuter, dual permission Reports): `GetFrameStatsAsync`, `GetWheelParticipationStatsAsync`, `GetWheelGiftStatsAsync` + DTO `Hl25ReportDtos`. Trang `Web/Pages/Hl25/Reports` (lọc khoảng ngày + Frame KPI+chart Chart.js line, Vòng quay KPI, bảng theo quà) + `Reports.js`.
- **✅ P7 (MiniApp API):** `HoaLinh25MiniAppController` (`api/mini-app/hl25`, `[AllowAnonymous]`, envelope `Hl25ApiResult<T>`) + `MiniAppHl25Service` (`[AllowAnonymous][RemoteService(false)][DisableValidation]`). 9 endpoint: config / participants register (upsert theo ZaloUserId) / me GET+PUT / frames / frames/share (cộng lượt chu kỳ, trần 2) / wheel (ẩn WinRate) / wheel/spin (**ACID** transaction + weighted random + trừ kho quà) / me/gifts. Build 0 errors. CHƯA commit.
- **🎉 MODULE HOÀN THÀNH P0-P7.** Còn lại: chạy `dotnet ef database update` khi deploy + FE ghép API.

---

## Trạng thái theo dõi
| Module | Trạng thái | Ghi chú |
|--------|-----------|---------|
| Golf Core & MiniApp | ✅ Vận hành | maintenance / feature nhỏ |
| Salon Beauty | ✅ Backend + UI | có thể còn polish UI |
| Caddie | ✅ Phase 1-7 | multi-caddie mới nhất |
| Hoa Linh | ✅ Phase 1-7 | loyalty + UrBox + Zalo OA |
| Documents | ✅ Xong | seeder 11 section |
