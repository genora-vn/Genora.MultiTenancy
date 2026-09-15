# Hoa Linh 25 — Admin update 2026-09-14

User yêu cầu triển khai approach sau phiên khôi phục context; sau đó chốt **tạo thiệp nhận lượt đầu, chia sẻ nhận lượt thứ hai**.

Đã triển khai thay đổi Domain/Application/DTO/Razor/JS/localization cho 5 nhóm Admin và MiniApp; kiểm tra 32 test pass (18 Domain + 14 Application), JS syntax/JSON, build solution thành công. EF xác nhận không thay đổi model so với migration cuối. Không sửa schema/dữ liệu cũ, không apply DB/commit/push/deploy.

Bảng trước/sau, định nghĩa báo cáo, tương thích API và checklist UAT ở [HOALINH25_ADMIN_UPDATE_20260914.md](../../../docs/HOALINH25_ADMIN_UPDATE_20260914.md).

Điểm phải nhớ:
- EarnedCycles giữ tên cũ nhưng là số lượt tự nhận; không còn nghĩa hai chu kỳ tạo+chia sẻ. Không thu hồi người đã có đủ hai lượt theo code cũ.
- AdminGrant không áp trần 2; số dư+sổ nhận lượt dùng transaction. Giới hạn trúng một lần giữ nguyên.
- Chỉ Won có GiftId → Delivered; gọi xác nhận lại không thay timestamp.
- UI phải gửi lại Id/SlotImageUrl/ColorHex của ô, ảnh nền/kim và màu ẩn để không mất dữ liệu khi Save. Backend giữ ID ô hiện có.
- UserFriendlyException dùng named `code:` và `message:`; constructor vị trí cũ đã làm code/message sai.
- Báo cáo chỉ tính quà Won/Delivered và bao gồm hết ngày ToDate; kho là hiện tại; lượt Admin hiển thị riêng.
- Test dùng mock/in-memory query; chưa chứng minh transaction rollback/concurrency bằng SQL Server, chưa UAT browser. DB đích vẫn cần xác minh migration 20260908054652 khi triển khai.
- Giữ nguyên thay đổi appsettings/log của user. Không sửa module gamification parked.
