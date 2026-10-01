# Hoa Linh Sales — Lịch sử xác nhận nhận quà (voucherType = 2)

Ngày bàn giao: 01/10/2026. Module **HL Sales**, tách biệt vòng quay HL25 và Gamification HLG.

**Cập nhật Host/local 01/10/2026:** API hỗ trợ Host với `TenantId = null` và Tenant với TenantId hiện tại. Đã bỏ lỗi `HlGiftReceipt:TenantRequired`. Migration bổ sung `20261001025429_AddHlGiftReceiptHostUniqueness` tạo unique index riêng cho các phiếu Host; giữ nguyên index Tenant.

## Phạm vi và quy tắc

- Ghi nhận yêu cầu nhận quà vào `HL.AppHlGiftReceipts`, trạng thái `Confirmed = 1` / **Đã xác nhận**. Đây là xác nhận yêu cầu, chưa phải chứng từ đã giao quà thực tế.
- Chỉ áp dụng entitlement do DMS trả về với `voucherType = 2`. Luồng đổi điểm/tiền `voucherType = 1` giữ nguyên.
- Một phiếu cho mỗi `(TenantId, CustCode, CampaignCode, CampaignPeriod, VoucherCode)`. Cùng chiến dịch có `QT34` và `QTHOA` được xác nhận riêng. Đổi điện thoại không tạo thêm entitlement cho cùng chi nhánh.
- Nhấn đúp/retry trả lại phiếu cũ. Kiểm tra lại trong transaction với SQL Server `UPDLOCK, HOLDLOCK`, kết hợp unique index; chỉ trả thành công sau commit. Giao dịch này chỉ ghi bảng mới.
- Backend kiểm tra số điện thoại có chi nhánh tương ứng qua DMS, rồi kiểm tra đúng quà/kỳ chiến dịch của chi nhánh. Không nhận tên, địa chỉ, số lượng hay trạng thái do FE tự khai.
- `voucherValue` của loại 2 được hiểu là **số lượng quà**: phải là số nguyên dương. FE không gửi số lượng. `campaignPeriod = null` từ DMS được biểu diễn bằng `0`.
- `startDate/endDate` là ngày chiến dịch được lưu để báo cáo. Không tự coi ngày kết thúc tích lũy là hạn nhận quà: giống luồng Sales hiện có, DMS còn trả entitlement hợp lệ thì được xác nhận. Muốn đặt hạn nhận quà độc lập cần bổ sung quy tắc nghiệp vụ riêng.
- Phiếu đã ghi là snapshot, không có API sửa/xóa. API này không gửi lệnh giao quà ngược về DMS, không gọi UrBox, không trừ điểm hay kho HL25.

## API FE

Gọi đúng domain tenant Sales. Không cần thêm `__tenant` khi domain đã resolve đúng tenant. Ví dụ bên dưới dùng `hoalinh.genora.vn`; đổi sang domain staging khi UAT. **SĐT `0900000001` chỉ là placeholder**, phải thay bằng số được luồng decode-phone xác nhận và thực sự thuộc `C79N1006893` trên DMS. Các lệnh dưới là cURL kiểu Bash/Postman; Windows có thể dùng `curl.exe` một dòng hoặc import vào Postman.

**Test local Host:** thay base URL bằng `https://localhost:44374` (launch profile của Web). Không gửi `__tenant` hoặc TenantId trong body; API ghi/đọc đúng database Default, `TenantId = null`. DMS cần được cấu hình trong ngữ cảnh Host như các API Sales hiện có. API Mini App giữ `[AllowAnonymous]`; trang/API quản trị vẫn phải đăng nhập và được cấp quyền Host. Host không bị kiểm tra feature dành cho Tenant. Không tự đoán tenant từ custCode hoặc dùng tenant đầu tiên.

### 1. Xác nhận yêu cầu nhận quà

