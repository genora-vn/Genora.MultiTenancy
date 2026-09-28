# HL25 0,05%/ô: dự báo 50 phần/ngày và 1.000 phần đến 30/10/2026

## Cấu hình và giới hạn dữ liệu

Năm ô quà × `0,05%` + ô “Chúc may mắn” `99,75%` = `100%`. Xác suất gặp một ô quà khi còn hàng là `0,25%`, tương đương trung bình **1 lần chọn ô quà/400 lượt quay đủ điều kiện**; riêng từng loại trung bình **1/2.000 lượt**. Một người có thể có hai lượt, nên không thể suy số **người** từ số **lượt** nếu thiếu thống kê lượt/người. Người đã trúng không còn đủ điều kiện trúng lần nữa.

Đọc SQL staging chỉ đọc ngày 28/09: tenant đã đăng ký “Dược phẩm Hoa Linh” vẫn đang có `1,67% × 5 + 91,65%`, năm quà `10/loại`, và một SpinLog. Working-tree appsettings hiện trỏ DB local không có tenant; bản cấu hình committed còn trỏ staging và được dùng chỉ đọc. **Không xác nhận được cấu hình 0,05% vừa chỉnh hoặc bốn ngày số liệu từ DB hiện truy cập**; các số lượt ngày 25–28/09 bên dưới do chủ dự án cung cấp. Không có thao tác quay/trừ kho lên DB.

Code runtime mới đã được chỉnh để giữ cách vận hành nạp lại quà mỗi ngày: mặc định `Hl25:WheelTargetEligibleSpins=0` → `PickWeighted` theo WinRate tuyệt đối; pacing chỉ bật khi cấu hình target dương rõ ràng. Bỏ chặn phát quà dựa trên tổng giải đã trao **toàn lịch sử cùng GiftId** vì Admin có thể nạp lại `RemainingQuantity` mỗi ngày trên quà đó. Runtime vẫn kiểm tra trạng thái và tồn kho live, giới hạn một quà/người, giao dịch và transaction-owned SQL lock. Không đổi API/DTO/schema. Nếu cấu hình runtime bên ngoài repo có target dương thì nhánh paced vẫn chạy; phải kiểm tra cấu hình thực trên production trước khi publish.

## Dự báo từ lượng lượt đã cung cấp

| Ngày | Lượt quay | Quà kỳ vọng ở 0,25% | Mỗi loại kỳ vọng ở 0,05% |
|---|---:|---:|---:|
| 25/09 | 1.131 | 2,83 | 0,57 |
| 26/09 | 5.476 | 13,69 | 2,74 |
| 27/09 | 6.020 | 15,05 | 3,01 |
| 28/09 | 9.441 | 23,60 | 4,72 |

Tổng bốn ngày **22.068 lượt**, bình quân **5.517 lượt/ngày**. Bảng kỳ vọng giả định tất cả đều là lượt quay **đủ điều kiện**, quà còn hàng, không có chặn ngày/gift đã hết; dữ liệu thực sẽ thấp hơn nếu một số người đã từng trúng. Ở ngày nhiều nhất 9.441 lượt, xác suất chọn ít nhất 50 ô quà theo phân phối nhị thức chỉ khoảng **0,000145%** (trước khi trừ hiệu ứng từng loại hết hàng). Không thể kỳ vọng hết 50 phần tổng trong ngày ở mức lưu lượng quan sát.

### Case 1: kho theo ngày

