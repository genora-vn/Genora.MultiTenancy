# RULES — Coding Conventions & Lessons Learned

> Tổng hợp từ 18 note `feedback_*` trong `.claude/memory/notes/feedback/`.
> Mỗi quy tắc kèm tên file gốc để tra cứu chi tiết. Đây là "phải nhớ" khi code trên repo này.

## ABP Framework — Data & Domain
- **WithDetailsAsync để load navigation props.** `GetAsync` không eager-load `Items`; dùng `WithDetailsAsync` khi cần child collection. → `feedback_abp_with_details.md`
- **Application layer KHÔNG dùng EF Core trực tiếp.** Count/ToList/FirstOrDefault qua `AsyncExecuter`. Tạo entity `Entity<Guid>` qua constructor. → `feedback_no_ef_in_application_layer.md`
- **EF migrations rỗng khi Web lock dll.** Kill Web process + clean `bin/obj` trước khi `migrations add`. → `feedback_ef_migration_dll_lock.md`
- **MARS + autoSave:** insert parent trước, child sau (qua repo). Không cascade insert / không SaveChanges một lần cho cả cây. → `feedback_mars_autosave_pattern.md`

## ABP Framework — Permission & Feature (Multi-tenant)
- **Dual permission pattern (Tenant/Host):** inject `ICurrentTenant` + helper `P()`, KHÔNG dùng `[Authorize]` cứng. → `feedback_abp_dual_permission_pattern.md`
- **Permission group:** gom vào group có sẵn, không tạo group riêng cho tính năng nhỏ. → `feedback_abp_permission_group_pattern.md`
- **RequireFeatures phải đặt trên root + MỌI child.** Thiếu 1 level là permission vẫn hiện; menu phải check feature trước. → `feedback_permission_require_features.md`
- **Internal AppService:** `[RemoteService(false)]` khi >1 complex param; `[DisableValidation]` + default=null khi param reference nhận null. → `feedback_appservice_multi_complex_param.md`

## Logging & Realtime
- **ABP ILogger không có `LogWarning` extension.** Dùng `Logger.LogException` hoặc catch rỗng phù hợp. → `feedback_abp_ilogger.md`
- **SignalR broadcast bọc try/catch rỗng.** Lỗi broadcast không được làm fail luồng đặt hàng chính. → `feedback_signalr_try_catch.md`

## Frontend / UI (Razor + JS)
- **Excel download dùng fetch+Blob+a.click().** `window.location.href` không trigger download với `genora.excel.download`. → `feedback_excel_download.md`
- **DataTables custom ajax cho `List<T>`.** `createAjax` chỉ dùng cho `PagedResultDto`; array thuần cần custom ajax. → `feedback_datatables_custom_ajax.md`
- **Money input vi-VN:** gọi `patchMoneyValidator()` để override `$.validator` number/range, strip dấu chấm trước validate. → `feedback_money_input_validation.md`
- **Disabled select phải kèm hidden input.** `<select disabled>` không POST value; thêm hidden + override phía server. → `feedback_disabled_select_hidden_input.md`

## Hoa Linh / Salon specifics
- **HL Payment dùng `ZaloPaymentSettingNames` constants**, không string cứng (gây "Undefined setting"). → `feedback_hl_payment_setting_names.md`
- **HL dual permission + JSON array parse:** Host 403 fix bằng `P()`; array wrap qua `DeserializeSmartResponse`; DTO dựa `SnakeCaseLower` policy (không cần `JsonPropertyName`). → `feedback_hl_dual_permission_and_json_parse.md`
- **Salon phone regex:** đầu 0 hoặc 84, pattern `^(0\d{9,10}|84\d{9,10})$`, maxlength 13. Sửa đồng bộ DTO + cshtml + JS + server. → `feedback_salon_phone_regex_0_or_84.md`

## Email templates
- **Scriban không hỗ trợ `empty`.** Đổi `!= empty` → `!= null`; pass object thật (không null) vào template. → `email_template_fixes.md`

## Quy trình / Handoff (BẮT BUỘC)
- **Cập nhật memory trước khi handoff.** Lưu note chi tiết + cập nhật `MEMORY.md` index TRƯỚC khi chuyển sang task tiếp theo. → `feedback_memory_update_before_handoff.md`
- **Parked branch phải đăng ký con trỏ trên `dev`.** Memory trong `.claude/` là branch-local → task đang tạm dừng trên feature branch CHƯA merge sẽ vô hình từ các nhánh khác. Khi tạm dừng (park) một feature branch: (1) giữ memory chi tiết TRÊN chính nhánh đó (single source of truth: `HANDOFF.md` + `architecture/` + `PROJECT_STATE.md`); (2) thêm 1 dòng vào bảng "⛔ Task tạm dừng (parked branches)" trong `ACTIVE_CONTEXT.md` **trên `dev`** (nhánh | HEAD commit | trạng thái 1 dòng | trỏ tới chi tiết). KHÔNG copy full memory của nhánh parked về `dev` (gây sai lệch "đã xong" + conflict khi merge thật).

---
Ngoài ra, các quy tắc ở CLAUDE.md gốc vẫn áp dụng: ưu tiên AppService thay vì logic ở Controller;
kiểm tra `Domain.Shared` (Constants/Enums) trước khi tạo Entity; mọi method AppService là `Task` (async).
