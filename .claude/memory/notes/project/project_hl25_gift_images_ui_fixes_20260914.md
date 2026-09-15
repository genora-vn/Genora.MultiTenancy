# Hoa Linh 25 — dropdown, lịch sử quay, ảnh kho quà (2026-09-14)

## Yêu cầu và kết quả
- **Dropdown Campaign/Participant/Gift:** bỏ `abp-select` + `abp-select-item` của các enum (ABP vẫn tự sinh option theo tên enum); dùng `<select asp-for>` + `<option>` tường minh với resource `Enum:Hl25*:n`. ASP.NET giữ giá trị đã chọn từ model. Campaign Create/Edit, Participant AgeGroup/Gender và Gift Create/Edit đều dùng cách này; vi/en đã có đủ key.
- **Tab lịch sử lượt quay:** `rowAction.visible` nhận row trực tiếp, không phải `{record}`. Sửa `record.rewardStatus`, guard null/undefined; `action`/`confirmMessage` vẫn dùng wrapper `data.record`. JS test khởi tạo Wheel.js + gọi callback bằng raw row tái hiện đúng contract.
- **Kho quà:** thêm nullable `Hl25Gift.WheelImageUrl` (`nvarchar(1024)`) + Admin read/write DTO + service mapping + EF config. `ImageUrl` vẫn là ảnh thông tin quà khi trúng.
- **Modal:** size Large, shared `_GiftForm.cshtml`, hai vùng ảnh riêng, preview/file PNG-JPG/URL/xóa ảnh. GiftCreate và GiftEdit đều có status select tường minh. `GiftModal.js` được load trước Wheel.js.
- **Upload:** code cũ gọi `uploadGiftImageByFile` KHÔNG có trong service/proxy. Đã bỏ, chuyển multipart form → IFormFile → RemoteStreamContent → `UploadGiftImageAsync` khi lưu. Validate 5MB và PNG/JPG. Service cho upload nếu có Create HOẶC Edit (đúng Tenant/Host + feature), để tài khoản chỉ có quyền tạo vẫn upload được.
- **VNĐ:** input text GiftValue (ví dụ `1.000.000`), formatter giữ precision bằng string, hỗ trợ phần lẻ `,50`; server parse theo định dạng vi-VN, không phụ thuộc culture request và không nhân nhầm do dấu chấm. Parse kiểm tra ngưỡng decimal(18,2). jQuery validator patch có scope cho input tiền, không sửa hành vi các input số khác.

## API GET /api/mini-app/hl25/wheel
Mỗi phần tử `slots` bổ sung:
- `wheelImageUrl`: ảnh WheelImageUrl tại kho quà, full URL, null nếu chưa cấu hình.
- `giftImageUrl`: ảnh ImageUrl của quà trúng, full URL.
- `slotImageUrl` giữ tương thích: **ảnh riêng ô → WheelImageUrl của quà → ImageUrl cũ**. Nếu muốn ảnh mới từ kho quà hiển thị trên ô mà ô đã có override, cần bỏ override riêng của ô hoặc FE dùng wheelImageUrl.
- Không thay đổi tỷ lệ quay, cơ chế cấp lượt, trần trúng; không trả WinRate.

## Migration và trạng thái chạy
- Migration mới **20260914111213_AddHl25GiftWheelImage** chỉ AddColumn nullable WheelImageUrl vào `hl25.AppHl25Gifts`; migration cũ không sửa. Snapshot chỉ thêm property tương ứng.
- SQL idempotent: [hl25_gift_wheel_image_20260914.sql](../../../docs/hl25_gift_wheel_image_20260914.sql), sinh từ mốc `20260908054652_AddHl25ProgramInfoFields` tới migration mới.
- **CHƯA apply DB / CHƯA restart host / CHƯA commit/deploy.** Host `Genora.MultiTenancy.Web` đang chạy giữ DLL (PID quan sát 33848), build mặc định fail copy do lock. Build xác minh dùng OutDir riêng tại thư mục temp `genora-hl25-validation`; không dừng tiến trình người dùng.
- Sau khi áp migration, rebuild/restart Web và reload trang để dùng DLL/localization/JS mới.

## Validation
- Web build với OutDir temp: thành công, 0 errors (còn warnings).
- Application hl25: **17 tests pass**, có 3 trường hợp ảnh mới/fallback cũ/override riêng ô và full URL.
- Web hl25: **14 tests pass** — parse tiền hợp lệ/sai/overflow, edit nạp cả hai ảnh, upload hai file vào đúng field, dán URL không upload. Validation service được mock trong các PageModel unit tests; không phải HTTP integration test.
- Node `test/hl25-ui-regressions.cjs`: **3 tests pass** — callback visibility raw row, format tiền, dropdown explicit có đủ key vi/en.
- EF `has-pending-model-changes --no-build`: no changes sau migration.
- Browser skill/runtime đã thử kết nối, browser list rỗng; không thể xác nhận giao diện chạy thật bằng browser trong phiên. Chưa smoke-test API trên host mới hoặc apply SQL.
