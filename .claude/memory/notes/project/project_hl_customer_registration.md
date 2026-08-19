---
name: project_hl_customer_registration
description: Hoa Linh Mini App — đăng ký/đồng bộ khách hàng vào dbo.AppCustomers khi CheckCustomer
metadata: 
  node_type: memory
  type: project
  originSessionId: 7f8cbb33-af3a-4ee6-a77c-2c0fec37463a
---

Luồng đăng ký tài khoản khách Hoa Linh vào `dbo.AppCustomers` (entity `Customer`) sau khi check bên HL DMS (branch dev, 2026-07).

**Service:** `IHlCustomerAppService`/`HlCustomerAppService` (interface đặt trong AppDtos.HoaLinh, impl trong AppServices/HoaLinh/). Method `UpsertFromHoaLinhAsync(HlCheckCustomerRequest request, HlCustomerDto? hlCustomer, ct)` — idempotent theo PhoneNumber:
- `hlCustomer != null` (tồn tại bên HL DMS): CustomerCode = hlCustomer.CustCode, CustomerSource = **HoaLinh (=5)**, map Address/Birthday từ DMS.
- `hlCustomer == null` (chưa có bên HL DMS): tự sinh mã prefix **HLKH{D6}**, CustomerSource = **ZaloMiniApp (=1)**, lưu thông tin từ Mini App (FullName/AvatarUrl/ZaloUserId/IsFollower).
- Trường không có → để null. Update chỉ ghi đè khi có giá trị mới (giữ data cũ).

**Enum:** thêm `CustomerSource.HoaLinh = 5` (Domain.Shared/Enums/CustomerSource.cs). Lưu int, không cần migration.

**DTO input:** `HlCheckCustomerRequest` (PhoneNumber, FullName, AvatarUrl, ZaloUserId, IsFollower, Note) — khớp payload upsertCustomer từ Mini App.

**Controller (HoaLinhMiniAppController, route api/mini-app/hl):** refactor CheckCustomer thành `CheckAndRegisterAsync` shared:
- `GET auth/{phone}` (cũ, backward-compat, không có info Mini App).
- `POST auth` (mới, body HlCheckCustomerRequest — nhận đủ thông tin Mini App).
- Cả hai: check HL DMS → **luôn upsert AppCustomers** (dù có/không bên HL, nguồn khác nhau) → **luôn trả về HlCustomerDto** (KHÔNG còn trả Fail message "chưa có trong hệ thống"). `UpsertFromHoaLinhAsync` trả `HlCustomerDto`: nếu có HL DMS → trả nguyên DTO DMS (đầy đủ loyalty/tier); nếu không → build từ entity Customer vừa lưu qua `MapEntityToDto` (CustCode/CustName/CustPhone/Address/Birthday, IsCustomer = (source==HoaLinh)). Trường không có → null.

**GetCustomer (GET customer/{phone}) — trả LIST (1 SĐT nhiều bản ghi = nhiều chi nhánh, mini app chọn chi nhánh):** check HL DMS trước (GetCustomerDetailAsync trả `List<HlCustomerDto>`) → nếu có bản ghi trả nguyên list; nếu không → fallback `GetFromAppCustomersAsync` trả list từ dbo.AppCustomers (mọi row cùng PhoneNumber). Luôn trả `HlApiResult<List<HlCustomerDto>>` (list rỗng nếu không có).

**BonusAmount trên auth + customer (2026-07 update):** HlCustomerDto thêm field `BonusAmount` (decimal). `EnrichBonusAmountAsync(List<HlCustomerDto>)` trong HlCustomerAppService: chỉ set BonusAmount (lấy từ dbo.AppCustomers theo CustomerCode) khi **custCode tồn tại + custChannel="OTC" (OrdinalIgnoreCase) + isGkhl==true**; ngược lại = 0. Gọi từ CẢ auth (CheckAndRegisterAsync, sau upsert) VÀ customer (GetCustomer, cho nhánh HL DMS); GetFromAppCustomersAsync tự gọi enrich cho nhánh fallback. Điều kiện dựa vào custChannel/isGkhl từ API HL DMS.

**Service:** `GetFromAppCustomersAsync(phone)` trả `List<HlCustomerDto>` (tất cả row cùng SĐT, OrderBy CreationTime) qua AsyncExecuter.ToListAsync; helper `MapEntityToDto(Customer)`. `[RemoteService(false)]` + `[DisableValidation]` trên class (xem [[feedback_appservice_multi_complex_param]]).

**Mẫu tham khảo:** MiniAppCustomerAppService.UpsertFromMiniAppAsync (golf, cùng entity Customer/dbo.AppCustomers), MiniAppSalonBeautyCustomerAppService. Liên quan [[project_customer_source_enum]], [[project_hoalinh_phase5_complete]].

DI: AddScoped<IHlCustomerAppService, HlCustomerAppService> trong module. Build 0 errors.
