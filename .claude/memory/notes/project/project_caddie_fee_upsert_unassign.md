---
name: project-caddie-fee-upsert-unassign
description: Booking.TotalCaddieFee cộng vào TotalAmount; admin hiển thị nhiều Caddie; API caddie upsert + unassign tự tính phí
metadata: 
  node_type: memory
  type: project
  originSessionId: 629ce865-5b9b-41f6-890a-9b940ef82561
  modified: 2026-07-30T10:20:42.523Z
---

## Batch 7 (2026-07-30) — fix duplicate caddie booking on update + admin caddie cancel giữ fee + admin change email phí
- **FIX duplicate AppCaddieBooking mỗi lần update (ROOT CAUSE)**: `UpdateFromMiniAppAsync` gọi `ReplacePlayersAsync` (xóa+tạo lại players) TRƯỚC `ReconcileInlineCaddieBookingAsync`. Players mới có CaddieBookingId=null (từ payload) → reconcile query `p.CaddieBookingId != null` không thấy → tạo AppCaddieBooking mới mỗi lần. FIX: capture `existingCaddieBookingId` từ `oldPlayers` (load TRƯỚC ReplacePlayersAsync) rồi truyền vào `ReconcileInlineCaddieBookingAsync(..., existingCaddieBookingId)`. Reconcile giờ tái dùng đúng 1 header, chỉ thêm/xóa detail + cập nhật TotalCaddieFee.
- **FIX admin caddie cancel reset fee** (CaddieBookingAppService.UpdateStatusAsync): bỏ `booking.TotalCaddieFee = 0m` + bỏ gọi ClearGolfBookingCaddieLinksAsync khi Cancel. Giờ chỉ đổi Status=Cancelled + ReleaseAllScheduleSlotsAsync (nhả lịch Available=1). GIỮ NGUYÊN TotalCaddieFee.
- **FIX admin change email thiếu phí Caddie** (AppBookingService.UpdateAsync BookingChangeRequestEmailModelDto): thêm HasCaddieFee/TotalCaddieFeeText/GrandTotalText từ entity.TotalCaddieFee (trước chỉ có ở mini-app flow).
- Build full solution 0 errors. KHÔNG entity/migration mới ở batch 7.

## Batch 6 (2026-07-30) — fix 500 update-bookings + multi-caddie rating + email phí + admin cancel + HL APIs + enum refactor
- **FIX 500 update-bookings (ROOT CAUSE)**: `AppCaddieBookingDetail.ScheduleId` có FK NOT NULL tới AppCaddieSchedules; luồng inline/reconcile insert detail với ScheduleId=Guid.Empty khi Caddie chưa có slot → FK violation. Fix: đổi `ScheduleId` sang `Guid?` (entity + FK config IsRequired(false) + constructor Empty→null); 3 helper Release*ScheduleAsync nhận `Guid?`; DTO MiniAppCreatedCaddieItemDto.ScheduleId → Guid?. Migration `20260729101739_MakeCaddieBookingDetailScheduleIdNullable` (đã apply).
- **Rating multi-caddie** (task 1): `MiniAppCreateCaddieRatingDto` thêm `Ratings: List<MiniAppCaddieRatingItemDto>{CaddieId,OverallRating,Comment,SkillRatings[]}` (field phẳng cũ giữ optional cho backward-compat). CreateRatingAsync loop mỗi caddie 1 AppCaddieRating; validate caddie thuộc booking + skip caddie đã đánh giá; dedup CaddieId.
- **Cancel fixes** (task 4): mini-app CancelLinkedCaddieBookingsAsync BỎ TotalCaddieFee=0 (giữ lịch sử phí, chỉ Status=Cancelled). Admin AppBookingService inject 3 caddie repo; UpdateAsync khi Status→CancelledRefund/CancelledNoRefund gọi CancelLinkedCaddieBookingsAsync (hủy caddie liên đới + nhả lịch, giữ TotalCaddieFee).
- **Email phí Caddie** (task 3): 2 email model DTO (New+Change) thêm HasCaddieFee/TotalCaddieFeeText/GrandTotalText. Template .tpl thêm `{{ if model.HasCaddieFee }}` dòng "Tổng phí đặt Caddie" + "Tổng cộng" dưới "Tổng giá trị đặt chỗ" trong THANH TOÁN (ẩn khi không có phí → tenant khác không ảnh hưởng). Populate MiniAppBookingAppService.
- **HL combo API** (task 5): HlProductComboDto{ComboCode,ProductCode,ProductName,Quantity}; GetProductCombosAsync (GET /api/ProductCombo?page&limit); GET /api/mini-app/hl/product-combos.
- **HL gift mark-used** (task 6): POST /api/mini-app/hl/gift-exchange/{id}/mark-used, body HlMarkGiftUsedRequest{CustomerCode?}; set HlGiftExchangeStatus.Used(3), guard chủ quà + chỉ Success→Used.
- **Enum refactor** (task 7): magic byte trong inline caddie helpers → CaddieBookingStatus/CaddiePaymentStatus/CaddieCheckinStatus/CaddieSlotStatus (Domain.Shared/Enums).
- Build full solution 0 errors. Migrations 20260728025444 + 20260729101739 đã apply.

