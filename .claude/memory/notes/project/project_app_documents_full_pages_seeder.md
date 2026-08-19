---
name: project-app-documents-full-pages-seeder
description: "Document seeder expanded with ~50 pages across 11 sections, partial class pattern, 2 new sections added"
metadata: 
  node_type: memory
  type: project
  originSessionId: c7f0aab2-9051-4ec9-be74-7c4ad7d1b062
---

AppDocumentsDataSeedContributor đã được mở rộng thành partial class với nhiều file:
- `.cs` — logic chính (SeedAsync + BuildSeeds + upsert pages)
- `.Pages.cs` — router GetPagesForSection()
- `.MiniAppSetup.cs` — 5 pages (Giới thiệu, Cấu hình chung, Cấu hình thanh toán, Cấu hình trang chủ, Tích hợp Zalo OA)
- `.GolfTeeTimes.cs` — 7 pages (bỏ cơ sở + lịch làm việc)
- `.SalonLocationSchedule.cs` — 3 pages (section mới "salon-location-schedule": Cơ sở & Lịch làm việc)
- `.CustomerBooking.cs` — 3 pages (chỉ Golf: KH Golf, Đặt chỗ Golf)
- `.CustomerBookingSalon.cs` — 3 pages (section mới "customer-booking-salon": KH Salon, Đặt lịch Salon)
- `.Loyalty.cs` — 2 pages
- `.Fnb.cs` — 5 pages
- `.Proshop.cs` — 5 pages
- `.SalonBeauty.cs` — 6 pages
- `.News.cs` — 2 pages
- `.SystemAdmin.cs` — 7 pages

Sections (11 total): mini-app-setup, golf-tee-times, salon-location-schedule, salon-beauty, proshop, fnb, customer-booking, customer-booking-salon, loyalty, news, system-admin

**Why:** User muốn tài liệu hướng dẫn đầy đủ cho toàn bộ menu, tách Salon ra section riêng (Cơ sở & Lịch làm việc, KH & Đặt chỗ Salon)
**How to apply:** Khi thêm chức năng mới, tạo PageSeed trong file partial tương ứng; khi thêm section mới, thêm vào BuildSeeds() + router + file partial mới

[[project_app_documents_feature]]
