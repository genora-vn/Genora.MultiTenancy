# Architecture — Overview

> Kiến trúc tổng quan Genora.MultiTenancy. Tổng hợp từ các note trong `.claude/memory/notes/`.

## Nền tảng
- **ABP Framework**, kiến trúc DDD, layered:
  - `Domain` / `Domain.Shared` — entities, enums, constants.
  - `Application` / `Application.Contracts` — AppService + DTO/interface.
  - `EntityFrameworkCore` — DbContext, migrations, EF config.
  - `HttpApi` — controllers.
  - `Web` — Razor Pages / UI (host).
- **Multi-tenancy:** enabled, có tenant dùng **separate database** (chi tiết `multi-tenancy.md`).

## Các module chính
| Module | Doc chi tiết |
|--------|--------------|
| Multi-tenancy & DB routing | [multi-tenancy.md](multi-tenancy.md) |
| Caddie (booking + rating) | [module-caddie.md](module-caddie.md) |
| Hoa Linh (DMS integration) | [module-hoalinh.md](module-hoalinh.md) |
| Salon Beauty | [module-salon-beauty.md](module-salon-beauty.md) |

## Convention xuyên suốt
- Entity prefix theo module: `App` (core), `AppHl` (Hoa Linh), `SalonBeauty*` (schema "Salon").
- Booking dùng aggregate root + child entity; child PHẢI có `IMultiTenant` (xem multi-tenancy).
- Pricing tính từ collection con (vd Booking từ `AppBookingPlayers`), không dựa field tổng ở root.
- Permission dual Host/Tenant + feature gate. Xem [../RULES.md](../RULES.md).
- MiniApp APIs prefix `/api/mini-app/...`; Admin dùng AppService + controller `/api/app/...`.

## Tích hợp ngoài
- **Zalo:** OA articles (news), Zalo Mini App auth, ZBS (Zalo Business Solution) booking.
- **VietQR:** deeplink HTTPS (`https://dl.vietqr.io/pay`), không dùng `vietqr://`.
- **UrBox:** eVoucher (cartPayVoucher, Signature RSA-SHA256).
- **Hoa Linh DMS:** HttpClient "HoaLinhDms", X-API-Key.
