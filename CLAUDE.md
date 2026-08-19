# Genora.MultiTenancy (ABP Backend)

## Project Context
- **Framework:** ABP Framework (DDD Architecture).
- **Core Modules:** Calendar Slots, Zalo Auths, Bookings, Golf Courses, News Services.
- **Tenancy:** Multi-tenancy enabled.

## Code Map (Luồng làm việc)
- **Entities:** Nằm tại `src/Genora.MultiTenancy.Domain/DomainModels/`.
- **DTOs & Interfaces:** Nằm tại `src/Genora.MultiTenancy.Application.Contracts/AppDtos/`.
- **Logic thực thi:** Nằm tại `src/Genora.MultiTenancy.Application/AppServices/`.
- **Controllers:** Nằm tại `src/Genora.MultiTenancy.HttpApi/Controllers/`.
- **UI/Pages:** Nằm tại `src/Genora.MultiTenancy.Web/Pages/`.

## Coding Rules
- Luôn ưu tiên dùng **AppService** thay vì viết logic trực tiếp ở Controller.
- Khi tạo Entity mới, phải kiểm tra `Domain.Shared` để xem các hằng số (Constants) hoặc Enums đã có chưa.
- Mọi Method trong AppService phải là `Task` (Async).

## Commands
- **Build:** `dotnet build`
- **Run Host:** `dotnet run --project src/Genora.MultiTenancy.Web`

## Project Memory (.claude)
Toàn bộ project memory được chuẩn hóa trong thư mục `.claude/`. Khi bắt đầu phiên mới, đọc theo thứ tự trong `LOAD_CONTEXT.md`.

- **Bắt đầu phiên mới:** [.claude/LOAD_CONTEXT.md](.claude/LOAD_CONTEXT.md)
- **Quy tắc & lessons learned:** [.claude/RULES.md](.claude/RULES.md)
- **Trạng thái module:** [.claude/PROJECT_STATE.md](.claude/PROJECT_STATE.md)
- **Việc đang làm dở:** [.claude/ACTIVE_CONTEXT.md](.claude/ACTIVE_CONTEXT.md)
- **Nhật ký task:** [.claude/TASK_LOG.md](.claude/TASK_LOG.md)
- **Index memory:** [.claude/MEMORY.md](.claude/MEMORY.md)
- **Kiến trúc:** `.claude/architecture/` · **Bàn giao:** `.claude/handover/HANDOFF.md`
- **Note gốc (zero-loss, 108 file):** `.claude/memory/notes/`
- **Salon Beauty (implementation cũ):** `.claude/memory/modules/salon-beauty/`