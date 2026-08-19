---
name: feedback-caddie-appservice-no-ef-in-application
description: Application layer không dùng Microsoft.EntityFrameworkCore — dùng AsyncExecuter thay thế
metadata: 
  node_type: memory
  type: feedback
  originSessionId: c7f0aab2-9051-4ec9-be74-7c4ad7d1b062
---

Application layer (Genora.MultiTenancy.Application) **KHÔNG** được reference `Microsoft.EntityFrameworkCore`. Điều này phá vỡ cấu trúc DDD của ABP Framework.

**Thay thế:**
- `query.CountAsync()` → `AsyncExecuter.CountAsync(query)`
- `query.ToListAsync()` → `AsyncExecuter.ToListAsync(query)`
- `query.FirstOrDefaultAsync()` → `AsyncExecuter.FirstOrDefaultAsync(query)`

`AsyncExecuter` là property có sẵn trong `ApplicationService` base class (từ `Volo.Abp.Linq`).

**Entity<Guid>.Id set accessor:**
- `Entity<Guid>` không cho set `Id` trực tiếp
- Phải dùng constructor: `new AppCaddieLanguage(guid, caddieId, languageId)` thay vì `new AppCaddieLanguage { Id = guid, ... }`

**Why:** Build lỗi khi dùng EF Core trực tiếp trong Application layer
**How to apply:** Luôn dùng AsyncExecuter cho async LINQ queries trong AppService; tạo constructor cho Entity<Guid> subclasses

[[feedback_mars_autosave_pattern]]
