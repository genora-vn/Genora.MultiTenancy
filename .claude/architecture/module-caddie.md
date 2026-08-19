# Architecture — Module Caddie

> Nguồn: `project_caddie_module_db_design.md`, `project_caddie_module_srs.md`,
> `project_caddie_module_phase1..final_complete.md`, và các note fix. Chi tiết đầy đủ trong `../memory/notes/project/`.

## Phạm vi (SRS 5 module)
Quản lý Caddie / Đặt Caddie / Lịch làm việc / Kỹ năng / Đánh giá.

## Convention DB
- SQL Server, PK = Guid, KHÔNG soft delete.
- Audit: created_at/by, updated_at/by. Naming: snake_case (DB) → PascalCase (C#).
- Enum lưu VARCHAR trong DB, dùng C# enum trong code. Multi-select dùng bảng mapping riêng.
- **`IMultiTenant` trên TẤT CẢ entities** (bắt buộc cho multi-tenant DB routing).

## 10 bảng
| Table | Entity C# | Mô tả |
|-------|-----------|-------|
| golf_courses | AppGolfCourse (thêm `HasNightShift`, `CaddieFee`) | — |
| languages | AppLanguage | Danh mục ngôn ngữ |
| caddies | AppCaddie | Thông tin caddie |
| caddie_voice_regions | AppCaddieVoiceRegion | Mapping vùng giọng nói (NORTH/CENTRAL/SOUTH) |
| caddie_languages | AppCaddieLanguage | Mapping ngoại ngữ |
| caddie_skills | AppCaddieSkill | Danh mục kỹ năng |
| caddie_bookings | AppCaddieBooking | Booking caddie |
| caddie_schedules | AppCaddieSchedule | Lịch làm việc (AVAILABLE/BOOKED/OFF) |
| caddie_ratings | AppCaddieRating | Đánh giá tổng thể (approval flow) |
| caddie_rating_details | AppCaddieRatingDetail | Đánh giá theo kỹ năng |

## ERD
```
GolfCourse (has_night_shift, caddie_fee)
  └── AppCaddie
        ├── AppCaddieLanguage → AppLanguage
        ├── AppCaddieVoiceRegion
        ├── AppCaddieSchedule
        │     └── AppCaddieBooking
        ├── AppCaddieRating
        │     └── AppCaddieRatingDetail → AppCaddieSkill
```

## Mapping sang ABP
- `AuditedAggregateRoot<Guid>`: Caddie, Booking, Rating (root).
- `Entity<Guid>`: mapping tables (CaddieLanguage, CaddieVoiceRegion, RatingDetail).
- `FullAuditedEntity<Guid>`: Schedule (cần track update).

## Tiến hóa gần đây
- **Multi-caddie per booking:** `AppCaddieBookingDetail` (1 booking - N caddie); booking gắn vào từng golf player qua `AppBookingPlayers` (thêm `CaddieId/CaddieBookingId/CaddieName`, migration 20260724091716).
- **CaddieFee:** `GolfCourse.CaddieFee`, `Booking.TotalCaddieFee` cộng vào `TotalAmount` (migration 20260725062150). API upsert (`CaddieBookingId?`) + unassign tự tính phí = count × CaddieFee.
- **Avatar:** bỏ base64 → `IRemoteStreamContent` qua ManageImageService (15MB), `FeatureProtectedCrud`.
- **MiniApp:** 6 endpoints `/api/mini-app/caddie/*`; rating theo mảng (đánh giá từng caddie).
