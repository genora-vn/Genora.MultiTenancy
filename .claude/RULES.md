# RULES — Coding Conventions & Lessons Learned

> Tổng hợp từ 20 note `feedback_*` trong `.claude/memory/notes/feedback/`.
> Mỗi quy tắc kèm tên file gốc để tra cứu chi tiết. Đây là "phải nhớ" khi code trên repo này.

## Memory System & Context Recovery (BẮT BUỘC)

### Session Startup Rule
- Mỗi session mới PHẢI nạp memory system trước khi phân tích hoặc code.
- Ưu tiên đọc theo thứ tự:
  1. `CLAUDE.md`
  2. `RULES.md`
  3. `ACTIVE_CONTEXT.md`
  4. `PROJECT_STATE.md`
  5. `MEMORY.md`
  6. `TASK_LOG.md`
- Nếu có thư mục handoff/task thì đọc thêm:
  - `.claude/tasks/*`
  - `.claude/memory/*`
  - các file handoff được tham chiếu trong `ACTIVE_CONTEXT.md`.

### Context Recovery Rule
- Trước khi kết luận:
  - không có task
  - thiếu context
  - không hiểu yêu cầu
  - cần user nhắc lại task
- PHẢI thực hiện:
  1. Đọc lại `ACTIVE_CONTEXT.md`
  2. Đọc lại `PROJECT_STATE.md`
  3. Đọc lại `MEMORY.md`
  4. Đọc lại `TASK_LOG.md`
  5. Đọc lại conversation hiện tại
  6. Tìm file task hoặc handoff liên quan
- Nếu task tồn tại trong bất kỳ nguồn nào ở trên thì tiếp tục thực hiện.
- Không được yêu cầu user mô tả lại task đã tồn tại trong memory system hoặc conversation hiện tại.

### Plan → Build Rule
- Khi user đã approve plan:
  - Không hỏi lại yêu cầu.
  - Không yêu cầu user gửi lại task.
  - Không quay về chế độ phân tích tổng quát.
- Plan đã được approve trở thành working context cho implementation.
- Nếu thiếu context trong quá trình build phải đọc lại memory system trước khi hỏi user.

### Build Mode Rule
- Khi đang ở chế độ build:
  - Ưu tiên đọc code hiện tại trước khi đặt câu hỏi.
  - Ưu tiên đọc memory system trước khi đặt câu hỏi.
  - Ưu tiên đọc task file trước khi đặt câu hỏi.
- Chỉ được dừng để hỏi user nếu:
  - Thiếu quyền truy cập.
  - Thiếu thông tin không tồn tại trong repository.
  - Có nhiều phương án nghiệp vụ khác nhau và không thể tự xác định.

### Task File Rule
- Với task lớn hoặc kéo dài nhiều session:
  - Tạo file task trong `.claude/tasks/`.
  - Task file là nguồn context chính của task.
  - Không yêu cầu user copy lại toàn bộ task nếu task file đã tồn tại.

### Branch Awareness Rule
- Trước khi thực hiện thay đổi:
  - Kiểm tra branch hiện tại.
  - Kiểm tra trạng thái git.
  - Kiểm tra task đang active trong memory.
- Không tự ý tạo branch mới nếu user đã chỉ định branch làm việc.

### Completion & Memory Update Rule
- Trước khi kết thúc task:
  - Cập nhật `ACTIVE_CONTEXT.md`.
  - Cập nhật `PROJECT_STATE.md`.
  - Cập nhật `TASK_LOG.md`.
- Nếu có bài học mới:
  - Tạo note mới trong memory system.
  - Cập nhật index trong `MEMORY.md`.
- Không handoff task khi memory chưa được cập nhật đầy đủ.

### Scope Protection Rule
- Chỉ sửa các file liên quan trực tiếp đến task.
- Không refactor ngoài phạm vi task nếu không cần thiết.
- Nếu phát hiện vấn đề khác:
  - Ghi nhận vào memory hoặc task log.
  - Không tự ý mở rộng phạm vi implementation.

### Repository Investigation Rule
- Trước khi tạo mới:
  - DTO
  - Enum
  - Constant
  - Repository method
  - Domain Service
  - Application Service
- Phải tìm và tái sử dụng implementation hiện có nếu phù hợp.
- Ưu tiên consistency với kiến trúc hiện tại của repository.

### Handoff Rule
- Khi chuẩn bị handoff:
  - Ghi rõ trạng thái hiện tại.
  - Ghi rõ việc đã hoàn thành.
  - Ghi rõ việc còn lại.
  - Ghi rõ branch đang làm việc.
  - Ghi rõ các file đã sửa.
- Mục tiêu là session mới có thể tiếp tục công việc mà không cần user nhắc lại bối cảnh.

## ABP Framework — Data & Domain
- **WithDetailsAsync để load navigation props.** `GetAsync` không eager-load `Items`; dùng `WithDetailsAsync` khi cần child collection. → `feedback_abp_with_details.md`
- **Application layer KHÔNG dùng EF Core trực tiếp.** Count/ToList/FirstOrDefault qua `AsyncExecuter`. Tạo entity `Entity<Guid>` qua constructor. → `feedback_no_ef_in_application_layer.md`
- **EF migrations rỗng khi Web lock dll.** Kill Web process + clean `bin/obj` trước khi `migrations add`. → `feedback_ef_migration_dll_lock.md`
- **DbMigrator kiểm tra database đích trước; đối chiếu history với schema từng tenant.** Chỉ dùng `master` khi database thật sự thiếu (SQL 4060/911). Reset history chỉ với preflight chính xác, backup transaction và dry-run mặc định. → `feedback_dbmigrator_target_first_schema_history_drift.md`
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

## Deployment / Production infra
- **Production ingress là XAMPP Apache, KHÔNG phải IIS.** Cổng `103.157.218.191:443` do `C:\xampp\apache\bin\httpd.exe` giữ; vhost `*.genora.vn` (`httpd-vhost.conf`) proxy thẳng vào ABP `.174:8868`. Trước khi debug URL Rewrite/ARR trên IIS, LUÔN kiểm `Get-NetTCPConnection -LocalPort 443 -State Listen` (PID 4/System=IIS; httpd.exe=Apache). Muốn chèn tầng gateway rate-limit cho mini-app phải sửa **vhost Apache** (`ProxyPass /api/mini-app/hl25 http://127.0.0.1:5088/...`), đặt vhost `ServerName` cụ thể TRƯỚC vhost wildcard. Dấu nhận biết luồng: response có `X-Powered-By: ASP.NET` = qua gateway/Apache; không có = ABP trực tiếp (ABP strip header này). → `feedback_prod_apache_ingress_not_iis.md`
- **Gateway + ABP trên production chạy env=Production** → config phải nằm trong `appsettings.Production.json` (không phải Staging). Gateway đọc config lúc startup → đổi `PermitLimit` phải recycle app pool `Genora.Tenant.Gateway`; quota process-local nên giữ MaxProcesses=1. → `project_gateway_production_apache_ingress_20260923.md`

---
Ngoài ra, các quy tắc ở CLAUDE.md gốc vẫn áp dụng: ưu tiên AppService thay vì logic ở Controller;
kiểm tra `Domain.Shared` (Constants/Enums) trước khi tạo Entity; mọi method AppService là `Task` (async).
