# HLG — đăng ký chủ nhà thuốc / nhân viên (2026-10-06)

## Quy tắc đã triển khai

- Một nhóm nhà thuốc có **5 tài khoản tổng cộng: 1 chủ + tối đa 4 nhân viên**, theo `PharmaPhone`, không chia hạn mức theo từng chi nhánh.
- `phone` là người đăng ký; `pharmaPhone` là chủ nhà thuốc. Nếu không truyền `pharmaPhone`, backend hiểu là chủ tự đăng ký (`pharmaPhone = phone`). Chuẩn hóa `+84`/`84` về `0`.
- DMS được tra bằng **số chủ nhà thuốc**. Nhân viên không bắt buộc có số cá nhân trong DMS. Trả tất cả chi nhánh hợp lệ của chủ, không lọc bỏ chi nhánh `isGkhl=false`.
- Nhân viên chỉ đăng ký khi số chủ đã tồn tại trong `dbo.AppCustomers` **cùng tenant**. Chủ đã đăng ký Sales cũng hợp lệ, không bắt buộc đã có profile HLG; vẫn dành một suất cho chủ.
- Backend kiểm tra lại DMS, chi nhánh và hạn mức trong `customer/upsert`. Preflight không giữ chỗ; khi hai người cùng thấy còn chỗ, kết quả POST mới quyết định.
- Nhóm đã đủ vẫn cho phép gửi lại yêu cầu của thành viên đã liên kết; không tạo thêm hồ sơ. Không cho tự đổi sang chủ khác hoặc đổi số điện thoại của profile đã liên kết qua PUT profile.
- `PharmacyCode`/`VgaCode` cũ được giữ nguyên. Hai trường mới nullable nằm ở `HLG.AppHlgUserProfiles`: `PharmaPhone nvarchar(20)` và `DmsCustomerCode nvarchar(50)`; không đổi schema `dbo.AppCustomers`.
- Theo xác nhận của anh: **giữ CustomerCode có sẵn ở Sales/HLG**, lưu mã chi nhánh đã chọn tại `DmsCustomerCode`. Không thay đổi `BonusPoint`/`BonusAmount` trong đăng ký.

## 1. CheckCustomer — API mới

```http
GET /api/mini-app/hlg/auth/{phone}?pharmaPhone={ownerPhone}
```

Chủ nhà thuốc:

```bash
curl --location 'https://localhost:44374/api/mini-app/hlg/auth/0916874340'
```

Nhân viên (đổi số ví dụ thành số đã giải mã từ Zalo):

```bash
curl --location 'https://localhost:44374/api/mini-app/hlg/auth/0900000002?pharmaPhone=0916874340'
```

Trả thành công theo envelope HLG hiện tại; ví dụ rút gọn:

```json
{
  "data": {
    "phone": "0916874340",
    "pharmaPhone": "0916874340",
    "isOwner": true,
    "canRegister": true,
    "linkedAccountCount": 1,
    "maxLinkedAccounts": 5,
    "branches": [
      {
        "custCode": "C4942027104",
        "custName": "2. Nhà Thuốc Hồng Nhung (Phạm Thị Hồng Nhung)",
        "address": "Số 43, đường Tiểu La, Xã Thăng Bình, Thành phố Đà Nẵng",
        "custPhone": "0916874340",
        "isCustomer": true,
        "isGkhl": false
      },
      {
        "custCode": "C49N1000170",
        "custName": "2. Nhà Thuốc Hồng Nhung (Phạm Thị Hồng Nhung)",
        "address": "43 Tiểu La, Xã Thăng Bình, Thành phố Đà Nẵng",
        "custPhone": "0916874340",
        "isCustomer": true,
        "isGkhl": true
      }
    ]
  }
}
```

`branches` dùng camelCase (khác snake_case từ DMS). Các trường còn lại của `HlCustomerDto` như nhà phân phối, nhân viên phụ trách, điểm/hạng DMS cũng được giữ trong response. Các trường bổ sung riêng của Sales (`existsInGenora`, `bonusAmount`, `source`) không được HLG enrich; FE không dùng chúng để kết luận điều kiện đăng ký. `linkedAccountCount` tính cả suất dành cho chủ nên tối thiểu 1 kể cả trước khi chủ đăng ký.

GET này không tạo/cập nhật Customer hoặc HLG profile. DMS client vẫn dùng logging có sẵn.

## 2. customer/upsert — API cập nhật

```http
POST /api/mini-app/hlg/customer/upsert
Content-Type: application/json
```

Chủ đăng ký trước:

