[HL25] Chỉnh API /api/mini-app/hl25/me/frames?zaloUserId=7974578522474094620 trả về resultImageUrl mà front end đọc được không bị chặn.
Bối cảnh
- Trước đây URL đang lưu trên server của tôi thì đọc được bình thường. Ví dụ: https://s3.ods.vn/c01-hoalinh-prod-01/uploads/hl25/650ccd37-aeb4-63e7-bac7-3a23a72f9cbc/7974578522474094620_20261013111809.jpg.
- Nhưng khi đẩy lên S3 Storage thì bị lỗi không đọc được. Ví dụ: https://s3.ods.vn/c01-hoalinh-prod-01/uploads/hl25/650ccd37-aeb4-63e7-bac7-3a23a72f9cbc/7974578522474094620_20261013111809.jpg.
- URL S3 trả 200 OK nhưng kèm Content-Encoding: aws-chunked. Khi tải bằng curl --compressed, tái hiện lỗi (61) Unrecognized content encoding type. URL server cũ tải bình thường.

Yêu cầu:
- Hãy kiểm tra lại API API /api/mini-app/hl25/me/frames?zaloUserId=7974578522474094620 trả về resultImageUrl mà front end zalo mini app có thể đọc được url.
- Gửi lại CURL  API để tôi test và kiểm tra trên front end. Lưu ý: API đã đúng nên không thay đổi cấu trúc response, chỉ kiểm tra lại đường dẫn ảnh để front end Zalo mini app không mở được URL lỗi thông báo như trên bối cảnh.