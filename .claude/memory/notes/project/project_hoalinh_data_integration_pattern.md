---
name: project-hoalinh-data-integration-pattern
description: "Pattern tích hợp dữ liệu Hoa Linh — sync API từ DMS, chiều dữ liệu, read-only vs CRUD"
metadata: 
  node_type: memory
  type: project
  originSessionId: dcf06de4-47b3-4551-89dd-8fabec86f6de
---

## Data Integration Pattern — Hoa Linh DMS ↔ Genora

### Nguyên tắc chính:
1. **Dữ liệu master từ Hoa Linh (DMS)** → Sync về Genora qua API → **Read-only** trên Admin Portal
2. **Dữ liệu Genora tạo ra** (Orders, Đổi quà, Users, Tin tức, Banner) → **Full CRUD** trên Admin Portal
3. Dữ liệu phát sinh từ Mini App lưu tại Genora, đồng bộ sang Hoa Linh khi cần

### 10 nhóm dữ liệu tích hợp:

| Entity | Nguồn | Chiều sync | Admin quyền | Ghi chú |
|--------|--------|-----------|-------------|---------|
| AppHlProductCategories | Hoa Linh API | Pull | Read-only | Danh mục SP |
| AppHlProducts | Hoa Linh API | Pull | Read-only | SP + giá + hình ảnh |
| AppHlCustomers | Hoa Linh API | Pull | Read-only | Đại lý/Nhà thuốc |
| AppHlCustomerBranches | Hoa Linh API | Pull | Read-only | Chi nhánh nhận hàng |
| AppHlOrders | Genora (Mini App) | Push to HL | Full CRUD | Đặt hàng từ Mini App |
| AppHlMiniAppUsers | Genora | Local | Full CRUD | User đăng nhập qua Zalo |
| AppHlLoyaltyPoints | Hoa Linh API | Pull | Read-only | Tổng điểm/ví điểm |
| AppHlLoyaltyTiers | Hoa Linh API | Pull | Read-only | Hạng thành viên |
| AppHlPointHistories | Hoa Linh API | Pull | Read-only | Lịch sử cộng/trừ điểm |
| AppHlGiftExchanges | Genora | Local | Full CRUD | Yêu cầu đổi quà |

### Sync strategy:
- **Pull (Hoa Linh → Genora):** Scheduled job hoặc manual trigger, lưu lại SyncLog
- **Push (Genora → Hoa Linh):** Real-time khi tạo đơn hàng, hoặc batch
- **SyncLog entity:** Thời gian, loại dữ liệu, kết quả, lỗi (nếu có)

### Prefix naming: `AppHl` (Hoa Linh) để phân biệt với entities khác trong hệ thống multi-tenant

### Xác thực Mini App:
- Zalo SĐT → Gọi API DMS check is_customer
- Nếu tồn tại → tạo/update AppHlMiniAppUsers, gắn CustomerCode
- Phân luồng: is_gkhl + cust_channel (GT/OTC) → hiển thị Loyalty tier

**Why:** Đây là architecture pattern cốt lõi — phân biệt rõ data ownership để tránh conflict khi sync.
**How to apply:** Entities từ Hoa Linh luôn có ExternalId/ExternalCode mapping. Admin UI cho những entities này chỉ hiển thị, không cho edit. Sync job ghi SyncLog mỗi lần chạy.

[[project-hoalinh-brd-overview]]
