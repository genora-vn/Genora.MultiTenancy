---
name: Salon Beauty Stylist UI Implementation
description: Hoàn thành UI quản lý Nhân viên (Stylist) cho module Salon Beauty - trang danh sách, modal thêm/sửa, filter, inline toggle
type: project
originSessionId: c16af227-6da9-4b5b-8fdf-c834e9511fda
---
# Salon Beauty - Quản lý Nhân viên (Stylist) UI

Đã hoàn thành implementation UI cho quản lý Nhân viên (Stylist) theo pattern của SalonBeautyCustomers.

## Files đã tạo

### 1. Trang danh sách
- **Index.cshtml**: Trang danh sách với bộ lọc và DataTable
  - Bộ lọc: Keyword (ID/tên), Level (dropdown), Role (dropdown), Status (dropdown), IsShowOnApp (dropdown)
  - Cột hiển thị: Avatar+Name, Level, Role, ExperienceYear, RatingAvg, TotalBooking, Status, IsShowOnApp
  - Cột IsShowOnApp có inline toggle switch (chỉ khi có quyền Edit)
  - Actions: Edit và Delete
  
- **Index.cshtml.cs**: Code-behind đơn giản (chỉ OnGet)

### 2. Modal Thêm mới
- **CreateModal.cshtml**: Form thêm mới nhân viên
  - Fields: DisplayName*, Phone, Gender, Role, Level, ExperienceYear, Avatar, SortOrder, Note
  - 2 radio switch buttons:
    - Status (Trạng thái Hoạt động): Active/Inactive
    - IsShowOnApp (Hiển thị trên Mini App): Yes/No
  - Validation: DisplayName required
  
- **CreateModal.cshtml.cs**: Code-behind với BuildSelectLists cho Gender, Role, Level

### 3. Modal Cập nhật
- **EditModal.cshtml**: Form cập nhật nhân viên (giống CreateModal)
- **EditModal.cshtml.cs**: Code-behind load data từ service, có UpdateSalonBeautyStylistDto local class

### 4. JavaScript
- **index.js**: Logic xử lý DataTable, filter, modal, inline toggle
  - DataTable với server-side processing
  - Filter auto-reload khi change
  - Inline toggle IsShowOnApp: click → get full entity → update → reload table
  - Form validation: DisplayName required, phone normalize (0-9 only, max 11 chars)
  - Modal lifecycle: mark clean, suppress unsaved confirm, cleanup DOM
  - Status và ShowOnApp toggle handlers trong modal

## Pattern đã áp dụng

1. **ABP Permission pattern**: Check tenant/host permission riêng biệt
2. **Salon shared CSS**: Dùng `/pages/salon/salon-shared.css` (đã có sẵn)
3. **Modal lifecycle**: Mark clean → suppress confirm → cleanup DOM (tránh dirty form warning)
4. **Inline toggle**: Get full entity → update single field → reload table
5. **Form validation**: Real-time validation, disable submit khi invalid
6. **Phone normalization**: Strip non-digit, max 11 chars

## Enums đã sử dụng

- **SalonBeautyGender**: Male=1, Female=2, Other=3
- **SalonBeautyStylistRole**: Junior=1, Senior=2, Manager=3
- **SalonBeautyStylistLevel**: Level1=1, Level2=2, Level3=3, Level4=4, Level5=5
- **Status**: byte 0=Inactive, 1=Active
- **IsShowOnApp**: bool true/false

## Localization keys cần thêm

Cần thêm các key sau vào file localization (vi.json / en.json):

```
SalonBeautyStylists:PageTitle
SalonBeautyStylists:PageSubTitle
SalonBeautyStylists:AddStylist
SalonBeautyStylists:KeywordPlaceholder
SalonBeautyStylists:AllLevels
SalonBeautyStylists:AllRoles
SalonBeautyStylists:AllStatus
SalonBeautyStylists:AllShowOnApp
SalonBeautyStylists:CreateTitle
SalonBeautyStylists:CreateSubTitle
SalonBeautyStylists:EditTitle
SalonBeautyStylists:EditSubTitle
SalonBeautyStylists:DisplayNamePlaceholder
SalonBeautyStylists:PhonePlaceholder
SalonBeautyStylists:GenderPlaceholder
SalonBeautyStylists:RolePlaceholder
SalonBeautyStylists:LevelPlaceholder
SalonBeautyStylists:AvatarPlaceholder
SalonBeautyStylists:NotePlaceholder
SalonBeautyStylists:StatusBoxTitle
SalonBeautyStylists:StatusBoxDescription
SalonBeautyStylists:ShowOnAppBoxTitle
SalonBeautyStylists:ShowOnAppBoxDescription
SalonBeautyStylists:RequiredFormWarning
SalonBeautyStylists:DeleteConfirm
SalonBeautyStylists:UpdateShowOnAppFailed
SalonBeautyStylists:ProxyNotFound

SalonBeautyStylist:DisplayName
SalonBeautyStylist:Phone
SalonBeautyStylist:Gender
SalonBeautyStylist:Role
SalonBeautyStylist:Level
SalonBeautyStylist:ExperienceYear
SalonBeautyStylist:Years
SalonBeautyStylist:RatingAvg
SalonBeautyStylist:TotalBooking
SalonBeautyStylist:Status
SalonBeautyStylist:StatusActive
SalonBeautyStylist:StatusInactive
SalonBeautyStylist:IsShowOnApp
SalonBeautyStylist:ShowOnAppYes
SalonBeautyStylist:ShowOnAppNo
SalonBeautyStylist:Avatar
SalonBeautyStylist:SortOrder
SalonBeautyStylist:Note

Enum:SalonBeautyGender.Male
Enum:SalonBeautyGender.Female
Enum:SalonBeautyGender.Other
Enum:SalonBeautyStylistRole.Junior
Enum:SalonBeautyStylistRole.Senior
Enum:SalonBeautyStylistRole.Manager
Enum:SalonBeautyStylistLevel.Level1
Enum:SalonBeautyStylistLevel.Level2
Enum:SalonBeautyStylistLevel.Level3
Enum:SalonBeautyStylistLevel.Level4
Enum:SalonBeautyStylistLevel.Level5
```

## Next steps

1. **Thêm localization keys** vào `Domain.Shared/Localization/MultiTenancy/vi.json` và `en.json`
2. **Thêm menu item** vào navigation menu (nếu chưa có)
3. **Test chức năng**:
   - Danh sách hiển thị đúng
   - Filter hoạt động
   - Thêm mới nhân viên
   - Cập nhật nhân viên
   - Xóa nhân viên
   - Inline toggle IsShowOnApp
4. **Tiếp tục các chức năng khác** của Salon Beauty module

## Why

Task này là phần đầu tiên của module Salon Beauty, tạo nền tảng UI pattern cho các chức năng khác (Services, Bookings, etc.). Pattern này đã được validate qua SalonBeautyCustomers và có thể tái sử dụng.

## How to apply

Khi làm các chức năng Salon Beauty khác (Services, Bookings, Loyalty), tham khảo pattern này:
- Bố cục trang: salon-page-head + salon-filter-card + salon-table-card
- Modal: salon-modal-header + salon-modal-body + salon-modal-footer
- Toggle switch: salon-status-box + salon-switch
- Form validation: real-time với updateSubmitState
- Modal lifecycle: markClean + suppressConfirm + cleanupDOM
