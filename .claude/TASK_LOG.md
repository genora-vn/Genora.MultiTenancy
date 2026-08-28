# TASK LOG — Nhật ký công việc

> Nhật ký các đợt task theo dòng thời gian, suy ra từ hậu tố ngày/round trên tên note
> trong `.claude/memory/notes/`. Sắp theo thứ tự gần đây nhất ở trên.
> Khi hoàn thành task mới, thêm 1 dòng vào đầu bảng.

## Cách đọc
- Mỗi dòng là 1 đợt làm việc. "Note" trỏ tới file chi tiết trong `memory/notes/project/`.
- Ngày lấy từ tên file (`_juneXX`, `_YYYY_MM_DD`) hoặc thứ tự phase/round.

## Nhật ký (mới → cũ)

| Mốc | Module | Nội dung | Note gốc |
|-----|--------|----------|----------|
| 2026-08-26 | UrBox / Hoa Linh (Admin) | **[HOTFIX 20260826]** Trang Đổi quà (`/HoaLinh/GiftExchanges`) + KH (`/AppCustomers`). **YC1 GiftExchanges:** modal "Voucher UrBox" render DANH SÁCH voucher (parse `code_link_gift[]` từ `urBoxResponse` — không chỉ FirstOrDefault), mỗi voucher có QR + button "Xem chi tiết quà (UrBox)" link riêng; list scroll `max-height:360px` không vỡ modal; tiêu đề "Voucher UrBox (N voucher)"; thêm dòng "Lý do thất bại" (lấy `msg` khi `done!=1`); bộ lọc `FilterText` thêm `CustomerCode`+`CustomerPhone` trong `HlGiftExchangeAppService.GetListAsync`. Sửa `GiftExchanges/index.js` + `Index.cshtml` (CSS `.hl-vc-list/.hl-vc-item`). **YC2 AppCustomers:** thêm field `BonusAmount` (Tiền thưởng) readonly trên EditModal cạnh Điểm thưởng+Hạng (3 cột) — thêm `BonusAmount` vào `AppCustomerDto` + prop PageModel (KHÔNG vào CreateUpdate DTO tránh ghi đè); localization `CustomerSource:HoaLinh` + `BonusAmount`/`BonusPoint` (vi+en). Build Web 0 errors. Nhánh `hotfix/20260826`. | `GiftExchanges/*`, `AppCustomers/EditModal.*`, `HlGiftExchangeAppService.cs` |
| 2026-08-26 | UrBox / Hoa Linh | **[HOTFIX 20260826]** Chi tiết đơn đổi quà Mini App (`UrBoxService.GetGiftTransactionDetailAsync`). **Fix 1 (v2 — API đối tác ngưng):** BỎ gọi `GetCartByTransactionAsync`, đọc chi tiết voucher TỪ cột `HlGiftExchange.UrBoxResponse` (JSON `cartPayVoucher` đã lưu lúc đổi): deserialize `UrBoxResponse<UrBoxRedeemData>` → map `data.cart.code_link_gift[]` (`UrBoxCodeLinkGift`) sang LIST `Vouchers` (Code/CodeImage/CodeDisplay/CodeDisplayType/Expired/Link) + giữ field cũ cho voucher đầu (tương thích ngược, FE KHÔNG đổi). Receiver lấy từ ttphone/ttemail/ttaddress. Đoạn ZBS trong `CreateOrderEvoucherAsync` cũng bỏ gọi API → lấy `expired` từ `firstCode` (response cartPayVoucher). Model `UrBoxGiftTransactionDetailDto` giữ nguyên. **Fix 2:** bỏ nhân số lượng ở `totalPoints` (`Sum(i => i.PointsRequired)` — FE đã nhân); set `HlGiftExchange.PointsRequired = totalPoints` để `PointsRequired == TotalPointsUsed`. **Script data-fix:** SQL sửa `TotalPointsUsed` gấp đôi cho bản ghi cũ (`HL.AppHlGiftExchanges`, điều kiện `Quantity>1 AND TotalPointsUsed = PointsRequired*Quantity`). Build HttpApi 0 errors. Nhánh `hotfix/20260826`. | `UrBoxService.cs`, `UrBoxGiftTransactionDetailDto.cs` |
| 2026-08-21 | Zalo OA | GetNewsDetail: map `content = url` khi type=image/photo (trong `MiniAppZaloNewsService.GetArticleDetailAsync` → `NormalizeImageBlocks`, chạy TRƯỚC khi cache) để Mini App đọc chung trường content, không phải sửa client. Áp cho cả 2 controller dùng chung service. Build 0 errors. LƯU Ý: bản cache detail cũ (chưa map) còn hiệu lực tới khi hết TTL `Zalo:NewsCacheMinutes`. | `MiniAppZaloNewsService.cs` |
| 2026-08-21 | Zalo OA | Fix API GetNewsDetail (MiniAppController): bổ sung `url` + `caption` vào `ZaloArticleBodyBlock` (ZaloArticleDtos.cs) để nhận ảnh khi type="image"; type="text" vẫn đọc `content`. Khớp response `/v2.0/article/getdetail`. Build 0 errors. | `ZaloArticleDtos.cs` |
| 2026-07-25 | Caddie | Fee + multi-caddie admin + upsert/unassign (migration 20260725062150) | `project_caddie_fee_upsert_unassign` |
| 2026-07-24 | Caddie | Booking gắn vào golf players (migration 20260724091716) | `project_caddie_booking_linked_to_golf_players` |
| 2026-07-09 | Hoa Linh | Loyalty đổi điểm/tiền + ledger FIFO + worker hết hạn (migration 20260709064009) | `project_hl_loyalty_points_redeem` |
| 2026-06-26 | Hoa Linh | UI update batch 3 (Dashboard/Brands/Products/Customers/Orders/GiftExchanges) | `project_hoalinh_ui_update3_june26` |
| 2026-06-25 | Hoa Linh | UI update batch 1-2 + API endpoints mới (Brands/ProductGroups/OrderHeaders) | `project_hoalinh_ui_update_june25`, `_ui_update2_june25` |
| 2026-06-16 | Caddie | Import + Email Cc/Bcc + Feature toggle | `project_caddie_email_feature_fixes_june16` |
| 2026-06-05 | Caddie | UI fixes June 05 (batch 1-3) | `project_caddie_ui_fixes_june05`, `_batch2` |
| 2026-06-03 | Caddie | UI fixes June 03 (Select2, flatpickr) | `project_caddie_ui_fixes_june03` |
| 2026-05-25 | Golf/Pro | ProOrder.CustomerId soft reference (migration 20260525102341) | `project_proorder_customer_soft_reference` |
| 2026-05-20 | Salon | Stylist/Booking LocationId + slot config (migrations 20260520052000, 20260520100420) | `project_salon_stylist_booking_locationid`, `_location_slot_config` |
| — | Hoa Linh | Phase 1-7 complete (foundation → dashboard/data-auth) | `project_hoalinh_phase1..7_complete` |
| — | Caddie | Phase 1-7 complete (SRS → final) | `project_caddie_module_phase1..final_complete` |
| — | Salon Beauty | Backend + UI complete | `memory/modules/salon-beauty/`, `project_salon_*` |
| — | Documents | Online docs site + seeder 11 section | `project_app_documents_*` |

> Ghi chú: Danh sách đầy đủ 88 note project nằm ở `memory/notes/project/`. Bảng này chỉ
> tổng hợp các mốc chính; xem [MEMORY.md](MEMORY.md) để có index đầy đủ theo chủ đề.

## Sự cố đáng nhớ
- **Prod antiforgery SSL** — proxy terminate TLS, `SecurePolicy=Always` gây lỗi login. → `project_prod_antiforgery_ssl_incident`
- **AppCustomers permission leak** — dropdown load qua AppService `[Authorize]` throw AbpAuthorizationException. → `project_appcustomers_page_customertype_permission_leak`

## Migration đã ghi nhận
- 20260520052000, 20260520100420 (Salon location/slot)
- 20260525102341 (ProOrder drop FK)
- 20260709064009 (HL loyalty)
- 20260724091716, 20260725062150 (Caddie players link + fee)
