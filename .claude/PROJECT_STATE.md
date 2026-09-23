# PROJECT STATE — Genora.MultiTenancy

## Gateway production rate-limit — 2026-09-23 ✅ HOẠT ĐỘNG

- Rate-limit mini-app tenant trên production ĐÃ CHẠY (burst test → 200 + 429 `Hl25:RateLimitExceeded`). Luồng: **Apache (XAMPP) :443 → YARP gateway `127.0.0.1:5088` → ABP `8868` (guard)**.
- **CỰC KỲ QUAN TRỌNG:** ingress thật trên production là **XAMPP Apache** (giữ `103.157.218.191:443`), KHÔNG phải IIS. Cấu hình route/rate-limit nằm ở `C:\xampp\apache\conf\extra\httpd-vhost.conf` (2 vhost cho `duocpham-hoalinh`/HL25 và `hoalinh`/HLG). Site IIS `Genora.Ingress.Production` vô tác dụng.
- Gateway + ABP đều env=Production (dùng `appsettings.Production.json`). Guard `TenantGatewayGuard.Enabled=true`. Bộ file chuẩn (đã commit, secret sanitize): `docs/tenant-gateway/`. Chi tiết: [note](memory/notes/project/project_gateway_production_apache_ingress_20260923.md).
- Còn lại (vận hành): trả `PermitLimit` hl25 về 500 + recycle gateway; reload Apache cho reject-rule 404; firewall 5088 chỉ localhost; gỡ DIAG_ALL khỏi IIS; chưa load-test SLA thực tế.

## HLG staging recovery — 2026-09-23 (current)

- Host HLG schema/history drift repaired on staging: 17 physical HLG tables, six HLG migrations including `20260919112304_AddHlgDesignContent`, five baseline history rows backed up in `dbo.HlgMigrationHistoryRepair_20260923`. Guarded repair SQL + migration preflight are in source. Host EF `--no-build` is up to date; no tenant DB writes.
- Tenant `HoaLinhMienNam` design-content migration also applied through explicit `--connection`; now 17 HLG tables/six HLG history rows, with `PharmacyCode` present. `Hlg.Management` is enabled and admin HLG roots are granted.
- Menu/pages exist in current source but are absent from deployed Web on both staging hosts (HLG JS/routes 404, HL25 JS 200). Local Web Release artifact `artifacts/hlg-web-staging-20260923` contains the missing files; deploy to both IIS sites and verify authenticated host/tenant menu. Web build 0 errors and 17 HLG Web tests pass; no IIS deployment/live signed-in UAT in this task. [Recovery](../docs/HLG_STAGING_RECOVERY_20260923.md) · [note](memory/notes/project/project_hlg_staging_schema_menu_recovery_20260923.md).

## HLG verification follow-up — 2026-09-19 (mới nhất)

- Corrective source đã commit trong HEAD `d4f67f9`; follow-up chưa commit fix session shipping ownership/finished guard và bổ sung URL validation cho các image field Admin còn thiếu.
- Web build0 errors;46 Application +3 Domain +17 Web +11 JS pass; EF model không pending.
- Browser UAT vẫn BLOCKED do không có browser surface (`apps=[]`, `browsers=[]`, iab unavailable). Migration AddHlgDesignContent vẫn chưa apply;3 UNKNOWN không đổi.

## HLG corrective design audit — 2026-09-19 (mới nhất)

- Đã inspect trực quan31/31 trang PDF ở cả2 lượt;60 screen/component/state;54 nhóm dữ liệu.34 nhóm CMS/cần làm rõ:31 COVERED,0 PARTIAL,0 MISSING,3 UNKNOWN. Coverage source không phải nghiệm thu browser.
- Đã bổ sung Ngành hàng→Nhãn hàng→Sản phẩm, nội dung/FAQ/media/video/related/CTA; CMS Home; game banner/badge; ranking theo game/cơ cấu giải/công bố người trúng; fulfillment; Users read-only chi tiết; PharmacyCode riêng HLG, retailer3, progress/game-history API. Giữ API Mini cũ.
- Migration mới `20260919112304_AddHlgDesignContent` + SQL idempotent:4 bảng/6 cột nullable HLG; review additive; EF no pending model changes; **chưa apply DB**.
- Web build0 errors;42 Application +3 Domain +17 Web +11 JS tests pass.13 proxy names xác minh bằng generator ABP đang cài; chưa chạy lại live proxy HTTP trong phiên corrective.
- Browser UAT BLOCKED: browser tool báo không có browser, list=[]; chưa có thao tác tenant CRUD. Không suy ra DB kết nối lỗi. Ba UNKNOWN: vòng quay; tự chọn/trao giải và xử lý hòa; thời điểm cấp/trừ quà. Công bố winner thủ công không tự tạo đơn giao quà.
- Bước tiếp: review/apply migration đúng workflow multi-database, cấu hình catalog thật/quyền Content và chạy UAT tenant. Xem [audit](docs/HLG_FULL_DESIGN_AUDIT_20260919.md), [note](memory/notes/project/project_hlg_corrective_design_audit_20260919.md).