```bash
curl --request POST 'https://hoalinh.genora.vn/api/mini-app/hl/gift-receipts' \
  --header 'Content-Type: application/json' \
  --data-raw '{
    "phoneNumber": "0900000001",
    "custCode": "C79N1006893",
    "campaignCode": "GIFT25NAM",
    "campaignPeriod": 1,
    "voucherCode": "QT34",
    "note": "Khách xác nhận nhận quà từ Zalo Mini App"
  }'
```

HTTP 200, dữ liệu minh họa (DTO thực tế có thêm các trường snapshot bên dưới):

```json
{
  "success": true,
  "data": {
    "id": "8d1d3baa-b6da-4f5f-9f84-0e0884a12273",
    "receiptCode": "GR-8D1D3BAAB6DA4F5F9F840E0884A12273",
    "custCode": "C79N1006893",
    "custName": "Tên khách hàng từ DMS",
    "phoneNumber": "0900000001",
    "address": "Địa chỉ chi nhánh từ DMS",
    "campaignCode": "GIFT25NAM",
    "campaignPeriod": 1,
    "voucherCode": "QT34",
    "voucherName": "Hộp quà tặng Hoa Linh 25 năm",
    "voucherType": 2,
    "voucherValue": 1,
    "quantity": 1,
    "status": 1,
    "isConfirmed": true,
    "confirmedAt": "2026-10-01T09:15:00"
  },
  "error": null,
  "message": "Đã xác nhận yêu cầu nhận quà."
}
```

Gọi lần hai cùng khóa trả **cùng id, receiptCode, confirmedAt**, không tạo phiếu mới. Muốn nhận quà khác trong ví dụ, đổi `voucherCode` thành `QTHOA`.

### 2. Lịch sử theo số điện thoại và mã chi nhánh

```bash
curl --get 'https://hoalinh.genora.vn/api/mini-app/hl/gift-receipts' \
  --data-urlencode 'phoneNumber=0900000001' \
  --data-urlencode 'custCode=C79N1006893' \
  --data-urlencode 'campaignCode=GIFT25NAM' \
  --data-urlencode 'campaignPeriod=1' \
  --data-urlencode 'skipCount=0' \
  --data-urlencode 'maxResultCount=100'
```

`phoneNumber`, `custCode` bắt buộc; bộ lọc chiến dịch/kỳ không bắt buộc. `data` có dạng `{ "totalCount": 2, "items": [ /* HlGiftReceiptDto */ ] }`. Không có phiếu: `totalCount=0`, `items=[]`. Phân trang tối đa 100 bản ghi/lần; FE đọc tiếp nếu `totalCount` lớn hơn số đã tải.

Tra cứu giới hạn theo tenant hiện tại **và số điện thoại đã xác nhận phiếu và mã chi nhánh**. Nếu một người khác dùng điện thoại khác của cùng chi nhánh, lịch sử điện thoại đó có thể rỗng; POST vẫn trả phiếu đã có và không tạo trùng quyền nhận của chi nhánh.

Trong ngữ cảnh Host, lịch sử chỉ gồm `TenantId IS NULL`; không gom dữ liệu Tenant vào kết quả Host. Cùng mã KH/chiến dịch/quà trong Host và trong Tenant là hai ngữ cảnh dữ liệu độc lập.

### 3. Quy tắc ghép nút Nhận quà

1. Load `/api/mini-app/hl/campaigns/{custCode}` như hiện tại. Loại 1 giữ luồng đổi điểm cũ.
2. Với loại 2, load lịch sử ở trên. Match `campaignCode + (campaignPeriod ?? 0) + voucherCode` trong chi nhánh đang chọn.
3. Nếu record `isConfirmed === true`, disabled nút và hiển thị **Đã xác nhận**. Cách này vẫn đúng sau F5/mở lại Mini App.
4. Khi bấm nhận, tạm khóa nút trong lúc POST. Chỉ đánh dấu đã xác nhận khi HTTP thành công và `success=true`, `data.isConfirmed=true`. Lưu phiếu vào state/cache của đúng chi nhánh. Nếu lỗi, hiển thị thông báo rồi mở lại nút.
5. Không cần đổi contract của API campaigns. Trạng thái được lấy từ API lịch sử/POST mới.

