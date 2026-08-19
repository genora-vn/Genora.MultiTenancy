---
name: SignalR broadcast không làm fail luồng chính
description: Wrap SignalR notify trong try/catch rỗng để lỗi broadcast không fail đặt hàng
type: feedback
---

Khi gọi `IProOrderRealtimeNotifier` hoặc `IFnbOrderRealtimeNotifier`, phải wrap trong `try { ... } catch { }` riêng:

```csharp
try { await _notifier.OrderCreatedAsync(order.Id); }
catch { /* SignalR broadcast không được làm thất bại luồng đặt hàng */ }
```

**Why:** Lỗi SignalR (hub disconnect, timeout) không được phép làm fail transaction đặt hàng của khách.

**How to apply:** Mọi broadcast SignalR trong AppService đều phải có try/catch riêng, không để exception nổi lên.
