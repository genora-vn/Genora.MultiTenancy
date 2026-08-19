---
name: project-promotion-policy-feature
description: "PromotionPolicy entity tách chính sách hoãn hủy ra khỏi GolfCourse, lookup theo (GolfCourseId, PromotionTypeId)"
metadata: 
  node_type: memory
  type: project
  originSessionId: 7efaee30-d586-4c55-96be-4a4c14b62d2a
---

AppPromotionPolicies entity mới (tách khỏi AppGolfCourses) cho phép cấu hình chính sách hoãn hủy động theo từng loại ưu đãi.

**Schema**: Id, GolfCourseId, PromotionTypeId, PolicyTitle, CancellationPolicyHours (int?, áp dụng T2-T6), CancellationPolicyHoursWeekend (int?, áp dụng T7+CN), CancellationPolicyContent, IMultiTenant. Unique index (TenantId, GolfCourseId, PromotionTypeId). Migrations: `20260518111048_Add_AppPromotionPolicies_Table`, `20260521155434_Add_CancellationPolicyHoursWeekend_To_AppPromotionPolicies`.

**Why**: Mỗi sân golf cần nhiều policy khác nhau theo loại ưu đãi (Best deal, Promotion, Normal). Trước đây 1 record GolfCourse chỉ có 1 cancellation policy chung, không đáp ứng được yêu cầu mini app.

**How to apply**:
- API `/api/mini-app/get-calendar-slots/{id}` đã enrich PolicyTitle, CancellationPolicyHours, CancellationPolicyContent từ AppPromotionPolicies dựa trên (GolfCourseId, PromotionTypeId của slot). PromotionTypeId vẫn trả từ slot như cũ.
- API `/api/mini-app/get-bookings` (`MiniAppBookingAppService.GetListMiniAppAsync`) tính `isCancellationPolicy` theo PromotionPolicy:
  - Không có policy match (slot.PromotionTypeId, booking.GolfCourseId) → `false` (hoãn hủy thoải mái).
  - Chọn cột giờ theo `playDateTime.DayOfWeek`: T7/CN dùng `CancellationPolicyHoursWeekend`, T2-T6 dùng `CancellationPolicyHours`.
  - Cột chọn ra null hoặc `<= 0` → `false` (unlimited window).
  - `Hours > 0`: so sánh khoảng cách từ `Clock.Now` đến **giờ chơi** = `PlayDate.Date + slot.TimeFrom` (KHÔNG phải `CreationTime`):
    - `remaining >= hours` → còn đủ thời gian theo policy → `false` (cho phép hủy).
    - `remaining < hours` → `true` (không cho hủy).
  - Ví dụ Best deal 72h: book 2026-05-18 19:22, play 2026-05-22 06:00. Tại 18/5 → remaining ~82h, false. Tại 19/5 19:22 → remaining ~58h, true.
  - Đã bỏ ràng buộc theo `GolfCourse.PromotionTypeIds` (CSV cũ); helper legacy giữ lại nhưng không gọi từ list mini app.
- UI cấu hình GolfCourse đã bỏ: CancellationPolicy textarea, CancellationPolicyHours input, PromotionTypeIds multi-select. Vẫn giữ lại column trong DB (không drop) và các property entity/DTO để không phá data cũ — chỉ ẩn khỏi UI.
- UI mới: `/AppPromotionPolicies` (Index + Create/Edit modal XL). Dùng dual permission pattern (`AppPromotionPolicies` / `HostAppPromotionPolicies`) + feature `MiniAppPromotionPolicy.Management`.
- Service `IAppPromotionPolicyService.GetEditDataAsync(Guid? id)` trả về DTO kèm AvailableGolfCourses (chỉ active) và AvailablePromotionTypes (chỉ Status=true) để render dropdown trong modal.
- AppService extend [[feedback_abp_dual_permission_pattern]] với base `FeatureProtectedCrudAppService`.

Menu: nằm trong group "Sân golf & Giờ chơi", order=2 (ngay sau cấu hình Sân golf).