Lỗi nghiệp vụ HTTP 400: `{ "success": false, "data": null, "error": "HlGiftReceipt:...", "message": "..." }`.

| Mã lỗi | Xử lý |
|---|---|
| `Disabled` | Tenant chưa bật tính năng |
| `InvalidData`, `InvalidPhone` | Kiểm tra dữ liệu đầu vào |
| `BranchNotFound` | Điện thoại không thuộc chi nhánh |
| `GiftNotFound` | DMS thiếu hoặc trả trùng entitlement; tải lại chiến dịch |
| `InvalidVoucherType` | Chỉ gửi quà loại 2 vào API mới |
| `InvalidQuantity`, `InvalidCampaignDate` | Dữ liệu nguồn DMS không hợp lệ |
| `DmsUnavailable` | Không ghi phiếu; cho phép thử lại |

Prefix đầy đủ là `HlGiftReceipt:`. Lỗi model binding/validation của ABP có thể dùng envelope chuẩn `{error:{message,...}}`; FE cần xử lý cả dạng này. DateTime dùng serializer/Clock hiện có của dự án, không tự nối `Z` vào chuỗi thời gian không có offset.

## Dữ liệu lưu và xuất Excel

Snapshot được giữ nguyên kể cả khi DMS đổi tên khách hàng, địa chỉ hoặc tên quà sau này.

| Nhóm | Trường |
|---|---|
| Định danh và cách ly tenant | Id, TenantId, ReceiptCode, CustCode, PhoneNumber |
| Khách hàng/chi nhánh | CustName, Address |
| Chiến dịch | CampaignCode, CampaignName, CampaignPeriod, CampaignStartDate, CampaignEndDate |
| Quà | VoucherCode, VoucherName, VoucherType, VoucherValue, Quantity |
| Xác nhận | Status, ConfirmedAt; DTO có IsConfirmed |
| Báo cáo kinh doanh | MembershipTier, AccumulatedSales, AccumulatedPoints |
| Phụ trách và phân phối | DsrCode, DsrName, DistributorCode, DistributorName |
| Kiểm toán | Source, Note, CreationTime, CreatorId, ConcurrencyStamp, ExtraProperties |

Các trường snapshot mở rộng có thể null nếu DMS không trả. Không lưu token Zalo hay credentials. Excel có **28 cột** phục vụ nghiệp vụ (không xuất thông tin kỹ thuật như TenantId/ConcurrencyStamp). Mã/SĐT giữ dạng text, không mất số 0 đầu và không biến chuỗi dữ liệu thành công thức Excel.

## Quản trị và triển khai

- Trang: **Hoa Linh Sales → Lịch sử nhận quà**, URL `/HoaLinh/GiftReceipts`.
- Danh sách phân trang, modal chi tiết, lọc từ khóa, mã KH, SĐT, mã chiến dịch, mã quà, kỳ, trạng thái, ngày xác nhận từ/đến. Ngày cuối được tính trọn ngày.
- Nút Excel xuất **tất cả dòng khớp bộ lọc**, không chỉ trang hiện tại. Endpoint: `GET /api/app/hl-sales-excel/gift-receipts`, sử dụng cùng filter; cần đăng nhập/quyền export. Dùng same-origin cookies như các trang Sales hiện có.
- Tenant cần bật cả `HoaLinh.Management` và **`HoaLinh.GiftReceipts`** (mới, mặc định false).
- Tenant permissions: `MultiTenancy.AppHlGiftReceipts` (xem) và `.Export` (xuất Excel). Host permissions tương ứng: `MultiTenancy.HostAppHlGiftReceipts` và `.Export`.
- Host dùng trang này để xem/xuất dữ liệu Host (`TenantId = null`) khi có quyền `MultiTenancy.HostAppHlGiftReceipts` / `.Export`, không cần bật tenant feature. Tenant dùng quyền `MultiTenancy.AppHlGiftReceipts` / `.Export` cùng các feature đã bật. Host không đọc xuyên các tenant; muốn xem Tenant phải chuyển ngữ cảnh theo cơ chế hiện có.