## FIX 2026-07-28 (batch 3) — link chuẩn CaddieBookingId/DetailId + unified create-booking

**ROOT CAUSE các lỗi trước**: `AppBookingPlayers.CaddieBookingId` bị lưu NHẦM = Id của `AppCaddieBookingDetails` (không phải header `AppCaddieBookings.Id`). Nên mọi query sync theo header id đều KHÔNG match → đổi/hủy Caddie không cập nhật AppBookingPlayers.

**Fix cấu trúc dữ liệu**: Thêm cột `AppCaddieBookingDetailId (Guid?)` vào `BookingPlayer` (migration `20260728025444_AddAppCaddieBookingDetailIdToBookingPlayer`, đã apply). Quy ước rõ: `CaddieBookingId` = AppCaddieBookings.Id (HEADER), `AppCaddieBookingDetailId` = AppCaddieBookingDetails.Id (DÒNG). Thêm field vào 3 DTO player (AppBookingPlayerDto, CreateUpdateBookingPlayerDto, MiniAppBookingPlayerInput) + persist ở 4 nơi write player (CreateFromMiniApp, ReplacePlayersAsync, admin SavePlayersAsync, GetAsync projection — projection này trước thiếu HẾT field caddie → là lý do cột Tên Caddie không hiện ở Admin ViewModal/EditModal).

**Fix ChangeCaddyAsync** (CaddieBookingAppService): query players theo `AppCaddieBookingDetailId == targetDetail.Id` (fallback header+oldCaddieId cho data cũ), update CaddieId+CaddieName+CaddieBookingId(header)+AppCaddieBookingDetailId.

**Fix Cancel** (`ClearGolfBookingCaddieLinksAsync`): query players match theo detailIds của booking (gồm cả data cũ lưu detailId trong CaddieBookingId) HOẶC header id. Gỡ null 4 field caddie ở AppBookingPlayers + set `AppBookings.TotalCaddieFee=null` + trừ TotalAmount = sum(players). KHÔNG đụng `AppCaddieBookings.TotalCaddieFee` (giữ lịch sử). Gọi trong UpdateStatusAsync(→Cancelled) + DeleteAsync.

**Fix EditModal Tổng tiền = 204400000 (×100 bug)**: hidden `Booking_TotalCaddieFee` render qua asp-for = "2000000.00", `normalizeMoney` strip dấu chấm → "200000000". Fix: (a) normalizeMoney cắt phần thập phân nếu match `/^\d+\.\d+$/`; (b) JS đọc phí Caddie từ DISPLAY field (số nguyên đã format) thay vì hidden, sync lại hidden bằng giá trị sạch. EditModal layout đổi `d-inline-flex width:98%` → `row g-3` + `col-md-3 col-sm-6` (hết scroll ngang).

