---
name: salon-deposit-loyalty-feature
description: "Tính năng Quản lý nạp tiền + cấu hình quy đổi điểm cho Salon Beauty: Deposit (Pending→Success/Cancelled), BonusTier, ExchangeRate setting, ledger BalanceBefore/After, MiniApp loyalty-detail API"
metadata: 
  node_type: memory
  type: project
  originSessionId: eccc6396-1889-4ab3-a51a-86af66f59b8e
---

# Salon Deposit & Loyalty Config (2026-05-23)

## Tổng quan
Tính năng nạp tiền của Salon Beauty: nhân viên CMS tạo lệnh nạp Pending → admin duyệt → cộng điểm vào ví khách trong cùng UoW transactional. Có cấu hình tỷ lệ quy đổi (1P = ?đ) và các mốc tặng thêm điểm (bonus tier).

## Domain
- `SalonBeautyDepositTransaction` (`AppSalonBeautyDepositTransactions`): TransactionCode=`DEP{yyyyMMdd}{seq:D4}`, Amount, ExchangeRate, BasePoint, BonusPoint, TotalPoint, BonusTierId, PaymentMethod, Status (1=Pending, 2=Success, 3=Cancelled), ApprovedBy/At, CancelledBy/At/Reason. Navigation Customer.
- `SalonBeautyLoyaltyBonusTier` (`AppSalonBeautyLoyaltyBonusTiers`): Name, MinAmount, BonusPoint, IsActive, DisplayOrder.
- `SalonBeautyCustomerLoyaltyTransaction` (cập nhật): thêm `BalanceBefore`, `BalanceAfter` (audit ledger), `ReferenceType` (1=Deposit, 2=Booking, 99=Manual), `ReferenceId`.

Enums: `SalonBeautyLoyaltyEnums.cs` (Domain.Shared) — DepositStatus, DepositPaymentMethod, LoyaltyTransactionType, LoyaltyReferenceType.

## Setting
- `Genora.SalonBeauty.Loyalty.ExchangeRate` (default `1000`) — đăng ký trong `MultiTenancySettingDefinitionProvider`. Helper: `SalonBeautyLoyaltySettingNames.ExchangeRate`.

## App services (transactional)
- `SalonBeautyDepositAppService` — CRUD + `PreviewAsync(amount)` + `ApproveAsync(id)` + `CancelAsync(id, dto)`.
  - **ApproveAsync** mở `_uowManager.Begin(requiresNew: true, isTransactional: true)` để chạy: tìm/tạo balance → cộng `TotalPoint` → ghi 1 row ledger với `BalanceBefore/After` → đổi Status sang Success → `uow.CompleteAsync()`.
  - GenerateTransactionCodeAsync: query count theo prefix `DEP{yyyyMMdd}` rồi `+1`.
- `SalonBeautyLoyaltyConfigAppService` — Get/Update setting (per-tenant `SetForCurrentTenantAsync` hoặc global `SetGlobalAsync`).
- `SalonBeautyLoyaltyBonusTierAppService` — CRUD đơn giản.
- `MiniAppSalonBeautyLoyaltyAppService` — thêm `GetDetailMiniAppAsync(customerId, max)` trả `MiniAppCustomerLoyaltyDetailDto { CurrentPoint, RecentTransactions[] }`.

