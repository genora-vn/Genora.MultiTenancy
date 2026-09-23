# Gateway production — Apache (XAMPP) là ingress thật, KHÔNG phải IIS (2026-09-23)

> Cấu hình rate-limit cho mini-app các tenant (Dược phẩm Hoa Linh, Hoa Linh Miền Nam) trên **production**.
> Kết quả: **THÀNH CÔNG** — burst test trả hỗn hợp 200 + 429 `Hl25:RateLimitExceeded`.

## TL;DR — nguyên nhân gốc (đắt giá nhất)

Trên server production, **cổng `103.157.218.191:443` do XAMPP Apache (`C:\xampp\apache\bin\httpd.exe`) chiếm**, KHÔNG phải IIS. Vhost wildcard `*.genora.vn` trong `httpd-vhost.conf` proxy thẳng mọi request vào ABP `http://103.157.218.174:8868/`, **bỏ qua cả IIS ingress lẫn YARP gateway**. Vì thế:
- Sửa `web.config` của site IIS `Genora.Ingress.Production` KHÔNG có tác dụng (request không tới IIS).
- Đặt `PermitLimit:1` không chặn được (gateway không nằm trong luồng).
- Guard ABP trả 403 (vì gọi thẳng ABP không có gateway key).

**Bài học:** đừng giả định IIS URL Rewrite nằm trong luồng chỉ vì IIS site có binding `.191:443`. Luôn kiểm `Get-NetTCPConnection -LocalPort 443 -State Listen` để xem process nào thật sự giữ cổng. (IIS = System/PID 4/http.sys; Apache = httpd.exe.)

## Kiến trúc 3 tầng đúng trên production

```
Client HTTPS → Apache :443 (.191, XAMPP)
   /api/mini-app/hl25|hlg (public, non-admin) → YARP gateway 127.0.0.1:5088 (rate-limit + inject key) → ABP 8868 (guard)
   /api/mini-app/.../admin                     → ABP 103.157.218.174:8868 (bypass gateway, giữ auth)
   còn lại (static, account, /api/mini-app/hl Sales, signalr) → ABP 8868
```

- **YARP gateway** = site IIS `Genora.Tenant.Gateway`, binding `*:5088`, env=**Production**, app pool Integrated, **MaxProcesses=1** (quota là process-local).
- **ABP** = site IIS `Genora.MultiTenancy`, binding `*:8868`, env=**Production**.
- IIS site `Genora.Ingress.Production` (physical path `C:\Genora\Deploy\ingress-production`, chỉ có web.config) **vô tác dụng** vì Apache cầm 443. Vẫn giữ lại nhưng không nằm trong luồng.

## Mapping tenant (production)

| Host | Profile | Tenant | TenantId | Quota/s |
|---|---|---|---|---|
| duocpham-hoalinh.genora.vn | Hl25 | Dược phẩm Hoa Linh | 209567fc-4850-44e9-11c8-3a23c58d15a4 | 500 |
| hoalinh.genora.vn | Hlg | Hoa Linh Miền Nam | 27e348a9-036c-bef8-fa2b-3a22fc202c26 | 300 |

- `hoalinh.genora.vn` còn phục vụ Hoa Linh Sales `/api/mini-app/hl` → đi thẳng ABP, KHÔNG rate-limit (đã chốt với anh). Guard KHÔNG được liệt kê `/api/mini-app/hl` trong PathPrefixes, nếu không sẽ 403 làm hỏng Sales.
- SharedKey mỗi tenant (>=32 ASCII, khác nhau) — GIÁ TRỊ nằm trong `artifacts/gateway-production-20260923/` (gitignored), KHÔNG commit vào memory.

## Bộ file production (docs/tenant-gateway/ — đã commit, secret đã sanitize thành placeholder)

> Trước ở `artifacts/gateway-production-20260923/` (gitignored, có key thật) nhưng đã chuyển sang `docs/tenant-gateway/` làm tài liệu chuẩn (2026-09-23): SharedKey/DB password thay bằng `<REPLACE_...>`; key thật điền trên server. Cấu trúc docs: `README.md`, `apache/genora-miniapp-vhosts.conf`, `gateway/appsettings.Production.json`, `gateway/web.config`, `abp/appsettings.Production.guard.json`, `iis-ingress-alternative/web.config`.

