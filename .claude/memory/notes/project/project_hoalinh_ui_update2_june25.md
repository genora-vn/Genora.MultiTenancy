---
name: project-hoalinh-ui-update2-june25
description: "Update UI Hoa Linh batch 2 — Brands page mới, Products filter brand, Customers filter Kênh/GKHL, Orders dùng OrderHeaders API + date filter, pagination page size tất cả trang"
metadata: 
  node_type: memory
  type: project
  originSessionId: dcf06de4-47b3-4551-89dd-8fabec86f6de
---

## Hoa Linh UI Update Batch 2 (2026-06-25)

### Trang mới: Brands (Danh mục sản phẩm)
- URL: /HoaLinh/Brands
- Menu item: "Danh mục SP" (icon fa-tags, order 2)
- Filter: text search (mã/tên) + dropdown trạng thái
- Detail modal: hiện info brand + danh sách SP theo brand (gọi getProductsByBrand)
- API mini app: GET /api/mini-app/hl/brands + GET /api/mini-app/hl/brands/{code}/products

### DTOs cập nhật:
- HlBrandDto: thêm ImageUrl, NoOfProduct (theo response thực tế)
- HlProductByBrandDto (mới): ProductGroupCode, ProductGroupName, Description, Instruction, ImageAvatarUrl, ImageUrl
- HlOrderHeaderDto: sửa theo response thực (OrderStatusCode, bỏ SchemeValue/NetValue/CreditNoteValue/ProcessDate/DeliveryDate)

### Trang Products updated:
- Thêm dropdown "Tất cả danh mục" (load brands → select options)
- Khi chọn brand → gọi getProductsByBrand API thay vì getProducts
- Tìm kiếm: tên, mã SP, nhóm SP, thương hiệu

### Trang Customers updated:
- Thêm dropdown Kênh (GT/OTC)
- Thêm dropdown GKHL (Có/Không)
- Tìm kiếm: tên, SĐT, mã KH, tên NV Sales
- Load tất cả data (500 records) → client-side filter

### Trang Orders updated:
- Chuyển từ getOrders (OrderDetails) → getOrderHeaders
- Thêm filter ngày (flatpickr from/to)
- Bảng hiện: Mã đơn, KH, NPP, Giá trị, Thành tiền, Trạng thái, Ngày đặt, NV Sales
- Detail modal vẫn gọi getOrderDetail (OrderDetails) để hiện items

### Pagination pattern chung:
- Tất cả trang có select page size (10/20/50/100)
- Client-side pagination: load data → filter → slice(start, start+pageSize)
- renderPagination: prev + 5 page buttons + next

**Why:** Theo yêu cầu cập nhật filters + pagination theo chuẩn AppNews pattern.
**How to apply:** Pattern chung: loadData() fetch từ API → allData = [...] → renderTable() applies filters + pagination client-side.

[[project-hoalinh-ui-update-june25]] [[project-hoalinh-phase3-complete]]