**UNIFIED create-booking (task 4)**: `MiniAppCreateBookingDto` thêm `CaddieAssignments: List<MiniAppInlineCaddieInput>?` (mỗi item: CaddieId, PlayerIndex?, Note). Khi có → `CreateFromMiniAppAsync` gọi `CreateInlineCaddieBookingAsync` trong CÙNG UoW: tạo AppCaddieBooking header + AppCaddieBookingDetail mỗi caddie, khóa AppCaddieSchedule (SlotStatus Available=1→Booked=2 nếu có), gán CaddieId/CaddieName/CaddieBookingId(header)/AppCaddieBookingDetailId(detail) vào player theo PlayerIndex, tự tính TotalCaddieFee = count × GolfCourse.CaddieFee, cập nhật AppBookings.TotalCaddieFee + TotalAmount. Khi `CaddieAssignments` null/rỗng → giữ nguyên logic cũ (mini app khác + API đặt Caddie riêng KHÔNG ảnh hưởng). Inject thêm AppCaddieBookingDetail/AppCaddie/AppCaddieSchedule repo vào MiniAppBookingAppService. `POST /api/mini-app/caddie/booking` (đặt Caddie riêng) giữ nguyên.

Build full solution 0 errors. Migration đã apply DB.

## UNIFIED update-booking (2026-07-28 batch 4) — khép kín vòng đời book Caddie kèm golf
- `MiniAppUpdateBookingDto` thêm `CaddieAssignments: List<MiniAppInlineCaddieInput>?` (giống create). Khi có → `UpdateFromMiniAppAsync` gọi `ReconcileInlineCaddieBookingAsync` NGAY SAU ReplacePlayersAsync, trong CÙNG UoW.
- `ReconcileInlineCaddieBookingAsync`: tìm AppCaddieBooking đã liên kết qua players (CaddieBookingId != null); tái dùng (cập nhật ngày/giờ) hoặc tạo mới nếu chưa có/đã hủy. items rỗng → gỡ hết detail + nhả lịch + set AppCaddieBooking.Status=4(Cancelled)+Fee=0, trả 0. Ngược lại: gỡ detail không còn trong list (+ReleaseCaddieScheduleAsync), gỡ liên kết Caddie khỏi TẤT CẢ players của booking rồi gán lại theo PlayerIndex (CaddieId/CaddieName/CaddieBookingId=header/AppCaddieBookingDetailId=detail), thêm detail mới + khóa lịch, tính lại TotalCaddieFee = count × GolfCourse.CaddieFee.
- Sau reconcile: booking golf TotalCaddieFee = fee>0?fee:null; TotalAmount = sum(players) + fee. reconciledPlayers OrderBy(CreationTime) để PlayerIndex khớp input.Players.
- Helper `ReleaseCaddieScheduleAsync` (no-op nếu scheduleId Empty). Dùng SlotStatus magic byte: Available=1, Booked=2; CaddieBooking Status Cancelled=4.
- `CaddieAssignments == null` → giữ nguyên logic cũ hoàn toàn (mini app khác + API caddie riêng POST /api/mini-app/caddie/booking KHÔNG ảnh hưởng).
- Build full solution 0 errors. KHÔNG entity/migration mới ở batch 4 (chỉ DTO field + logic).

## Hủy booking golf → hủy Caddie liên đới + enrich get-bookings list (2026-07-29 batch 5)
- **Cancel cascade** (`CancelFromMiniAppAsync`): sau khi set booking golf CancelledRefund, gọi `CancelLinkedCaddieBookingsAsync(booking.Id)`: tìm players.CaddieBookingId (header, distinct) → mỗi AppCaddieBooking chưa hủy: nhả toàn bộ lịch Caddie (ReleaseCaddieScheduleAsync theo detail.ScheduleId) + Status=4(Cancelled) + TotalCaddieFee=0 + CancelReason. No-op nếu booking golf không có player liên kết Caddie (mini app khác an toàn).
- **get-bookings list enrich** (`GetListMiniAppAsync`): `BookingListData` thêm Players (List<AppBookingPlayerDto> full, kèm CaddieId/CaddieBookingId/AppCaddieBookingDetailId/CaddieName) + Utilities (List<int> từ Utility split) + IsExportInvoice + CompanyName/TaxCode/CompanyAddress/InvoiceEmail (giống detail để mini app edit từ list). Load allListPlayers 1 lần cho cả trang (playersByBooking OrderBy CreationTime), itemById map lấy field từ entity gốc. Caddies filter player.CaddieId!=null.
- Build full solution 0 errors. KHÔNG entity/migration mới (chỉ DTO field + logic).

