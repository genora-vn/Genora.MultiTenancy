---
name: MiniAppProOrderService thiếu SignalR notifier
description: MiniApp là con đường đặt hàng chính — thiếu inject IProOrderRealtimeNotifier → staff không nhận notify
type: project
---

`MiniAppProOrderService` ban đầu không inject `IProOrderRealtimeNotifier`. Khi customer đặt đơn qua Mini App (luồng phổ biến nhất), không có broadcast SignalR nào được gửi → staff không thấy chuông báo đơn mới.

**Why:** Notifier chỉ được inject trong `AppProOrderService` (luồng staff), bỏ sót `MiniAppProOrderService` (luồng khách).

**How to apply:** Khi tạo service mới cho Mini App liên quan đến orders, luôn kiểm tra xem có cần inject notifier không. Broadcast trong `CreateAsync` và `CancelAsync`.
