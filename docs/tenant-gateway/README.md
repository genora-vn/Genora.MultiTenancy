# YARP nhiều tenant — triển khai và kiểm thử staging

Ngày 2026-09-21. Gateway dùng chung có project/assembly `Genora.MultiTenancy.Gateway` và project kiểm thử `Genora.MultiTenancy.Gateway.Tests`. Tên đã được đổi từ project HL25 trước khi triển khai staging; namespace, solution, launch profile và DLL trong `web.config` đã cập nhật đồng bộ. Không thay controller/DTO/business rule, cache, DB, migration hay UI quản trị Host. Cấu hình quota bằng file triển khai/environment variables; chưa có trang quản trị quota trong ABP.

## Phạm vi

```text
Mini App → domain tenant HTTPS → ingress/Ocelot hiện có
  /api/mini-app/hl25/* → YARP (tenant HL25, tổng 500/s) → ABP IIS
  /api/mini-app/hlg/*  → YARP (tenant HLG, tổng 300/s)  → ABP IIS
  Admin/static/Account/SignalR → tuyến ABP hiện có
ABP phân giải tenant → database/schema theo cấu hình ABP hiện có
```

Gateway không đọc SQL, không quyết định schema và không chuyển connection string. Khóa quota là TenantId cấu hình; không lấy tenant từ header/query do client gửi. Public host + API allowlist chọn route. Tất cả API/alias của một tenant dùng chung quota; hai tenant độc lập, kể cả dùng cùng API profile. Host quản trị `staging.genora.vn` / `production.genora.vn` giữ tuyến ABP, không thêm vào gateway tenant entries.

| Môi trường | Profile | Public hostname | TenantId đã đối chiếu | Quota/s |
|---|---|---|---|---:|
| Staging | Hl25 | duocphamhoalinh-staging.genora.vn | 650ccd37-aeb4-63e7-bac7-3a23a72f9cbc | 500 |
| Staging | Hlg | hoalinh-staging.genora.vn | 8cfc81eb-4693-2434-c5b2-3a21cccfe131 | 300 |
| Production | Hl25 | duocpham-hoalinh.genora.vn | 209567fc-4850-44e9-11c8-3a23c58d15a4 | 500 |
| Production | Hlg | hoalinh.genora.vn | 27e348a9-036c-bef8-fa2b-3a22fc202c26 | 300 |

GUID lấy từ một GET application-configuration read-only theo hostname (HL25 production xác minh phiên 20/09; các hostname khác 21/09). Host staging/production trả `currentTenant.id=null`. HLG tenant name thực tế là **Hoa Linh Miền Nam**; hostname/GUID là dữ kiện định tuyến, không suy diễn database name HLG. Phải xác minh lại sau restore/clone DB, vì staging và production có GUID khác nhau.

## Mẫu cấu hình và trường còn phải điền

- `gateway.Staging.example.json` / `gateway.Production.example.json`: mẫu cho YARP.
- `abp-guard.Staging.example.json` / `abp-guard.Production.example.json`: **merge section** vào cấu hình triển khai ABP; không thay toàn bộ appsettings.
- `ocelot-routes.*.example.json`: mảng **route fragments**, không phải file Ocelot hoàn chỉnh. Merge vào `ReRoutes` của version đang chạy; bản mới dùng `Routes`. Không thay GlobalConfiguration/BaseUrl hoặc các route dự án khác.

`BackendAddress` và `SharedKey` cố ý để trống: startup sẽ từ chối cấu hình thiếu. Chưa có địa chỉ/port IIS **staging** nội bộ. Điền origin nội bộ đã test, ví dụ HTTP loopback khi YARP cùng máy ABP hoặc HTTPS có chứng chỉ hợp lệ khi khác máy. Không dùng domain public phía trên làm backend sau cutover để tránh vòng lặp. Không copy port production8868 sang staging theo suy đoán. Production8868 từng phản hồi **HTTP**, còn HTTPS handshake thất bại; chỉ dùng `http://127.0.0.1:8868/` khi xác minh binding trên chính máy ABP. Code từ chối HTTP không phải loopback; không tắt TLS validation.

