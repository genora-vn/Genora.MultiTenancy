# HLG — Lọc chi nhánh DMS theo customer channel (OTC) + cấu hình động — 2026-10-20

Branch `hotfix/20261020`. Task: [.claude/tasks/20261020_hlg_checkcustomer_dynamic_customer_channel_validation.md](../../../tasks/20261020_hlg_checkcustomer_dynamic_customer_channel_validation.md).

## Vấn đề

`GET /api/mini-app/hlg/auth/{phone}` (CheckCustomer) trả về **toàn bộ** chi nhánh DMS của số điện thoại. DMS chứa cả `RETAILER` (cửa hàng/tạp hóa/siêu thị) và `CONSUMER` (tiêu dùng tự do), nên người không thuộc Kênh Nhà Thuốc vẫn qua bước kiểm tra và đăng ký được vào chương trình.

## Root cause

`HlgProfileAppService.Registration.cs` → `GetDmsBranchesAsync` là **điểm lọc duy nhất**, chỉ lọc `IsCustomer != false`, có `CustCode`, độ dài ≤50, và phone khớp `pharmaPhone`. Không có điều kiện nào trên `CustChannel`. Helper này dùng chung cho `CheckCustomerAsync` và `UpsertCustomerAsync` → sửa 1 chỗ bịt cả 2 luồng.

## Luồng dữ liệu

Controller `auth/{phone}` → `CheckCustomerAsync` → `GetDmsBranchesAsync` → `IHlApiClientService.GetCustomerByPhoneAsync` → `GET {HoaLinhApi:BaseUrl}/api/get-customer-by-phone?phone=` (header `X-API-Key`), deserialize `JsonNamingPolicy.SnakeCaseLower` nên `cust_channel` → `HlCustomerDto.CustChannel` tự động (KHÔNG cần `JsonPropertyName`).

## Đã làm

- **MỚI** `Domain.Shared/Hlg/HlgRegistrationOptions.cs` — section `HlgRegistration`, `AllowedCustomerChannels: string[]?`, `ChannelRestrictedMessage: string?`.
- `Domain.Shared/Hlg/HlgRegistrationRules.cs` — thêm const `ChannelNotAllowed` (message user yêu cầu nguyên văn), `DefaultAllowedCustomerChannels = ["OTC"]`, `AllowAllChannelsToken = "*"`, helper `NormalizeAllowedChannels` / `AllowsAllChannels` / `IsChannelAllowed`.
- `MultiTenancyApplicationModule.cs` — `Configure<HlgRegistrationOptions>` cạnh `HlLoyaltyOptions`.
- `HlgProfileAppService.cs` — inject `IOptionsSnapshot<HlgRegistrationOptions>` (ctor +1 param, cuối danh sách).
- `HlgProfileAppService.Registration.cs` — `GetDmsBranchesAsync` tách `matched` (lọc cũ) → nếu 0 thì vẫn `DmsCustomerNotFound`; sau đó lọc channel → nếu 0 thì `ChannelNotAllowed`; **dedup `CustCode` chạy SAU bước lọc** để bản ghi non-OTC không chiếm chỗ mã trùng. Log `LogInformation` số chi nhánh bị loại + `CustCode`.
- `HoaLinhGamificationController.RegistrationFailure` — `"HlgRegistration:ChannelNotAllowed"` → **403**.
- `Web/appsettings.json` — section `HlgRegistration` với `[ "OTC" ]`.
- Docs: mục 7 trong `docs/HLG_PHARMACY_REGISTRATION_API_20261006.md` + 1 dòng trong bảng lỗi mục 4.

## Quy tắc đã chốt với user

1. Cấu hình bằng **appsettings** (không dùng ABP Setting per-tenant) → đổi channel = sửa appsettings + recycle app pool.
2. Lọc áp dụng cho **cả** `auth/{phone}` và `customer/upsert`.
3. `cust_channel` null/rỗng/giá trị lạ → **loại bỏ** (fail-closed).
4. DMS trả **0 bản ghi** → giữ `DmsCustomerNotFound` (404, có hotline). Chỉ khi **có** bản ghi mà không bản ghi nào thuộc allowed channel mới trả 403 message Kênh Nhà Thuốc.

Fail-closed: config thiếu/rỗng/toàn khoảng trắng → fallback `["OTC"]`. Muốn mở mọi kênh phải ghi `[ "*" ]` tường minh. So sánh `OrdinalIgnoreCase` + `Trim()`, dedup trùng lặp.

## Bẫy gặp khi làm

- `_logger.LogInformation` lỗi CS1061 trong file partial `Registration.cs` vì file đó **thiếu `using Microsoft.Extensions.Logging;`** (extension method cần namespace ngay trong file gọi, dù field khai báo ở partial khác đã có using).
- `[InlineData(new[]{"a","b"}, 2)]` lỗi **CS0182**: C# attribute không cho nested array trong `params object[]`. Workaround đã dùng: truyền string `"OTC|RETAILER"` rồi `.Split('|')` trong test.
- Test `Branch()` helper trong 2 file test KHÔNG set `CustChannel` → sau khi bật filter mọi test registration sẽ đỏ. Đã thêm default `channel = "OTC"` ở `HlgPharmacyRegistrationTests.Branch` và `CustChannel = "OTC"` ở fake DMS của `HlgPharmacyRegistrationSqlServerTests`.

## Verify

- `dotnet build` Application **0 errors**, HttpApi **0 errors**, EF Core test project **0 errors**.
- `dotnet test --filter Hlg` (Application.Tests) **162/162 PASS**, gồm 11 case mới: toàn non-OTC (RETAILER/CONSUMER/rỗng/null) → `ChannelNotAllowed` + đúng message; mix channel → chỉ còn OTC và upsert mã retailer bị `InvalidBranch`; `" otc "` vẫn match; config `OTC|RETAILER`/`*`/`CONSUMER`; config rỗng/trùng → fail-closed OTC; override message.
- **Không migration**, không đổi shape DTO/response → FE không phải sửa.
- Chưa commit, chưa deploy, chưa UAT với DMS thật.

## Lưu ý còn lại

`cust_channel` **không được lưu** ở DB Genora (`HlgUserProfile` chỉ có `PharmaPhone` + `DmsCustomerCode`; không có cột channel nào trong `DomainModels/AppHlg/`) → **không thể** đếm bằng SQL xem có bao nhiêu hồ sơ đã đăng ký đang gắn chi nhánh non-OTC. Muốn biết phải đối soát từng `PharmaPhone` với DMS. Khách đã đăng ký ở chi nhánh non-OTC khi gửi lại `customer/upsert` sẽ bị 403 — đúng yêu cầu nghiệp vụ nhưng cần biết trước khi deploy.

Nếu DMS đổi/thiếu field `cust_channel` → fail-closed chặn hết; mở nhanh bằng `"AllowedCustomerChannels": [ "*" ]` + recycle app pool, không cần deploy code.
