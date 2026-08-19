---
name: payment-method-toggles-and-news-edit-lazy-load
description: "Bổ sung 2 ABP Setting bật/tắt 'Thanh toán tại quầy' và 'Thanh toán chuyển khoản' (page ZaloZns) + trả về Mini App calendar slot APIs; News EditModal lazy load ContentHtml để mở modal nhanh"
metadata: 
  node_type: memory
  type: project
  originSessionId: eccc6396-1889-4ab3-a51a-86af66f59b8e
---

# Payment Method Toggles + News EditModal Lazy Load (2026-05-22)

## 1. Payment Method Toggles cho Mini App

**Settings keys** (`ZaloPaymentSettingNames`):
- `Genora.Payment.IsPayAtCounterEnabled` — bool, default true
- `Genora.Payment.IsPayBankTransferEnabled` — bool, default true

Setting lưu per-tenant (qua `ISettingManager.SetForCurrentTenantAsync`) hoặc global (host).

**Page `/UpgradeSettings/ZaloZns`** (cshtml + .cshtml.cs):
- Thêm 2 form-switch (Bootstrap 5) `Thanh toán tại quầy` + `Thanh toán chuyển khoản` ngay dưới block thông tin ngân hàng.
- `OnGetAsync` fallback default `true` khi setting null/parse fail.

**APIs Mini App** trả về 2 trường bool:

`GET /api/mini-app/get-calendar-slots` → `MiniAppCalendarSlotDto`:
- `IsPayAtCounterEnabled`
- `IsPayBankTransferEnabled`

`GET /api/mini-app/get-calendar-slots/{id}` → `AppCalendarSlotDto` cũng trả thêm 2 field cùng tên.

**Service `MiniAppCalendarSlotService`** inject `ISettingProvider`, helper `GetPaymentToggleAsync()` đọc 2 setting với fallback `true`. Mỗi response (kể cả early-return BadRequest) đều set 2 cờ.

**Why:** Tenant cần khả năng tắt riêng từng hình thức thanh toán mà không phải sửa code; Mini App căn cứ 2 cờ này để ẩn/hiện UI option.

**How to apply:**
- Mini App đọc trực tiếp 2 field bool từ payload, không cần endpoint riêng.
- Khi cả 2 cùng false → Mini App nên báo "không có hình thức thanh toán khả dụng".
- Setting null = default true (fail-open) để không block khách khi tenant chưa cấu hình.

## 2. News EditModal mở chậm khi ContentHtml nặng

**Triệu chứng:** Click Edit ở môi trường staging/prod thấy modal mở rất lâu khi tin tức có ContentHtml lớn (ảnh base64 nhúng hoặc text dài). Local dev không thấy do latency thấp.

**Root cause:**
- `EditModal.cshtml.cs.OnGetAsync` load full `News` (gồm `ContentHtml`) → server render textarea với toàn bộ HTML → modal HTML response phải transfer cả MB qua mạng → Bootstrap modal phải parse + insert vào DOM trước khi show.
- Summernote init xảy ra **sau** khi modal đã có sẵn nội dung HTML lớn → re-paint/double-buffer chậm.

**Fix:**
1. `EditModal.cshtml.cs.OnGetAsync` set `News.ContentHtml = string.Empty` trước khi render → modal HTML nhẹ → mở ngay.
2. Thêm handler riêng `OnGetContentAsync` (Razor Pages convention `?handler=Content`) trả `JsonResult { contentHtml }`.
3. `Index.js` thêm `lazyLoadEditContent(modal)` chạy trong `editModal.onOpen`:
   - Lấy `Id` từ hidden input
   - Set placeholder loading vào summernote `<p><em>Đang tải nội dung…</em></p>`
   - AJAX GET `/AppNews/EditModal?id={id}&handler=Content`
   - Set `summernote('code', html)` khi resolved; fallback `$editor.val(html)` nếu summernote chưa init xong

**Endpoint format:** Razor Pages handler convention — `OnGet{Name}Async` matches `?handler={name}` (case-insensitive). Không cần thêm route.

**Why không paginate/strip image:** ContentHtml là single field, không có cấu trúc paginate; ảnh inline base64 là pattern hợp lệ với summernote. Lazy load tách network cost ra khỏi modal-open critical path là cách rẻ nhất.

**Trade-off:** User thấy modal mở ngay nhưng nội dung xuất hiện sau ~100-500ms. UX tốt hơn freeze cả vài giây khi click Edit.

## File map
- App: `AppServices/AppPayments/ZaloPaymentSettingNames.cs` (+2 hằng số)
- App: `AppServices/AppCalendarSlots/MiniAppCalendarSlotService.cs` (+inject `ISettingProvider`, helper `GetPaymentToggleAsync`, set cờ ở list/detail)
- App.Contracts: `AppDtos/AppCalendarSlots/MiniAppCalendarSlotDto.cs` + `AppCalendarSlotDto.cs` (+2 prop bool)
- Web: `Pages/UpgradeSettings/ZaloZns.cshtml(.cs)` (+2 form-switch + load/save setting)
- Web: `Pages/AppNews/EditModal.cshtml.cs` (xóa ContentHtml lúc render + handler `OnGetContentAsync`)
- Web: `Pages/AppNews/Index.js` (`lazyLoadEditContent` trong `editModal.onOpen`)

## Related
- [[project_payment_configuration]] — pattern cấu hình thanh toán (Bank QR)
- [[feedback_signalr_try_catch]] — pattern fallback cho external resource
