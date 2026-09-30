# HLG — Xuất Excel kết quả sự kiện xếp hạng (2026-09-30)

## Yêu cầu
- Thêm action xuất dữ liệu Excel cho từng sự kiện xếp hạng.
- Chỉ hiển thị/chấp nhận xuất sau khi sự kiện kết thúc.
- Báo cáo có thông tin người chơi, trò chơi, điểm và số lượt chơi.

## Triển khai
- `HlgRankingAdminDto.CanExportResults` được tính theo `Clock.Now > EndAt`; action `Xuất Excel kết quả` nằm trong dropdown dòng sự kiện ở `/Hlg/Ranking` và chỉ hiện khi cờ này là true.
- Endpoint `GET api/app/hlg-ranking-excel/export?eventId=...` gọi `IHlgRankingAdminAppService.ExportResultsAsync`; backend kiểm tra permission, tenant scope và thời gian kết thúc nên không thể gọi sớm bằng URL trực tiếp.
- Chỉ thống kê các session đã finish trong `[StartAt, EndAt]`, và lọc `GameId` nếu sự kiện chỉ định một game.
- Mỗi dòng là một cặp người chơi–trò chơi. Cột: thứ hạng sự kiện, mã/tên/điện thoại/Zalo người chơi, tên game, số lượt, tổng điểm game, điểm cao nhất/lượt, số câu đúng/tổng câu, tổng điểm sự kiện, thời gian lượt đầu/gần nhất.
- File giữ mã người chơi và số điện thoại ở dạng text để không mất số 0 đầu.
- Không có migration/schema change.

## Kiểm tra
- `dotnet build src/Genora.MultiTenancy.Web/Genora.MultiTenancy.Web.csproj --no-restore -p:OutDir=...` — PASS, 0 errors (warning cũ của solution).
- `dotnet test test/Genora.MultiTenancy.Application.Tests/Genora.MultiTenancy.Application.Tests.csproj --no-restore --filter FullyQualifiedName~HlgAdminTests -p:OutDir=...` — PASS 20/20.
- `node --check` cho `Pages/Hlg/admin.js` và `Pages/Hlg/Ranking/index.js` — PASS.
- Runtime follow-up: sửa callback DataTables `rowAction.visible` nhận trực tiếp `record` (không phải wrapper `data.record`); lỗi cũ làm bảng đứng ở trạng thái loading dù API trả 200. `node --check` và `git diff --check` PASS sau sửa.

## Chưa làm
- Chưa UAT bằng tenant DB/browser thật và chưa deploy.
