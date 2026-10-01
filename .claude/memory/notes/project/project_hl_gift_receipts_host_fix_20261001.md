# Hoa Linh Sales — sửa Host/local nhận quà — 2026-10-01

## Yêu cầu và nguyên nhân

User test local bằng Host, nhận `HlGiftReceipt:TenantRequired`; xác nhận Host phải lưu `TenantId = null`, còn staging/production dùng domain+database tenant riêng. Admin Host/Tenant sử dụng theo quyền của ngữ cảnh tương ứng.

Nguyên nhân: `MiniAppHlGiftReceiptService.TenantAsync()` chặn null trước khi gọi DMS hoặc repository. Trang Admin/service/menu đã có nhánh quyền Host hợp lệ; lỗi nằm ở luồng tạo/đọc phiếu Mini App. Chỉ xóa guard là chưa đủ: entity ctor và custom repository nhận Guid không nullable; raw SQL `TenantId = @id` không match NULL; unique index ban đầu có filter `TenantId IS NOT NULL`, không bảo vệ phiếu Host.

## Thay đổi

- Mini App đổi thành `CheckScopeAsync(): Task<Guid?>`: lấy đúng CurrentTenant.Id; Host không kiểm tra tenant features, Tenant vẫn cần Management+GiftReceipts. Không đoán/fallback sang tenant khác, không đổi database bằng tay, không nhận TenantId từ client.
- `HlGiftReceipt` constructor + `IHlGiftReceiptRepository.FindForConfirmationAsync` và EF implementation nhận `Guid?`.
- Query khóa có hai nhánh SQL parameterized: Host `TenantId IS NULL`, Tenant `TenantId = @id`; giữ UPDLOCK/HOLDLOCK và transaction tới commit.
- Giữ unique index Tenant cũ. Thêm unique index Host trên `(CustCode,CampaignCode,CampaignPeriod,VoucherCode)` với filter `TenantId IS NULL`.
- Migration **`20261001025429_AddHlGiftReceiptHostUniqueness`**, Designer/snapshot và `Migrations/Scripts/AddHlGiftReceiptHostUniqueness.sql`. Chỉ CREATE INDEX mới, không sửa migration cũ/table/data/index Tenant. Chưa apply vào DB cấu hình trong phiên này.
- Admin/page/menu vẫn phân quyền Host `MultiTenancy.HostAppHlGiftReceipts` + `.Export` và Tenant `MultiTenancy.AppHlGiftReceipts` + `.Export`. Không nới quyền admin hoặc đọc xuyên tenant. Host đọc/xuất đúng phiếu null TenantId; Tenant chỉ phiếu của mình.
- Mini App giữ AllowAnonymous theo pattern Sales/Blouse hiện có để FE test local; permission bắt buộc ở admin API/page. DMS vẫn xác minh phone→branch và voucherType2 entitlement trước khi tạo phiếu. Không thêm auth-token protocol hoặc đổi contract FE.

## Kiểm thử thực chạy

- Application: **63/63 HoaLinhSales** (28 receipt tests). Bổ sung Host tạo phiếu null/retry/history/admin detail/export; cùng mã entitlement Host/Tenant độc lập; không đọc chéo; tenant features disabled vẫn chặn; Host không có export grant vẫn bị từ chối; locked recheck cho cả Host và Tenant.
- Web: **12/12 HlGiftReceipt**. Có **2 HTTP local tests** dùng ASP.NET TestServer đi qua MVC routing → controller thật → business service thật: Host null/Tenant, POST200/isConfirmed true, retry cùng ID, GET200/count1. DMS và repository/UOW là fake cô lập (không gọi production, không ghi DB). Page Host/Tenant có grant hiển thị Export, thiếu grant bị chặn.
- EF: **4/4**, gồm index riêng cho Host và migration chỉ thêm index. Có check DI registration và model/index Tenant cũ.
- `dotnet ef migrations has-pending-model-changes --no-build`: model khớp snapshot. SQL script được sinh qua EF, review chỉ CREATE INDEX + history row. `git diff --check` sạch.
- Build Web **thành công, 0 errors / 388 warnings**, với OutputPath `artifacts/gift-receipts-validation/web/` riêng để không dừng Web/VS của user. Không chạy .NET builds song song vì share obj.
- Không UAT browser thật, không chạy request vào instance Web đang dùng DMS/DB thật, không test SQL concurrency/load thật, không tự apply migration hoặc deploy. Kết quả HTTP ở trên là local TestServer, không phải chứng nhận DMS staging đã chấp nhận entitlement.

## Cách tiếp tục/test bằng FE

- LaunchSettings Web xác nhận URL `https://localhost:44374/`. Khi không gửi tenant header/cookie và hostname localhost, API dùng Host/null + database Default. Kiểm tra cấu hình DMS ở Host đã có như các API Sales khác.
- Trong thư mục EF, kiểm tra connection của DbMigrator đúng DB cần cập nhật trước khi chạy `dotnet ef database update 20261001025429_AddHlGiftReceiptHostUniqueness`. Nếu chưa có migration tạo bảng ban đầu, EF apply theo chuỗi; nếu đã có, chỉ thêm index. Không khẳng định user chưa apply migration cũ vì user có thể tự chạy.
- Restart/rebuild Web, cấp Host root/Export permissions; gọi POST/GET đường dẫn cũ với base URL local. Admin `/HoaLinh/GiftReceipts` hiển thị phiếu Host.
- Staging/production vẫn theo tenant URL+DB hiện có; giữ feature và permissions Tenant. Không thay TenantAutoMigrateMiddleware/gateway/appsettings của user.
- [API/cURL/runbook cập nhật](../../../../docs/HOALINH_GIFT_RECEIPTS_API_20261001.md). Note này supersede guard TenantRequired và việc Host chưa tạo được dữ liệu trong bản bàn giao trước.
