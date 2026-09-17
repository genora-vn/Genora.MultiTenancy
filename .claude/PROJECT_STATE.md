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

## Module: Hoa Linh 25 Năm (hl25) — ✅ HOÀN THÀNH (nhánh `feature/dev-hoalinh-gamification`)
- **✅ Cập nhật 2026-09-16:**
  - **Participants page:** bổ sung cột ZaloUserId, AgeGroup, ảnh thiệp, lời chúc, lịch sử quay; full URL cho ảnh thiệp
  - **Excel export Participants:** 16 cột (STT + 15 cột dữ liệu), full URL cho ảnh thiệp
  - **Reports page:** thêm chart doughnut phân bổ giới tính (Nam/Nữ/Khác) song song chart nhóm tuổi
  - **CreateFrame API:** lưu ảnh thiệp với tên `ZaloUserId_yyyyMMddHHmmss.ext` (flag `RenameWithTimestamp`)
  - **Frame Creations tab:** thêm button "Xuất Excel" và "Tải ảnh ZIP" (download toàn bộ ảnh thiệp)
- **✅ Cập nhật 2026-09-14:**
  - **Follow-up UI/kho quà:** dropdown explicit vi/en; fix tab lịch sử quay; ảnh vòng quay riêng WheelImageUrl + modal hai ảnh/upload/VNĐ; API wheel trả wheelImageUrl/giftImageUrl. **Migration 20260914111213_AddHl25GiftWheelImage chưa apply**
  - **Admin update:** quy tắc **tạo thiệp lượt đầu; chia sẻ lượt hai**; giữ dữ liệu lượt cũ, AdminGrant ngoại lệ. Guard trao quà Won→Delivered; transaction cấp lượt; giữ ID/ảnh wheel; validate ngày/kho; báo cáo hết ngày cuối/tách AdminGrant
- **✅ Delta 2026-09 (cập nhật theo Figma FE mới):** (1) mỗi người **tối đa TRÚNG 1 lần**; (2) `Hl25SpinResultDto` thêm cờ FE cho 3 màn kết quả; (3) **nhóm tuổi** `Hl25AgeGroup` thay `BirthDate`; (4) `MaxWishLength` 500→250
- **✅ P2 tinh giản mạnh (2026-09-07):** BỎ 5 field `LogoUrl`/`BannerUrl`/`TvcUrl`/`TvcHtml`/`GamePlayHtml` khỏi `Hl25AppConfig`. Giữ `ProgramName`/`RulesHtml`/`StartTime`/`EndTime`/`Scope`/`OrganizerName`/`IsActive`
- **✅ Báo cáo phân bổ nhóm tuổi (P6):** `GetAgeGroupStatsAsync` + chart doughnut Chart.js
- **✅ Bổ sung 7 MiniApp read API + chuẩn hóa mã lỗi:** Frame + Wheel endpoints, `Hl25ErrorCodes` (12 mã)
- **Thiết kế:** UI Figma, 10 entity, 8 Phase plan. Tài liệu: `docs/HOALINH25_ADMIN_SCHEMA.md`
- **✅ P0-P7 HOÀN THÀNH:** Foundation, Entities+DB, Settings, Frames, Wheel, Participants, Reports, MiniApp API (17 endpoints)
- **🎉 MODULE HOÀN THÀNH.** Còn lại: chạy `dotnet ef database update` khi deploy + FE ghép API

## Module: Hoa Linh Gamification (HLG) — 🔨 ĐANG BUILD (đã merge vào `feature/dev-hoalinh-gamification`)
- **Merge 2026-09-16:** đã merge từ `feature/hoalinh-gamification` (commit `b507697`) vào `feature/dev-hoalinh-gamification`, fix 4 conflict files, build thành công
- **Backend mini-app HOÀN TẤT 100%** — ~24 endpoint theo contract
- **Phase 0-6 xong:** Auth+Profile, Knowledge base, Games engine, Rewards & Shipping, Ranking, Live-feed SignalR
- **Phase 7 (Admin Razor UI) ĐANG DỞ:** đã xong Rewards admin CrudAppService, đang dở Razor Pages
- **Migration đã sinh:** `AddHlgModule`, `AddHlgKnowledge`, `AddHlgGames`, `AddHlgRewards`, `AddHlgRanking`
- **Schema:** HLG (tenant riêng)
- **Xem chi tiết:** `architecture/module-hlg.md`

---

## Trạng thái theo dõi
| Module | Trạng thái | Ghi chú |
|--------|-----------|---------|
| Golf Core & MiniApp | ✅ Vận hành | maintenance / feature nhỏ |
| Salon Beauty | ✅ Backend + UI | có thể còn polish UI |
| Caddie | ✅ Phase 1-7 | multi-caddie mới nhất |
| Hoa Linh | ✅ Phase 1-7 | loyalty + UrBox + Zalo OA |
| Documents | ✅ Xong | seeder 11 section |
| Hoa Linh Gamification (HLG) | 🔄 Đã merge vào dev | merge 2026-09-16, Phase 0-6 xong, Phase 7 (Admin UI) dở |