## Permissions (group `SalonBeauty` + `SalonBeautyHost`)
- `SalonBeautyDeposits` (Default/Create/Edit/Delete/**Approve/Cancel**) + Host counterpart, `RequireFeatures(SalonBeauty.Management)` cho Tenant root + mọi child.
- `SalonBeautyLoyaltyConfig` (Default/Edit) + Host counterpart.

## API endpoints
- Tenant CRUD đi qua `genora.multiTenancy.appServices.salonBeauties.salonBeautyDeposit` JS proxy.
- MiniApp: `GET /api/mini-app/salon-beauty/customers/{customerId}/loyalty` (balance), `GET /api/mini-app/salon-beauty/customers/{customerId}/loyalty-detail?maxResultCount=10` (detail).

## Web pages
- `/SalonBeautyDeposits` — Index + CreateModal (preview live qua `service.preview(amount)`) + DetailModal (xem chi tiết, badge status). Action column: View/Approve (chỉ khi status=1)/Cancel (prompt reason)/Delete.
- `/SalonBeautyLoyaltyConfig` — 1 trang gồm: form ExchangeRate (left) + DataTable BonusTiers (right) + Modal `BonusTierModal` (handler `OnGetAsync(Guid? id)` để dùng chung Create/Edit).

Menu: gắn vào group `MenuGroup.SalonBeauty` với 2 entry mới (Deposits, LoyaltyConfig).

## Notes & Gotchas
- ABP `[UnitOfWork(IsTransactional = true)]` (named arg) **không hợp lệ** trên runtime hiện tại — bỏ attribute, dùng ambient UoW của `ApplicationService`. **KHÔNG mở `_uowManager.Begin(requiresNew: true)`** cho ApproveAsync vì UoW lồng `requiresNew=true` tạo scope ISOLATED, dẫn tới các Insert/Update vào `AppSalonBeautyCustomerLoyaltyBalances`/`AppSalonBeautyCustomerLoyaltyTransactions` không commit hoặc không visible với caller — **bug thực tế:** tạo deposit thành công, Approve không cộng điểm + không ghi ledger. Fix bằng cách bỏ nested UoW.
- `GetListAsync` **KHÔNG dùng cross-aggregate LINQ join** giữa Deposit/Customer/Tier — multi-tenant DB filter làm join hang/treo (bug "list loading mãi"). Fix bằng load deposits trước → distinct CustomerIds/TierIds → 2 query Contains() → ToDictionary lookup in-memory.
- Razor Page modal: dùng `<abp-modal-footer buttons="@(AbpModalButtons.Cancel | AbpModalButtons.Save)"></abp-modal-footer>` — KHÔNG dùng `buttons="Cancel,Save"` (gây lỗi parse cshtml CS1002/CS1513/CS0103 Save).
- Ledger entry phải có `BalanceBefore = balance.CurrentPoint trước khi cộng` và `BalanceAfter = sau khi cộng` (audit invariant: BalanceAfter = BalanceBefore + Point).
- `DEP{date}{seq}` count theo prefix có thể race condition giữa các transaction concurrent — chấp nhận vì là CMS, không phải hot path.
- ExchangeRate input: `min="1" max="9999" step="1"` (không phải `step="100"` — gây lỗi HTML5 "valid values are 901 and 1001"). UI: card ExchangeRate `col-md-3`, BonusTiers `col-md-9` cho cân đối.
- Customer dropdown trên list page + Create/Edit modal dùng **select2** (lib có sẵn ở `wwwroot/libs/select2`) cho UX search-as-you-type. Modal phải set `dropdownParent: $('.modal-content')` để không bị clip.
- Modal redesign theo style `SalonBeautyServiceCategories`: `service-modal` > `service-modal-header` (icon + h3 title + p sub) + `service-modal-body` (col-12 layout) + `service-modal-footer` (Detail thì Close căn phải qua `justify-content-end`; Create/Edit thì Cancel + Save căn trái mặc định).
- **DataTables visible callback** PHẢI guard `data && data.record` — ABP `_createButtonDropdown` gọi `visible(undefined)` để pre-check trước khi có row, code cũ `data.record.status` crash → "Cannot read properties of undefined (reading 'status')" + danh sách kẹt loading. Pattern an toàn: `function (data) { if (!cond) return false; var s = getRowStatus(data); if (s === null) return true; return s === 1; }`.
- Format MinAmount/Amount: input `type="text" inputmode="numeric"` + lib JS strip non-digits + thousand separator on input/keyup/paste, strip lại trước submit để bind `decimal`. Smart parse `"5000000.00"` → `"5000000"` (decimal từ DB) + `"5,000,000"` → `"5000000"` (formatted user input). Wrap trong `service-input-addon` + suffix VND giống `SalonBeautyServices` Price.
- Đơn vị điểm: dùng key locale `SalonBeautyDeposits:PointUnit` = "điểm" thay vì hardcode "P". Toàn bộ render điểm phải `.toLocaleString('vi-VN')` để format thousand separator (5,500 điểm).
- Deposit có 4 modal: `CreateModal`, `EditModal` (chỉ enable khi status=Pending, không cho đổi Customer — readonly), `DetailModal` (service-modal style + footer Close căn phải), tất cả share inline JS `__initDepositForm` để init select2 + format amount + recalc preview (col-md-6 grid 2x2: Rate+Base, Bonus+Total).
- File map mới:
  - Domain.Shared: `Enums/SalonBeautyLoyaltyEnums.cs`
  - Domain: `DomainModels/AppSalonBeauty/SalonBeautyDepositTransaction/`, `SalonBeautyLoyaltyBonusTier/`, update `SalonBeautyCustomerLoyaltyTransaction.cs`
  - EF: 2 entity mappings + 2 DbSet trong `MultiTenancyDbContext.cs`. Migration: `20260523040926_Add_SalonBeautyDeposit_LoyaltyBonusTier`.
  - Application.Contracts: `AppDtos/SalonBeauties/SalonBeautyDeposits/*`, `SalonBeautyLoyaltyBonusTiers/*`, `SalonBeautyLoyaltyConfigs/*`
  - Application: `AppServices/SalonBeauties/SalonBeautyDepositAppService.cs`, `SalonBeautyLoyaltyConfigAppService.cs`, `SalonBeautyLoyaltyBonusTierAppService.cs`, `SalonBeautyLoyaltySettingNames.cs`. Update `MiniAppSalonBeautyLoyaltyAppService.cs` + automapper profile.
  - HttpApi: `SalonBeautyMiniAppController.cs` thêm endpoint `loyalty-detail`.
  - Web: `Pages/SalonBeautyDeposits/{Index,CreateModal,DetailModal}.cshtml(.cs) + index.js`, `Pages/SalonBeautyLoyaltyConfig/{Index,BonusTierModal}.cshtml(.cs) + index.js`. Menu: thêm 2 entry trong `groupSalonBeauty`.
  - Localization vi.json: thêm khóa Permission/Menu/SalonBeautyDeposits/SalonBeautyLoyaltyConfig.

## Related
- [[feedback_mars_autosave_pattern]] — pattern insert parent rồi child với autoSave:true
- [[project_salon_booking_mars_fix]] — pattern transactional cho SalonBeauty
- [[feedback_abp_dual_permission_pattern]] — ICurrentTenant + helper map permission
- [[feedback_permission_require_features]] — Tenant phải RequireFeatures trên root + child

## Update 2026-05-23 — Hoàn thiện UX deposit modals

### Bug đã fix
1. **Save không lưu được** trên CreateModal/EditModal: form wrap `<abp-modal>` (parent) nên ABP ModalManager không hook submit. Fix: tự bind submit handler trong `index.js` (delegate trên `.deposit-form`), gửi qua `abp.ajax` rồi tự đóng modal qua `bootstrap.Modal.getOrCreateInstance`.
2. **EditModal hiển thị `5000000,00` thay vì `5,000,000`**: inline script `__initDepositForm` chỉ được định nghĩa trong CreateModal, mở Edit độc lập không có function đó. Fix: chuyển toàn bộ logic format/preview/select2 từ inline script sang `index.js` (load sẵn ở Index, áp dụng cho cả 2 modal qua `shown.bs.modal` delegate). `stripDigits` được sửa để bóc cả phần thập phân (`.00` hoặc `,00`) trước khi format.
3. **Block "Xem trước số điểm" không hiện**: hệ quả của #2 — không gọi `recalcPreview` khi mở Edit do thiếu `__initDepositForm`. Fix theo cùng giải pháp ở #2.
4. **Cancel reason dùng `window.prompt`**: thay bằng Bootstrap modal `#DepositCancelModal` ngay trong `Index.cshtml` với header (title "Hủy lệnh nạp tiền" + sub "Vui lòng nhập lý do hủy"), body (textarea required + validation "Vui lòng nhập lý do hủy") và footer (Hủy bỏ + Xác nhận hủy đỏ). Pattern tham chiếu: `SalonBeautyBookings/Index.cshtml#ListCancelModal`.
5. **Customer select (filter + CreateModal) thấp hơn input khác**: select2 default height ~30px; các form-control filter cao 45px (đồng bộ với button Search). Fix: CSS rule `.deposit-filter-row .select2-container--default .select2-selection--single { height: 45px; display:flex; align-items:center; }` + tương tự cho `.modal .select2-container...`. Cũng force `.deposit-filter-row .form-control, .form-select { height: 45px }` cho đồng nhất.

### Pattern submit cho form wrap `<abp-modal>`
Khi form là parent của `<abp-modal>` (không phải child của `.modal-content`), ABP ModalManager không tự bắt submit. Phải tự bind:
```js
$(document).on('click', '.deposit-form .service-btn-primary[type="submit"]', function (e) {
    e.preventDefault(); e.stopImmediatePropagation();
    return submitDepositForm($(this).closest('form'));
});
$(document).on('submit', '.deposit-form', ...);
```
Submit handler post `FormData` qua `abp.ajax`, sau đó đóng modal qua `bootstrap.Modal.getOrCreateInstance($modal[0]).hide()` và reload datatable.

### DOM traversal direction (form wrap modal)
Form WRAP `.modal` → tìm modal từ form: `$form.find('.modal').first()` (DOWN). Tìm form từ modal: `$modal.closest('form.deposit-form')` (UP). Cả `shown.bs.modal` handler và submit handler đều phải tuân theo direction này, sai chiều sẽ không tìm được element.

### Unsaved-changes guard popup sau khi Save (gotcha)
ABP có "unsaved changes" guard hook `hide.bs.modal` của modal — nếu form có dirty fields (value khác defaultValue) thì khi `bootstrap.Modal.hide()` được gọi, ABP sẽ popup `abp.message.confirm("Bạn có những thay đổi chưa được lưu", "Bạn có chắc không ?")`. Sau khi save thành công, dirty flags vẫn còn → popup hiện không cần thiết.

**Fix**: trước khi gọi `modal.hide()`, làm 2 việc:
1. `markDepositFormClean($form)` — set `defaultValue = value` cho mọi `input/textarea/select` (kể cả `defaultChecked`/`defaultSelected`) → form không còn dirty
2. `suppressUnsavedChangesConfirmTemporarily(3000)` — tạm override `abp.message.confirm` 3s, intercept message chứa "chưa được lưu/unsaved" hoặc title "Bạn có chắc/are you sure" và auto-resolve `true`, sau 3s restore lại

Pattern gốc copy từ `Pages/SalonBeautyServices/index.js` (`markSalonFormAsClean` + `suppressUnsavedChangesConfirmTemporarily`).

### File map update
- Web: `Pages/SalonBeautyDeposits/index.js` — toàn bộ logic format/preview/select2/submit/cancel-modal
- Web: `Pages/SalonBeautyDeposits/Index.cshtml` — thêm `#DepositCancelModal` + CSS height đồng bộ 45px
- Web: `Pages/SalonBeautyDeposits/CreateModal.cshtml` — bỏ inline `<script>`, đổi class `service-category-form` → `deposit-form`
- Web: `Pages/SalonBeautyDeposits/EditModal.cshtml` — bỏ inline `<script>`, đổi class
- Localization vi.json: thêm `CancelModalTitle`, `CancelModalSubTitle`, `CancelReasonPlaceholder`, `CancelReasonRequired`, `CancelConfirm`, `CancelFailed`
