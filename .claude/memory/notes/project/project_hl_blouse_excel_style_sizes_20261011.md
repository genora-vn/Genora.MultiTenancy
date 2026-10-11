# Hoa Linh Sales — Excel Blouse theo dáng/size — 2026-10-11

## Phạm vi

- User gọi chung là HLG, nhưng trang `/HoaLinh/BlouseRegistrations` thuộc Hoa Linh Sales (schema HL). Chỉ cập nhật export, không đổi module game HLG.
- Branch `hotfix/20261011`, starting HEAD `6156986`. Giữ nguyên các thay đổi appsettings và log có sẵn; không commit/push/deploy.
- File tham chiếu được đọc trực tiếp, không sửa: `C:\Users\DPC\Downloads\HoaLinhSales_BlouseRegistrations_20261006_114724.xlsx`, sheet `Đăng ký áo Blouse`, Q4:X13. Không sao chép dữ liệu khách production vào repo/test.

## Bố cục / tính số lượng

- Hai dòng tiêu đề; Q1:T1 merge `Dáng Nam`, U1:X1 merge `Dáng Nữ`; mỗi nhóm M/L/XL/2XL tại dòng 2. A:P và Y:AB merge dọc dòng 1–2. Freeze 2 dòng; giữ dòng trống 3 theo mẫu, đơn bắt đầu dòng 4. AutoFilter A2:AB cuối dữ liệu.
- Q/R/S/T = Nam M/L/XL/2XL; U/V/W/X = Nữ M/L/XL/2XL. Y trạng thái, Z ghi chú thêm, AA ghi chú nội bộ, AB ngày đăng ký.
- Tính trực tiếp từ `HlBlouseRegistration.Items`: group `(Style, SizeCode.Trim().ToUpperInvariant())`, SUM Quantity gồm cả áo Free và Exchange, cộng các dòng trùng. Dùng snapshot của đơn, không join cấu hình size hiện tại, không parse chuỗi mô tả.
- Số lượng là Excel Number, không có số lượng để trống theo mẫu. Mã KH/SĐT giữ Text, ngày AB giữ DateTime. O/P vẫn giữ chi tiết áo tặng/đổi, kể cả size ngoài M/L/XL/2XL nếu catalog được mở rộng sau này (các cột mới chỉ thể hiện bốn size yêu cầu).
- Hai chỗ mẫu nhập lệch được báo rõ với user và xuất theo chi tiết đơn: O4 `Nữ L x2` phải vào **V4**, mẫu nhập U4; O12/P12 `Nam 2XL x2` + `Nam 2XL x10` phải vào **T12=12**, mẫu nhập X12.

## Code / kiểm thử

- `Application/AppServices/HoaLinh/HlSalesExportAppService.cs`: CreateBlouseRegistrationSheet, 8 số lượng, đẩy cột cuối sang Y:AB. `Finish` thêm optional headerRow=1; chỉ Blouse truyền 2, các export Sales khác giữ layout cũ.
- Existing endpoint GET `api/app/hl-sales-excel/blouse-registrations` và nút sales.download đã đúng, không cần đổi JS/controller/DTO. Giữ bộ lọc text/status/date, xuất toàn bộ kết quả không phân trang, quyền Host/Tenant cũ.
- `test/.../HoaLinhSales/HlSalesExportTests.cs` chuyển partial để tái dùng fixture; file mới `HlSalesBlouseExportTests.cs` chứa **8 cases**: layout/native types; 10 dòng mẫu; đủ 8 buckets + duplicate/case/trim; date end-of-day + status/text + ignore paging; empty report; inverted dates; permission Host/Tenant.
- `dotnet test ...Application.Tests --no-restore -c Release --filter FullyQualifiedName~HoaLinhSales`: **71/71 PASS** (gồm các export Sales cũ).
- Application baseline build PASS; Web Release build **0 errors /71 existing warnings**. `git diff --check` PASS. Logs: ignored `artifacts/blouse-excel-20261011/`.
- Test gọi đúng export service, ghi XLSX vào stream rồi mở lại bằng ClosedXML để kiểm tra cell/merge/filter/type. Runtime preview của skill Spreadsheets không khả dụng; không thực hiện visual Excel/browser UAT. Mẫu được đối chiếu qua OpenXML read-only.
- Không migration, seed, API contract change hoặc DB writes. Không sửa file Excel nguồn, không tải/xuất dữ liệu production mới.

## Bàn giao

- Deploy/restart Web theo quy trình hiện có, vào `/HoaLinh/BlouseRegistrations`, chọn bộ lọc → Xuất Excel.
- Kiểm tra Q:X, cộng áo tặng+đổi, thử filter/sum theo size trong Excel. Chưa deploy hoặc UAT browser/Excel native trong task.
