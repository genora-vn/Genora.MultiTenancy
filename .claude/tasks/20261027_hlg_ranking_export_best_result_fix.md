STATUS: DONE — Implementation/test/memory hoàn thành 2026-10-27, chưa commit/deploy.

Branch thực tế: `dev`, HEAD nền `425c30e`.

## Kết quả implementation theo plan được duyệt

- User duyệt chỉ sửa **“Xuất báo cáo” / ExportReportAsync**, không sửa BXH mini-app, xét giải, ExportResultsAsync/chốt kết quả, reset BonusPoint hay snapshot cũ.
- Root cause: BuildRowsAsync cộng các phiên đạt và loại toàn bộ failed trước tính điểm; BestScore cũ đã là Max (không phải SUM). Fix chọn một BEST/CustomerId/GameId từ tất cả finished sessions theo điểm DESC → câu đúng DESC → tổng answer time ASC → FinishedAt ASC → SessionId. Các số liệu điểm/câu đúng/tổng câu lấy cùng phiên.
- EventScore = tổng một BEST/game được chọn; 98 vẫn là 98, không ép 100 theo cấu hình lý thuyết. Hạng report theo tổng BEST/câu đúng/thời gian/CustomerId. Missing time = unknown, không phải 0; genuine score0 giữ nguyên.
- Giữ 19 cột/API/DTO/schema, số lượt/first-last, filters tenant/event/game, enrich PharmaPhone/địa chỉ/quà và GamesCompleted passed-only. Không migration/ghi DB ứng dụng.
- Source sửa: HlgRankingAdminAppService.cs (chỉ BuildRowsAsync); HlgAdminTests.cs (12 regression cases mới, kiểm tra workbook thực sinh).
- **174/174 HLG Application tests PASS**, Web build **0 errors / 78 warnings**, diff-check sạch. Kết quả mô phỏng dữ liệu mẫu: 161→87, 147→76, 144→76; failed-only có BEST thật; multi-game87+50=137, chỉ chọn game87→87.
- Cập nhật ACTIVE_CONTEXT, PROJECT_STATE, TASK_LOG, MEMORY index, project note và HANDOFF. Chưa live DB/browser UAT; user test tiếp “Xuất báo cáo” với game đã kết thúc. Không commit/deploy. [Note chi tiết](../memory/notes/project/project_hlg_ranking_export_best_result_20261027.md).

---

## Yêu cầu gốc (giữ nguyên để đối chiếu)

[HLG] Điều chỉnh lại dữ liệu đổ ra Excel khi click vào action Xuất Excel kết quả trong tính năng quản trị Sự kiện xếp hạng (/Hlg/Ranking).

BỐI CẢNH
- File Excel kết quả trò chơi (game) Thử tài tốc độ: Dạ Hương Tuần 1 đang có 3 trường hợp (khách hàng) có Điểm trò chơi, Điểm cao nhất/lượt, Số câu đúng, Tổng số câu, Tổng điểm sự kiện đang bị cộng dồn theo số lượt chơi (xem file excel kết quả Tuần 1 đã kết thúc mới xuất được "C:\Users\DPC\Downloads\HLG_KetQua_Thử tài tốc độ_ Trả lời nhanh về Dạ Hương - bao cao_20261026_163119.xlsx"). Còn những khách hàng khác đều đúng (chỉ lấy cấu hình và kết quả của lượt chơi có số điểm cao nhất, thời gian trả lời nhanh nhất, số câu đúng nhiều nhất).
- Cấu hình Trò chơi (game) đang có các thông số và kết quả phải được như sau:
  + Cấu hình: Số câu hỏi mỗi lượt chơi = 10, Số câu được phép sai = 1, Điểm cơ bản mỗi câu = 10.
  + Kết quả: Số lượt chơi > 1, Điểm trò chơi max chỉ là 100 (10 câu x mỗi câu 10 điểm), Điểm cao nhất/lượt lấy lượt chơi có điểm cao nhất, Số câu đúng lấy số câu đúng nhiều nhất, Tổng số câu theo cấu hình chỉ có 10 câu, Tổng điểm sự kiện cũng max theo cấu hình (10 câu x mỗi câu 10 điểm = 100).
  + Ví dụ 3 kết quả đang sai: 
    Số lượt chơi	Điểm trò chơi	Điểm cao nhất/lượt	Số câu đúng	Tổng số câu	Tổng điểm sự kiện
    3	            161	            87	                18	        20	        161
	2	            147	            76	                18	        20	        147
	3	            144	            76	                18	        20	        144
  + Ví dụ các kết quả đúng
    Số lượt chơi	Điểm trò chơi	Điểm cao nhất/lượt	Số câu đúng	Tổng số câu	Tổng điểm sự kiện
    1	            100	            100	                10	        10	        100
	2	            100	            100	                10	        10	        100
	6	            98	            98	                10	        10	        100
- Báo cáo đang chỉ trả ra số liệu: Số lượt chơi, Điểm trò chơi, Điểm cao nhất/lượt, Số câu đúng, Tổng số câu, Tổng điểm sự kiện cho những người thoải mãn điều kiện trả lời đúng 9/10 câu như cấu hình trò chơi mà những người khác đều không trả ra kết quả cao nhất (vì họ không đạt trả lời 9/10 câu mà chỉ dưới 9/10 câu). Đang hiển thị giá trị 0 cho toàn bộ các cột Điểm trò chơi, Điểm cao nhất/lượt, Số câu đúng, Tổng số câu, Tổng điểm sự kiện. Hãy trả ra kết quả cao nhất của người chơi đó thay vì chỉ hiển thị ra toàn giá trị 0 để có số liệu báo cáo chính xác. Ví dụ:
  + Ví dụ các kết quả đúng
    Số lượt chơi	Điểm trò chơi	Điểm cao nhất/lượt	Số câu đúng	Tổng số câu	Tổng điểm sự kiện
    5	            0	            0	                0	        0	        0
	6	            0	            0	                0	        0	        0
	8	            0	            0	                0	        0	        0
    10	            0	            0	                0	        0	        0
	18	            0	            0	                0	        0	        0
	46              0	            0	                0	        0	        0

YÊU CẦU
- Hãy kiểm tra điều kiện và lấy ra dữ liệu kết quả cao nhất của người chơi và sắp xếp thứ hạng chính xác. Không có những trường hợp cộng dồn Điểm trò chơi, Điểm cao nhất/lượt, Số câu đúng, Tổng số câu, Tổng điểm sự kiện khi người chơi chơi nhiều lượt. Phải căn cứ vào cấu hình và kết quả tốt nhất của người chơi.
- Đổ thêm dữ liệu cho các cột Điểm trò chơi, Điểm cao nhất/lượt, Số câu đúng, Tổng số câu, Tổng điểm sự kiện lấy kết quả cao nhất của người chơi để lấy căn cứ để báo cáo thay vì chỉ lấy những người đủ điều kiện theo cấu hình (ví dụ: Đang chỉ lấy ra số liệu cho các cột Điểm trò chơi, Điểm cao nhất/lượt, Số câu đúng, Tổng số câu, Tổng điểm sự kiện cho những người đang trả lời đúng 9/10 câu theo câu hình).

LƯU Ý ĐẶC BIỆT:
- Báo cáo đang hoạt động khá chính xác và ổn định rồi chỉ tìm ra 2 lỗi trên và khắc phục để báo cáo chính xác nhất không làm thay đổi logic hay kiến trúc API, service,...
- Kiểm tra và báo cáo lại nguyên do để tôi xác nhận trước khi sửa code.
