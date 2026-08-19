---
name: project-hoalinh-phase2-complete
description: "Phase 2 API Client Service hoàn thành — IHlApiClientService + HlApiClientService + DTOs + HttpClient named \"HoaLinhDms\""
metadata: 
  node_type: memory
  type: project
  originSessionId: dcf06de4-47b3-4551-89dd-8fabec86f6de
---

## Hoa Linh Phase 2 Complete — API Client Service (2026-06-23)

### DTOs (Application.Contracts/AppDtos/HoaLinh/):
- HlApiResponses.cs — `HlPagedResponse<T>` (paged) + `HlApiResult<T>` (wrapper trả về Admin/MiniApp)
- HlCustomerDto.cs — mapping tất cả fields từ API /get-customer-by-phone + /Customers
- HlSalemanDto.cs — mapping từ API /Salemans
- HlProductDto.cs — mapping từ API /Products (có brand_code, image_url)
- HlOrderDetailDto.cs — mapping từ API /OrderDetails (mỗi record = 1 SP trong đơn)
- HlCampaignDto.cs — mapping từ API /CustomerCampaigns

### Interface (Application.Contracts/AppDtos/HoaLinh/IHlApiClientService.cs):
- GetCustomerByPhoneAsync(phone) — check tồn tại + lấy loyalty info
- GetCustomerDetailAsync(phone) — chi tiết KH
- GetCustomersAsync(page, limit, search) — danh sách KH
- GetSalemansAsync(page, limit) — danh sách Sales
- GetSalemanDetailAsync(dsrCode) — chi tiết Sale
- GetProductsAsync(page, limit, search) — danh sách SP
- GetProductDetailAsync(productCode) — chi tiết SP
- GetOrdersAsync(page, limit, customerCode) — danh sách orders
- GetOrderDetailAsync(orderNumber) — chi tiết order
- GetCampaignsAsync(page, limit) — danh sách campaigns
- GetCampaignDetailAsync(custCode) — chi tiết campaign

### Implementation (Application/AppServices/HoaLinh/HlApiClientService.cs):
- Inject IHttpClientFactory ("HoaLinhDms"), IRepository<HlApiLog>, ICurrentTenant, ILogger
- Private helper GetAsync<T> — Stopwatch timing, ghi log mọi call, truncate body 4000 chars
- Error handling: Timeout, HttpRequestException, JsonException, generic Exception
- SaveLogAsync trong try/catch rỗng (không fail luồng chính)

### Registration (MultiTenancyApplicationModule.cs):
- AddHttpClient("HoaLinhDms") — BaseUrl + ApiKey từ config + timeout 30s
- AddScoped<IHlApiClientService, HlApiClientService>()

### Config (appsettings.json):
```json
"HoaLinhApi": {
  "BaseUrl": "https://dmsapi.hoalinh.io.vn",
  "ApiKey": "HLDms_ZaloOrder_SecretKey_2026",
  "TimeoutSeconds": 30
}
```

### API Endpoints mapping:
| Method | HL Endpoint | Auth |
|--------|------------|------|
| GET | /api/get-customer-by-phone?phone={phone} | X-API-Key header |
| GET | /api/Customers/{phone} | X-API-Key header |
| GET | /api/Customers?page&limit&search | X-API-Key header |
| GET | /api/Salemans?page&limit | X-API-Key header |
| GET | /api/Salemans/{dsrCode} | X-API-Key header |
| GET | /api/Products?page&limit&search | X-API-Key header |
| GET | /api/Products/{productCode} | X-API-Key header |
| GET | /api/OrderDetails?page&limit&customer_code | X-API-Key header |
| GET | /api/OrderDetails/{orderNumber} | X-API-Key header |
| GET | /api/CustomerCampaigns?page&limit | X-API-Key header |
| GET | /api/CustomerCampaigns/{custCode} | X-API-Key header |

**Why:** Đây là lớp trung gian duy nhất gọi API HL. Admin UI và MiniApp Controller đều gọi qua service này.
**How to apply:** Inject IHlApiClientService, gọi method tương ứng, check result.Success trước khi dùng Data.

[[project-hoalinh-phase1-complete]] [[project-hoalinh-brd-overview]]