> Trạng thái tổng hợp của từng module, tổng hợp từ các note `*_phase*_complete`, `*_progress`,
> `*_complete` trong `.claude/memory/notes/project/`. Cập nhật khi hoàn thành mốc lớn.

## Tổng quan
- **Gateway IIS audit 2026-09-21:** source vàmẫuARR+key/configgenerator đã sửa;60gateway/42Webtests,build/publishPASS. Stagingdeploychưahoạtđộngtheologuser; bindingAppPool/cutoverUATcầnxácminh, chưaproduction-ready. [Note](memory/notes/project/project_gateway_iis_staging_fix_20260921.md).
- **Tên project gateway hiện tại (2026-09-21):** `Genora.MultiTenancy.Gateway` + `Genora.MultiTenancy.Gateway.Tests`; solution/namespace/IIS/runbook đồng bộ, publish +42 tests PASS. Cấu hình nhiều tenant giữ nguyên. [Note](memory/notes/project/project_gateway_rename_20260921.md).
- **Gateway multi-tenant 2026-09-21:** source/config/runbook staging ready; HL25=500 / HLG=300 theo tenantGUID, generic opt-in origin guard. Build +42 gateway/40 Web/12 Node tests PASS; không migration. Chưa deploy/SQLloadUAT, IISorigin staging chưa được cung cấp. [Note](memory/notes/project/project_multi_tenant_yarp_gateway_20260921.md).
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

## Module: Hoa Linh Sales (Hoa Linh Gắn Kết / Dược phẩm) — HOÀN THÀNH Phase 1-7
- **Định danh:** DB `HoaLinhMienNam`, schema `HL`; khác HL25/HLG.
- **Admin 2026-09-17:** sửa lọc ngày PointHistory, thêm lọc ngày GiftExchanges, Excel theo bộ lọc cho 3 trang PointHistory/GiftExchanges/Orders. Web build + 14 Application/8 JS tests pass; chưa UAT runtime/deploy; không migration mới.
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
- **Gateway2026-09-20 (`hotfix/20260920`): source ready, deployment pending.** Separate YARP with shared500RPS quota and opt-in ABP guard; keep tenant hostname. Gateway/Web build PASS;19 gateway +24 Web +7 Node tests PASS. No migration/business API change; no IIS/Ocelot UAT or SQL-backed250–500RPS benchmark. [Note](memory/notes/project/project_hl25_yarp_gateway_20260920.md).
- **Performance 2026-09-20 (`hotfix/20260920`):** cache 4 read APIs theo tenant 20 phút + after-commit invalidation; single-process stampede protection. Migration 20260920100056 thêm 3 index chưa apply. 81 .NET + 3 JS tests pass; chưa browser/load UAT và chưa xác nhận capacity 1.000 CCU. [Note](memory/notes/project/project_hl25_cache_indexes_20260920.md).
- **Staging fix 2026-09-18:** corrective migration 20260918093000_EnsureHl25ParticipantAgeGroup (DB HL25: DuocPhamHoaLinh; bỏ qua DB không có bảng HL25) bổ sung AgeGroup cho DB áp migration HL25 cũ còn BirthDate. EF build/script/no model change checked; chưa apply target.
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
- **DbMigrator fix 2026-09-18:** sample seeder bỏ qua host, chỉ seed tenant bật Hlg.Management và scope đúng tenant; 3 Domain tests/DbMigrator build pass. Host history/schema HLG drift đã xác nhận, chưa repair DB hoặc chạy lại migrate thật.
- **Merge 2026-09-16:** đã merge từ `feature/hoalinh-gamification` (commit `b507697`) vào `feature/dev-hoalinh-gamification`, fix 4 conflict files, build thành công
- **Backend mini-app HOÀN TẤT 100%** — ~24 endpoint theo contract
- **Phase 0-6 xong:** Auth+Profile, Knowledge base, Games engine, Rewards & Shipping, Ranking, Live-feed SignalR
- **Phase 7 (Admin Razor UI), 2026-09-18:** đã triển khai đủ 5 nhóm và menu/quyền/VI-EN; Web build + 13 Application/7 JS tests pass. Còn UAT với tenant thật; xem note project_hlg_admin_razor_ui_20260918.
- **PDF audit 2026-09-19:** kiểm tra trực quan đủ 31/31 trang; game schedule/status được enforce server-side, profile accuracy lấy từ completed sessions, ranking không trả event hết hạn, `VgaCode` thêm optional vào profile contract. Solution build + 13 backend + 7 JS pass; không migration.
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
| Hoa Linh Sales | ✅ Phase 1-7 + Admin filters/Excel | DB HoaLinhMienNam/schema HL; cập nhật 17/09 chưa UAT runtime |
| Documents | ✅ Xong | seeder 11 section |
| Hoa Linh Gamification (HLG) | 🔄 Đã merge vào dev | merge 2026-09-16, Phase 0-6 xong, Phase 7 (Admin UI) dở |
