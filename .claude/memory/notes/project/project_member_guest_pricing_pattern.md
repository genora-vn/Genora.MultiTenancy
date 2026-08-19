---
name: MiniApp Member/MemberGuest pricing pattern
description: Cách lấy MemberGuestPrice, VisitorPrice, IsMemberSupported từ CalendarSlotPrices + GolfCourse config
type: project
originSessionId: 1d4a0f8f-d6a1-47b3-80bb-c4889c494f00
---
GolfCourse có 2 field mới: `IsMemberSupported` (bool), `MaxMemberGuest` (int?).

CustomerType codes liên quan:
- `MB`  = Member
- `MBG` = Member Accompanied Guest
- `VIS` = Visitor

**Pattern lấy giá trong API MiniApp (CalendarSlots + BookingDetail):**

- `VisitorPrice` = lookup `AppCalendarSlotPrices` WHERE `CustomerTypeId = VIS.Id` AND `CalendarSlotId`, rồi dùng `PriceByHoleHelper.GetPriceByNumberHoles(row, numberOfHoles)`
- `MemberGuestPrice` = lookup tương tự với `MBG.Id`, CHỈ trả về khi `golfCourse.IsMemberSupported == true` AND `currentCustomerType.Code == "MB"`
- `IsMemberSupported` = `golfCourse.IsMemberSupported` (luôn trả)
- `MaxMemberGuest` = `golfCourse.MaxMemberGuest` nếu `IsMemberSupported=true`, else `null`

**Pattern tính OriginalPrice/OriginalTotalAmount:**
- `OriginalPrice` (per golfer) = `CustomerType.OriginalPrice` của loại KH đang book (fallback logic phức tạp nếu chưa cấu hình)
- `OriginalTotalAmount` = `CustomerType.OriginalPrice * numberOfGolfers`
- KHÔNG lấy từ `AppCalendarSlotPrices` VIS row cho trường này

**Why:** Frontend cần phân biệt giá gốc (theo hạng KH) với giá niêm yết (VIS) để tính discount % và hiển thị đúng tổng tiền booking Member/Member Guest.

**How to apply:** Bất kỳ API MiniApp nào trả dữ liệu booking hoặc calendar slot cần đủ 5 field này để frontend tính tổng tiền đúng khi có Member trong nhóm.
