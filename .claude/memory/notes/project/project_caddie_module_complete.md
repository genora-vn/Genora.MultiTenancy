---
name: project-caddie-module-complete
description: "Caddie module hoàn thành Phase 1-7 — entities, migrations, AppServices, UI pages, Mini App APIs"
metadata: 
  node_type: memory
  type: project
  originSessionId: c7f0aab2-9051-4ec9-be74-7c4ad7d1b062
---

## Caddie Module — HOÀN THÀNH (2026-06-01)

### Tổng quan:
Module quản trị Caddie cho sân golf, bao gồm 7 phases đã implement đầy đủ.

### Files tạo mới (tổng ~50 files):

**Domain.Shared/Enums (9 files):**
CaddieStatus, CaddieGender, CaddieVoiceRegion, CaddieShiftCode, CaddieSlotStatus, CaddieBookingStatus, CaddiePaymentStatus, CaddieCheckinStatus, CaddieRatingApprovalStatus

**Domain/DomainModels/AppCaddie (9 files):**
AppCaddie, AppCaddieLanguage, AppCaddieVoiceRegion, AppLanguage, AppCaddieSkill, AppCaddieSchedule, AppCaddieBooking, AppCaddieRating, AppCaddieRatingDetail

**EntityFrameworkCore:**
- MultiTenancyDbContextModelCreatingExtensionsCaddie.cs
- Migrations: AddCaddieModule + AddCaddieNoteField

**Application.Contracts/AppDtos/Caddies (6 files):**
CaddieDto, CreateUpdateCaddieDto, GetCaddieListInput, ICaddieAppService, CaddieSubDtos, CaddieScheduleDtos, CaddieBookingDtos, CaddieRatingDtos, MiniAppCaddieDtos

**Application/AppServices/Caddies (6 files):**
CaddieAppService, CaddieSkillAppService, CaddieLanguageAppService, CaddieScheduleAppService, CaddieBookingAppService, CaddieRatingAppService, MiniAppCaddieAppService

**Web/Pages (6 page groups):**
- AppCaddies/ (Index, CreateModal, EditModal, Detail + JS + CSS)
- AppCaddieSkills/ (Index, CreateModal, EditModal + JS)
- AppLanguages/ (Index, CreateModal, EditModal + JS)
- AppCaddieSchedules/ (Index Calendar, CreateModal + JS)
- AppCaddieBookings/ (Index + JS)
- AppCaddieRatings/ (Index + JS)

**Features + Permissions + Menu + Localization**

### Mini App APIs (MiniAppCaddieAppService):
1. `GetAvailableCaddiesAsync(date, startTime)` — danh sách caddie available
2. `GetCaddieDetailAsync(caddieId)` — chi tiết + recent reviews
3. `CreateBookingAsync(input, customerId, name, phone)` — đặt caddie + lock slot
4. `GetBookingHistoryAsync(customerId)` — lịch sử booking
5. `CreateRatingAsync(input, customerId)` — đánh giá caddie + skill ratings
6. `GetActiveSkillsAsync()` — danh sách kỹ năng cho form đánh giá

### Business Rules đã implement:
- Caddie code auto-generate (CD-001, CD-002...)
- experience_year tính runtime từ JoinDate
- rating_avg chỉ tính từ APPROVED ratings
- Max 2 ca/ngày (3 nếu night shift)
- Status workflow: NEW → CONFIRMED → COMPLETED (CANCELLED từ NEW/CONFIRMED)
- Cancel bắt buộc lý do
- Booking lock/release schedule slot
- Rating approval workflow (PENDING → APPROVED/REJECTED)
- Mỗi booking chỉ 1 rating
- Chỉ COMPLETED booking mới được rating

### Lưu ý kỹ thuật:
- Application layer dùng AsyncExecuter (KHÔNG dùng Microsoft.EntityFrameworkCore)
- Entity<Guid> subclasses dùng constructor cho Id
- Dual permission pattern P(tenant, host)
- GolfCourse entity thêm HasNightShift field

**Why:** Module Caddie hoàn chỉnh, sẵn sàng test
**How to apply:** Chạy migration, bật feature Caddie.Management, gán permissions, seed data Languages + Skills

[[project_caddie_module_srs]] [[project_caddie_module_db_design]] [[feedback_no_ef_in_application_layer]]
