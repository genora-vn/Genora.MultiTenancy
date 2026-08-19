---
name: ABP ILogger không có LogWarning extension
description: ApplicationService.Logger là Volo.Abp ILogger — không có LogWarning extension method
type: feedback
---

Trong `ApplicationService`, property `Logger` có kiểu `ILogger` của **Volo.Abp**, không phải `Microsoft.Extensions.Logging.ILogger`. Extension methods như `LogWarning(ex, "...", args)` **không tồn tại** trên kiểu này.

**Why:** Dẫn đến `error CS1061` khi build.

**How to apply:** Trong AppService, nếu cần log: dùng `Logger.LogException(ex)` (ABP built-in) hoặc đơn giản hơn là `catch { }` rỗng với comment giải thích. Không dùng `Logger.LogWarning(ex, message, args)`.
