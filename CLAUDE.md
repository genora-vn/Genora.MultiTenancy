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