Migration: `20260930171009_AddHlGiftReceipts`. SQL tại `src/Genora.MultiTenancy.EntityFrameworkCore/Migrations/Scripts/AddHlGiftReceipts.sql`.

Migration tiếp theo: **`20261001025429_AddHlGiftReceiptHostUniqueness`**, SQL cùng thư mục `AddHlGiftReceiptHostUniqueness.sql`. Chỉ thêm unique index `(CustCode, CampaignCode, CampaignPeriod, VoucherCode) WHERE TenantId IS NULL`, không sửa/xóa dữ liệu và không thay index Tenant. Database đã apply migration đầu chỉ cần apply migration tiếp theo; database chưa có tính năng cần cả hai theo thứ tự. Không sửa migration cũ để tránh bỏ sót database đã cập nhật.

Ví dụ chạy EF trong thư mục `src/Genora.MultiTenancy.EntityFrameworkCore` sau khi kiểm tra connection Default của DbMigrator đúng database muốn cập nhật:

```powershell
dotnet ef database update 20261001025429_AddHlGiftReceiptHostUniqueness
```

1. Kiểm tra backup/schema/history của DB đích. SQL này là incremental từ `20260929091139_AddHlBlouseModule`, yêu cầu baseline đã có schema `HL` và bảng migration history. Không dùng script incremental để dựng database rỗng.
2. Apply migration bằng quy trình DbMigrator được kiểm soát, hoặc chạy SQL đã review trong đúng database host/tenant dự kiến dùng trang. Chú ý: lệnh EF không truyền connection dùng cấu hình **DbMigrator**, thường là host; lệnh DbMigrator mặc định có thể duyệt tất cả tenant.
3. Deploy Web/backend mới, bật feature **chỉ tenant cần dùng**, gán quyền xem/export cho role. Không cần đổi gateway, TenantAutoMigrateMiddleware hoặc quyền Sales cũ.
4. UAT bằng entitlement test do DMS cung cấp. Chưa apply migration hoặc bật feature vào database đang vận hành trong task này.
5. Nếu cần ngừng chức năng mới, tắt feature. Không dùng Down migration xóa bảng đã có lịch sử.

## Checklist nghiệm thu

- Cùng chi nhánh nhận QT34 rồi QTHOA: hai phiếu; lặp QT34 nhiều lần/nhấn đồng thời vẫn một phiếu cho QT34.
- Thử sai điện thoại/chi nhánh, loại 1, DMS lỗi: không tạo phiếu.
- Reload Mini App: lịch sử trả `isConfirmed=true`, nút disabled đúng quà.
- Admin lọc SĐT+chi nhánh+ngày, xem chi tiết; Excel có đúng các dòng và snapshot, bao gồm cuối ngày.
- Role chỉ được xem không có nút export; gọi thẳng export cũng bị từ chối.
- Tenant chưa bật feature không truy cập được tính năng mới; API Sales cũ tiếp tục giữ nguyên luồng.
- Local Host không gửi tenant: POST thành công, retry cùng ID, GET trả đúng phiếu Host; trang quản trị Host có quyền đọc/export được, không có quyền bị từ chối. Chuyển Tenant không thấy các phiếu Host.

Kiểm thử tự động dùng mock DMS/repository, kiểm tra EF SQL Server model/migration và JavaScript. Có kiểm thử HTTP local bằng ASP.NET TestServer chạy controller + business service thật cho Host/Tenant; chỉ DMS/repository được thay bằng dữ liệu test, không ghi phiếu lên môi trường đang vận hành. Chưa có UAT trình duyệt/DMS/SQL Server thật hoặc stress test đồng thời. API tiếp tục theo cơ chế anonymous + số điện thoại của Sales hiện có: DMS xác minh quan hệ chi nhánh, **không phải xác thực danh tính người gọi bằng token**. Đây là giới hạn kế thừa cần lưu ý khi nghiệm thu; không thay kiến trúc đăng nhập trong phạm vi tính năng này.
