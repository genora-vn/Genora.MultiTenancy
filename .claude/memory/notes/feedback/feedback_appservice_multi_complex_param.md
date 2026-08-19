---
name: feedback_appservice_multi_complex_param
description: "ApplicationService nội bộ có method >1 complex param phải [RemoteService(false)]"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 7f8cbb33-af3a-4ee6-a77c-2c0fec37463a
---

Service kế thừa `ApplicationService` (hoặc implement I...AppService) mặc định bị ABP auto-expose thành REST API theo convention. Nếu một public method có **>1 tham số complex-type**, ABP ném `AbpException: "Only one complex type allowed as argument to a controller action that's binding source is 'Body'"` khi build route — làm hỏng CẢ các endpoint khác (lỗi ở tầng khởi tạo route, không phải runtime của method đó).

**Why:** ABP auto-API chỉ cho phép 1 complex type bind vào body. Method như `UpsertFromHoaLinhAsync(HlCheckCustomerRequest req, HlCustomerDto? hl, ct)` có 2 → vi phạm.

**How to apply:** Service nội bộ (chỉ controller/service khác gọi, không cần auto-API) → gắn `[RemoteService(false)]` lên class (using Volo.Abp). Nếu CẦN expose API thì gộp param thành 1 DTO bọc ngoài. Gặp lần đầu ở HlCustomerAppService (2026-07), fix bằng [RemoteService(false)]. Liên quan [[project_hl_customer_registration]].

**Lỗi kèm theo — AbpValidationException khi param reference-type = null:** ABP validation interceptor chạy trên MỌI method của ApplicationService, coi param reference-type KHÔNG có default là bắt buộc → truyền null ném "Method arguments are not valid". Annotation nullable (`Foo?`) KHÔNG đủ để optional với ABP — PHẢI có giá trị default `= null` để `parameter.IsOptional == true`. Fix triệt để: (1) thêm `= null` cho param optional trong interface, (2) gắn `[DisableValidation]` (using Volo.Abp.Validation) lên service nội bộ đã validate ở tầng controller.