```bash
curl --location 'https://localhost:44374/api/mini-app/hlg/customer/upsert' \
  --header 'Content-Type: application/json' \
  --data '{
    "phone": "0916874340",
    "pharmaPhone": "0916874340",
    "customerCode": "C49N1000170",
    "fullName": "Phạm Thị Hồng Nhung",
    "customerType": "pharmacy",
    "address": "43 Tiểu La, Xã Thăng Bình, Thành phố Đà Nẵng",
    "isFollower": true
  }'
```

Nhân viên chọn cùng chi nhánh:

```bash
curl --location 'https://localhost:44374/api/mini-app/hlg/customer/upsert' \
  --header 'Content-Type: application/json' \
  --data '{
    "phone": "0900000002",
    "pharmaPhone": "0916874340",
    "customerCode": "C49N1000170",
    "fullName": "Nhân viên nhà thuốc",
    "customerType": "pharmacy",
    "address": "43 Tiểu La, Xã Thăng Bình, Thành phố Đà Nẵng",
    "isFollower": true
  }'
```

Thêm `zaloUserId`, `avatarUrl`, `gender`, `birthday` từ dữ liệu Zalo khi có; các field cũ vẫn hỗ trợ. FE truyền `customerType` hợp lệ để hoàn tất đăng ký (`isRegistered=true`); backend giữ quy tắc cũ về hồ sơ chưa chọn loại khách hàng.

**Phân biệt mã trong request và response:**

| Trường hợp | Request `customerCode` | Response `data.customerCode` / AppCustomers | Response `data.dmsCustomerCode` / HLG profile |
|---|---|---|---|
| Chủ hoàn toàn mới | Mã chi nhánh từ GET | Mã DMS được chọn | Mã DMS được chọn |
| Nhân viên hoàn toàn mới | Mã chi nhánh từ GET | Mã mới dạng `HLGKH000001` | Mã DMS được chọn |
| Khách Sales/HLG có mã sẵn | Mã chi nhánh từ GET | Giữ mã hiện có | Mã DMS được chọn |

Mã DMS phải thuộc danh sách chi nhánh của `pharmaPhone`; gửi mã tùy ý bị từ chối. Ràng buộc SQL hiện tại là unique `(TenantId, CustomerCode)` với filter `[IsActive] = 1 AND [CustomerCode] IS NOT NULL`. Không gán một mã DMS cho nhiều khách hàng đang hoạt động. Backend cũng tránh tái sử dụng mã của hồ sơ bị xóa mềm.

Nếu chủ mới chọn mã DMS đã thuộc khách hàng khác, trả lỗi 409 để đối soát danh tính; không tự merge hoặc chuyển mã Sales.

## 3. Lấy lại hồ sơ sau đăng ký

```bash
curl --location 'https://localhost:44374/api/mini-app/hlg/customer/by-phone?phone=0900000002'
```

Response `GamificationUserDto` bổ sung `pharmaPhone`, `customerCode`, `dmsCustomerCode`. Dữ liệu cũ chưa liên kết có `pharmaPhone`/`dmsCustomerCode = null`. Không tự suy luận chủ từ `PharmacyCode` cũ.

## 4. Lỗi và xử lý FE

Giữ convention HLG: lỗi nghiệp vụ trả **HTTP 200**, mã lỗi nằm ở `body.error`. Không dùng `response.ok` làm dấu hiệu đăng ký thành công. Khi có `error`, FE hiển thị `message` và không cho đi tiếp; không đọc `data.branches` khi `data` null.

| `error` | Điều kiện | `message` |
|---|---|---|
| 404 | Không có chi nhánh DMS hợp lệ của số chủ | Số điện thoại của bạn chưa có trong hệ thống. Xin vui lòng liên hệ Hotline 0977872631 để được hỗ trợ |
| 403 | Nhân viên đăng ký trước chủ | Vui lòng thông báo chủ nhà thuốc đăng ký tài khoản trước khi bạn thực hiện đăng ký. |
| 409 | Nhóm đã đủ 5, đang thêm người mới | Số lượng tài khoản liên kết đã vượt quá quy định của Nhà thuốc. |
| 400 | Chưa chọn chi nhánh / mã không thuộc số chủ / input sai | Thông báo tương ứng |
| 409 | Đổi nhóm, trùng định danh, hồ sơ đã xóa, mã bị chiếm hoặc khóa đang bận | Thông báo tương ứng |
| 503 | DMS không kiểm tra được | Không thể kiểm tra thông tin nhà thuốc lúc này. Vui lòng thử lại sau. |

Ví dụ nhóm đã đủ:

```json
{"error":409,"message":"Số lượng tài khoản liên kết đã vượt quá quy định của Nhà thuốc.","data":null}
```

