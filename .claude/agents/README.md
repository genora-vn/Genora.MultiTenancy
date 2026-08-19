# Agents

> Thư mục dành cho custom agent/subagent định nghĩa riêng cho dự án Genora.MultiTenancy.

## Trạng thái hiện tại
**Chưa có custom agent nào cho dự án này.**

Audit user-level (`~\.claude\`) không phát hiện định nghĩa agent riêng gắn với repo này.
Các subagent transcript tìm thấy (`projects\**\subagents\*.jsonl`) chỉ là log phiên chạy,
không phải định nghĩa agent tái sử dụng — nên KHÔNG migrate.

## Khi cần thêm agent
Đặt file định nghĩa agent tại đây (ví dụ theo format của công cụ đang dùng), kèm mô tả:
- Mục đích agent
- Khi nào nên dùng
- Công cụ được phép
