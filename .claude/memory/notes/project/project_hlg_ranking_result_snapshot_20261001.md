# HLG ranking result snapshot — 2026-10-01

## Mục tiêu
- Khi xuất Excel một event đã kết thúc, lưu toàn bộ dòng báo cáo người chơi–trò chơi để kết quả cũ không bị thay đổi theo dữ liệu session về sau.

## Triển khai
- Thêm entity/bảng `HLG.AppHlgRankingResultSnapshots`, mỗi dòng là một cặp `EventId`–`CustomerId`–`GameId` và chứa toàn bộ cột của Excel.
- Lần xuất đầu tính từ `HlgGameSession`, lưu snapshot rồi trả Excel; lần xuất sau đọc snapshot, không tính lại session.
- Trong cùng transaction của lần xuất đầu, đặt `Customer.BonusPoint = 0` cho đúng các customer có kết quả trong event. Xuất lại từ snapshot không clear lần nữa.
- Unique index `(TenantId, EventId, CustomerId, GameId)` chống lưu trùng.
- Chặn đổi game/khoảng thời gian và chặn xóa event sau khi đã có snapshot; vẫn cho sửa metadata không ảnh hưởng kết quả.
- Migration `20261001040131_AddHlgRankingResultSnapshots` và SQL idempotent đã sinh, chưa apply DB.

## Xác minh
- Web build: 0 errors.
- 61 Application tests HLG pass, gồm test lưu/reset lần đầu và dùng snapshot mà không reset khi xuất lại.
- `dotnet ef migrations has-pending-model-changes`: không có thay đổi model chưa migration.
- Không chạy `database update`, không ghi DB thật.

## Lưu ý
- Event không có dòng kết quả sẽ xuất file rỗng và không có row snapshot.
- Các thay đổi appsettings/log có sẵn của user được giữ nguyên.