Luồng FE: decode-phone → nhập/xác định số chủ → gọi CheckCustomer với **cả số người đăng ký và số chủ** → chọn `branches[].custCode` và địa chỉ → POST upsert → chỉ đi tiếp nếu không có `error` và `data.isRegistered=true`.

## 5. Host, tenant và rollout

- Local Host dùng `TenantId=null`; tenant dùng context tenant do ABP resolve. GET và POST phải đi cùng tenant. Khi test tenant trên localhost có thể thêm header `__tenant: <TENANT_GUID>`; Host bỏ header này. Trên staging/production sử dụng đúng URL tenant được cấu hình/gateway hiện có.
- Backend tái sử dụng `IHlApiClientService`/named HTTP client `HoaLinhDms`, API key ở cấu hình server. FE không gọi DMS trực tiếp và không nhận API key.
- Migration mới: `20261006054856_AddHlgPharmacyRegistration`; [SQL idempotent](HLG_PHARMACY_REGISTRATION_20261006.sql). Chỉ hai nullable columns + một index HLG, không seed/backfill, không thay Customer hoặc dữ liệu Sales.
- SQL chỉ chứa migration mới, giả định database đã có schema/history hợp lệ tới `20261001040131_AddHlgRankingResultSnapshots`. Review và apply vào đúng Host/tenant DB phục vụ HLG trước khi chạy binary mới. Script dựa vào `__EFMigrationsHistory`, không tự sửa schema drift.
- **Chưa apply migration vào local app database, staging hoặc production trong task này.** Chỉ áp dụng DDL vào database SQL Server LocalDB tạm phục vụ test và đã dọn database đó.
- Cần cập nhật FE cùng đợt: POST đăng ký nay bắt buộc chọn `customerCode`; FE cũ gọi upsert thiếu mã chi nhánh nhận lỗi 400. Login/read của hồ sơ đã có vẫn dùng `customer/by-phone`; không dùng upsert cũ để thay cho login.
- Không đổi gateway, `TenantAutoMigrateMiddleware`, Sales auth, HL25 wheel hoặc logic điểm. HLG và Sales **dùng chung HoaLinhMienNam/AppCustomers/BonusPoint** theo xác nhận mới; không được dựa vào giả định tách DB trong tài liệu cũ.

## 6. Transaction và kiểm thử

- DMS check chạy trước khi giữ khóa. ABP Unit of Work transactional `requiresNew` bao trọn recheck local state, Customer/Profile insert/update và commit.
- SQL `sp_getapplock`, scope database + tenant (Host có key riêng), `LockOwner=Transaction`, timeout 15 giây. Đăng ký được tuần tự hóa trong mỗi tenant để bảo vệ cả hạn mức và mã HLGKH; khóa tự nhả khi commit/rollback, dùng được nhiều process/server. Tham chiếu: [Microsoft SQL Server sp_getapplock](https://learn.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-getapplock-transact-sql).
- Luồng GET tạo profile legacy cũng dùng khóa/recheck để không tạo trùng với POST đăng ký. Hồ sơ chưa đăng ký không thể dùng PUT profile để bỏ qua DMS gate.
- Bộ test dùng dữ liệu DMS giả lập có kiểm soát; **chưa gọi DMS thật hoặc đăng ký vào tenant thật**. UAT với dữ liệu DMS hiện tại vẫn là bước sau migration/FE integration.
- Logs build/test: `artifacts/hlg-pharmacy-registration-20261006/`. Chi tiết kết quả cuối cùng xem note memory cùng ngày.

Chạy lại (PowerShell, tại repo root):

```powershell
dotnet build Genora.MultiTenancy.sln --no-restore -c Release
dotnet test test/Genora.MultiTenancy.Application.Tests --no-build --no-restore -c Release --filter FullyQualifiedName~Hlg
dotnet test test/Genora.MultiTenancy.Web.Tests --no-build --no-restore -c Release --filter FullyQualifiedName~Hlg
$env:HLG_REGISTRATION_LOCALDB_TESTS='1'
dotnet test test/Genora.MultiTenancy.EntityFrameworkCore.Tests --no-build --no-restore -c Release --filter FullyQualifiedName~HlgPharmacyRegistration
Remove-Item Env:HLG_REGISTRATION_LOCALDB_TESTS
```

SQL tests cố định `(localdb)\MSSQLLocalDB`, tên database tự sinh `HlgRegistrationTest_<guid>`, không đọc appsettings. Khi không bật biến môi trường, test SQL được skip rõ ràng; hai test migration/model vẫn chạy.
