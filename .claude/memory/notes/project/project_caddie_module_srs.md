---
name: project-caddie-module-srs
description: "SRS nghiệp vụ quản lý Caddie — 5 module, entities, business rules, enums, workflows"
metadata: 
  node_type: memory
  type: project
  originSessionId: c7f0aab2-9051-4ec9-be74-7c4ad7d1b062
---

## Module Quản lý Caddie — SRS Summary

### 5 Module chính:
1. **Quản lý Caddie** — CRUD caddie (code, name, avatar, gender, phone, golf_course, join_date, height_cm, voice_regions, languages, rating_avg, total_booking, status, is_show_on_app)
2. **Đặt Caddie** — Booking caddie từ Mini App (booking_code, customer, caddie, schedule, booking_date, start_time, number_of_holes, status workflow, payment_status, checkin_status)
3. **Quản lý Lịch Caddie** — Schedule theo ca (MORNING/AFTERNOON/NIGHT), slot_status (AVAILABLE/BOOKED/OFF), max 2 ca/ngày (3 nếu sân có đèn)
4. **Quản lý Kỹ năng Caddie** — Danh mục skill (COURSE_KNOWLEDGE, DISTANCE_SUPPORT, PUTTING_LINE, ATTITUDE, COMMUNICATION...)
5. **Đánh giá Caddie** — Rating overall (1-5) + rating theo skill, approval workflow (PENDING/APPROVED/REJECTED)

### Entities cần tạo:
- `AppCaddie` — thông tin caddie
- `AppCaddieLanguage` — mapping caddie ↔ language (many-to-many)
- `AppCaddieVoiceRegion` — mapping caddie ↔ voice region (NORTH/CENTRAL/SOUTH)
- `AppCaddieSkill` — danh mục kỹ năng
- `AppCaddieBooking` — booking caddie
- `AppCaddieSchedule` — lịch làm việc caddie
- `AppCaddieRating` — đánh giá tổng thể
- `AppCaddieRatingDetail` — đánh giá theo kỹ năng
- `AppLanguage` — danh mục ngôn ngữ (dùng chung)

### Enums:
- CaddieStatus: Active=1, Inactive=2
- CaddieShiftCode: Morning=1, Afternoon=2, Night=3
- CaddieSlotStatus: Available=1, Booked=2, Off=3
- CaddieBookingStatus: New=1, Confirmed=2, Completed=3, Cancelled=4
- CaddiePaymentStatus: Unpaid=1, Paid=2
- CaddieCheckinStatus: NotCheckedIn=1, CheckedIn=2
- CaddieRatingApprovalStatus: Pending=1, Approved=2, Rejected=3
- VoiceRegion: North=1, Central=2, South=3

### Business Rules quan trọng:
- caddie_code unique, hệ thống tự sinh
- experience_year tính runtime từ join_date (không lưu DB)
- rating_avg chỉ tính từ rating APPROVED
- Một caddie tối đa 2 ca/ngày (3 nếu sân has_night_shift)
- NIGHT shift chỉ cho sân hỗ trợ (has_night_shift=true trên GolfCourse)
- Không cho double booking caddie cùng thời gian
- Booking thành công → lock slot (BOOKED), hủy → release (AVAILABLE)
- Status workflow: NEW → CONFIRMED → COMPLETED (hoặc CANCELLED từ NEW/CONFIRMED)
- Booking COMPLETED mới được đánh giá, mỗi booking chỉ 1 lần
- Đánh giá mặc định PENDING, chỉ APPROVED mới hiển thị Mini App

### Quan hệ FK:
- Caddie → GolfCourse (golf_course_id)
- CaddieLanguage → Caddie + Language
- CaddieSchedule → Caddie
- CaddieBooking → Customer + Caddie + Schedule + GolfCourse
- CaddieRating → Booking + Customer + Caddie
- CaddieRatingDetail → Rating + Skill

**Why:** Xây dựng module quản trị Caddie cho sân golf, cho phép khách đặt caddie qua Mini App
**How to apply:** Tham khảo pattern Salon Beauty (Stylist + Booking + TimeSlot) nhưng adapt cho nghiệp vụ golf caddie

[[project_salon_stylist_ui]] [[project_salon_booking_ui]] [[project_salon_location_timeslot_ui]]
