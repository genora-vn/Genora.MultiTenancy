# DbMigrator: kiểm tra database đích và đối chiếu migration history với schema

- Khi migrate nhiều tenant, thử kết nối database đích trước. Chỉ truy cập `master` để tạo database khi SQL Server trả lỗi database không tồn tại (4060/911); lỗi mạng, TLS hoặc quyền phải được báo đúng nguyên nhân. Không đưa connection string đầy đủ vào log hay `BusinessException.Data`.
- `__EFMigrationsHistory` không chứng minh bảng còn tồn tại. Trước khi sửa lỗi migration thiếu bảng, đối chiếu history với schema trên **từng database riêng biệt**; feature flag tenant không tách bộ EF migrations chung.
- Chỉ reset history bằng script có preflight chính xác: khóa tên database, xác nhận toàn bộ schema module trống, history đúng tập migration dự kiến, backup history trong transaction, mặc định dry-run và từ chối chạy lặp. Nếu schema còn một phần, phải thiết kế phương án phục hồi riêng thay vì xóa history hàng loạt.
- Sau khi phục hồi, chạy DbMigrator toàn bộ tenant và kiểm tra số bảng cùng các migration row trên mọi database; đừng chỉ xác nhận database gây lỗi đầu tiên.

Áp dụng thực tế: `docs/HLG_DBMIGRATOR_RECOVERY_20260923.md` và `docs/HLG_TEST1_SCHEMA_REPAIR_20260923.sql`.
