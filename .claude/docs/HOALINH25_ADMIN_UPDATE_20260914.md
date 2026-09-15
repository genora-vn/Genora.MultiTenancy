# Hoa Linh 25 — cập nhật Admin ngày 14/09/2026

## Quyết định nghiệp vụ mới nhất

Anh đã chốt trong phiên: **Tạo thiệp nhận lượt đầu; chia sẻ nhận lượt thứ hai.**

- Tạo nhiều thiệp chỉ cấp **1 lượt tự nhận**. Chia sẻ thành công qua Zalo/Facebook cấp **lượt thứ hai**, chỉ một lần trên người tham gia.
- Chia sẻ lại cùng thiệp hoặc chia sẻ thiệp khác sau khi đủ 2 lượt không cấp thêm lượt.
- `EarnedCycles` giữ tên cột/API để tương thích, nay biểu thị số lượt tự nhận: 0 → chưa nhận; 1 → đã nhận lượt tạo thiệp; 2 → đã đủ hai lượt. `TotalSpinTurns` gồm cả lượt tự nhận và Admin cấp.
- AdminGrant vẫn nằm ngoài trần tự nhận, được ghi sổ riêng; giới hạn trúng tối đa một lần vẫn áp dụng.
- Giữ nguyên lượt đã cấp trước đợt sửa, kể cả người có 2 lượt từ tạo thiệp theo code cũ. Không chạy script thu hồi/backfill.

Tài liệu này ưu tiên hơn mô tả lịch sử “hai chu kỳ tạo + chia sẻ” trong schema/phase plan trước đây.

## Bảng thay đổi đã triển khai

| Nhóm | Trước | Sau | Tác động |
|---|---|---|---|
| Cấu hình chương trình | Có thể lưu ngày kết thúc trước ngày bắt đầu | Service kiểm tra khoảng thời gian | AppService; lỗi vi/en |
| Frame | Mỗi lần tạo thiệp có thể cộng lượt đến trần 2 | Chỉ lần tạo đầu được cấp lượt; tạo thiệp + cấp lượt + sổ lượt trong transaction | Domain + MiniApp API; không đổi schema |
| Chiến dịch Frame | Chưa kiểm tra khoảng thời gian | Kiểm tra khi tạo/sửa chiến dịch | AppService; lỗi vi/en |
| Chia sẻ | Chấp nhận nền tảng None/ngoài enum; còn logic theo chu kỳ cũ | Chỉ Zalo/Facebook; cấp lượt thứ hai; không cấp lặp | Domain + API; mã `Hl25:InvalidSharePlatform` |
| Kho quà | Có thể nhập tồn kho lớn hơn tổng hoặc status ngoài enum qua API | Kiểm tra `0 <= RemainingQuantity <= TotalQuantity`, status hợp lệ | AppService; lỗi vi/en |
| Cấu hình vòng quay | Save xóa/tạo lại mọi slot; UI bỏ mất ảnh/màu/ID slot và ảnh nền/kim | Giữ ID ô còn dùng; chỉ bỏ ô bị xóa khỏi form; bảo toàn các trường ẩn; chặn ID lặp/khác vòng, tỷ lệ âm/ngoài khoảng và danh sách rỗng | AppService + Wheel.js |
| Trao quà | API gán status tùy ý; gọi lại làm đổi thời điểm trao | Chỉ Won có GiftId → Delivered; gọi lại Delivered giữ timestamp; UI hỏi xác nhận và hiển thị địa chỉ snapshot lúc quay | Domain + AppService + Wheel.js |
| Người tham gia | AdminGrant cập nhật số dư và sổ lượt qua các lệnh lưu riêng | Transaction ghi cùng số dư/sổ; kiểm tra độ dài note, tràn số; bảng hiển thị lượt tự nhận; modal mô tả quy tắc | AppService + Razor/JS |
| Báo cáo | Ngày cuối bị cắt lúc 00:00; định nghĩa trúng khác nhau giữa báo cáo | Bao gồm hết ngày cuối; chỉ Won/Delivered có GiftId; tổng cấp tách tự nhận/Admin | DTO bổ sung + AppService + Razor/JS |
| Lỗi MiniApp | Nhiều `UserFriendlyException` truyền nhầm vị trí message/code | Dùng named arguments, trả code và thông báo đúng contract | API; không đổi envelope/endpoints |

## Định nghĩa báo cáo và tương thích

- Vòng quay: lọc theo `SpinTime`, số trúng chỉ gồm `Won`/`Delivered` có quà. Số đã trao là trạng thái hiện tại của các lượt quay trong khoảng lọc, không phải lọc theo `DeliveredTime`.
- Cấp lượt: lọc `GrantedTime`; `AutomaticTurnsGranted + AdminTurnsGranted = TotalTurnsGranted`. Hai trường mới là bổ sung vào DTO, không bỏ trường cũ.
- Tồn kho và số người còn lượt phản ánh hiện tại, không tái dựng số dư lịch sử ở ngày cuối.
- Frame giữ cách thống kê theo thiệp tạo trong kỳ; “đã chia sẻ” là trạng thái của các thiệp đó. Nhóm tuổi lọc ngày tham gia.
- MiniApp `CanShareForMoreTurn` chỉ true khi `EarnedCycles == 1`. FE tiếp tục dùng `RemainingSpinTurns` để hiện “Tiếp tục quay”.
- `decode-phone` giữ response Zalo trực tiếp; các endpoint hl25 khác giữ envelope hiện có.
- Kiểm tra lỗi mới/chuẩn hóa trong FE: dùng `error` là `Hl25:*`, `message` là thông báo; không dựa vào lỗi `error` chung từ constructor sai trước đây.

## Validation đã chạy

- Domain: **18 tests pass** — cấp lượt, lặp thao tác, dữ liệu cũ, ngoại lệ AdminGrant, overflow, trao quà, tồn kho/khoảng ngày.
- Application: **14 tests pass** — luồng tạo/chia sẻ/sổ lượt, ownership, enum/mã lỗi, upload quá 5MB, báo cáo ngày cuối/trạng thái, quyền Tenant/Host + feature gate, giữ ID slot/ảnh và validation slot.
- `node --check` cho Wheel.js, Participants.js, Reports.js; JSON vi/en hợp lệ, key hl25 không trùng.
- Build solution `--no-restore` thành công (còn warnings của solution). Web/bin cần chạy ngoài sandbox do quyền copy JS.
- `dotnet ef migrations has-pending-model-changes --no-build`: **No changes**. Không cần migration mới cho đợt này.

## Kiểm tra còn cần tại môi trường triển khai

- Chưa chạy browser UAT, chưa gọi API trên host/DB thật, chưa kiểm thử tải/requests đồng thời trên SQL Server. Unit tests repository giả lập không xác minh rollback/locking của DB. Luồng dữ liệu dùng transaction và cơ chế concurrency có sẵn của ABP; chưa thêm retry tự động cho xung đột.
- Xác minh DB đích đã có `20260908054652_AddHl25ProgramInfoFields` trước deploy; phiên này chỉ kiểm tra model/migration offline, không đọc hoặc apply migration trên DB đích.
- UAT: user mới tạo 2 thiệp chỉ có 1 lượt; chia sẻ thêm được 1; lặp không tăng; Admin cấp được ghi riêng; quay/trao quà giữ giới hạn một lần; lưu cấu hình giữ ảnh và các ô; báo cáo đối soát ngày cuối.
- Source và memory chưa commit/push/deploy trong phiên. Giữ nguyên appsettings và log đã thay đổi từ trước.
