---
name: project_zalo_oa_articles
description: "Zalo OA article (news) integration — list + detail via ZaloApiClient, exposed on HoaLinh Mini App"
metadata: 
  node_type: memory
  type: project
  originSessionId: 7f8cbb33-af3a-4ee6-a77c-2c0fec37463a
---

Tích hợp lấy tin tức (bài viết Zalo OA) cho Mini App Hoa Linh (branch dev, 2026-07).

**Service (shared, multi-tenant):** thêm 2 method vào `IZaloApiClient`/`ZaloApiClient` (AppServices/AppZaloAuths/) — service Zalo dùng chung cho mọi tenant:
- `GetArticleListAsync(offset, limit, type, ct)` → GET `/v2.0/article/getslice` (openapi.zalo.me), header `access_token`. type mặc định "normal".
- `GetArticleDetailAsync(articleId, ct)` → GET `/v2.0/article/getdetail?id={id}`.
- Base URL từ config `Zalo:OpenApiBaseUrl` (đã có sẵn trong appsettings).

**Access token:** helper private `ResolveAccessTokenAsync()` — lấy từ `_tokenProvider.GetAccessTokenAsync()` (ZaloAuth active theo tenant, có sẵn); **fallback sang config `Zalo:TestAccessToken`** nếu DB rỗng/lỗi (vì Host chưa cấp quyền OA, chưa test được token thật per-tenant). `SendWithAccessTokenGetAsync` retry 1 lần khi `IsLikelyInvalidToken`. Token test đã đặt trong appsettings.json `Zalo:TestAccessToken` (tạm, xóa/để trống khi lên prod thật).

**DTOs:** `ZaloArticleDtos.cs` (Contracts/AppDtos/AppZaloAuths/) — `ZaloArticleListResponse`/`ZaloArticleDetailResponse` kế thừa `ZaloBaseResponse` (error/message), dùng `[JsonPropertyName]` snake_case (total_view, create_date, link_view, cover.photo_url, body[].content...). Deserialize qua `_zaloJsonOptions` (đã có, AllowReadingFromString).

**Controller:** thêm region News vào `HoaLinhMiniAppController` (route `api/mini-app/hl`):
- GET `news?offset=0&limit=10&type=normal`
- GET `news/{articleId}`
Controller đã sẵn inject `IZaloApiClient _zaloApiClient`.

**Update 2026-07-21 — copy sang MiniAppController (golf/generic, route `api/mini-app`):** thêm region News 2 endpoint `[AllowAnonymous]`:
- GET `api/mini-app/news?offset=0&limit=10&type=normal`
- GET `api/mini-app/news/{articleId}`
Điểm khác HoaLinh: thiếu articleId trả `BadRequest("Thiếu mã bài viết")` (string thuần), KHÔNG dùng `HlApiResult` (DTO riêng Hoa Linh). Access token resolve theo tenant → mỗi tenant lấy bài viết Zalo OA của mình.

**Update 2026-07-21 — refactor sang AppService + cache (3 đề xuất tối ưu):**
- **AppService:** `IMiniAppZaloNewsService`/`MiniAppZaloNewsService` (Contracts + Application/AppServices/AppZaloAuths/) gom logic gọi `IZaloApiClient.GetArticleList/DetailAsync`. Đúng coding rule (controller không gọi thẳng client). Interface TÁCH RIÊNG khỏi `IMiniAppNewsService` (tin nội bộ bảng AppNews) — 2 nguồn tin KHÁC nhau (Zalo OA vs AppNews), đặt tên `ZaloNews` để không nhầm. DI `AddScoped` trong MultiTenancyApplicationModule cạnh AddTransient IZaloApiClient.
- **Cache per-tenant:** dùng `IDistributedCache<ZaloArticleListResponse>` + `<ZaloArticleDetailResponse>` (Volo.Abp.Caching, pattern giống ProvinceLookupAppService). Key `zalo-news:list:{tenantId|host}:{type}:{offset}:{limit}` và `zalo-news:detail:{tenantId|host}:{id}` — CurrentTenant.Id đảm bảo cách ly tenant. CHỈ cache khi `result.Error==0` (không cache lỗi/token hết hạn). TTL từ config `Zalo:NewsCacheMinutes` (đã thêm vào appsettings.json = 5), default 5 phút nếu thiếu/không parse được. Cache get/set bọc try/catch (SafeGet/SafeSet) — Redis chết KHÔNG làm sập luồng đọc tin, fallback gọi thẳng API. Logger.LogException (ABP không có LogWarning ext — xem [[feedback_abp_ilogger]]).
- **Envelope:** KHÔNG tạo DTO mới — response `ZaloArticleListResponse`/`ZaloArticleDetailResponse` đã kế thừa `ZaloBaseResponse` (error/message + data), tức đã là envelope chuẩn `{error,message,data}`. Giữ nguyên shape JSON để KHÔNG phá app Mini App đang chạy; "chuẩn hóa" = cả 2 controller đi qua cùng 1 service trả cùng kiểu. Service trả envelope `Error=-1` + message tiếng Việt khi client null/thiếu id.
- **Cả 2 controller** (`MiniAppController` + `HoaLinhMiniAppController`) giờ inject `IMiniAppZaloNewsService _zaloNews` và gọi `_zaloNews.GetArticleList/DetailAsync` thay vì `_zaloApiClient`. Build HttpApi + Application 0 errors.

**Chi tiết kỹ thuật:** `_logger` trong BaseZaloClient là private → ZaloApiClient tự giữ field `_apiLogger` (gán từ constructor logger param). Thêm 2 log action `GET_ARTICLE_LIST`/`GET_ARTICLE_DETAIL` vào `ZaloLogActions` (Domain.Shared/Enums/ZaloLogAction.cs).

Build 0 errors. Liên quan: [[project_hoalinh_phase5_complete]], [[project_urbox_integration]].
