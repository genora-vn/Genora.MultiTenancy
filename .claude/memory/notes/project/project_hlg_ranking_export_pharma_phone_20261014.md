# HLG Ranking Excel — thêm SĐT Chủ nhà thuốc — 2026-10-14

## Kết quả
- Commit `5d4f2cd` đã thêm cột `SĐT Chủ nhà thuốc` ngay sau `Zalo User ID` trong export sự kiện xếp hạng.
- `HlgRankingAdminAppService.EnrichExportRowsAsync` tra `HLG.AppHlgUserProfiles` bằng `CustomerId` và `TenantId` hiện tại; chỉ lấy profile chưa xóa. Số điện thoại được xuất dạng text để giữ số 0 đầu.
- Cả `ExportReportAsync` và `ExportResultsAsync`/xuất lại snapshot dùng chung bước enrich. PharmaPhone là dữ liệu profile hiện thời, không đóng băng vào snapshot. Profile thiếu hoặc `PharmaPhone` null thì ô để trống; không suy ra từ số điện thoại người chơi hay `PharmacyCode`.
- File tăng 18→19 cột. Sau khi thêm cột G, các cột trò chơi và số liệu dịch sang phải; định dạng số/ngày, filter range, header freeze được cập nhật. Không đổi route/API, không migration.

## Kiểm chứng đã chạy trong phiên triển khai
- HLG Application tests: **148/148 PASS**.
- Web build: **0 errors, 61 warnings**.
- `git diff --check` sạch. Kiểm thử workbook xác nhận header/vị trí cột, kiểu text/số 0 đầu, dịch cột số/ngày/trạng thái/địa chỉ, AutoFilter 19 cột; cover export report Host/Tenant, owner/employee, snapshot re-export, profile thiếu/null/deleted và export rỗng.
- Đây là kết quả từ phiên triển khai trước merge; không khẳng định đã chạy lại trên HEAD hiện tại.

## Git và vận hành
- Đã commit `5d4f2cd` và merge lên `dev` trong merge commit `779367f` (xác nhận bằng `git log` ngày 2026-10-14).
- Chưa có bằng chứng deploy hoặc UAT trên môi trường staging/production. Khi kiểm tra thực tế, restart Web và xuất một sự kiện phù hợp; profile legacy thiếu PharmaPhone sẽ hiện ô trống.
- Không cập nhật memory trong phiên implementation vì yêu cầu lúc đó chỉ giới hạn các file code/test; note này được bổ sung trong phiên chuẩn bị bàn giao sau.