## Caddie fee + multi-caddie admin + upsert/unassign (2026-07-25)

Nối tiếp [[project_caddie_booking_linked_to_golf_players]] (AppBookingPlayers đã có CaddieId/CaddieBookingId/CaddieName). 4 phần:

### P1 — Booking.TotalCaddieFee + cộng vào TotalAmount
- Entity `Booking` (AppBookings) thêm `decimal? TotalCaddieFee`. DbContext: `HasColumnType("decimal(18,2)")`. Migration `20260725062150_AddTotalCaddieFeeToBooking` (đã database update).
- DTO thêm field: `MiniAppCreateBookingDto`, `MiniAppUpdateBookingDto` (DTO RIÊNG — dễ quên), `CreateUpdateAppBookingDto`, `AppBookingDto`.
- Logic: `TotalAmount = sum(players) + (TotalCaddieFee ?? 0)`. Sửa: MiniAppBookingAppService.CreateFromMiniAppAsync + UpdateFromMiniAppAsync; AppBookingService.CreateAsync + UpdateAsync (admin).
- **ViewModal.cshtml.cs**: `TotalAmountText` tính lại từ sum(players)+caddieFee (không dùng entity.TotalAmount); thêm `TotalCaddieFeeText`.

### FIX 2026-07-26 — CreateFromMiniAppAsync chưa lưu TotalCaddieFee (BUG)
- CreateFromMiniAppAsync ban đầu chỉ cộng `input.TotalCaddieFee` vào `input.TotalAmount` nhưng **quên set `booking.TotalCaddieFee`** → cột AppBookings.TotalCaddieFee luôn null dù đã cộng vào TotalAmount. Đã thêm `booking.TotalCaddieFee = input.TotalCaddieFee;` sau `booking.NumberHole` (trước InsertAsync).
- UpdateFromMiniAppAsync đã đúng từ trước (set booking.TotalCaddieFee + cộng TotalAmount ở lines ~542-543). `ReplacePlayersAsync` xóa hết player cũ rồi insert lại từ input, đã carry CaddieId/CaddieBookingId/CaddieName → cho phép đổi/xóa Caddie của từng người chơi qua update (player gửi thiếu caddie fields = gỡ gán; giá trị mới = đổi). Không cần thêm code cho yêu cầu update caddie.

### FIX 2026-07-27 (batch 2) — MiniApp caddie APIs + admin UI polish + cancel sync + cross-check
- **MiniApp caddie history** (`GetBookingHistoryAsync`): thêm `Caddies` list (List<MiniAppBookingCaddieDetailDto>) vào MiniAppCaddieBookingHistoryDto — trước chỉ flatten first caddie. Load detail.Note + full AppCaddie, build caddieList giống API detail; giữ field phẳng (CaddieName/Avatar/RatingAvg) = first caddie cho tương thích ngược.
- **MiniApp golf booking list+detail** (`GetListMiniAppAsync`+`GetMiniAppAsync`): thêm `Caddies` (List<MiniAppBookingGolfCaddieDto>{CaddieBookingId,CaddieId,CaddieName,PlayerName}) đọc từ AppBookingPlayers (chỉ player CaddieId!=null → mini app khác rỗng). Detail thêm `TotalCaddieFee` từ booking. DTO mới MiniAppBookingGolfCaddieDto trong MiniAppBookingListDto.cs (dùng chung list+detail).
- **Cross-check phí Caddie** (`ResolveCaddieFeeFromLinkedBookingsAsync` — 2 overload cho MiniAppBookingPlayerInput + CreateUpdateBookingPlayerDto): đọc TotalCaddieFee THỰC TẾ từ AppCaddieBooking (qua players CaddieBookingId, sum distinct) làm nguồn chân lý thay input.TotalCaddieFee. Nếu players không có CaddieBookingId → trả null → fallback input cũ (mini app khác không ảnh hưởng). Áp dụng CreateFromMiniAppAsync + UpdateFromMiniAppAsync. Inject IRepository<AppCaddieBooking> vào MiniAppBookingAppService.
- **Caddie change/cancel sync** (CaddieBookingAppService — inject IRepository<BookingPlayer> _golfPlayerRepo + IRepository<Booking> _golfBookingRepo):
  - `ChangeCaddyAsync`: sau khi đổi detail, update AppBookingPlayers (player gán oldCaddieId → CaddieId=new + CaddieName=new).
  - `ClearGolfBookingCaddieLinksAsync(caddieBookingId)`: gán null CaddieBookingId/CaddieId/CaddieName cho players + set AppBookings.TotalCaddieFee=null + TotalAmount=sum(players). Gọi trong UpdateStatusAsync(Cancelled, +TotalCaddieFee=0) và DeleteAsync.
