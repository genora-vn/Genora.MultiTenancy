---
name: project_caddie_email_feature_fixes_june16
description: Fix caddie schedule import duplicate key + email Cc/Bcc + feature menu toggle + email template visibility
metadata: 
  node_type: memory
  type: project
  originSessionId: cb23966a-9de2-4485-88c4-a0598c2cffcd
---

## 1. Fix Caddie Schedule Import — Nhiều khung giờ / 1 ca
**Root cause:** Unique index `IX_AppCaddieSchedules_TenantId_Caddie_Date_Shift` chỉ gồm (TenantId, CaddieId, WorkDate, ShiftCode) → không cho phép nhiều time slot cùng ca (vd Sáng: 6:00-9:00 + 9:00-12:00).

**Fix:**
- Đổi unique index thành (TenantId, CaddieId, WorkDate, ShiftCode, **StartTime**) → index name: `IX_AppCaddieSchedules_TenantId_Caddie_Date_Shift_Start`
- `CreateAsync` query check đúng 4 trường (CaddieId, WorkDate, ShiftCode, StartTime) → upsert chính xác từng time slot
- Migration: `20260616072142_Update_CaddieSchedule_UniqueIndex_Add_StartTime`

**Business:** 1 caddie có thể có nhiều khung giờ trong cùng 1 ca:
- Ca sáng (1): 6:00-9:00 + 9:00-12:00
- Ca chiều (2): 12:30-15:30 + 15:30-18:30
- Ca tối (3): 18:30-21:30

**Files:** EF config + CaddieScheduleAppService.CreateAsync + Migration

## 2. Fix Email Cc/Bcc — Không gửi được cho Cc/Bcc recipients
**Root cause:** `SendEmailJob` chỉ dùng `IEmailSender.SendAsync(to, subject, body)` loop cho từng To — hoàn toàn bỏ qua CcEmails và BccEmails.

**Fix:** Chuyển sang dùng `MailMessage` object để gửi email đầy đủ To/Cc/Bcc trong 1 message duy nhất. Người nhận thấy được danh sách To + Cc (Bcc thì ẩn theo RFC).

**File:** `Application/AppServices/AppEmails/Jobs/SendEmailJob.cs`

## 3. Feature Toggle — Menu Gửi email / Cấu hình template Email
Thêm 2 features mới:
- `MiniAppEmail.AllowEmailSettings` — bật/tắt menu "Gửi email"
- `MiniAppEmail.AllowEmailTemplate` — bật/tắt menu "Cấu hình template Email"

Menu tenant kiểm tra feature trước khi AddItem. Host luôn thấy (không bị feature gate).

**Files:**
- `Application.Contracts/Features/AppEmails/AppEmailFeatures.cs` — thêm 2 constants
- `Application.Contracts/Features/AppEmails/AppEmailFeatureDefinitionProvider.cs` — define 2 features
- `Domain.Shared/Localization/MultiTenancy/en.json` — localization strings
- `Web/Menus/MultiTenancyMenuContributor.cs` — conditional menu items

**Behavior:**
- Uncheck "Bật quản lý cài đặt" (Management=false) → ẩn cả 2 menu (existing behavior)
- Management=true + AllowEmailSettings=true + AllowEmailTemplate=false → hiện "Gửi email", ẩn "Cấu hình template Email"
- Management=true + AllowEmailSettings=false + AllowEmailTemplate=true → ẩn "Gửi email", hiện "Cấu hình template Email"
- Management=true + cả 2 = true → hiện cả 2

## 4. Fix Caddie Rating — EntityNotFoundException trên staging
**Root cause:** `RecalculateCaddieRatingJob` nhận `TenantId` trong args nhưng KHÔNG switch tenant context trước khi query. Trên staging (separate DB per tenant), job chạy ở host context → query host DB → không tìm thấy caddie.

**Fix:** Inject `ICurrentTenant` và wrap toàn bộ logic trong `using (_currentTenant.Change(args.TenantId))`.

**File:** `Application/AppServices/Caddies/RecalculateCaddieRatingJob.cs`

[[project_multitenant_db_routing]]
