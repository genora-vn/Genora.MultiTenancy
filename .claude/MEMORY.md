# MEMORY INDEX — Genora.MultiTenancy

> Index tổng hợp của project memory. Đây là bản **curated dễ đọc**.
> Nội dung nguyên văn (nguồn chân lý, zero-loss) nằm tại `.claude/memory/notes/`.
> Nguồn gốc: migrate từ `~\.claude\projects\D--Genora-...-Genora-MultiTenancy\memory\` (108 file).

## Điều hướng nhanh
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
