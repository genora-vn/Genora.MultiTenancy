---
name: caddie-module-ui-fixes-complete
description: "Caddie Module UI fixes complete — Select2 tags, validation, GolfCourseId null pattern, Avatar base64, Mini App APIs consolidated vào MiniAppController"
metadata: 
  node_type: memory
  type: project
  originSessionId: b1919660-fc80-43ea-8191-7be9a8aab9cf
---

Caddie Module UI fixes + Mini App APIs hoàn thành (2026-06-03):

## 1. UI Fixes (Create/Edit Modal)
- **VoiceRegions + Languages** → Select2 tags với hidden inputs (SelectedVoiceRegions[0], SelectedLanguageIds[0])
- **Validation** → CaddieName required, disable Save button khi rỗng, inline error message
- **Avatar** → base64 upload, StringLength 1MB (1048576), 2MB file size limit client-side
- **GolfCourseId** → bỏ hidden input ở CreateModal (để DTO.GolfCourseId = null), AppService xử lý Guid.Empty fallback

## 2. Backend Fixes
- **CaddieAppService.cs:314** → `GolfCourseId = entity.GolfCourseId ?? Guid.Empty` (cast Guid? → Guid)
- **MiniAppCaddieAppService.cs:255** → tương tự cast cho booking.GolfCourseId
- **EditModal.cshtml** → parse byte/Guid đúng type, dùng if/else thay `selected="@bool"` (Razor quirk)

## 3. Mini App APIs (Consolidated vào MiniAppController)
**Base Route:** `/api/mini-app/caddie/*`

**Đã di chuyển** từ CaddieMiniAppController → MiniAppController, xóa CaddieMiniAppController.

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/caddie/available?bookingDate=&startTime=` | Danh sách caddie available theo ngày/giờ |
| GET | `/caddie/{id}` | Chi tiết caddie + 5 recent reviews |
| POST | `/caddie/booking` | Đặt caddie (headers: X-Customer-Id/Name/Phone) |
| GET | `/caddie/booking/history?customerId=` | Lịch sử booking của customer |
| POST | `/caddie/rating` | Đánh giá caddie (header: X-Customer-Id) |
| GET | `/caddie/skills` | Danh sách kỹ năng active (cho form đánh giá) |

**Why:** 
- Gom tất cả Mini App APIs vào 1 controller (MiniAppController) thay vì tách riêng từng module
- Tránh duplicate `/decode-phone` endpoint (đã có ở MiniAppController)
- Pattern nhất quán với Salon Beauty, FnB, Proshop (đều nằm trong MiniAppController)

**How to apply:** 
- Frontend Mini App gọi API với headers X-Customer-* (từ Zalo user context)
- Backend verify customer ownership cho booking/rating
- CURL examples: `C:\Users\DPC\Desktop\Caddie_MiniApp_API_CURL.md`

## 4. AppDocuments Data Seed
- **Tạm tắt** toàn bộ logic seed trong `AppDocumentsDataSeedContributor.SeedAsync()` để tránh ghi đè dữ liệu đã edit qua CMS
- Thêm `return;` đầu method + comment block toàn bộ code
- Bật lại khi cần: xóa `return;` + uncomment
