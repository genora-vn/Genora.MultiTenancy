---
name: ef-migration-dll-lock
description: EF migrations add tạo body rỗng khi Web process đang chạy lock dll → kill process + clean bin/obj
metadata: 
  node_type: memory
  type: feedback
  originSessionId: a89ec214-fd79-45a5-a94c-5c74851065ce
---

# EF Migrations: body rỗng khi Web đang chạy

Khi gọi `dotnet ef migrations add <Name> -s ../Genora.MultiTenancy.Web`, nếu instance `Genora.MultiTenancy.Web` đang chạy (debug/run), `dotnet build` chained sẽ failed silent ở step copy DLL — EF rơi về snapshot cũ → file `Up()/Down()` rỗng dù entity đã thay đổi thật sự.

**Why:** MSBuild copy `Genora.MultiTenancy.Domain.dll` vào `Web/bin/Debug/net9.0/` thất bại vì process Web đang lock file. Build "thành công" với --no-build, nhưng snapshot mà EF dùng vẫn là DLL cũ (chưa có entity field mới). EF compare model mới vs snapshot cũ → tưởng không có gì thay đổi → migration body rỗng.

**How to apply:** Trước khi `dotnet ef migrations add`, làm theo trình tự:
1. `Get-Process | Where-Object { $_.ProcessName -like "Genora*" }` — check process
2. Nếu có Web đang chạy: `Stop-Process -Id <id> -Force`
3. `rm -rf src/<project>/bin src/<project>/obj` cho Domain + Domain.Shared + EFCore + Web
4. `dotnet build src/Genora.MultiTenancy.EntityFrameworkCore/...` để rebuild fresh
5. `dotnet ef migrations add <Name> -s ../Genora.MultiTenancy.Web --no-build`

Nếu lỡ tạo migration body rỗng: `dotnet ef migrations remove -s ../Genora.MultiTenancy.Web --no-build` (hoặc xóa file) rồi làm lại.

## Triệu chứng nhận diện
- `dotnet build` báo `error MSB3027 ... locked by: "Genora.MultiTenancy.Web (PID)"` ở Web project nhưng các project con vẫn build OK
- Migration sinh ra `protected override void Up(MigrationBuilder migrationBuilder) { }` body rỗng
- Snapshot file (`MultiTenancyDbContextModelSnapshot.cs`) chưa có column mới
