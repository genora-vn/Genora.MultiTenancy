---
name: project-salon-zbs-booking-and-service-review
description: Salon Beauty — gửi ZBS BookingCreated khi tạo booking (CMS + Mini App) và ZBS ServiceReview khi UpdateStatusAsync = Completed; thêm cấu hình ServiceReview TemplateId
metadata: 
  node_type: memory
  type: project
  originSessionId: b0157f35-01a5-463a-9e01-3b5eb1102803
---

Bổ sung 2 luồng ZBS cho Salon Beauty Booking đồng bộ với pattern Booking golf:

1. **Cấu hình ServiceReview TemplateId** — cùng hàng với Cập nhật booking trên `/UpgradeSettings/ZaloZns`.
   - Const mới: `ZaloSettingNames.ZbsServiceReview = "Genora.Zalo.Zbs.Templates.ServiceReview"`.
   - Đăng ký trong `ZaloSettingDefinitionProvider` (Global + Tenant providers).
   - `ZaloZbsTemplateResolver` thêm case `"ServiceReview"`.
   - `ZaloZns.cshtml(.cs)` thêm `Input.ServiceReview` (load + save), label loc-key `UpgradeSettings:ZaloZns:Template:ServiceReview` (vi: "Đánh giá chất lượng dịch vụ TemplateId" / en: "Service review TemplateId").

2. **ZBS BookingCreated cho Salon** — enqueue ngay sau khi save booking thành công ở:
   - `SalonBeautyBookingAppService.CreateAsync` (CMS).
   - `MiniAppSalonBeautyBookingAppService.CreateMiniAppAsync` (Mini App).
   - DI: `IBackgroundJobManager` + `ILogger<...>` (Mini App service tận dụng `Logger` base).
   - Helper `EnqueueBookingCreatedZbsAsync` đặt trong từng service (Salon dùng `Helpers.PhoneHelper`, Mini App load location qua `_locationRepository`).
   - `TemplateKey = "BookingCreated"`; bọc `try/catch` log warning, không fail luồng tạo booking.
   - `TemplateData`:
     - `customer_name = customer.Name`
     - `booking_code = booking.BookingCode`
     - `schedule_time = "{dd/MM/yyyy} {HH:mm}"` (ghép từ `BookingDate` + `StartTime`, format invariant)
     - `address = location.Address` (rỗng nếu không có LocationId)

3. **ZBS ServiceReview** — chỉ enqueue khi `UpdateStatusAsync` set status = `SalonBeautyBookingStatus.Completed`.
   - `TemplateKey = "ServiceReview"`; cùng error-handling try/catch warning.
   - `TemplateData`:
     - `customer_name = customer.Name`
     - `schedule_time = "{dd/MM/yyyy} {HH:mm}"`
   - Không trigger từ flow CheckinAsync/UpdatePaymentAsync (kể cả khi payment update đẩy status thành Completed) — chỉ UpdateStatusAsync theo yêu cầu rõ ràng.

**Why:** đồng bộ ZNS notification cho khách Salon (đặt lịch mới + nhắc đánh giá khi xong dịch vụ); pattern enqueue đồng nhất với `AppBookingService` để bộ phận BE dễ maintain template mapping qua `ZaloZbsTemplateResolver`.

**How to apply:** khi cần thêm template ZBS Salon mới — (1) thêm const trong `ZaloSettingNames`, (2) đăng ký SettingDefinition, (3) thêm case trong `ZaloZbsTemplateResolver`, (4) thêm Input vào `ZaloZns.cshtml(.cs)` + key i18n vi/en, (5) enqueue qua `IBackgroundJobManager` + try/catch log warning, không bao giờ fail luồng nghiệp vụ.

Liên quan: [[project-salon-booking-ui]], [[project-salon-booking-history-change-stylist]], [[project-payment-toggles-and-news-edit-lazy-load]].
