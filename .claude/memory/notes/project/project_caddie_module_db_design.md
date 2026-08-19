---
name: project-caddie-module-db-design
description: "Database design module Caddie Booking & Rating — 10 tables, FK relationships, ERD"
metadata: 
  node_type: memory
  type: project
  originSessionId: c7f0aab2-9051-4ec9-be74-7c4ad7d1b062
---

## Database Design — Module Caddie Booking & Rating

### Quy ước:
- Database: SQL Server
- PK: UUID (Guid)
- Soft Delete: Không sử dụng
- Audit: created_at, created_by, updated_at, updated_by
- Naming: snake_case (DB) → PascalCase (C# entity)
- Enum: lưu VARCHAR trong DB, dùng C# enum trong code
- Multi Select: bảng mapping riêng

### 10 Bảng:

| # | Table | Entity C# | Mô tả |
|---|-------|-----------|--------|
| 1 | golf_courses | (đã có AppGolfCourse) | Thêm field has_night_shift |
| 2 | languages | AppLanguage | Danh mục ngôn ngữ |
| 3 | caddies | AppCaddie | Thông tin caddie |
| 4 | caddie_voice_regions | AppCaddieVoiceRegion | Mapping vùng giọng nói |
| 5 | caddie_languages | AppCaddieLanguage | Mapping ngoại ngữ |
| 6 | caddie_skills | AppCaddieSkill | Danh mục kỹ năng |
| 7 | caddie_bookings | AppCaddieBooking | Booking caddie |
| 8 | caddie_schedules | AppCaddieSchedule | Lịch làm việc |
| 9 | caddie_ratings | AppCaddieRating | Đánh giá tổng thể |
| 10 | caddie_rating_details | AppCaddieRatingDetail | Đánh giá theo kỹ năng |

### Chi tiết cấu trúc:

#### AppCaddie
- Id (Guid, PK)
- CaddieCode (string 50, unique)
- CaddieName (string 255)
- Avatar (string 500, nullable)
- Gender (string 20, nullable — MALE/FEMALE)
- Phone (string 20, nullable)
- GolfCourseId (Guid, FK → AppGolfCourse)
- JoinDate (DateTime?, nullable)
- HeightCm (int?, nullable)
- RatingAvg (decimal(2,1)?, nullable)
- TotalBooking (int?, nullable)
- Status (string 20 — ACTIVE/INACTIVE)
- IsShowOnApp (bool)
- CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

#### AppCaddieLanguage
- Id (Guid, PK)
- CaddieId (Guid, FK → AppCaddie)
- LanguageId (Guid, FK → AppLanguage)
- CreatedAt

#### AppCaddieVoiceRegion (mapping caddie ↔ VoiceRegion enum)
- Id (Guid, PK)
- CaddieId (Guid, FK → AppCaddie)
- VoiceRegion (string 20 — NORTH/CENTRAL/SOUTH)
- CreatedAt

#### AppLanguage
- Id (Guid, PK)
- LanguageCode (string 20, unique)
- LanguageName (string 100)
- NativeName (string 100, nullable)
- Status (string 20 — ACTIVE/INACTIVE)
- SortOrder (int?, nullable)
- CreatedAt, UpdatedAt

#### AppCaddieSkill
- Id (Guid, PK)
- SkillCode (string 50, unique)
- SkillName (string 255)
- Description (string 1000, nullable)
- SortOrder (int?, nullable)
- Status (string 20 — ACTIVE/INACTIVE)
- CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

#### AppCaddieBooking
- Id (Guid, PK)
- BookingCode (string 50, unique)
- CustomerId (Guid, FK → AppCustomer)
- CustomerName (string 255 — snapshot)
- Phone (string 20 — snapshot)
- GolfCourseId (Guid, FK → AppGolfCourse)
- CaddieId (Guid, FK → AppCaddie)
- ScheduleId (Guid, FK → AppCaddieSchedule)
- BookingDate (Date)
- StartTime (Time)
- NumberOfHoles (int?, nullable — 9/18)
- Note (string 1000, nullable)
- Status (string 20 — NEW/CONFIRMED/COMPLETED/CANCELLED)
- PaymentStatus (string 20 — UNPAID/PAID)
- CheckinStatus (string 30 — NOT_CHECKED_IN/CHECKED_IN)
- CheckinTime (DateTime?, nullable)
- CancelReason (string 1000, nullable)
- CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

#### AppCaddieSchedule
- Id (Guid, PK)
- CaddieId (Guid, FK → AppCaddie)
- WorkDate (Date)
- ShiftCode (string 20 — MORNING/AFTERNOON/NIGHT)
- StartTime (Time)
- EndTime (Time)
- SlotStatus (string 20 — AVAILABLE/BOOKED/OFF)
- BookingId (Guid?, nullable, FK → AppCaddieBooking)
- IsNightShift (bool?, nullable)
- Note (string 1000, nullable)
- CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

#### AppCaddieRating
- Id (Guid, PK)
- BookingId (Guid, FK → AppCaddieBooking)
- CustomerId (Guid, FK → AppCustomer)
- CaddieId (Guid, FK → AppCaddie)
- OverallRating (int — 1-5)
- Comment (string 2000, nullable)
- ApprovalStatus (string 20 — PENDING/APPROVED/REJECTED)
- ApprovedAt (DateTime?, nullable)
- ApprovedBy (Guid?, nullable)
- RejectReason (string 1000, nullable)
- CreatedAt, CreatedBy

#### AppCaddieRatingDetail
- Id (Guid, PK)
- RatingId (Guid, FK → AppCaddieRating)
- SkillId (Guid, FK → AppCaddieSkill)
- Score (int — 1-5)

### ERD:
```
GolfCourse (has_night_shift mới)
  └── AppCaddie
        ├── AppCaddieLanguage → AppLanguage
        ├── AppCaddieVoiceRegion
        ├── AppCaddieSchedule
        │     └── AppCaddieBooking
        ├── AppCaddieRating
        │     └── AppCaddieRatingDetail → AppCaddieSkill
```

### Lưu ý mapping sang ABP:
- Dùng `AuditedAggregateRoot<Guid>` cho root entities (Caddie, Booking, Rating)
- Dùng `Entity<Guid>` cho mapping tables (CaddieLanguage, CaddieVoiceRegion, RatingDetail)
- Dùng `FullAuditedEntity<Guid>` cho Schedule (cần track update)
- GolfCourse entity đã có → chỉ thêm field `HasNightShift` (bool)
- IMultiTenant trên tất cả entities (bắt buộc cho multi-tenant DB routing)

**Why:** Database design chi tiết cho module Caddie, mapping sang C# entities
**How to apply:** Tạo entities theo cấu trúc này, migration EF Core, đảm bảo IMultiTenant

[[project_caddie_module_srs]]