- **Admin ViewModal**: thêm `TotalBookingText` (sum players, chưa gồm phí). Khi có phí Caddie → hiện 3 dòng: Tổng phí Caddy + Tổng tiền Booking + Tổng tiền; không có phí → chỉ Tổng tiền (tenant khác giữ nguyên). Cột Tên Caddy đã có từ trước.
- **Admin EditModal fixes**:
  - Scroll ngang: đổi `<div d-inline-flex width:98%>` (6×col-md-3=150%) → `<div class="row g-3">` với col-md-3 col-sm-6 → tự wrap, KHÔNG scroll ngang.
  - Tổng tiền SAI (BUG): hidden `Booking_TotalCaddieFee` render qua asp-for = "2000000.00" (decimal), `normalizeMoney` strip dấu chấm → "200000000" (×100). Fix `normalizeMoney`: nếu match `/^\d+\.\d+$/` (invariant decimal) thì cắt phần thập phân trước khi lọc digit.
  - Cột Tên Caddie đã có từ prior task (showCaddieCol + hidden preserve CaddieId/CaddieBookingId/CaddieName).
- Build full solution 0 errors. KHÔNG có entity/migration mới (chỉ DTO + projection + logic + UI).

### FIX 2026-07-27 — Admin AppBookings không hiển thị TotalCaddieFee (ROOT CAUSE) + EditModal caddie
- **Root cause list/modal trống**: `AppBookingService.GetListAsync` (projection thủ công ~L300) VÀ `GetAsync` (projection ~L373) đều **quên map `TotalCaddieFee = b.TotalCaddieFee`** dù DB đã lưu. Đã thêm vào cả 2 projection. (AppBookingService KHÔNG dùng AutoMapper cho list/detail — projection thủ công, dễ sót field mới.)
- **List Index.js**: cột "Tổng giá trị booking" (BookingTotalPrice) render = `totalAmount - totalCaddieFee` (giữ giá booking thuần); thêm cột "Tổng cộng" = `totalAmount` (đã gồm phí) ngay sau; cột "Tổng phí Caddy" render rỗng nếu null/0.
- **ViewModal**: đã đúng từ prior task (dòng Tổng phí Caddy trước Tổng tiền, ẩn khi null/0; cột Caddy trong bảng golfer) — giờ hiển thị được vì DTO đã map.
- **EditModal.cshtml.cs**: map `TotalCaddieFee` vào CreateUpdateAppBookingDto + map CaddieId/CaddieBookingId/CaddieName vào từng Players (để preserve khi update — nếu không sẽ bị wipe do ReplacePlayersAsync).
- **EditModal.cshtml**: hidden `Booking.TotalCaddieFee` (data-val=false, luôn post) + group `#CaddieFeeGroup` (readonly, ẩn nếu không có phí); đổi label Tổng tiền → "Tổng tiền Booking"; thêm group `#GrandTotalGroup` "Tổng tiền" = booking + phí (readonly, ẩn nếu không phí). Bảng golfer: cột "Tên Caddie" conditional (`showCaddieCol` = có player nào CaddieName), + hidden CaddieId/CaddieBookingId/CaddieName mỗi player để preserve. `data-show-caddie` trên tbody; Add row JS chèn caddie cell khi showCaddie. JS recalcTotalAmountFromPlayers cập nhật thêm `#Booking_GrandTotal_Display` = total + caddieFee.
- **Multi-tenant safe**: tất cả field/cột caddie ẩn khi TotalCaddieFee null/CaddieName trống → tenant khác (không dùng Caddie module) không bị ảnh hưởng. Chỉ Blue Diamond dùng.
- Build full solution 0 errors. KHÔNG có thay đổi entity/DTO/migration ở lần này (chỉ projection + UI).

