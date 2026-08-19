---
name: project_hl_campaign_detail_and_admin_customer_merge
description: HL MiniApp campaign detail endpoint + admin Customers merge AppCustomers + filter nguồn
metadata: 
  node_type: memory
  type: project
  originSessionId: 7f8cbb33-af3a-4ee6-a77c-2c0fec37463a
---

Cập nhật module Hoa Linh (branch dev, 2026-07).

**1. Campaign detail (MiniApp):** endpoint `GET api/mini-app/hl/campaigns/{custCode}` trong HoaLinhMiniAppController → `_hlApi.GetCampaignDetailAsync(custCode)` (đã có sẵn, gọi `/api/CustomerCampaigns/{custCode}`, trả list). Bổ sung `HlCampaignDto` các trường detail: CampaignPeriod, DisplayType, AccumulatedSales(decimal?), AccumulatedPoints(int?), MembershipTier, VoucherCode, VoucherName (deserialize qua SnakeCaseLower nên KHÔNG cần JsonPropertyName).

**2. Admin Customers merge + filter nguồn (/HoaLinh/Customers):**
- `HlAdminAppService.GetCustomersAsync(page, limit, search, int? source)` — inject thêm `IRepository<Customer, Guid>` (dbo.AppCustomers). Logic: lấy list từ API HL DMS → build dict AppCustomers theo CustomerCode (OrdinalIgnoreCase) → (a) mỗi KH API: nếu map được custCode↔CustomerCode thì `EnrichFromGenora` (ưu tiên data API, chỉ bổ sung field thiếu, source=CustomerSource của Genora, ExistsInGenora=true), không map thì source=HoaLinh; (b) sau đó thêm KH CHỈ có trong Genora (`MapGenoraToDto`); (c) filter theo `source` nếu có. Trả HlPagedResponse gộp (TotalRecords=merged.Count, TotalPages=1). Vẫn giữ data-level filter dsrCode (Sales).
- `HlCustomerDto` thêm: `Source(int?)`, `SourceText(string?)`, `ExistsInGenora(bool)`. Helper `GetSourceText`: ZaloMiniApp→"Genora (Mini App)", HoaLinh→"Hoa Linh (DMS)", Manual→"Nhập tay", Extent→"Import", Other→"Khác".
- UI (Pages/HoaLinh/Customers): index.js thêm filter `#FilterSource` (client-side theo `String(i.source)`), cột "Nguồn" với `sourceBadge` (source===5 hl-src-dms xanh lá, ===1 hl-src-mini xanh dương, khác hl-src-other xám); colspan 10→11. Index.cshtml thêm dropdown FilterSource (5=Hoa Linh DMS, 1=Genora Mini App, 2=Nhập tay, 3=Import, 4=Khác) + th "Nguồn" + badge styles. Filter channel/gkhl/source đều render client-side; getCustomers vẫn load 500 bản ghi.

Liên quan [[project_hl_customer_registration]] (CustomerSource.HoaLinh=5), [[project_customer_source_enum]], [[project_hoalinh_phase3_complete]]. Build 0 errors.
