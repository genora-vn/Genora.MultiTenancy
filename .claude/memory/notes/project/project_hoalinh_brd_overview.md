---
name: project-hoalinh-brd-overview
description: BRD tổng quan dự án Hoa Linh — Zalo Mini App & Dealer Loyalty Platform
metadata: 
  node_type: memory
  type: project
  originSessionId: dcf06de4-47b3-4551-89dd-8fabec86f6de
---

## Dự án: Zalo Mini App & Dealer Loyalty Platform — Dược phẩm Hoa Linh

**Khách hàng:** Công ty Cổ phần Dược phẩm Hoa Linh
**Loại dự án:** Zalo Mini App + Hệ thống quản trị (Admin Portal) + Tích hợp API DMS

### 2 thành phần chính:

**1. Zalo Mini App (cho Đại lý/Nhà thuốc):**
- Trang chủ (banners, thông báo, sản phẩm nổi bật)
- Tra cứu sản phẩm (nhóm SP, tìm kiếm, chi tiết)
- Đặt hàng (chọn chi nhánh, giỏ hàng, xác nhận)
- Lịch sử đơn hàng (trạng thái giao hàng + thanh toán)
- Loyalty Point (tổng điểm, lịch sử cộng/trừ)
- Đổi quà (UrBox voucher integration)
- Tin tức & Cẩm nang
- Thông tin tài khoản

**2. Admin Portal (nội bộ Hoa Linh):**
- Dashboard tổng quan
- Quản lý khách hàng + Customer 360
- Quản lý chi nhánh
- Quản lý sản phẩm
- Quản lý đơn hàng
- Quản lý Loyalty (điểm, chiến dịch)
- Quản lý đổi quà
- CMS Tin tức
- Quản lý người dùng & phân quyền (RBAC: Admin/Sales/Marketing/Kế toán)
- Đồng bộ dữ liệu API

### Ma trận tích hợp dữ liệu (Section 6.3):

| # | Nhóm dữ liệu | Nguồn | Chiều | Quyền Admin |
|---|---|---|---|---|
| 1 | Danh mục sản phẩm | Hoa Linh | API → Genora | Chỉ xem |
| 2 | Sản phẩm | Hoa Linh | API → Genora | Chỉ xem |
| 3 | Khách hàng | Hoa Linh | API → Genora | Chỉ xem |
| 4 | Chi nhánh khách hàng | Hoa Linh | API → Genora | Chỉ xem |
| 5 | Đơn hàng | Mini App (Genora) | Genora → Hoa Linh | Xem và quản lý |
| 6 | Người dùng Mini App | Genora | Genora | Xem và quản lý |
| 7 | Điểm thưởng | Hoa Linh | API → Genora | Chỉ xem |
| 8 | Hạng thành viên Loyalty | Hoa Linh | API → Genora | Chỉ xem |
| 9 | Lịch sử điểm thưởng | Hoa Linh | API → Genora | Chỉ xem |
| 10 | Lịch sử đổi quà | Genora | Genora | Xem và quản lý |
| 11-13 | Tin tức/Banner/Config | Genora | Genora | Quản lý |

### Luồng xác thực Mini App:
1. Lấy SĐT Zalo → Tra cứu DMS Hoa Linh
2. Không tồn tại → Từ chối + hiển thị hotline
3. Tồn tại → Đăng nhập thành công
4. Phân luồng GKHL: is_gkhl + cust_channel (GT="Gắn kết Hoa Linh" cố định / OTC=hạng theo API)

### Roles phân quyền:
- Admin: toàn quyền
- Sales: chỉ xem KH được phân công + đơn hàng + điểm thưởng của KH đó
- Marketing: CMS + Loyalty campaigns
- Kế toán: thanh toán + công nợ + đối soát điểm

### Trạng thái đơn hàng:
- Giao hàng: Chờ xác nhận → Đang xử lý → Đang giao → Hoàn thành / Hủy
- Thanh toán: Chưa thanh toán / Đã thanh toán / Công nợ

### Ngoài phạm vi: ERP/SAP/Oracle/DMS/WMS, cổng thanh toán, quản lý kho, CRM nâng cao

**Why:** Đây là BRD gốc — mọi thiết kế entities, API sync, UI phải align theo tài liệu này.
**How to apply:** Khi implement từng module, check lại section tương ứng trong BRD. Dữ liệu sync từ Hoa Linh = read-only trên Admin. Dữ liệu Genora tạo ra (Orders, đổi quà, users) = full CRUD.

[[project-hoalinh-data-integration-pattern]]
