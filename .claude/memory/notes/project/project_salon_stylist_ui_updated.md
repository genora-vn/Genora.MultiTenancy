---
name: Salon Beauty Stylist UI - Updated Design Implementation
description: Cập nhật UI quản lý Stylist theo thiết kế mới với avatar upload, badge system, và styling hiện đại
type: project
originSessionId: c16af227-6da9-4b5b-8fdf-c834e9511fda
---
# Salon Beauty - Quản lý Stylist UI (Updated Design)

Đã hoàn thành cập nhật UI cho quản lý Stylist theo thiết kế mới với avatar upload, badge system màu sắc, và styling hiện đại.

## Files đã cập nhật

### 1. CSS mới - stylist-page.css
- **Tách riêng từ salon-shared.css** để có styling độc lập
- **Badge system** với color coding:
  - Master (Level 5): Yellow (#fff3cd / #856404)
  - Senior (Level 3-4): Blue (#d1ecf1 / #0c5460)
  - Junior (Level 1-2): Red (#f8d7da / #721c24)
  - Manager role: Green (#d4edda / #155724)
- **Avatar upload section**: Clickable area với preview, border dashed, hover effect
- **Form controls**: Rounded borders (8px), focus states với blue shadow
- **Toggle switches**: Inline và modal variants, 44px width
- **Modal structure**: Header với title wrap, body scrollable, footer với buttons

### 2. Index.cshtml
- **CSS reference**: Đổi từ `salon-shared.css` → `stylist-page.css`
- **Class names**: Đổi từ `salon-*` → `stylist-*` (page, filter-card, table-card, btn-primary)
- **Labels**: Chuyển sang tiếng Việt
  - "Quản lý danh sách Stylist"
  - "Tìm kiếm", "Cấp độ", "Chuyên môn", "Trạng thái", "Hiển thị App"
- **Filter structure**: Giữ nguyên 5 filters + search button

### 3. CreateModal.cshtml
- **Avatar upload section** (đầu modal body):
  ```html
  <div class="stylist-avatar-upload" onclick="document.getElementById('AvatarFileInput').click()">
      <div class="stylist-avatar-preview">
          <img id="AvatarPreview" src="" alt="" style="display:none;" />
          <i class="fa fa-camera upload-icon" id="UploadIcon"></i>
      </div>
      <div class="stylist-avatar-upload-text">
          <strong>AVATAR STYLIST (TRÒN, TỐI ĐA 2MB)</strong>
          <span>Click để tải ảnh lên</span>
      </div>
      <input type="file" id="AvatarFileInput" accept="image/*" style="display:none;" />
  </div>
  <input asp-for="Stylist.Avatar" type="hidden" id="AvatarUrlInput" />
  ```
- **Class names**: Đổi từ `salon-*` → `stylist-*` toàn bộ
- **Form class**: `salon-stylist-form` → `stylist-form`
- **Modal class**: `salon-stylist-modal` → `stylist-modal`
- **Labels**: Chuyển sang tiếng Việt hardcoded (không dùng @L[])
- **Xóa salon-input-icon**: Không còn icon trong input fields

### 4. EditModal.cshtml
- **Avatar upload section** với preview existing avatar:
  ```html
  <img id="EditAvatarPreview" 
       src="@(string.IsNullOrEmpty(Model.Stylist.Avatar) ? "" : Model.Stylist.Avatar)" 
       style="@(string.IsNullOrEmpty(Model.Stylist.Avatar) ? "display:none;" : "")" />
  <i class="fa fa-camera upload-icon" id="EditUploadIcon" 
     style="@(string.IsNullOrEmpty(Model.Stylist.Avatar) ? "" : "display:none;")"></i>
  ```
- **File input ID**: `EditAvatarFileInput` (khác với Create)
- **Preview ID**: `EditAvatarPreview`, `EditUploadIcon`, `EditAvatarUrlInput`
- **Cùng cấu trúc** như CreateModal: class names, labels tiếng Việt

### 5. index.js
- **Avatar upload handlers**:
  ```javascript
  $(document).on('change', '#AvatarFileInput, #EditAvatarFileInput', function () {
      var file = this.files[0];
      // Validate size (max 2MB)
      if (file.size > 2 * 1024 * 1024) {
          abp.notify.warn('Kích thước ảnh không được vượt quá 2MB');
          return;
      }
      // Validate type
      if (!file.type.match('image.*')) {
          abp.notify.warn('Vui lòng chọn file ảnh');
          return;
      }
      // Preview with FileReader
      var reader = new FileReader();
      reader.onload = function (e) {
          $(previewId).attr('src', e.target.result).show();
          $(iconId).hide();
          $(urlInputId).val(e.target.result); // Base64
      };
      reader.readAsDataURL(file);
  });
  ```
- **Class name updates**:
  - `salon-status-box` → `stylist-status-box`
  - `salon-status-value` → `stylist-status-value`
  - `salon-showonapp-value` → `stylist-showonapp-value`
  - `salon-submit-button` → `stylist-submit-button`
  - `salon-phone-input` → `stylist-phone-input`
  - `salon-stylist-form` → `stylist-form`
  - `salon-stylist-modal` → `stylist-modal`
  - `salon-inline-showonapp-toggle` → `stylist-inline-showonapp-toggle`
  - `salon-inline-switch` → `stylist-inline-switch`
- **Render functions**: Giữ nguyên logic, chỉ update class names trong HTML output
- **Notification messages**: Chuyển sang tiếng Việt

## Pattern đã áp dụng

1. **Avatar upload pattern**:
   - Click vào div → trigger hidden file input
   - FileReader.readAsDataURL → preview + store base64 vào hidden input
   - Validate: max 2MB, image type only
   - Edit mode: show existing avatar, hide icon khi có ảnh

2. **Badge color mapping**:
   - Level: 1-2 → junior (red), 3-4 → senior (blue), 5 → master (yellow)
   - Role: 1 → junior (red), 2 → senior (blue), 3 → manager (green)

3. **CSS class naming**: `stylist-*` prefix cho tất cả custom classes

4. **Form validation**: Real-time validation, disable submit khi DisplayName empty

5. **Toggle switches**: Cả inline (table) và modal variants

## Avatar Upload Flow

1. **User clicks** avatar upload area
2. **Hidden file input** triggered
3. **Validation**: Check size (≤2MB) và type (image/*)
4. **FileReader** đọc file → base64
5. **Preview**: Show image, hide camera icon
6. **Hidden input**: Store base64 string vào `Stylist.Avatar`
7. **Submit**: Base64 string được POST lên server

## Styling Highlights

- **Border radius**: 8px cho inputs, 12px cho cards/badges
- **Colors**: Blue primary (#0d6efd), gray secondary (#6c757d)
- **Spacing**: 24px page padding, 16px card padding
- **Typography**: 14px body, 13px labels, 12px small text
- **Transitions**: 0.2s cho hover/focus states

## Why

Thiết kế mới cải thiện UX với:
- Avatar upload trực quan hơn (click to upload)
- Badge system dễ phân biệt level/role
- Styling hiện đại, consistent
- Form validation real-time
- Mobile responsive

## How to apply

Khi làm các chức năng Salon Beauty khác (Services, Bookings):
1. Tạo CSS riêng với prefix tương ứng (service-*, booking-*)
2. Avatar upload: dùng pattern FileReader + base64
3. Badge system: map enum → CSS class với màu sắc
4. Form validation: real-time với updateSubmitState
5. Toggle switches: inline (table) + modal variants
