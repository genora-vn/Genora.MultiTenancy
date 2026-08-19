---
name: project_urbox_integration
description: "UrBox eVoucher integration — service, controller, DTOs, signature, settings for gift redemption"
metadata: 
  node_type: memory
  type: project
  originSessionId: 7f8cbb33-af3a-4ee6-a77c-2c0fec37463a
---

Tích hợp hệ thống UrBox (kho quà eVoucher đổi thưởng bằng điểm). Tham khảo pattern module Hoa Linh ([[project_hoalinh_phase2_complete]]).

**Update 2026-07 (đổi quà thật + trạng thái + trừ tiền):**
- `HlGiftExchangeStatus` đổi giá trị: **0=Failed, 1=Success, 2=Processing, 3=Used** (KHÔNG còn Pending/Approved/Rejected/Completed). Đã sửa mọi usage: entity default=Processing, admin ApproveOrReject (approve→Success, reject→Failed, chỉ khi Processing), UrBoxService, MiniApp CreateGiftExchange.
- `UrBoxRedeemData`/`UrBoxRedeemCart`/`UrBoxCodeLinkGift` bổ sung đầy đủ field theo response cartPayVoucher: pay, cart_created, linkCart/linkCombo/linkShippingInfo, cart.cartNo/money_total/money_ship/link_gift[], code_link_gift[].{code,code_image,serial,price,expired,expired_time,priceId,gift_id,token,ttphone,code_display,...}.
- `CreateOrderEvoucherAsync`: thành công KHI `done==1 && status==200` → set Status=Success, lưu UrBoxVoucherCode + full raw JSON vào `UrBoxResponse`, gọi `DeductBonusAmountAsync(siteUserId, exchange)` trừ `Customer.BonusAmount` -= TotalPointsUsed (clamp>=0, try/catch không fail luồng). Thất bại → Status=Failed, KHÔNG trừ điểm. Cần inject `IRepository<Customer,Guid>` vào UrBoxService.
- Admin GiftExchanges UI: filter status mới (0/1/2/3); modal `modal-xl modal-dialog-scrollable`, parse `urBoxResponse` JSON (thêm field vào HlGiftExchangeDto + MapToDto) hiển thị mã/tên voucher/số tiền/serial/QR ảnh (code_image)/hạn dùng/hiệu lực/hotline + nút "Xem chi tiết quà (UrBox)" mở link_gift tab mới; bỏ ngày duyệt/địa chỉ nhận/ghi chú KH/ghi chú nội bộ; CSS .hl-vc-* card layout. KHÔNG cần migration (dùng cột UrBoxResponse sẵn có).

**Update 2026-07 (redeem response trả Id gift-exchange):**
- `UrBoxRedeemData` thêm field `Id` (Guid?, [JsonPropertyName("id")], cùng cấp transaction_id) — gán từ `exchange.Id` trong nhánh success của CreateOrderEvoucherAsync (sau InsertAsync). Mini App dùng Id này gọi `GET carts/{id}` (GetGiftTransactionDetailAsync). Đây là field backend gán, KHÔNG phải field UrBox trả.

**Update 2026-07 (GetCartByTransaction redesign + fix using_time):**
- **Fix crash deserialize:** UrBox getByTransaction trả `using_time:""` (string rỗng) → `UsingTime` trong `UrBoxCartTransactionItemDto` phải `string?` (KHÔNG long?). `UrBoxReceiverDto` thêm email/phone (response thật có `receiver.{email,phone,address}`).
- **GetCartByTransaction đổi hoàn toàn:** endpoint `GET api/mini-app/urbox/carts/{id}` giờ nhận **Guid Id của HL.AppHlGiftExchanges** (KHÔNG phải transaction_id). Service method mới `GetGiftTransactionDetailAsync(Guid giftExchangeId)`:
  1. FindAsync HlGiftExchange theo Id.
  2. Cắt transaction_id từ `InternalNote` (format "UrBox transaction_id=xxxx", lấy sau '=', dừng ở khoảng trắng/'|') qua helper `ExtractTransactionId`.
  3. Gọi getByTransaction (transaction_id) → code/QR(code_image)/expired/link/receiver.
  4. Gọi gift detail (theo `GiftCode`) → Note + Content + danh sách Office + brand.
  5. Map tất cả vào `UrBoxGiftTransactionDetailDto` (AppDtos/UrBox/) trả cho Mini App: Id/ExchangeCode/Status(0-3)+StatusText/GiftName/VoucherCode/CodeImage/Expired/MoneyTotal/LinkGift/Note/Content/Offices[]/Receiver*/... Helpers thêm: ExtractTransactionId, ParseDecimal, GetExchangeStatusText. Controller cần `using System` cho Guid.

