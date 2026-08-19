---
name: customer-type-original-price-by-special-date
description: AppCustomerTypes có 4 trường OriginalPrice (Weekday/Weekend/Holiday/MemberDay); modal render input động theo AppSpecialDates; GetCalendarSlots resolve price theo PlayDate
metadata: 
  node_type: memory
  type: project
  originSessionId: eccc6396-1889-4ab3-a51a-86af66f59b8e
---

# CustomerType OriginalPrice theo loại ngày (Weekday/Weekend/Holiday/MemberDay)

**Ngày:** 2026-05-22

## Schema thay đổi

`AppCustomerTypes` thêm 3 cột:
- `OriginalPriceWeekend` decimal(18,2)? — Giá gốc Ngày cuối tuần
- `OriginalPriceHoliday` decimal(18,2)? — Giá gốc Ngày lễ
- `OriginalPriceMemberDay` decimal(18,2)? — Giá gốc Member day

Giữ nguyên `OriginalPrice` = Giá gốc Ngày trong tuần (Weekday).

Migration: `20260522040252_Add_OriginalPriceWeekendHolidayMemberDay_To_AppCustomerTypes`.

## Modal Create/Edit dynamic input

Modal `/AppCustomerTypes/CreateModal` + `/EditModal` không hardcode "Giá gốc" nữa, mà render input động dựa trên `AppSpecialDates.GetListAsync()`:
- Mỗi `SpecialDate` (IsActive=true) match canonical name → 1 input tương ứng:
  - "Ngày trong tuần" → `OriginalPrice`
  - "Ngày cuối tuần" → `OriginalPriceWeekend`
  - "Ngày lễ" → `OriginalPriceHoliday`
  - "Member day" → `OriginalPriceMemberDay`
- Label hiển thị: "Giá gốc trong tuần", "Giá gốc cuối tuần", "Giá gốc ngày lễ", "Giá gốc Member day".
- Nếu không có SpecialDate active → fallback render 1 input "Giá gốc" (OriginalPrice) để không phá UX cũ.

**Mapping helper:** `CustomerTypeOriginalPriceFieldMap` (Application.Contracts) — `ResolveField`, `ResolveLabel`, `GetValue`, `SetValue`.
**Build helper:** `CreateModalModel.BuildFields(specialDates, dto)` — dùng chung cho cả Create và Edit (Edit gọi qua static).

## Logic GetCalendarSlots resolve price

`MiniAppCalendarSlotService` inject thêm `IRepository<SpecialDate, Guid>`. Helper `CustomerTypeOriginalPriceResolver` (Application/Helpers):

**Priority resolve kind theo `PlayDate`:**
1. **Holiday** — match `DatesJson` của entry "Ngày lễ" (so sánh `Date == playDate.Date`)
2. **MemberDay** — match `WeekdaysMask` của entry "Member day" (override Weekday/Weekend khi trùng thứ)
3. **Weekend** — match `WeekdaysMask` của entry "Ngày cuối tuần"
4. **Weekday** — fallback default

**Weekday index convention:** ABP `WeekdaysMask` dùng Mon=0..Sun=6 (bitmask). DateTime.DayOfWeek (Sun=0..Sat=6) → convert bằng `((int)date.DayOfWeek + 6) % 7`.

**GetOriginalPriceByKind:** trả về `OriginalPrice*` tương ứng kind, fallback về `OriginalPrice` (Weekday) nếu kind đó không có cấu hình giá hoặc <=0.

**Đã apply trong:**
- `GetListMiniAppAsync` — `item.OriginalPrice`, `OriginalBillTotalPrice` (cả nhánh isCurrentMember và Visitor)
- `GetMiniAppAsync(GetMiniAppCalendarSlotDetailInput)` — `dto.OriginalPrice`, `OriginalBillTotalPrice` (cả 2 nhánh)
- `OriginalPriceSource` field log dạng `CustomerType:{Code}:{Kind}` để debug.

**Chưa apply** (vẫn dùng `OriginalPrice` flat): `MiniAppBookingAppService.CreateFromMiniAppAsync` — chưa resolve theo PlayDate. Nếu cần đồng bộ snapshot khi đặt booking, follow-up task tách riêng.

## Why
Mỗi loại khách hàng cần giá gốc khác nhau theo ngày trong tuần / cuối tuần / lễ / Member day để Mini App hiển thị giá gốc & tính discount đúng. Trước đây chỉ có 1 trường `OriginalPrice` không phản ánh được sự khác biệt giá theo loại ngày → discount tính sai khi PlayDate rơi vào cuối tuần/lễ.

## How to apply
- Tạo loại ngày mới trong `AppSpecialDates` chỉ work với 4 canonical name đã list. Tên khác → không generate input price.
- Khi user nhập giá ở modal, hidden input `name="CustomerType.OriginalPrice*"` được sync từ display input qua class `.ct-price-display` + `data-target` (jQuery delegate).
- Khi check giá ở `MiniAppCalendarSlotService`, không hard-code `currentCustomerType.OriginalPrice` nữa — dùng `CustomerTypeOriginalPriceResolver.GetOriginalPriceByKind(ct, slotKind)`.
- MemberDay = Weekday-index trùng (vd Thứ 5) sẽ override Weekday do priority MemberDay > Weekend > Weekday.

## File map
- Domain: `DomainModels/AppCustomerTypes/CustomerType.cs`
- EF: `EntityFrameworkCore/MultiTenancyDbContextModelCreatingExtensionsMiniApp.cs` + Migration `20260522040252_*`
- App.Contracts: `AppDtos/AppCustomerTypes/{AppCustomerTypeDto,CreateUpdateAppCustomerTypeDto,CustomerTypeOriginalPriceFieldMap}.cs`
- App: `Helpers/CustomerTypeOriginalPriceResolver.cs`, `AppServices/AppCalendarSlots/MiniAppCalendarSlotService.cs` (+inject `IRepository<SpecialDate, Guid>`)
- Web: `Pages/AppCustomerTypes/{CreateModal,EditModal}.cshtml(.cs)`

## Related
- [[feedback_money_input_validation]]
- [[feedback_ef_migration_dll_lock]]
- [[project_promotion_policy_feature]]
