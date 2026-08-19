---
name: GolfCourse Member config fields
description: GolfCourse entity có IsMemberSupported và MaxMemberGuest, dùng để kiểm soát booking policy Member
type: project
originSessionId: 1d4a0f8f-d6a1-47b3-80bb-c4889c494f00
---
Entity `GolfCourse` (bảng `AppGolfCourses`) có 2 field mới:

```csharp
public bool IsMemberSupported { get; set; } = false;
public int? MaxMemberGuest { get; set; }
```

- `IsMemberSupported`: sân có áp dụng chính sách Member/Member Guest không
- `MaxMemberGuest`: số lượng Guest tối đa mà 1 Member có thể đưa vào booking (chỉ áp dụng khi `IsMemberSupported = true`)

UI tại trang cấu hình sân golf (EditModal, CreateModal) section "Chính sách & điều khoản", cùng hàng với Loại ưu đãi / Số giờ hoãn hủy.

Migration: `Add_IsMemberSupported_MaxMemberGuest_To_GolfCourse` (tạo trong session này).

**Why:** Phục vụ kiểm soát booking rule, pricing policy Member Guest, tránh sai tổng tiền.

**How to apply:** Khi cần biết sân có Member policy không, luôn đọc từ `GolfCourse.IsMemberSupported` thay vì hard-code hay config nơi khác.