Mỗi tenant dùng secret ngẫu nhiên riêng >=32 ký tự ASCII không khoảng trắng. Cấp qua environment variables của đúng App Pool/process hoặc secret store triển khai, không Git/log/FE/Ocelot:

```text
# Process gateway (ví dụ tên biến; không phải giá trị secret)
TenantGateway__Tenants__hl25__SharedKey
TenantGateway__Tenants__hlg__SharedKey
TenantGateway__Tenants__hl25__BackendAddress
TenantGateway__Tenants__hlg__BackendAddress
# Process ABP: giá trị tương ứng phải giống gateway của tenant đó
TenantGatewayGuard__Tenants__hl25__SharedKey
TenantGatewayGuard__Tenants__hlg__SharedKey
```

`BackendTenantOrigin` là origin public ABP của tenant, độc lập địa chỉ kết nối. YARP pin Host/X-Forwarded-Host/Proto và tenant GUID; bỏ cookie, Authorization, client Forwarded/override tenant. Hiện hai profile là controller Mini App anonymous. Không dùng AdditionalRoutes cho API cần cookie/JWT trước khi đánh giá cơ chế xác thực; token Zalo trong body vẫn giữ nguyên. CORS mẫu là `https://h5.zdn.vn`, cần đối chiếu Origin thật của Mini App, thêm exact origins khi cần; không wildcard.

Guard ABP nằm sau `UseMultiTenancy`, trước auto-migration. Khi bật, tenant+prefix được bảo vệ yêu cầu cả shared key và GUID khớp `ICurrentTenant`. Gửi tenant khác/Host không được chấp nhận. Secret bị xóa trước middleware nghiệp vụ. Admin HL25 `/api/mini-app/hl25/admin` được loại trừ để giữ authorization hiện có. Tenant/module ngoài danh sách không bị thay đổi. Guard mặc định tắt khi chưa cấu hình, nên **chưa chặn bypass cho đến khi bật**.

## Thêm tenant hoặc thêm module của cùng tenant

Thêm một entry cùng cấu trúc vào `TenantGateway:Tenants`, dùng TenantId thật, exact `PublicHosts`, địa chỉ IIS/origin, secret riêng, `PermitLimit` nguyên dương và `ApiProfiles`. Ví dụ tenant thứ ba có thể dùng `ApiProfiles: ["Hl25"]`, limit200; không cần sửa C#. Nếu **một tenant** dùng cả HL25 và HLG thì đặt `ApiProfiles: ["Hl25", "Hlg"]` trong **một entry**, và đưa cả hai prefix vào guard; quota được tính tổng hai module. Thêm hostname alias trong `PublicHosts`, quota vẫn giữ chung GUID.

Module khác có thể dùng `ApiProfiles: []` và explicit `AdditionalRoutes`:

```json
"AdditionalRoutes": [
  { "Path": "/api/mini-app/example/items", "Methods": ["GET"] },
  { "Path": "/api/mini-app/example/items/{id:guid}", "Methods": ["GET"] }
]
```

Cập nhật prefix tương ứng bên guard và route ở ingress. Chỉ cho phép Mini App routes cụ thể, không wildcard/catch-all hoặc segment admin. Mỗi TenantId/hostname/secret phải riêng; trùng bị từ chối startup. `Enabled:false` chỉ loại entry khỏi YARP; không tự tắt tenant trong ABP. Nếu tắt tuyến chương trình, giữ guard để tránh fallback trực tiếp. API mới trong controller cần cập nhật profile/AdditionalRoutes và regression tests.

Thay đổi config cần restart gateway có kiểm soát; không có live sync DB/Host Admin hoặc hot reload được cam kết. Environment variables có ưu tiên hơn JSON: xóa giá trị cũ nếu đang override.

