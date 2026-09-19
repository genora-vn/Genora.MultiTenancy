# MEMORY INDEX — Genora.MultiTenancy

> Index tổng hợp của project memory. Đây là bản **curated dễ đọc**.
> Nội dung nguyên văn (nguồn chân lý, zero-loss) nằm tại `.claude/memory/notes/`.
> Nguồn gốc: migrate từ `~\.claude\projects\D--Genora-...-Genora-MultiTenancy\memory\` (108 file).

## Điều hướng nhanh
- **HLG corrective full design audit2026-09-19 (mới nhất):** [note](memory/notes/project/project_hlg_corrective_design_audit_20260919.md) · [matrix/report](docs/HLG_FULL_DESIGN_AUDIT_20260919.md).31/31 pages×2; hierarchy/content/CMS/prizes/fulfillment; migration mới chưa apply;42App+3Domain+17Web+11JS pass; browser BLOCKED và3UNKNOWN.
- **HLG Admin Razor UI 2026-09-18:** [note](memory/notes/project/project_hlg_admin_razor_ui_20260918.md). Đủ 5 nhóm, menu/quyền/VI-EN; 13 Application + 7 JS tests, Web build pass; còn UAT tenant thật.
- **HL25 staging AgeGroup repair 2026-09-18:** [note](memory/notes/project/project_hl25_staging_agegroup_migration_fix_20260918.md). Migration mới 20260918093000_EnsureHl25ParticipantAgeGroup (DB HL25: DuocPhamHoaLinh; bỏ qua DB không có bảng HL25) sửa DB từng apply BirthDate cũ; giữ dữ liệu, thêm Unknown=0; chưa áp staging.
- **Hoa Linh Sales Excel money format 2026-09-18:** [note](memory/notes/project/project_hl_sales_excel_money_format_fix_20260918.md). 3 cột tiền dùng #,##0, bỏ dấu chấm cuối; 14 Application tests pass. User đã xác nhận download hoạt động.
- **Hoa Linh Sales Excel runtime fix 2026-09-18:** [note](memory/notes/project/project_hl_sales_excel_tenant_cookie_fix_20260918.md). Bỏ getTenantIdCookie không tồn tại, giữ same-origin cookie; 12 JS tests pass, gồm download thật.
- **DbMigrator HLG host seed lỗi 2026-09-18:** [note](memory/notes/project/project_hlg_host_seed_migration_failure_20260918.md). Host thiếu bảng HLG dù history đã ghi; sửa seed tenant-only; 3 Domain tests + DbMigrator build pass. Chưa sửa DB hoặc chạy lại migrate thật.
- **Hoa Linh Sales — Admin filters/Excel 2026-09-17 (mới nhất):** [note](memory/notes/project/project_hl_sales_admin_filters_excel_20260917.md). DB HoaLinhMienNam/schema HL, PointHistory/GiftExchanges/Orders; build + 14 Application/8 JS tests pass; chưa runtime UAT.
- **Hoa Linh 25 — UI + ảnh vòng quay 2026-09-14 (mới nhất):** [note UI/images](memory/notes/project/project_hl25_gift_images_ui_fixes_20260914.md). Dropdown explicit vi/en; callback raw row; WheelImageUrl; modal/upload/VNĐ; migration 20260914111213 chưa apply.
- **Hoa Linh 25 — cập nhật Admin 2026-09-14, trạng thái mới nhất:** [note triển khai](memory/notes/project/project_hl25_admin_update_20260914.md) · [bảng delta + kiểm tra](docs/HOALINH25_ADMIN_UPDATE_20260914.md). Tạo thiệp nhận lượt đầu/chia sẻ lượt hai; 32 tests; không migration mới.
- **Hoa Linh 25 — baseline và approach 2026-09-14:** [context review](memory/notes/project/project_hl25_context_review_20260914.md). Đối chiếu Git/source; khác biệt quy tắc cấp lượt, trạng thái commit và migration; chưa triển khai chức năng mới.
- Quy tắc làm việc & lessons learned → [RULES.md](RULES.md)
- Trạng thái từng module → [PROJECT_STATE.md](PROJECT_STATE.md)
- Việc đang làm dở → [ACTIVE_CONTEXT.md](ACTIVE_CONTEXT.md)
- Nhật ký task → [TASK_LOG.md](TASK_LOG.md)
- Kiến trúc → [architecture/](architecture/)
- Khởi động phiên mới → [LOAD_CONTEXT.md](LOAD_CONTEXT.md)
- Bàn giao → [handover/HANDOFF.md](handover/HANDOFF.md)
- Toàn bộ note gốc → [memory/notes/](memory/notes/)

---

## Feedback — quy tắc làm việc (18 note)
Nằm tại `memory/notes/feedback/`. Tổng hợp trong [RULES.md](RULES.md).

- ABP WithDetailsAsync load navigation props — `feedback_abp_with_details.md`
- SignalR broadcast trong try/catch rỗng — `feedback_signalr_try_catch.md`
- ABP ILogger không có LogWarning extension — `feedback_abp_ilogger.md`
- genora.excel.download dùng fetch+Blob — `feedback_excel_download.md`
- MARS + autoSave insert parent trước child — `feedback_mars_autosave_pattern.md`
- ABP Tenant/Host dual permission pattern — `feedback_abp_dual_permission_pattern.md`
- ABP Permission group gom vào group có sẵn — `feedback_abp_permission_group_pattern.md`
- HL Payment dùng ZaloPaymentSettingNames constants — `feedback_hl_payment_setting_names.md`
- ABP DataTables custom ajax cho List<T> — `feedback_datatables_custom_ajax.md`
- Permission Tenant RequireFeatures trên root + mọi child — `feedback_permission_require_features.md`
- Money input vi-VN patchMoneyValidator() — `feedback_money_input_validation.md`
- Cập nhật memory bắt buộc trước handoff — `feedback_memory_update_before_handoff.md`
- Disabled select phải kèm hidden input — `feedback_disabled_select_hidden_input.md`
- Email template Scriban `!= empty` → `!= null` — `email_template_fixes.md`
- EF migrations body rỗng khi Web lock dll — `feedback_ef_migration_dll_lock.md`
- Salon phone regex đầu 0 hoặc 84 — `feedback_salon_phone_regex_0_or_84.md`
- Application layer không dùng EF Core (AsyncExecuter) — `feedback_no_ef_in_application_layer.md`
- ABP internal AppService multi complex param + null validation — `feedback_appservice_multi_complex_param.md`
- HL dual permission + JSON array parse + DTO không JsonPropertyName — `feedback_hl_dual_permission_and_json_parse.md`

## Project — Golf core & MiniApp (33 note)
Nằm tại `memory/notes/project/`. Xem tóm tắt module trong [PROJECT_STATE.md](PROJECT_STATE.md).
Các chủ đề: MiniApp notifier/cancel-booking/itemId-null, multi-tenant DB routing, PaymentConfiguration,
Member/Guest pricing, Booking TotalAmount từ AppBookingPlayers, CalendarSlot pricing/available/deal filter,
PromotionPolicy, CustomerType original price, VietQR deeplink, Excel export, UI filter pattern,
prod antiforgery SSL incident, Serilog interceptor toggle, validate VGA code, AppCustomers permission leak.

## Project — Salon Beauty (17 note)
Stylist/Booking/Location/TimeSlot UI, TimeSlot capacity + peak hour, MARS fix, deposit + loyalty,
MiniApp payment endpoints, customer detail redesign, booking history + change stylist, ZBS + service review.
Bản implementation đầy đủ cũ: `memory/modules/salon-beauty/`.

## Project — Docs & Caddie (22 note)
Online docs site /Documents + seeder. Caddie: SRS 5 module, DB design 10 tables, Phase 1-7 complete,
UI fixes nhiều đợt, avatar refactor, CaddieFee + BookingDetails multi-caddie, booking gắn golf players.

## Project — Hoa Linh module (16 note)
BRD overview, data integration pattern (prefix AppHl, SyncLog), Phase 1-7 complete,
API client HoaLinhDms, Admin Portal UI, CRUD Orders/GiftExchanges, MiniApp APIs,
UrBox eVoucher, Zalo OA articles, customer registration upsert, campaign detail,
loyalty points redeem (FIFO ledger + expire worker), gift exchange status enum.
