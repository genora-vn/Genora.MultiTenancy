---
name: project_validate_vga_code_api
description: API validate VGA Code + Email recalculate + EditModal auto-price + prepare-order fix
metadata: 
  node_type: memory
  type: project
  originSessionId: cb23966a-9de2-4485-88c4-a0598c2cffcd
---

## API: Validate VGA Code
GET /api/mini-app/validate-vga-code?vgaCode={code}&calendarSlotId={slotId}&numberHoles={n}&usedVgaCodes={c1}&usedVgaCodes={c2}

Front-end gọi API kiểm tra VgaCode tồn tại trong AppCustomers → trả về giá theo loại khách hàng.
Response: { isValid, customerId, customerTypeCode, customerTypeName, pricePerGolfer, originalPrice, message }

## Dedup VGA — mỗi mã chỉ 1 người chơi trong cùng booking (2026-07-16)
- Thêm param optional `List<string>? usedVgaCodes` (interface + controller `[FromQuery]` + service). FE gửi các mã VGA đã nhập cho những người chơi KHÁC trong cùng booking.
- `ValidateVgaCodeAsync` check trước tiên (bước 0): nếu `normalizedCode` trùng (OrdinalIgnoreCase) với 1 mã trong `usedVgaCodes` → trả `IsValid=false, Message="Mã hội viên không hợp lệ hoặc đã bị trùng"`. Mã không tồn tại → `Message="Mã hội viên không hợp lệ"`. Hợp lệ → trả thêm `CustomerId` (Id KH sở hữu mã) + `Message="Mã hội viên hợp lệ"`.
- `ValidateVgaCodeResultDto` thêm field `CustomerId (Guid?)` + `Message (string?)`.
- Interface cần `using System.Collections.Generic;` cho List<>.

## BUG FIX: update-bookings không lưu đúng PricePerPlayer từng người (2026-07-16)
- `MiniAppBookingAppService.ReplacePlayersAsync` (dùng bởi UpdateFromMiniAppAsync): TRƯỚC ghi đè MỌI player = `pricePerGolfer` cấp booking (giá booker) → người bỏ VGA vẫn bị lưu giá Member. SAU: `playerPrice = p.PricePerPlayer ?? pricePerGolfer` (ưu tiên giá riêng từng người từ input, chỉ fallback khi null). Cả constructor + gán `player.PricePerPlayer` đều dùng `playerPrice`.
- `UpdateFromMiniAppAsync` dòng ~526: `booking.TotalAmount` TRƯỚC = `recalculatedPricePerGolfer * NumberOfGolfers` (flat, sai khi giá khác nhau). SAU = sum `p.PricePerPlayer` khi có players, fallback `× NumberOfGolfers`. (Luồng Create đã đúng sẵn — dùng p.PricePerGolfer từng player.)

## Email Pricing — BuildPriceBreakdownItemsAsync
Cả AppBookingService + MiniAppBookingAppService:
- Thêm param `List<BookingPlayer>? players = null`
- MB count = 1 + validMemberCompanions (companion có VgaCode khớp + CustomerType MB)
- MBG count = min(maxMemberGuest - validMemberCompanions, remaining)

## Booking Detail API (MiniApp) — MaxMemberGuest recalculate
GET /api/mini-app/get-bookings/{id}?customerId={cid}
- MaxMemberGuest giảm theo validMemberCompanions
- CustomerBillTotalPrice recalculate MB/MBG/VIS

## Admin EditModal — VgaCode auto-validate (Fixed)
- AppBookingDto thêm `CustomerTypeCode` (Code thay vì Name)
- EditModal.cshtml.cs set `CustomerTypeCode = dto.CustomerTypeCode`
- Form dùng `data-customer-type-code` (not data-customer-type)
- JS đọc `$form.attr('data-customer-type-code')` so sánh với 'MB'
- Blur event gọi /api/mini-app/validate-vga-code → update price + recalc total

## Create Booking TotalAmount Fix
- Trước: `input.PricePerGolfer * input.NumberOfGolfers` → sai khi MB + MBG giá khác nhau
- Sau: `input.Players.Sum(p => p.PricePerGolfer)` → lấy tổng giá thực tế từ từng player
- Front-end đã gửi đúng pricePerGolfer cho từng player (MB=1.2M, MBG=1.8M)
- Fallback nếu không có players: dùng PricePerGolfer × NumberOfGolfers

## Prepare-order Amount Fix
- Trước: dùng `booking.TotalAmount` (flat stored, có thể sai khi recalculate)
- Sau: sum `PricePerPlayer` từ AppBookingPlayers → fallback booking.TotalAmount
- Thêm `IRepository<BookingPlayer, Guid> _playerRepo` vào MiniAppPaymentAppService

**Files changed:**
- `Application.Contracts/AppDtos/AppBookings/AppBookingDto.cs` — thêm CustomerTypeCode
- `Application/AppServices/AppBookings/AppBookingService.cs` — set CustomerTypeCode + BuildPriceBreakdown
- `Application/AppServices/AppBookings/MiniAppBookingAppService.cs` — BuildPriceBreakdown + GetMiniAppAsync
- `Application/AppServices/AppCalendarSlots/MiniAppCalendarSlotService.cs` — ValidateVgaCodeAsync
- `Application/AppServices/AppPayments/MiniAppPaymentAppService.cs` — sum players price
- `Application.Contracts/AppDtos/AppCalendarSlots/ValidateVgaCodeResultDto.cs` — DTO
- `Application.Contracts/AppDtos/AppCalendarSlots/IMiniAppCalendarSlotService.cs` — interface
- `HttpApi/Controllers/MiniAppController.cs` — endpoint
- `Web/Pages/AppBookings/EditModal.cshtml` — JS validate + data-customer-type-code
- `Web/Pages/AppBookings/EditModal.cshtml.cs` — CustomerTypeCode property

[[project_calendar_slot_pricing_api]] [[project_customer_type_original_price_by_special_date]] [[project_member_guest_pricing_pattern]] [[project_booking_total_amount_pattern]]