## Windows Server / IIS / ingress

1. Build/publish riêng từ repository root:

   ```powershell
   dotnet publish src/Genora.MultiTenancy.Gateway/Genora.MultiTenancy.Gateway.csproj -c Release -o artifacts/tenant-gateway
   dotnet publish src/Genora.MultiTenancy.Web/Genora.MultiTenancy.Web.csproj -c Release -o artifacts/abp-web
   ```

2. Gateway là IIS site/App Pool riêng, .NET9 Hosting Bundle phù hợp artifact, No Managed Code, **1 worker**, AlwaysRunning/preload sau khi kiểm tra môi trường. Copy mẫu staging thành `appsettings.Staging.json` trong **thư mục publish gateway** và điền các trường bắt buộc qua deployment config/env; đặt `ASPNETCORE_ENVIRONMENT=Staging` cho đúng process. File `.example.json` không tự được load. Merge guard vào config **ABP staging**; giữ connection strings và cấu hình hiện có. Không đổi appsettings source của user.
3. Khóa binding gateway/origin bằng firewall để chỉ ingress được gọi. Port5088 trong mẫu là đề xuất, phải kiểm tra chưa dùng. `Urls` loopback phục vụ chạy Kestrel trực tiếp; khi host IIS, IIS binding mới là địa chỉ ingress gọi. Không nhầm port IIS public của ABP với port YARP. HTTP loopback ingress→YARP mẫu chỉ áp dụng khi cùng máy; khác máy dùng mạng nội bộ/TLS phù hợp.
4. Ingress phải nhận HTTPS/cert cho **cả hai hostname staging**. Route theo exact hostname + prefix tới YARP, giữ **Host thực tế** đúng hostname tenant. Mẫu `UpstreamHeaderTransform.Host` áp cho request, cần xác minh version Ocelot đang triển khai; source Ocelot không nằm trong repository này. Chỉ gửi `X-Forwarded-Host` mà Host là127.0.0.1 sẽ bị YARP404. Không hạ bảo vệ bằng việc tin X-Forwarded-Host từ client.
5. Trước các route prefix Priority100, giữ/thêm tuyến **HL25 admin/export về ABP** với priority cao hơn, bảo toàn authentication; path ngoài prefix, tài nguyên `/uploads`, Account, `/api/abp/*`, Admin Razor và `/signalr-hubs/hlg-live-feed` theo tuyến ABP hiện có. Hub WebSocket không nằm trong quota HTTP này. Không đưa toàn domain vào YARP rồi làm mất Admin/static. Không fallback sang ABP khi YARP trả429/5xx (sẽ bypass quota); không retry POST/PUT ở ingress.
6. ABP bật `ReverseProxy:Enabled`, giới hạn KnownProxies đúng IP YARP. Xác minh ForwardedHeaders để ảnh/redirect giữ HTTPS đúng domain. Không trust all proxies. Kiểm tra request-size limit ở mọi hop đáp ứng upload5MB+multipart; không tăng vô hạn. Health `/health/live` chỉ kiểm tra process, không kiểm tra DB/tenant.
7. Dùng maintenance/cutover window staging: publish hai app, cấu hình guard và secrets, khởi động, kiểm thử trực tiếp YARP, rồi route traffic ingress. Không disable guard để chữa lỗi502/403. Log theo tenant/route/status/thời gian; không log shared key, Zalo token/phone hoặc body.

Chế độ cũ `Hl25Gateway` / `Hl25GatewayGuard` vẫn dùng được. Khi chuyển: bỏ cấu hình legacy TenantId khỏi gateway và tắt `Hl25GatewayGuard:Enabled`, bật generic guard cùng lúc chuyển cấu hình. Không bật hai mode; startup sẽ fail. Chuyển/rollback hai app đồng bộ trong maintenance, giữ origin firewall; không mở public ABP để né guard. Giữ bản publish/config trước cutover để rollback.

