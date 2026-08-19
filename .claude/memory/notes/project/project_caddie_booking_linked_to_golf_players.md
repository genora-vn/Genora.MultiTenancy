---
name: project-caddie-booking-linked-to-golf-players
description: "Book nhiều Caddie trả về list CaddieId + CaddieBookingId, gắn vào từng người chơi golf qua 3 cột mới trên AppBookingPlayers"
metadata: 
  node_type: memory
  type: project
  originSessionId: 629ce865-5b9b-41f6-890a-9b940ef82561
  modified: 2026-07-24T09:18:33.437Z
---

## Caddie booking gắn vào người chơi golf (2026-07-24)

**Nghiệp vụ Mini App:** gọi API đặt Caddie TRƯỚC (`POST /api/mini-app/caddie/booking`) → nhận `caddieBookingId` + danh sách `caddies[].caddieId` → truyền vào từng `players[]` khi gọi API booking golf (`POST /api/mini-app/create-booking`).

### 1. Entity `BookingPlayer` (bảng `AppBookingPlayers`) — thêm 3 cột nullable (soft reference, KHÔNG FK, đúng pattern [[project_proorder_customer_soft_reference]]):
- `Guid? CaddieId` — caddie gắn với người chơi này
- `Guid? CaddieBookingId` — trỏ về AppCaddieBooking (header) liên kết booking Caddie ↔ booking golf
- `string? CaddieName` (StringLength 255) — denormalize để hiển thị, tránh join AppCaddies
- DbContext: chỉ cấu hình `b.Property(x => x.CaddieName).HasMaxLength(255)`; 2 Guid? theo convention.
- Migration: `20260724091716_AddCaddieToBookingPlayer` (3 AddColumn nullable). Đã `database update` OK. Kill Web process trước khi add migration ([[feedback_ef_migration_dll_lock]]).

### 2. DTOs golf booking — thêm `CaddieId` + `CaddieBookingId` + `CaddieName` vào:
- `MiniAppBookingPlayerInput` (MiniAppCreateBookingDto.cs) — input Mini App
- `CreateUpdateBookingPlayerDto` (CreateUpdateAppBookingDto.cs) — admin/update
- `AppBookingPlayerDto` (AppBookingDto.cs) — output; AutoMapper `CreateMap<BookingPlayer, AppBookingPlayerDto>()` tự map (cùng tên).

### 3. Service golf `MiniAppBookingAppService`:
- `CreateFromMiniAppAsync` (~L214): set `player.CaddieId/CaddieBookingId/CaddieName = p.*` khi tạo BookingPlayer.
- `ReplacePlayersAsync` (~L1228): tương tự để giữ khi update booking.

### 4. Caddie booking — `MiniAppCaddieAppService.CreateBookingAsync` (đã hỗ trợ book nhiều caddie sẵn: input `Caddies: List<{CaddieId, Note}>`, tạo 1 AppCaddieBooking + N AppCaddieBookingDetail, khóa AppCaddieSchedule slot).
- ĐỔI kiểu trả về từ `MiniAppCaddieBookingHistoryDto` (chỉ 1 caddie `firstCaddie`) → **`MiniAppCreatedCaddieBookingDto`** trả `CaddieBookingId` (booking.Id) + `Caddies: List<MiniAppCreatedCaddieItemDto>{CaddieBookingDetailId, CaddieId, CaddieName, CaddieCode, CaddieAvatar, RatingAvg, ScheduleId, Note}`.
- Build caddieMap (Dictionary CaddieId→AppCaddie) từ caddieItems, thu thập item trong vòng tạo detail.
- DTOs mới trong MiniAppCaddieDtos.cs: `MiniAppCreatedCaddieBookingDto`, `MiniAppCreatedCaddieItemDto`, `MiniAppCreatedCaddieBookingResponse : ZaloBaseResponse`.
- Controller `MiniAppController.CreateCaddieBooking` (~L485): đổi trả `MiniAppCreatedCaddieBookingResponse` (wrap Data = result).

### CURL
**Đặt Caddie (gọi trước):**
```
POST /api/mini-app/caddie/booking
{ "customerId":"<guid>", "caddies":[{"caddieId":"<c1>","note":""},{"caddieId":"<c2>"}],
  "bookingDate":"2026-08-01","startTime":"08:00:00","numberOfHoles":18,
  "totalCaddieFee":600000,"paymentMethod":0 }
```
→ Response.Data: { caddieBookingId, bookingCode, caddies:[{caddieBookingDetailId, caddieId, caddieName, ...}] }

**Booking golf (truyền caddie vào player):**
```
POST /api/mini-app/create-booking
{ "customerId":"<guid>","playDate":"2026-08-01","golfCourseId":"<guid>","calendarSlotId":"<guid>",
  "numberOfGolfers":2,
  "players":[
    {"playerName":"A","pricePerGolfer":1200000,"caddieId":"<c1>","caddieBookingId":"<cbId>","caddieName":"Caddie A"},
    {"playerName":"B","pricePerGolfer":1200000,"caddieId":"<c2>","caddieBookingId":"<cbId>"}],
  "pricePerGolfer":1200000,"totalAmount":2400000,"paymentMethod":1,"status":0,"source":1,
  "numberHoles":18,"isExportInvoice":false }
```

Build HttpApi 0 errors. Xem [[project_validate_vga_code_api]] (PricePerPlayer từng người), [[project_caddie_caddiefee_bookingdetails]] (multi-caddy AppCaddieBookingDetail), [[feedback_no_ef_in_application_layer]] (AsyncExecuter).
