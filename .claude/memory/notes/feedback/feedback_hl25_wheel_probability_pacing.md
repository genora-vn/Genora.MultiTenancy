# HL25 wheel: phân biệt xác suất, denominator và pacing

- `WinRate` trong Admin là phần trăm tuyệt đối. Tổng slot phải chính xác 100%, không random trên `Sum(WinRate)` khi tổng sai vì thao tác đó âm thầm đổi xác suất thật.
- `n × p` là kỳ vọng, không bảo đảm tồn kho đến lượt n. Nếu business cần mỗi quà rải đều trong 3.000 lượt, phải có pacing/stratified allocation và test nhiều batch; không giảm WinRate theo cảm tính.
- Đối soát bằng SpinLog và **eligible spins trước lần trúng đầu**, tách total spins/prior-winner spins; `RewardStatus=Delivered` vẫn là quà đã cấp. `TotalQuantity - RemainingQuantity` có thể do Admin sửa kho, không mặc nhiên bằng số Won. Rate/Config hiện tại không chứng minh rate/Config của lịch sử nếu log không snapshot.
- Multi-node concurrency cần serialization hoặc atomic update trong cùng transaction trước khi đọc participant/stock. Lock bảo vệ kho không thay thế request-idempotency key nếu user còn hai lượt quay.

Case gốc: [project note](../project/project_hl25_wheel_pacing_audit_20260928.md) và `docs/HL25_WHEEL_DISTRIBUTION_20260928.md`.
