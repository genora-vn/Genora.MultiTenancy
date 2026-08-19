---
name: project_hl_loyalty_points_redeem
description: HL Loyalty đổi điểm/tiền từ chiến dịch + ledger FIFO + worker hết hạn + admin Lịch sử điểm thưởng
metadata: 
  node_type: memory
  type: project
  originSessionId: 797b3657-ed24-4a86-ae64-e7834aac2834
---

Tính năng Loyalty đổi điểm/tiền từ chiến dịch HL DMS (branch dev, 2026-07). Migration `20260709064009_Add_HlPoints_And_BonusAmount` đã apply.

**Quyết định nghiệp vụ (đã chốt):** đổi bằng điểm HOẶC tiền (khách chọn); mỗi (khách+chiến dịch) chỉ đổi 1 lần; ledger theo lô FIFO; **tách 2 quỹ**: `dbo.AppCustomers.BonusPoint` (điểm) + cột mới `BonusAmount` (tiền); worker quét hết hạn mỗi giờ.

**Enums (Domain.Shared/Enums):** `HlPointTransactionType` (Earn=1/Spend=2/Expire=3/Adjust=4), `HlPointUnit` (Point=1/Amount=2), `HlPointBatchStatus` (Active=1/Exhausted=2/Expired=3).

**Entities (Domain/DomainModels/AppHlPoints, schema HL):**
- `HlPointBatch` (AppHlPointBatches): lô đã đổi. BatchCode `PB{yyMMdd}{D4}`, Unit, SourceValue/ConvertedValue/RemainingValue, Status, ExchangedAt, ExpireDate=+1 năm. Unique index (TenantId,CustomerCode,CampaignCode) chặn đổi trùng DB-level + (TenantId,Status,ExpireDate) cho worker.
- `HlPointTransaction` (AppHlPointTransactions): sổ cái. Type/Unit/Value(±)/BalancePointAfter/BalanceAmountAfter/BatchId/RefCode/Description.
- Customer thêm `BonusAmount` (decimal).
- DbSet + config trong MultiTenancyDbContext + MultiTenancyDbContextModelCreatingExtensionsHoaLinh.

**Service `HlPointAppService` ([RemoteService(false)]+[DisableValidation]):**
- `RedeemFromCampaignAsync`: UoW transactional; chặn trùng; gọi `_hlApi.GetCampaignDetailAsync(custCode)` tìm theo CampaignCode; SourceValue theo Unit (accumulatedPoints/accumulatedSales); tạo lô +1 năm; cộng BonusPoint HOẶC BonusAmount; ghi txn Earn.
- **Tỉ lệ quy đổi (2026-07 update):** `ConvertedValue = Round(SourceValue * rate, 2)` với rate từ `HlLoyaltyOptions` (section HlLoyalty): PointRate (đổi điểm) / AmountRate (đổi tiền), mặc định 1 = giữ nguyên. Options bind trong MultiTenancyApplicationModule, inject IOptionsSnapshot vào service.
- **Redeem theo voucherType (2026-07 update):** RedeemFromCampaignAsync KHÔNG còn theo input.Unit (deprecated) mà quyết định theo `campaign.VoucherType`: type=1 → đổi TIỀN, `SourceValue = VoucherValue`, unit=Amount, cộng BonusAmount (dùng đổi quà UrBox). type=2 (quà hàng hóa)/3 (voucher giảm giá %) → throw "chưa hỗ trợ" (mở rộng sau). HlPointBatch lưu đầy đủ voucher info: AccumulatedSales/AccumulatedPoints/VoucherCode/VoucherName/VoucherType/VoucherValue (migration 20260710115735_Add_HlPointBatch_VoucherFields). HlPointBatchDto + MapBatch trả các field này.
- **Tiêu điểm đổi quà (2026-07 update):** `CreateGiftExchange` (controller) gọi `_hlPointService.SpendAsync(customerCode, unit=Point, totalPointsUsed, exchangeCode, "Đổi quà: {gift}")` TRƯỚC khi tạo HlGiftExchange; UserFriendlyException (thiếu điểm/không thấy KH) → trả Fail, KHÔNG tạo yêu cầu đổi quà. RefCode = exchangeCode.
- **BUG FIX quan trọng (2026-07):** SpendAsync ban đầu guard theo `customer.BonusPoint >= value` (số dư THÔ) → vẫn cho đổi quà sau khi điểm hết hạn, vì BonusPoint có thể còn điểm "mồ côi" (có sẵn từ trước, không thuộc lô nào) hoặc lệch với lô. **Nguồn chân lý = tổng RemainingValue của lô Active CÒN HẠN (ExpireDate > now).** SpendAsync + GetBalanceAsync đều filter `x.ExpireDate > now` (race-proof khi worker chưa chạy), guard `available = sum(RemainingValue) >= value`. GetBalanceAsync trả BonusPoint/BonusAmount = tổng lô còn hiệu lực (KHÔNG dùng customer.BonusPoint thô).
- `SpendAsync`: FIFO trừ lô Active cùng Unit theo ExpireDate tăng dần; clamp quỹ >=0; txn Spend. (Dùng cho luồng đổi quà tương lai.)
- `GetBalanceAsync` (điểm+tiền+lô còn hạn), `GetCustomerHistoryAsync`.

**Admin (`HlAdminAppService`, dual permission AppHlLoyalty/HostAppHlLoyalty tái dùng):** `GetPointHistoryAsync(filter)` + `GetPointBatchesAsync(page,limit,search)` — inject IRepository<HlPointTransaction/HlPointBatch>.

**MiniApp API (HoaLinhMiniAppController, api/mini-app/hl):** POST `loyalty/redeem` (body HlRedeemPointInput), GET `loyalty/balance/{customerCode}`, GET `loyalty/history/{customerCode}`.

**Admin UI (Pages/HoaLinh/PointHistory):** 2 tab (Lịch sử giao dịch + Lô điểm đã đổi), filter search/type/date, client-paging, badge màu. Menu "Lịch sử điểm thưởng" (AppHlPointHistory, icon fa-coins, order 7) guard bằng permission Loyalty. Localization vi/en.

**Worker `HlPointExpireWorker`** (AsyncPeriodicBackgroundWorkerBase, pattern AuditLogCleanupWorker): mỗi giờ, disable IMultiTenant filter, quét lô Active & ExpireDate<=now → trừ RemainingValue khỏi BonusPoint/BonusAmount (clamp>=0) + txn Expire + lô Status=Expired. Options `HlPointExpireOptions` (Domain.Shared/HoaLinh, section HlPointExpire). Đăng ký trong MultiTenancyWebModule (cạnh AuditLogCleanupWorker), bind options trong MultiTenancyApplicationModule.

Liên quan [[project_hl_campaign_detail_and_admin_customer_merge]], [[feedback_appservice_multi_complex_param]], [[project_customer_source_enum]]. Build 0 errors.
