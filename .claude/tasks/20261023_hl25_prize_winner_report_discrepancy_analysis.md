[HL25] Kiểm tra xem vì sao có sự chênh lệch giữa số người trúng quà và báo cáo Vòng quay theo Quà tặng (cơ cấu giải & tồn kho) của module Hoa Linh 25 năm.

BỐI CẢNH
- Trên chức năng quản trị Người dùng — Hoa Linh 25 Năm (/Hl25/Participants) tính năng Xuất Excel từ ngày 25-09-2026 đến 23-10-2026 và lọc ra Cột Số quà đã trúng = 1 thì đếm tổng trên file excel thì chỉ có 984 người trúng. Đọc file Excel tôi đã lọc và export "C:\Users\DPC\Downloads\Hl25Participants_20261023_140935.xlsx".
- Trên chức năng quản trị Báo cáo thống kê — Hoa Linh 25 Năm (/Hl25/Reports) và lọc từ ngày 25-09-2026 đến 23-10-2026 tính năng thống kê Vòng quay theo Quà tặng (cơ cấu giải & tồn kho) hiển thị cột Đã trúng của 5 quà tặng đều hiển thị là 200 (tổng là 1000). Xem ảnh tôi lọc và bôi đỏ "C:\Users\DPC\Downloads\2026-10-23_14-25-32.png".
- Chênh lệch giữa số người trúng và số lượng trúng quà trên thống kê đang lệch 16 người (quà).

YÊU CẦU
- Kiểm tra tính năng Xuất Excel của chức năng quản trị Người dùng — Hoa Linh 25 Năm (/Hl25/Participants) vì sao có sự chênh lệch, liệu có sai số ở query hay đang hiểu sai về giá trị thống kê.
- Làm sao để thống kê chính xác (có thể tạo các query để lọc trực tiếp trên Sql Server) minh chứng các con số chính xác để làm báo cáo.

LƯU Ý ĐẶC BIỆT:
- Tìm hiểu và báo cáo lại cụ thể nguyên nhân và đưa ra phương án để thống kê chính xác.
- Hệ thống đang vận hành ổn định chỉ có nhu cầu làm báo cáo chính xác nên không thay đổi code, logic và gợi ý phương án để thống kê các báo cáo được chính xác.
- Xong hãy cập nhật lại kết quả vào file này.

Kết quả xử lý 2026-10-23:
Kết luận chính xác: có 1.000 người trúng hợp lệ. Excel chỉ có 984 người vì bộ lọc ngày tham gia đã loại 16 người khỏi danh sách xuất.

Kết quả SQL xác nhận:

| Chỉ số | Kết quả |
|---|---:|
| Lượt quay trúng | 1.000 |
| Người trúng khác nhau | 1.000 |
| Người trúng trong phạm vi Excel | 984 |
| Người trúng nằm ngoài phạm vi Excel | 16 |
| Người trong Excel không có lượt trúng trong kỳ | 0 |
| Lượt trúng dư do cùng participant trúng nhiều lần | 0 |
| Chênh lệch đã giải thích | 16 |

Các dòng chi tiết đang hiển thị đều có:

- ExclusionReason = JOINED_BEFORE_25_SEP.
- JoinedTime ngày 20/09/2026.
- SpinTime từ 25/09/2026 trở đi.
- TotalGiftsWon = 1, RewardStatus = 1, ParticipantIsDeleted = 0.

Nghĩa là những người này đăng ký trước ngày bắt đầu lọc nhưng quay trúng trong kỳ báo cáo. Reports tính họ theo ngày quay trúng; Excel loại họ theo ngày đăng ký.

Ảnh chỉ hiển thị 9 dòng chi tiết nên chưa thể khẳng định cả 16 đều đăng ký trước 25/09. Tuy nhiên, nguyên nhân tổng chênh 16 do phạm vi chọn người của Excel đã được chứng minh đầy đủ.

Các kết quả còn lại cũng xác nhận:

- Không phát hiện lệch giữa bộ đếm TotalGiftsWon và lịch sử trúng.
- Không phát hiện nhiều hồ sơ trúng cùng số điện thoại.
- Mỗi loại quà có 200 lượt trúng và 200 người trúng; tổng 1.000.
- Cả 5 quà đều còn hiển thị, không bị soft-delete.
- Chưa có quà được đánh dấu Đã trao.
- Không có lượt trúng phát sinh trong khoảng thời gian kiểm tra giữa hai lần lấy báo cáo.

Số liệu dùng báo cáo trúng thưởng cho kỳ 25/09–23/10 là: 1.000 người, 1.000 phần quà, mỗi loại 200 phần. Con số 984 chỉ đúng với nhóm người có ngày đăng ký nằm trong điều kiện lọc Excel hiện tại.

Để lấy danh sách đầy đủ phục vụ giao quà, cần chọn từ SpinLog trúng trong kỳ rồi nối thông tin Participant, bao gồm cả người đăng ký trước 25/09. Không dùng ngày tham gia để giới hạn danh sách người trúng trong kỳ.