### P2 — Admin Lịch sử Đặt Caddy (AppCaddieBookings)
- List: DTO `CaddieBookingDto` thêm `CaddieNames` (nối chuỗi tất cả tên caddie booking). `CaddieBookingAppService.GetListAsync` build từ allBookingDetails+caddies. index.js: cột Caddy đọc `caddieNames` (fallback caddieName), thêm cột "Tổng phí Caddy" (totalCaddieFee).
- Detail (Detail.cshtml.cs): bỏ single-caddie, load `List<BookingCaddieInfo>` (nested class) từ AppCaddieBookingDetails; load `List<CaddieRatingInfo>` group theo `AppCaddieRating.CaddieId` (rating có sẵn CaddieId). Inject thêm `IRepository<AppCaddieBookingDetail>`.
- Detail.cshtml: card Thông tin Caddy loop `Model.Caddies` mỗi caddie 1 block + nút `.btn-change-caddy` data-old-caddie-id; card Chi tiết đánh giá loop `Model.CaddieRatings` mỗi caddie 1 block.
- detail.js: `.btn-change-caddy` truyền oldCaddieId; dropdown loại TẤT CẢ caddie đã book (getBookedCaddieIds từ các nút). `changeCaddy(bookingId, newCaddieId, note, oldCaddieId)`.
- `CaddieBookingAppService.ChangeCaddyAsync` thêm param `Guid? oldCaddieId=null` (target detail theo oldCaddieId, chặn newCaddie trùng caddie đã có).

### P3 — Admin Quản lý booking (AppBookings)
- Index.js: thêm cột "Tổng phí Caddy" (totalCaddieFee) trước cột Tổng tiền; render rỗng nếu null/0.
- ViewModal.cshtml: dòng "Tổng phí Caddy" (@Model.TotalCaddieFeeText) chỉ hiện khi `Booking.TotalCaddieFee>0`, đặt trước TotalAmount; bảng golfer thêm cột "Caddy" (p.CaddieName) chỉ khi `Players.Any(p=>CaddieName!=null)` (biến `showCaddieColumn`).

### P4 — API Caddie Upsert + Unassign (MiniAppCaddieAppService)
- Giữ route `POST /api/mini-app/caddie/booking` (không đổi tên). `MiniAppCreateCaddieBookingDto` thêm `Guid? CaddieBookingId` → có = UPDATE (reconcile: giữ caddie cũ/cập nhật note, thêm caddie mới+khóa schedule, xóa caddie không còn+nhả schedule), null = INSERT.
- **Server tự tính** `TotalCaddieFee = số caddie × GolfCourse.CaddieFee` (helper `ComputeCaddieFeeAsync`), KHÔNG dùng input.TotalCaddieFee nữa.
- API mới `POST /api/mini-app/caddie/booking/unassign-caddie` body `{caddieBookingId, caddieId}` → `UnassignCaddieAsync`: xóa detail+nhả schedule, clear AppBookingPlayers (CaddieId/CaddieName/CaddieBookingId=null của player gắn caddie đó). Hết caddie → AppCaddieBooking.Status=Cancelled+TotalCaddieFee=0. Còn caddie → recompute.
- `SyncGolfBookingCaddieFeeAsync(caddieBookingId, newFee)`: tìm golf booking qua AppBookingPlayers.CaddieBookingId, set TotalCaddieFee + TotalAmount = sum(players)+newFee cho TẤT CẢ booking liên kết.
- Inject thêm vào MiniAppCaddieAppService: `IRepository<Booking>` (_golfBookingRepo) + `IRepository<BookingPlayer>` (_bookingPlayerRepo). Có helper `ReleaseScheduleSlotAsync` riêng (không dùng của CaddieBookingAppService).
- DTO `MiniAppUnassignCaddieDto` trong MiniAppCaddieDtos.cs. Controller trả `MiniAppCreatedCaddieBookingResponse` (message phân biệt insert/update/unassign).

Build full solution 0 errors. Xem [[project_caddie_caddiefee_bookingdetails]] (GolfCourse.CaddieFee), [[feedback_ef_migration_dll_lock]], [[feedback_datatables_custom_ajax]].