## Quota thực tế

ASP.NET Core sliding window1s,10segment100ms,queue0, chung theo tenant; vượt trả429 với Retry-After>=1s, no-store, CORS. HL25 trả error string/success=false; HLG trả error429 numeric/data=null, đúng kiểu envelope hiện có. Preflight CORS hợp lệ và health không tiêu thụ quota. Không retry writes/cache response tại gateway.

Đây là bộ đếm **trong một process**: nhiều YARP instances/workers hoặc overlapped recycle làm tăng tổng quota. Không bật web garden/scale-out; điều phối stop/start tránh hai process nhận traffic cùng lúc. Muốn HA/global quota xuyên node cần bộ đếm dùng chung (chưa triển khai Redis). Restart reset quota; thuật toán chia segment không cam kết chặn chính xác trong mọi khoảng trượt1000ms tùy ý.

500+300 là tổng ngân sách **tối đa800RPS** của hai tenant, không phải bằng chứng máy6vCPU/16GB cùng SQL+16IIS chịu được800RPS. Quota không hạn chế số request nặng đang xử lý; cần đo cả peak và concurrency. Host/tenant khác vẫn dùng tài nguyên chung. Chốt ngưỡng vận hành từ số đo, hạ quota nếu server không đủ.

## Kiểm thử triển khai

**Smoke có kiểm soát:** `pwsh -File docs/tenant-gateway/smoke-staging.ps1 -ConfirmStaging` gửi đúng4GET tới public staging: hai application-configuration của tenant và hai read API. Không tạo dữ liệu, không load test. Nó xác minh GUID/envelope, không chứng minh request đã đi qua gateway; cần thêm bước429/origin403 dưới đây.

| Kiểm tra sau deploy | Kỳ vọng | Trạng thái phiên21/09 |
|---|---|---|
| GUID staging và Host quản trị | đúng bảng, Host id=null | PASS probe read-only trước deploy |
| YARP route/tenant/quota/guard | spoof bị chặn; độc lập giữa tenant | PASS tests tự động, chưa IIS |
| Public HL25config + HLGcategories | HTTP200, envelope đúng, ảnhHTTPS đúnghost | BLOCKED chưa cutover |
| Tạm quota staging2/3; burst6GET mỗi tenant | HL25~2OK, HLG~3OK, còn429; hồi phục sau>=1s | BLOCKED chưa deploy; segment/timer cần ghi thực tế |
| HL25overload + HLG tải thấp | HLG không bị ăn quota HL25 | BLOCKED chưa deploy |
| Origin trực tiếp không sharedkey; fake tenant |403 trên hai prefix; không trả dữ liệu tenantkhác | BLOCKED cần IIS origin |
| Admin/Host/static/HLGSignalR | hoạt động như cũ, không qua quota public | BLOCKED cần staging UAT |
| Browser MiniApp CORS/Retry-After | origins thật đọc được200/429, preflight không vàoSQL | BLOCKED cần FE staging |
| SQL-backed mixed writes/1000CCU | đối soát dữ liệu và resource/SLA | BLOCKED chưa chạy tải |

Burst ở quota thấp cần tách khỏi traffic khác; trả quota500/300 và restart sau test. Khi quota đang thấp, không chạy load script capacity rồi coi429 là lỗi business. Xác minh direct origin bằng hostname tenant đúng (TLS `--resolve` nếu cần), không dùng `-k`, không đưa secret lên command history. Feature HLG/tài khoản/dữ liệu test phải sẵn sàng; GUID available không chứng minh module đã enabled hoặc đủ bảng.

**Đo đồng thời hai tenant, read-only** với `tests/load/tenant-gateway/read-rps.js` (cần k6 trên máy phát tải riêng):

