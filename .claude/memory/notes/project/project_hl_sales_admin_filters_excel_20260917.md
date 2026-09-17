# Hoa Linh Sales — lọc ngày và Excel Admin (2026-09-17)

## Phạm vi và tên gọi
- **Hoa Linh Sales** = module Hoa Linh cũ, Mini App Dược Phẩm Hoa Linh – Hoa Linh Gắn Kết (Hoa Linh Miền Nam).
- Database nghiệp vụ: **HoaLinhMienNam**, schema **HL**. Không phải HL25 (hl25) hay Gamification (HLG).
- Nhánh đang làm: `feature/hoalinh-sales`, baseline HEAD `501c10c`.

## Đã sửa
- `/HoaLinh/PointHistory`: input text + flatpickr dd/MM/yyyy, placeholder từ/đến ngày; chuẩn hóa ISO trước request để tránh lỗi DateFrom/DateTo với 17/09/2026. Kiểm tra ngày thật và from<=to ở JS, kiểm tra khoảng ngày ở backend. Cả tab giao dịch và lô điểm đều lọc ngày; giao dịch theo CreationTime, lô theo ExchangedAt; >= đầu ngày, < đầu ngày kế tiếp.
- `/HoaLinh/GiftExchanges`: lọc ngày CreationTime + từ khóa + trạng thái (kể cả Failed=0); cùng logic cho list và Excel.
- Excel cho cả ba trang, toàn bộ kết quả lọc, không áp phân trang. Nút tải dùng fetch+Blob+a.click; chặn HTML đăng nhập/lỗi API thành file Excel. Mã và SĐT lưu text; tiền/ngày giờ lưu giá trị native Excel.
- Điểm thưởng: 11 cột đúng yêu cầu. Tab giao dịch left join BatchId để bổ sung hạng/chiến dịch/voucher; giữ Spend/Expire/Adjust không có lô (metadata trống, giá trị có dấu theo giao dịch). Earn dùng BatchCode; giao dịch khác dùng RefCode, fallback Id. Tab lô xuất ConvertedValue/ExchangedAt.
- Đổi quà: 11 cột đúng yêu cầu. Số tiền lấy `data.cart.money_total` từ UrBoxResponse lưu sẵn; KHÔNG nhân lại số lượng và KHÔNG dùng TotalPointsUsed làm tiền. Response cũ/không hợp lệ/thiếu money_total để trống tiền (không bịa số). Mã giao dịch UrBox lấy chuỗi `UrBox transaction_id=...` từ InternalNote.
- Đơn hàng: STT + 7 cột tương ứng bảng hiện có (Nguồn, Mã đơn hàng, Khách hàng, Thành tiền, Trạng thái, Ngày đặt, Nhân viên Sales). Dữ liệu gộp Genora + DMS, dùng chung GetOrdersAsync/FilterOrders cho bảng và Excel. Bỏ giới hạn 500/nguồn: Genora query đầy đủ; DMS đọc từng trang theo metadata. Lỗi DMS không xuất file thiếu; phát hiện trang lặp/trả rỗng khi còn dữ liệu. Normalize ngày DMS trước lọc; bộ lọc nguồn/từ khóa/trạng thái/ngày giữ hành vi hiện có, tìm từ khóa thêm cả Sales như trước.
- Quyền: dùng cặp Host/Tenant Default hiện có theo từng tính năng; không thêm quyền mới.

## Files chính
- `Application/AppServices/HoaLinh/{HlSalesQuery,HlSalesExportAppService}.cs`
- `Application.Contracts/AppDtos/HoaLinh/HlSalesDtos.cs`
- `HttpApi/Controllers/HlSalesExcelController.cs`
- `Web/Pages/HoaLinh/sales.js` + 3 trang PointHistory/GiftExchanges/Orders.
- Endpoints GET `api/app/hl-sales-excel/{point-history,gift-exchanges,orders}`.

## Kiểm tra và giới hạn
- Web build OutDir riêng: **0 errors** (warnings sẵn có).
- **14 Application tests pass**: ranh giới ngày, ngày đảo ngược, metadata/text/numeric/date Excel, money JSON cũ, list/export filter đồng nhất, DMS nhiều trang/lỗi, permission Host/Tenant.
- **8 JS regression tests pass**: ngày VN/ISO/không hợp lệ, from>to, filter list/export, tab batch.
- Chưa UAT browser/SQL Server thật hoặc gọi DMS thật; phiên không có browser tool. JS proxy mới theo namespace/convention của HlOrder/HlAdmin, cần smoke-test host sau rebuild/restart.
- **Không thay đổi schema, không migration mới, không apply DB, không commit/push/deploy.** Không sửa hai appsettings.json vốn đang có thay đổi của user.
- Nếu Orders có nhiều dữ liệu DMS, thao tác tìm kiếm/xuất sẽ gọi API nhiều trang; cần kiểm tra thời gian đáp ứng khi UAT.
