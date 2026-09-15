# Hoa Linh 25 — Khôi phục context và approach cập nhật Admin (2026-09-14)

## Phạm vi phiên
- User yêu cầu đọc LOAD_CONTEXT + memory, xác nhận mức độ hiểu và đề xuất approach cập nhật hệ thống quản trị. Chưa có yêu cầu thay đổi chức năng cụ thể.
- Đã đọc context, rules, state, task log, các memory index, architecture và schema hl25; đối chiếu các luồng trọng tâm với source + Git.
- Nhánh hiện tại `feature/dev-hoalinh-25years`, HEAD `31d8e11` (2026-09-14). Các cập nhật 08/09 đã commit (Settings `7c51b88`, UX `befe83e`, decode-phone `b0e09da`, CreateFrame `60e76b4`); ghi chú cũ "CHƯA commit" không còn phản ánh trạng thái này. Chưa fetch remote trong phiên.
- Working tree ban đầu có thay đổi appsettings Web/DbMigrator và log; không chỉnh các file này.

## Baseline đã đối chiếu
- 10 entity schema `hl25`; 5 trang Admin: Settings, Frames, Wheel, Participants, Reports. Feature `Hl25.Management`, phân quyền Tenant/Host; không quản lý Points (gamification là nhánh parked riêng).
- Controller `HoaLinh25MiniAppController` có 17 endpoint, prefix `/api/mini-app/hl25`; decode-phone trả trực tiếp response Zalo, các endpoint còn lại dùng envelope hl25.
- Settings có IntroductionHtml, Format, GiftDeliveryTime; AgeGroup thay BirthDate; lời chúc tối đa 250; quay giới hạn trúng 1 lần qua TotalGiftsWon.

## Sai lệch memory cần dùng đúng khi tiếp tục
1. **Lượt quay:** source hiện tại CreateFrameAsync cộng +1, ShareFrameAsync cũng cộng +1 nếu EarnedCycles < 2. Hai hành động dùng chung trần 2. Tạo 1 thiệp rồi chia sẻ có thể nhận đủ 2 lượt; tạo 2 thiệp cũng có thể đủ 2 lượt dù chưa chia sẻ. Không được tiếp tục coi mô tả cũ "mỗi chu kỳ tạo + chia sẻ mới cộng 1 lượt" là hành vi đang chạy. Đây là đối chiếu source, chưa thay đổi quyết định nghiệp vụ.
2. **AdminGrant:** cộng lượt thủ công không áp trần 2 và không tăng EarnedCycles. Cần phân biệt lượt tự nhận với lượt Admin cấp trong UI/báo cáo.
3. **Trao thưởng:** luồng thiết kế Won → Delivered, nhưng UpdateRewardStatusAsync hiện gán status trực tiếp; chưa validate chuyển trạng thái ở method này. Cần rà soát khi cập nhật vận hành trao quà.
4. **Migration:** source + `dotnet ef migrations list --no-build --no-connect` xác nhận có `20260825160252_AddHl25Module` và `20260908054652_AddHl25ProgramInfoFields` (3 AddColumn). Note 08/09 nói migration nền đã apply; note cũ nói chưa apply. Phiên này không kết nối DB nên CHƯA xác minh migration nào đã apply trên DB đích. Không sửa migration cũ in-place dựa vào ghi chú lỗi thời.
5. ACTIVE_CONTEXT/PROJECT_STATE/schema chứa mô tả lịch sử xen lẫn hiện trạng; HANDOFF cũ từ 18/08 và MEMORY.md ở root là lịch sử payment, không dùng làm trạng thái hl25 mới nhất.

## Approach đề xuất (chưa triển khai tính năng)
1. Lập bảng yêu cầu cập nhật theo từng trang: hiện tại → mong muốn → tác động UI/API/DB; thống nhất cách cấp lượt và ngoại lệ AdminGrant trước khi sửa logic.
2. Ưu tiên Wheel + Participants: nguồn cấp lượt, lịch sử đối soát, tồn kho, giới hạn trúng, chuyển trạng thái trao quà; kiểm tra request lặp/đồng thời.
3. Cập nhật Settings + Frames theo nhu cầu quản trị cụ thể, đồng bộ DTO/AppService/Razor/JS/localization; sau đó cập nhật Reports theo định nghĩa chỉ số đã thống nhất.
4. Xác minh DB đích và migration history trước thay đổi schema; dùng migration mới nếu schema đã triển khai. Kiểm tra quyền Tenant/Host, feature gate, luồng MiniApp và build; ghi lại memory sau mỗi đợt.

## Validation
- Không thấy TODO/FIXME trong AppServices/Hl25 và Web/Pages/Hl25; không tìm thấy file test tên chứa Hl25. Điều này không chứng minh đã kiểm thử đủ.
- Build solution `--no-restore`: lần đầu có 5 lỗi MSB3021 do quyền copy JS vào Web/bin, 383 warnings. Chạy lại ngoài sandbox thành công: **0 errors, 1 warning** (incremental build); không kết luận đã giải quyết toàn bộ warnings của lần đầu. Không chạy tests/runtime trong phiên.
- Không chạy host, không apply migration, không thay đổi source nghiệp vụ trong phiên này.