- `apache/genora-miniapp-vhosts.conf` — **file quyết định**: 2 vhost Apache route public hl25/hlg qua gateway 5088; admin + phần còn lại → ABP; reject cross-host bằng `RewriteRule "^/api/mini-app/hlg(/|$)" - [R=404,L]` (và ngược lại). Chèn TRƯỚC vhost `tenant-proxy.genora.vn` (`*.genora.vn`); ServerName cụ thể thắng wildcard.
- `gateway/appsettings.Production.json` — TenantGateway config (hl25=500, hlg=300, key, BackendAddress http://127.0.0.1:8868/, ApiProfiles Hl25/Hlg, AdditionalRoutes []).
- `gateway/web.config` — env=Production, httpErrors PassThrough (để 429 giữ nguyên).
- `abp/appsettings.Production.json` — full config production + `TenantGatewayGuard.Enabled=true`; hl25 PathPrefixes `/api/mini-app/hl25` (exclude `/admin`), hlg PathPrefixes `/api/mini-app/hlg` (exclude `/admin`). Bỏ field `GatewayKey` (không có trong schema). PHẢI đặt tên `appsettings.Production.json` vì ABP chạy env=Production (không phải Staging).

## Cơ chế (source code)

- Gateway `TenantGatewayExtensions.cs`: SlidingWindowRateLimiter partition theo tenantId, Window=1s, Segments=10, QueueLimit=0. Vượt → 429 + Retry-After. Inject header `X-Genora-Gateway-Key`=SharedKey + `X-Genora-Gateway-Tenant`=GUID, set Host=BackendTenantOrigin authority.
- Guard `TenantGatewayGuardMiddleware.cs` (ABP, sau UseMultiTenancy): nếu path khớp PathPrefix của tenant đang resolve (theo hostname) và không nằm trong ExcludedPathPrefixes → yêu cầu key + GUID khớp; sai → 403. Config đọc lúc startup.
- `GatewayApiProfiles.cs`: profile Hl25 (16 endpoint) và Hlg (24 endpoint) liệt kê TƯỜNG MINH. Validator CẤM catch-all/`*`/segment admin. `AdditionalRoutes` là `List<{Path,Methods}>` (object), KHÔNG phải mảng string.
- ABP Web strip header `X-Powered-By` → dùng làm dấu nhận biết: response CÓ `X-Powered-By: ASP.NET` = đi qua gateway; KHÔNG có = đi thẳng ABP.

## Hành trình chẩn đoán (các red herring đã loại)

1. Ban đầu tưởng do IIS ingress route thẳng 8868 / sai tên file env / AdditionalRoutes catch-all sai schema → sửa hết nhưng vẫn 403/200.
2. Tưởng do binding IIS → hóa ra binding IIS đúng (ingress owns .191:443).
3. Tưởng do global rewrite rules → globalRules rỗng.
4. Tưởng do leading-slash pattern `^api/...` → thêm `^/?` vẫn không đổi → SAI.
5. **DIAG_ALL (`.*` → CustomResponse 418) đặt đầu site IIS ingress vẫn KHÔNG trả 418** → chứng minh dứt khoát IIS ingress KHÔNG nằm trong luồng.
6. `Get-NetTCPConnection -LocalPort 443` → `.191:443` do `httpd.exe` (Apache) giữ → GỐC RỄ.

## Verification (2026-09-23)

- Gateway direct: `curl 127.0.0.1:5088/api/mini-app/hl25/config -H "Host: duocpham-hoalinh.genora.vn"` → 200 (gateway + key + guard OK).
- Public qua Apache: `duocpham-hoalinh…/hl25/config` và `hoalinh…/hlg/knowledge/categories` → 200, có `X-Powered-By` (qua gateway).
- Burst 20 request đồng thời (PermitLimit=1) → hỗn hợp 200 + 429 `Hl25:RateLimitExceeded`. **Rate-limit hoạt động.**

## Việc còn lại / lưu ý vận hành

- **TRẢ `PermitLimit` hl25 về 500** (đang để 1 để test) trong `appsettings.Production.json` của gateway, rồi `Restart-WebAppPool 'Genora.Tenant.Gateway'` (config đọc lúc startup — phải recycle mới có hiệu lực).
- **Reload Apache** sau khi thêm 2 reject rule để `hoalinh…/hl25/*` và `duocpham-hoalinh…/hlg/*` trả 404. Máy KHÔNG chạy Apache dạng service "Apache2.4" → reload qua XAMPP Control Panel (Stop/Start).
- **Firewall cổng 5088** chỉ cho localhost (tránh gọi thẳng .191:5088/.174:5088 né rate-limit). ABP guard đã là 1 lớp chặn gọi thẳng 8868.
- Gỡ rule `DIAG_ALL` khỏi IIS ingress web.config (đã dùng để chẩn đoán).
- Quota là **process-local, 1 worker**. Nhiều worker/recycle overlap sẽ nhân quota. Chưa load-test SLA thực tế.
- Nếu FE gọi endpoint public mới chưa có trong ApiProfiles → gateway trả 404; phải bổ sung vào `GatewayApiProfiles.cs` (và guard PathPrefix nếu cần) + rebuild gateway.
