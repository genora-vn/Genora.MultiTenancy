---
name: project-hoalinh-phase5-complete
description: "Phase 5 Mini App APIs hoàn thành — HoaLinhMiniAppController 13 endpoints, route /api/mini-app/hl/, AllowAnonymous"
metadata: 
  node_type: memory
  type: project
  originSessionId: dcf06de4-47b3-4551-89dd-8fabec86f6de
  modified: 2026-07-23T09:20:08.640Z
---

## Hoa Linh Phase 5 Complete — Mini App APIs (2026-06-24)

### Controller: HoaLinhMiniAppController
- File: HttpApi/Controllers/HoaLinhMiniAppController.cs
- Route: `/api/mini-app/hl/`
- Attributes: [IgnoreAntiforgeryToken] [RemoteService(false)] [AllowAnonymous]
- Pattern: Mini App → Genora → API Hoa Linh DMS

### Endpoints (13 total):

| # | Method | Endpoint | Mô tả | Source |
|---|--------|----------|--------|--------|
| 1 | GET | /auth/{phone} | Check KH tồn tại DMS | API HL |
| 2 | GET | /customer/{phone} | Chi tiết KH | API HL |
| 3 | GET | /products | Danh sách SP | API HL |
| 4 | GET | /products/{code} | Chi tiết SP | API HL |
| 5 | POST | /orders | Tạo đơn hàng | Lưu DB Genora |
| 6 | GET | /orders | Lịch sử đơn (DMS) | API HL |
| 7 | GET | /orders/{orderNumber} | Chi tiết đơn (DMS) | API HL |
| 8 | GET | /my-orders | Đơn hàng Mini App | DB Genora |
| 9 | POST | /orders/{id}/cancel | Hủy đơn Mini App | DB Genora |
| 10 | GET | /loyalty/{phone} | Điểm + hạng TV | API HL |
| 11 | GET | /campaigns | Danh sách chiến dịch | API HL |
| 12 | POST | /gift-exchange | Tạo yêu cầu đổi quà | Lưu DB Genora |
| 13 | GET | /gift-exchange | Lịch sử đổi quà | DB Genora |
| 14 | GET | /saleman/{dsrCode} | Thông tin Sales | API HL |
| + | GET | /payment/methods | DS hình thức thanh toán khả dụng | ABP Setting |

### Update 2026-07-22 — GET /api/mini-app/hl/payment/methods
- Thêm vào #region Payment của HoaLinhMiniAppController. Trả `HlApiResult<List<HlPaymentMethodDto>>`.
- **Logic nằm trong `HlPaymentService.GetPaymentMethodsAsync()`** (đúng coding rule ưu tiên AppService — KHÔNG để trong controller). Controller chỉ gọi service + wrap HlApiResult. Interface `IHlPaymentService` thêm method này.
- Service đọc 2 cờ ABP Setting qua `ISettingProvider _settings` (đã inject sẵn trong HlPaymentService) + private helper `GetPaymentToggleAsync()` (copy pattern MiniAppCalendarSlotService): `ZaloPaymentSettingNames.IsPayAtCounterEnabled` / `.IsPayBankTransferEnabled`, fallback `true` khi null/parse fail (fail-open).
- Chỉ add method vào list khi cờ bật: COUNTER="Thanh toán tại quầy", BANK_TRANSFER="Thanh toán chuyển khoản". Cả 2 tắt → list rỗng.
- DTO `HlPaymentMethodDto` (Code/Name/IsEnabled) trong HlExtraDtos.cs (namespace AppDtos.HoaLinh). HlPaymentService thêm `using System.Collections.Generic` cho List<>.
- Ban đầu (2026-07-22) đặt helper + `ISettingProvider` trong controller; sau đó REFACTOR: gỡ field `_settingProvider` + helper khỏi controller, chuyển hết vào service (controller giờ không còn ref ISettingProvider/ZaloPaymentSettingNames).
- Setting per-tenant nên mỗi tenant HL trả cấu hình riêng. Build HttpApi + Application 0 errors. Xem [[feedback_hl_payment_setting_names]], [[project_payment_toggles_and_news_edit_lazy_load]].

### Update 2026-07-23 — GET /orders ưu tiên nguồn theo ZaloOrderNumber (ĐẢO logic cũ)
- Endpoint `GET /api/mini-app/hl/orders` merge đơn DMS Hoa Linh (`_hlApi.GetOrderHeaderZaloAsync`) + Genora DB. **Logic ưu tiên nguồn đã ĐẢO ngược so với trước:**
  - **`ZaloOrderNumber != null/rỗng`** (đơn Genora đã đồng bộ lên DMS) → **ưu tiên bản DMS API**, KHÔNG lấy bản Genora DB. Trước đây làm ngược (giữ Genora, bỏ DMS).
  - **`ZaloOrderNumber` null/rỗng** (đơn thuần DMS, không phải từ Mini App Genora) → hiển thị từ DMS API.
  - Đơn Genora DB **không** bị DMS ghi nhận qua ZaloOrderNumber → vẫn lấy từ Genora DB.
- Cơ chế: gom `dmsSyncedGenoraCodes = HashSet` các `ZaloOrderNumber != null` từ DMS. Bước 4a (Source=genora) `.Where(o => !dmsSyncedGenoraCodes.Contains(o.OrderCode))` để loại đơn đã có bản DMS. Bước 4b (Source=hoalinh) trả **TẤT CẢ** `hlOrders` (bỏ filter `!matchedGenoraCodes.Contains` cũ).
- Vẫn giữ vòng sync status DMS→Genora DB (`MapHlStatusToDeliveryStatus`, UpdateAsync IsSyncedToHl/SyncedAt) để dữ liệu DB nhất quán, nhưng KHÔNG dùng bản Genora đó để hiển thị khi ZaloOrderNumber != null.
- Thêm field `ZaloOrderNumber` vào cả 2 shape output (Genora dùng `o.OrderCode`, DMS dùng `h.ZaloOrderNumber`) để frontend phân biệt.
- File: HttpApi/Controllers/HoaLinhMiniAppController.cs (method GetOrders ~L345). Build HttpApi 0 errors.

### Request DTOs (HlMiniAppRequests.cs):
- HlCreateOrderRequest + HlCreateOrderItemRequest
- HlCreateGiftExchangeRequest

### Logic đặc biệt:
- OrderCode format: `HL-{yyMMdd}{4 char random}` (e.g. HL-260624A1B2)
- GiftExchange code: `HLGE-{yyMMdd}{4 char random}`
- Auth check: is_customer == false → trả message yêu cầu liên hệ NVKD
- /my-orders: WithDetails eager-load Items, filter by CustomerCode
- TotalAmount = SubTotal - DiscountAmount - SystemDiscount

**Why:** Controller trung gian duy nhất cho Zalo Mini App. Tách biệt /orders (DMS history) vs /my-orders (Genora DB).
**How to apply:** Mini App frontend gọi /api/mini-app/hl/{endpoint}. Auth flow: decode-phone → /auth/{phone} → nếu OK → /customer/{phone}.

[[project-hoalinh-phase4-complete]] [[project-hoalinh-phase2-complete]]
