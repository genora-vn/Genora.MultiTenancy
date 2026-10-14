[HL25] Kiểm tra và sửa lỗi API Tạo thiêp (CreateFrame) và API Upload image (upload-image) trong HoaLinh25MiniAppController bị duplicate ảnh trên S3 Storage gây bộ nhớ bị chiếm một cách dư thừa.
STATUS: DONE — implementation và memory đã cập nhật trên branch hotfix/20261014; chưa commit/deploy.
Bối cảnh
- Ảnh đã được đẩy lên S3 Storage thành công tuy nhiên đang có 2 ảnh giống nhau trên S3 Storage. Ví dụ: File ảnh 7974578522474094620_20261014155831.jpg và 8b9ac15dddde4911ada41b3d54cdd786.jpg.
- Kiểm tra đoạn tôi chẩn đoán là liên quan đến class CopyAsync và đoạn check khi front end mini app truyền RenameWithTimestamp để tên ảnh đúng cấu trúc ZaloUserId_yyyyMMddHHmmss.ext.
- Trường hợp AppSettings không bật cấu hình S3 mà sử dụng cấu hình Local thì mới lưu trên window server của tôi. Còn bật "Provider": "S3" thì lưu duy nhất 1 ảnh lên S3 Storage.

Yêu cầu:
- Hãy kiểm tra lại API API Tạo thiêp (CreateFrame) và API Upload image (upload-image) trong HoaLinh25MiniAppController xử lý vấn đề ảnh bị duplicate trên S3 Storage.
- Tôi chỉ muốn trên S3 Storage lưu duy nhất 1 ảnh có cấu ZaloUserId_yyyyMMddHHmmss.ext (Ví dụ: 7974578522474094620_20261014155831.jpg), để tracking báo cáo sau này.
- Sau khi xử lý xong cập nhật kết quả vào file này.

Kết quả xử lý 2026-10-14:
- Root cause: `upload-image` gọi `ManageImageService.UploadImageAsync` sinh tên GUID, PutObject lên S3. `CreateFrame` với `RenameWithTimestamp=true` gọi `CopyAsync`/CopyObject sang tên ZaloUserId_timestamp; copy không xóa nguồn.
- `MiniAppHl25Service` giờ xác định nguồn tạm đúng public S3 URL, `uploads/hl25/{tenantId}` và tên GUID. Sau copy + lưu frame + commit UOW mới xóa nguồn nếu chưa có frame cũ tham chiếu. Nếu nguồn đã mang tên ZaloUserId_timestamp thì không copy lại; nếu copy không tìm thấy nguồn thì báo lỗi thay vì lưu URL GUID.
- `ManageImageService.CopyAsync` không còn coi mọi URL HTTP tuyệt đối là S3: URL Local sau ToFullUrl vẫn được đọc từ Local; đích ghi theo Provider/whitelist. Khi nguồn và đích khác provider, copy bytes sang provider đích. Provider=Local không tạo object S3 mới trong CreateFrame.
- `upload-image` vẫn nhận một file và trả cùng DTO URL. Vì request này không có ZaloUserId, GUID vẫn cần làm tên tạm cho tới khi `CreateFrame` thành công. Sau luồng bình thường trên S3 chỉ còn object ZaloUserId_yyyyMMddHHmmss.ext. `CopyAsync` giữ nguyên nghĩa copy và không tự xóa nguồn; Local vẫn ghi wwwroot theo cấu hình.
- Files code: `src/Genora.MultiTenancy.Application/AppServices/Hl25/MiniAppHl25Service.cs`, `src/Genora.MultiTenancy.Application/AppServices/AppImages/ManageImageService.cs`; test: `test/Genora.MultiTenancy.Application.Tests/Hl25/Hl25MiniAppTurnTests.cs`, `test/Genora.MultiTenancy.Application.Tests/AppImages/ManageImageServiceStorageSelectionTests.cs`.
- Test: 66/66 pass (toàn bộ HL25 + AppImages, gồm lỗi CopyAsync và xóa nguồn thất bại); HttpApi build 0 errors, 9 warnings. Không migration, không đổi HTTP API contract/response. Chưa test bucket S3 thật.
- Giới hạn: upload bị bỏ dở, S3 delete thất bại hoặc GUID đã được frame cũ tham chiếu thì nguồn được giữ lại; không tự dọn ảnh lịch sử. Tên timestamp chỉ có giây nên 2 thiệp cùng Zalo ID trong một giây có thể trùng key; cần theo dõi nếu có traffic như vậy.