```powershell
$env:HL25_BASE_URL = 'https://duocphamhoalinh-staging.genora.vn'
$env:HLG_BASE_URL = 'https://hoalinh-staging.genora.vn'
$env:CONFIRM_HL25 = $env:HL25_BASE_URL
$env:CONFIRM_HLG = $env:HLG_BASE_URL
$env:TENANT_GATEWAY_LOAD_APPROVED = 'yes'
$env:HL25_RPS = '25'
$env:HLG_RPS = '25'
$env:DURATION = '2m'
$env:MODE = 'capacity'
k6 run --summary-export tenant-gateway-read-25-25.json tests/load/tenant-gateway/read-rps.js
```

Một iteration=mộtGET; mỗi scenario có quota tải riêng. Tăng HL25 50→100→250→400→500; HLG25→100→200→300 sau khi mức trước ổn. Chạy riêng rồi kết hợp; baseline business250RPS là HL25. Với `MODE=overload`, gửi HL25>500, HLG thấp để kiểm tra độc lập rồi đảo lại;429 được ghi riêng, không che5xx/HTTP200businessfail. Script fail nếu **toàn bộ** request chỉ429 hoặc dropped_iterations>0. Báo successful RPS theo counter `gateway_business_success`, 429 ratio, actual `http_reqs`, p95/p99 thành công theo từng tenant; không coi offered RPS là throughput thành công. Biên500/300 có thể429 do burst/timer.

Hành trình HL25 register/upload/create/share/spin dùng script gated sẵn `tests/load/hl25-gateway/journey.js` và [quy trình tải/đối soát](../hl25-gateway/README.md). Không tự retry POST chưa biết commit. HLG mới có read-load ở đây; chưa có script ghi game/redeem end-to-end. 1000CCU cần cohort người dùng/think-time, không đồng nghĩa1000RPS. Soak ít nhất45phút qua TTLcache20phút và auto-migrate30phút; kiểm tra cold-cache riêng. Theo dõi PerfMon CPU/RAM/SQLwaits/IO/IISqueue và SLA các site khác. Dừng nếu CPU>90%60s, RAMkhả dụng<1GB, lỗi/timeout>1%1phút hoặc site khác mấtSLA. SQL cùng máy nên phải chốt bộ nhớ và cạnh tranhIO theo số đo.

Request-time `TenantAutoMigrateMiddleware` hiện vẫn có cache30phút/khôngsingleflight, cache HL25 process-local: các rủi ro trước đó còn nguyên, gateway không sửa. Không kết luận sẵn sàng500RPS/1000CCU chỉ từ build/test middleware.

## Lệnh verify source

```powershell
dotnet build src/Genora.MultiTenancy.Gateway/Genora.MultiTenancy.Gateway.csproj -c Release --no-restore
dotnet build src/Genora.MultiTenancy.Web/Genora.MultiTenancy.Web.csproj -c Hl25Performance --no-restore
dotnet test test/Genora.MultiTenancy.Gateway.Tests/Genora.MultiTenancy.Gateway.Tests.csproj -c Release --no-restore
dotnet test test/Genora.MultiTenancy.Web.Tests/Genora.MultiTenancy.Web.Tests.csproj -c Hl25Performance --filter FullyQualifiedName~Hl25 --no-restore
node --test tests/load/hl25-gateway/scripts.test.cjs tests/load/tenant-gateway/scripts.test.cjs
```

`Hl25Performance` dùng output riêng tránh DLL Web đang chạy bị lock, không phải environment staging. Tests gateway dùng TestServer→Kestrel loopback; guard dùng tenant giả lập, không SQL/IIS. Kết quả thực chạy và blocker lưu trong `.claude/memory/notes/project/project_multi_tenant_yarp_gateway_20260921.md`.

Tham khảo: [YARP route rate limiting](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/rate-limiting?view=aspnetcore-9.0), [Host transforms](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/transforms-request?view=aspnetcore-9.0), [Ocelot request header transforms](https://ocelot.readthedocs.io/en/latest/features/headerstransformation.html). Mẫu Ocelot phải UAT với version thực tế, không giả định schema mới nhất tương thích bản của anh.