**Cấu trúc đã tạo (branch dev, 2026-07):**
- Enum: `UrBoxProductType` (EVoucher=1, Physical=2), `UrBoxResponseStatus` (static class map mã lỗi UrBox→tiếng Việt, Success=200, 300-311/400/500) tại `Domain.Shared/Enums/`.
- Settings: `UrBoxSettings` (Application.Contracts/AppDtos/UrBox/) bind section `"UrBoxSetting"` trong appsettings.json — UrBoxApiUrl, AppSecret, AppId, CampaignCode, IsSendSms, MinimumBonusPoint, BonusPointRate, PrivateKeyPath, các *Path.
- DTOs (AppDtos/UrBox/): `UrBoxResponse<T>` wrapper (done/msg/status/data), `UrBoxPagedData<T>` (items/totalPage/totalResult/brand_count), brand/category/gift-item/gift-detail/cart DTOs, redeem input + cartPayVoucher request + signature payload + response. **Dùng `[JsonPropertyName]` tường minh** vì UrBox trộn snake_case (gift_id) và camelCase (totalPage) — KHÔNG dùng SnakeCaseLower policy như HL.
- **Update 2026-07 (full field coverage đúng response thật):** `UrBoxGiftItemDto`/`UrBoxGiftDetailDto` bổ sung đầy đủ: brand, brand_online, parent_cat_id, view, code_quantity, code_display/code_display_type, price_promo/start_promo/end_promo/is_promo/is_unfix, usage_check, weight, justGetOrder + `office[]` (field=office). Thêm `UrBoxOfficeDto` (id/brand_id/code/address/phone/city_id/district_id/ward_id/lat/long/isApply/title_city/brand_title/brand_img_src). Cart: `UrBoxCartDto` thêm site_id/linkCombo; `UrBoxCartItemDto` (getlist detail) thêm app_id/urcard_id/justGetOrder/type/usage_status/usage_status_code/using_time/delivery/deliveryCode/delivery_required/images_rectangle/code_display_type; getByTransaction dùng DTO RIÊNG `UrBoxCartTransactionItemDto` (priceId/finish_time/delivery_tracking/estimateDelivery/created_timestamp) + `UrBoxReceiverDto` + customer(object). Số string giữ string (UrBox trả số dạng string).
- Service: `IUrBoxService`/`UrBoxService` (Application/AppServices/UrBox/) — HttpClient "UrBox" (System.Text.Json). **TẤT CẢ API tra cứu = GET + query string** (brand/category/gift/gift-detail/cart-list/cart-by-transaction); **CHỈ cartPayVoucher (đổi quà) = POST + JSON body + header Signature**. (Lưu ý quan trọng: ban đầu tôi làm category/gift/cart bằng POST → Urbox không trả data; đã sửa 2026-07 sang GET.)
- Signature: `UrBoxSignatureHelper` — RSA-SHA256 PKCS#1, .NET 9 native `RSA.ImportFromPem` (KHÔNG cần BouncyCastle). Quy trình: serialize→sort field alphabet (Ordinal)→compact JSON (System.Text.Json.Nodes, UnsafeRelaxedJsonEscaping)→sign→Base64. Payload ký KHÔNG có ttphone.
- Controller: `UrBoxMiniAppController` route `api/mini-app/urbox` — [AllowAnonymous][RemoteService(false)]. Endpoints: brands, categories, gifts, gifts/{id}, carts, carts/{txId}, POST redeem.
- DI: Configure<UrBoxSettings> + AddHttpClient("UrBox") + AddScoped<IUrBoxService> trong MultiTenancyApplicationModule (đọc BaseUrl/Timeout qua configuration[key] vì `.Get<T>()` extension không có sẵn — Microsoft.Extensions.Configuration.Binder chưa reference).

**Redeem lưu lịch sử vào `HlGiftExchange` (AppHlGiftExchanges)** — tái dùng entity có sẵn field UrBoxVoucherCode/UrBoxResponse. Status Pending→Approved(200)/Rejected(khác). ExchangeCode format `UB-{yyMMdd}{seq}`.

**BLOCKER chưa xong:** cần file RSA private key PEM tại `src/Genora.MultiTenancy.Web/Keys/urbox_private_key.pem` (UrBox cấp keypair). Chưa có folder Keys trong Web project. Redeem sẽ throw FileNotFoundException tới khi có key. Các API đọc (brand/category/gift/cart) chạy được ngay không cần key.

Build: 0 errors (Web project full build). Chưa chạy migration mới (không thêm entity/field DB).
