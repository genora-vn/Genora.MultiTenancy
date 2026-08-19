---
name: project-hoalinh-ui-update3-june26
description: "Fix pagination (Previous/Next ở dưới), thêm trang Salemans + Customer Detail 360, search chỉ khi nhấn button"
metadata: 
  node_type: memory
  type: project
  originSessionId: dcf06de4-47b3-4551-89dd-8fabec86f6de
---

## Hoa Linh MiniApp Orders API rework (2026-07-01)
- Param: `customerCode` (bắt buộc) + `zaloOrderNumber` (không bắt buộc) + page/limit.
- `GetOrderHeaderZaloAsync(customerCode, zaloOrderNumber = null)` — zaloOrderNumber giờ optional; chỉ append `&zalo_order_number=` vào URL khi có giá trị (sửa cả IHlApiClientService + HlApiClientService).
- Truyền cả 2 → lọc theo mã KH + mã order Genora (query thêm `x.OrderCode == zaloOrderNumber` bên Genora).
- Sync status: DMS có `zalo_order_number == OrderCode` Genora → update DeliveryStatus + ExternalOrderCode + IsSyncedToHl + SyncedAt. Các mã đã match được gom vào `matchedGenoraCodes` (HashSet OrdinalIgnoreCase) để LOẠI khỏi list hoalinh, tránh nhân đôi.
- Trả về gộp: đơn Genora (`Source="genora"`, Id là Guid) + đơn thuần DMS chưa map (`Source="hoalinh"`, Id=null, OrderCode=OrderNumber, BranchName=DistributorName, DeliveryStatus=OrderStatusCode, PaymentStatus=0). Dùng `.Cast<object>().Concat()` vì 2 anonymous type khác shape.

### GET /api/mini-app/hl/orders/{orderNumber} — thêm param source
- Param: `source` (genora|hoalinh, mặc định hoalinh) + `customerCode` (optional).
- `source=genora` → đọc HL.AppHlOrders bằng `WithDetailsAsync(x => x.Items)` (BẮT BUỘC truyền explicit navigation, không dùng `WithDetailsAsync()` rỗng vì Items sẽ không load → trả `data: []`); trải phẳng mỗi item thành 1 record `HlOrderDetailDto` (giống DMS): DistributorCode=BranchCode, DsrCode=ReceiverCode, DsrName=ReceiverName, ProductPrice=item.Price, TotalAmount/NetValue/GrossValue=item.Amount, OrderNumber=ExternalOrderCode, ZaloOrderNumber=OrderCode, OrderDate/OrderTime tách từ CreationTime. Trả `List<HlOrderDetailDto>`.
- `source=hoalinh` (mặc định) → gọi `_hlApi.GetOrderDetailAsync(orderNumber)` như cũ.
- HlOrder thêm cột `ReceiverCode` (mã trình dược viên/dsrCode); migration 20260701084729_Add_HlOrder_ReceiverCode. CreateOrder nhận body param `receiveCode` → `HlCreateOrderRequest.ReceiveCode` → order.ReceiverCode.

## Hoa Linh UI Update Batch 3 (2026-06-26)

### Fix Pagination tất cả trang:
- Page size select di chuyển xuống dưới cùng bên phải (cùng hàng với pagination)
- Previous / Next buttons thay cho << / >>
- Hiển thị "Hiển thị **X** / **Y** {entity}" thay vì "Trang 1/5"
- Search chỉ khi nhấn button Search hoặc Enter (không auto-search khi thay đổi dropdown)
- Áp dụng cho: Brands, Products, Customers, Orders, ApiLogs, Salemans

### Trang mới: Nhân viên Sales (/HoaLinh/Salemans)
- URL: /HoaLinh/Salemans
- Menu: "Nhân viên Sales" (icon fa-user-tie, order 5)
- Filter: text (tên, mã, SĐT, email) + dropdown Khu vực (auto-populate) + dropdown Giới tính
- Detail modal: hiện đầy đủ info nhân viên
- Data: gọi getSalemans(1, 500) → client-side filter + pagination

### Trang mới: Customer Detail 360 (/HoaLinh/Customers/Detail?phone=...)
- URL: /HoaLinh/Customers/Detail?phone={phone}
- Profile section: tên, mã, SĐT, hạng TV badge, stat cards (điểm, doanh số, lên hạng, GKHL)
- Info section: kênh, phân nhóm, nhóm, địa chỉ, NV Sales, NPP, ngày sinh
- Tab "Lịch sử đơn hàng": filter orderHeaders by customerCode
- Tab "Danh sách chi nhánh": getCustomerDetail(phone) → multiple results = branches
- Link "Xem chi tiết" ở cột cuối bảng Customers (thay popup modal)

### Customers Index updated:
- Bỏ click row mở popup → thêm cột action với nút link "Xem chi tiết" → navigate tới Detail page
- Filter channel/GKHL giữ nguyên nhưng chỉ apply khi nhấn Search

### Menu updated:
```
Hoa Linh
├── Tổng quan (order 1)
├── Danh mục SP (order 2)
├── Sản phẩm (order 3)
├── Khách hàng (order 4)
├── Nhân viên Sales (order 5)  ← MỚI
├── Đơn hàng (order 6)
├── Loyalty (order 7)
├── Đổi quà (order 8)
└── Nhật ký API (order 9)
```

**Why:** UX cải thiện — pagination chuẩn AppNews, Customer 360 giống SalonBeauty pattern, Salemans cho quản lý NV kinh doanh.
**How to apply:** Pattern Detail page: cshtml.cs bind Phone query param → JS đọc window.customerPhone → gọi API load data.

[[project-hoalinh-ui-update2-june25]] [[project-hoalinh-phase3-complete]]
