---
name: SerilogCommandInterceptor đang tạm tắt
description: Interceptor log SQL vẫn được register DI nhưng không gắn vào EF — tắt qua comment AddInterceptors trong OnConfiguring
type: project
originSessionId: 03f1f8f0-a727-4f28-8cd7-f688971b0a28
---
`SerilogCommandInterceptor` (file EntityFrameworkCore/Diagnostics/SerilogCommandInterceptor.cs) gây nặng source nên đã tạm tắt:

- `MultiTenancyEntityFrameworkCoreModule.cs:67` — vẫn giữ `context.Services.AddSingleton<SerilogCommandInterceptor>()` để DI resolve được constructor DbContext.
- `MultiTenancyDbContext.cs:136-139` — comment dòng `optionsBuilder.AddInterceptors(_sqlInterceptor)`. Interceptor vẫn được tạo nhưng không attach → không log SQL.

**Why:** Nếu chỉ comment `AddSingleton` mà không comment `AddInterceptors`, DbContext constructor (có tham số `SerilogCommandInterceptor`) sẽ throw vì container không resolve được → app gãy.

**How to apply:** Muốn bật lại log SQL thì uncomment 2 dòng `if (_sqlInterceptor is not null) optionsBuilder.AddInterceptors(_sqlInterceptor);` trong `OnConfiguring`. Không tháo `AddSingleton` trừ khi cũng gỡ tham số khỏi constructor DbContext.
