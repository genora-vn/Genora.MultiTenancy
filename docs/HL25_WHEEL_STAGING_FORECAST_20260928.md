# Kiểm tra staging và dự báo kho HL25 — 2026-09-28

> Follow-up [0,05% và nạp kho hằng ngày](HL25_WHEEL_005_FORECAST_20260928.md) đã đổi mặc định runtime sang weighted random; pacing 3.000 lượt trong tài liệu này chỉ áp dụng khi cấu hình target dương rõ ràng.

## Phạm vi xác minh

Đọc **chỉ đọc** SQL Server staging từ `Web/DbMigrator appsettings.json` (server `103.157.218.187,1433`), host DB `GenoraMultiTenancy`, tenant đăng ký “Dược phẩm Hoa Linh” `650ccd37-aeb4-63e7-bac7-3a23a72f9cbc`, tenant DB `DuocPhamHoaLinh`. Không gọi endpoint quay, không trừ kho hay sửa cấu hình. Các test chạy thuật toán source hiện tại trong bộ Domain.Tests; chưa xác minh binary Web đã deploy trên staging/production.

Trong tenant này, có **một** WheelConfig active `dd83891a-0608-05ac-4563-3a23a742cc4a`, sáu slot: năm ô quà mỗi ô `1.6700%`, ô không trúng `91.6500%`, tổng `100.0000%`. Năm quà hiện có **10 phần/loại, tổng 50**, đều còn đủ và status `Available`. Có **một** SpinLog, không trúng; chưa có dữ liệu thực tế 1.600 lượt hoặc 50 phần đã trao để xác nhận nguyên nhân lịch sử. Cùng DB còn các dòng mang TenantId `2e0a7b41-cc0e-62cb-e300-3a23c506ecac`, **khác** tenant đăng ký ở host, với tỷ lệ `20/18/18/18/18/8` và kho `1000/500/500/500/500`; không được trộn chúng vào báo cáo tenant HL25 staging.

## Test chính thuật toán source

- 100.000 lượt `PickWeighted`: mỗi ô quà xấp xỉ 1,67%, ô không trúng xấp xỉ 91,65%; thuật toán xác suất cơ bản đúng khi tổng tỷ lệ bằng 100.
- 200 bộ cấu hình độc lập × 3.000 lượt `PickPaced`, kho giả lập 50/loại: tại lượt **eligible** 1.600 mỗi loại có 26 hoặc 27 lần được chọn; tại 3.000 mỗi loại đúng 50. Lần thứ 50 nằm đâu đó trong cửa sổ 2.941–3.000, có thể **trước** lượt thứ 3.000; không có ràng buộc ngày/giờ.
- Với kho staging 10/loại và target mặc định 3.000, thuật toán chỉ xếp **10/loại** trong 3.000 lượt, tức xác suất thực tế mỗi loại khoảng **0,333%**, dù WinRate hiển thị 1,67%. Đây là hệ quả `min(TotalQuantity, targetSpins × WinRate)`; khi kho thấp hơn số lượt kỳ vọng, WinRate trở thành trần/đầu vào lịch phát, không phải tỷ lệ thực tế tuyệt đối.
- Với tổng 1.000 phần chia đều 200/loại, target hiện tại 3.000: ngày đầu 3.000 eligible lượt phát đúng 50/loại (250 tổng); sau đó code chuyển lại weighted random 1,67%/loại. Trong 100 mô phỏng, lượt eligible mà **cả năm loại** đều hết có mean **12.884,58**, min **11.869**, max **14.440**. Tại lượt 12.000, số đã phát trung bình mỗi loại khoảng 195/200. Test có chặn stock 200/loại; mọi phần phát không vượt 200.
- Nếu đổi target toàn chiến dịch thành 99.000 eligible lượt, code hiện tại rải 200/loại đến lượt 99.000; tại lượt 3.000 chỉ 6–7/loại, tại 12.000 chỉ 24–25/loại. Điều đó là **~0,202% thực tế/loại**, không còn 1,67% thực tế. Cần công khai khác biệt giữa tỷ lệ hiển thị và tỷ lệ phát thực tế trước khi dùng cấu hình này.

`dotnet test test/Genora.MultiTenancy.Domain.Tests/Genora.MultiTenancy.Domain.Tests.csproj --filter FullyQualifiedName~Hl25WheelDistributionTests --no-restore`: **8/8 pass**. Những test mới là simulation không DB write.

## Đánh giá mốc 30/10

Từ **28/09 đến hết 30/10/2026** là 33 ngày, giả định mỗi ngày 3.000 **eligible** lượt thì có 99.000 lượt. Với 1,67% × 5, nhu cầu kỳ vọng là **8.266,5 phần**, vượt kho 1.000 hơn 8 lần. Ngay cả bỏ qua variance, 1.000 / (3.000 × 8,35%) ≈ **3,99 ngày**; ngày thứ tư là khoảng 12.000 lượt. Để 1.000 phần trải đều 99.000 lượt, toàn bộ quà phải có xác suất thực tế khoảng **1,0101%**, mỗi loại (nếu chia đều) khoảng **0,2020%**, hoặc lượng lượt eligible trung bình chỉ khoảng **363/ngày** nếu giữ xác suất 8,35%.

Vì vậy không thể đồng thời bảo đảm **1,67% thực tế mỗi loại**, **3.000 eligible lượt/ngày**, **tổng 1.000 phần** và **còn quà tới 30/10**. Không nên chỉ chỉnh 1,67 xuống một số khác khi chưa chốt lượng lượt/dữ liệu vận hành. Nếu business muốn giữ ngày kết thúc, cần pacing theo **quota toàn chiến dịch / ngày còn lại / lượng lượt dự báo**, và Admin phải hiển thị rõ tỷ lệ thực tế, hoặc tăng kho dự kiến lên cỡ 8.267 phần. `Hl25:WheelTargetEligibleSpins=99000` là mô phỏng được nhưng không phải cơ chế ngày tự cập nhật: target cố định sẽ sai nếu lưu lượng lệch, chạy trễ hoặc ngày kết thúc đổi.

Các mốc ở trên đều tính trên **eligible spins**, là lượt của người chưa từng trúng. Tổng API spins có thể cao hơn; staging mới một spin nên chưa ước lượng được tỷ lệ eligible/tổng hoặc lưu lượng thực. Source changes chưa deploy, chưa load test transaction lock trên staging và chưa đối soát dữ liệu production đã trúng; do đó chưa thể chứng nhận production đạt mốc ngày.