- Nếu “50 phần/ngày” là **tổng năm loại**, chia đều `10/loại`: 50 lượt chọn ô quà có kỳ vọng sau **20.000 eligible spins**, nhưng hết **đủ cả năm kho** còn chậm hơn vì các ô trúng lệch nhau. Simulation xác suất 0,05%/ô, stock cap 10/ô, 1.000 batch: mean **27.765 lượt**, median **27.465**, mốc 95% **36.978** lượt. Ở 20.000 lượt, xác suất hết cả năm loại chỉ khoảng **5%** (xấp xỉ Poisson).
- Nếu mỗi loại thực sự là **50 phần**, tổng **250 phần/ngày**: kỳ vọng 250 lần chọn ô sau **100.000 lượt**; simulation 500 batch với cap50/ô cho mean **117.406 lượt** tới khi hết đủ cả năm, mốc95% **136.763** lượt.
- Kho theo ngày chỉ giới hạn **số tối đa** có thể phát ngày đó; 0,05% không bảo đảm phát hết. Code hiện tại không có pacing/reset theo ngày, nên muốn chắc chắn rải đủ quota mỗi ngày phải bổ sung cơ chế ngày có kiểm soát.

### Case 2: đến hết 30/10/2026

Từ **29/09 đến hết 30/10** còn **32 ngày**. Nếu bắt đầu kho mới từ 29/09 và **1.000 phần là tổng năm loại** (`200/loại`): kỳ vọng 1.000 lần chọn ô tại **400.000 eligible spins** → **12.500 lượt/ngày**. Do chặn tồn kho riêng từng loại, simulation 200 batch: để **cả năm loại** đều cạn, mean **433.281 lượt** (~**13.540/ngày**), mốc95% **467.167 lượt** (~**14.599/ngày**). Đây là xác suất, không phải bảo đảm lịch.

Nếu đọc đúng câu “**1.000 phần mỗi loại**”, tổng kho là **5.000**: kỳ vọng 2.000.000 lượt (~62.500/ngày); simulation 100 batch để hết cả năm mean **2.073.874 lượt** (~64.809/ngày), mốc95% ~**2.157.944 lượt** (~67.436/ngày).

Giữ lưu lượng bình quân bốn ngày 5.517 lượt/ngày thêm32 ngày → **176.544 lượt**, kỳ vọng **441 quà tổng**. Giữ mức ngày cao nhất 9.441 lượt/ngày → **302.112 lượt**, kỳ vọng **755 quà tổng**. Cả hai đều không làm hết kho **1.000 tổng** ở 0,05%/loại; mức 5.000 tổng càng xa. Lưu lượng đang tăng nên đây là hai kịch bản tham chiếu, không phải forecast chắc chắn.

Nếu muốn 1.000 **tổng** phần được phát hết **theo kỳ vọng** với lưu lượng không đổi, rate cần khoảng **0,1133%/loại**, ô trượt **99,4335%** tại 5.517 lượt/ngày; hoặc **0,0662%/loại**, ô trượt **99,6690%** tại 9.441 lượt/ngày. Đây chỉ là tỷ lệ dựa trên hai giả định lưu lượng, vẫn có variance và stock cap; cần rà lại mỗi ngày. Nếu mốc 30/10 là bắt buộc, cần date-aware quota/pacing, không thể bảo đảm chỉ bằng một WinRate cố định.

## Verification và rollout

- Có thể chạy lại mô phỏng tồn kho bằng `node tests/simulation/hl25-wheel-005.mjs`; script dùng các khoảng cumulative tương đương thuật toán weighted, PRNG seed cố định và chặn kho từng GiftId.
- `PickWeighted` của production source chạy 1.000.000 spin với cấu hình trên: từng ô quà **510/525/520/511/567**, ô trượt **997.367**; gần kỳ vọng 500/loại và 997.500 trượt.
- Application HL25 tests **46/46**, Domain wheel tests **9/9**; EF Core và Web Release build **0 errors**. Có test hồi quy quà được nạp lại trên **cùng GiftId** vẫn phát được cho participant mới. Không có migration.
- Chưa chạy authenticated HTTP/load test ở staging/production. Global SQL application lock vẫn xếp hàng lượt quay cùng wheel; cần đo p95 latency, 429/`WheelBusy` khi tải thực tế trước khi khẳng định an toàn vận hành. Chưa deploy. Trước publish cần xác minh target pacing bên ngoài repo bằng 0/không có, đúng tenant ID, WinRate và stock 0,05/99,75, và đo SpinLog/eligible ratio thực tế.
