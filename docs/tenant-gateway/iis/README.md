# Sửa cấu hình IIS staging — ARR → YARP → ABP

## Kết luận từ source, log và hai ảnh ngày 21/09/2026

Kiến trúc ba site phù hợp, nhưng cấu hình gửi kèm **chưa đủ điều kiện chuyển production**. Đã kiểm tra source tại commit `e4ec431` và ảnh trong Downloads; chưa đăng nhập Windows Server hoặc xác minh binding/config hiệu lực trực tiếp.

| Vấn đề xác định | Cách xử lý |
|---|---|
| HL25/HLG dùng cùng SharedKey | Mỗi tenant một key >=32 ký tự; key của cùng tenant phải giống giữa Gateway và ABP. Không bỏ validation. |
| Bản ABP mục3 bật cả Hl25GatewayGuard và TenantGatewayGuard | Chế độ nhiều tenant: legacy **false**, generic **true**. Bản cấu hình đầu câu hỏi để legacy false vẫn lỗi vì key trùng. |
| Gateway có hai tệp: appsettings.Staging.json thiếu key/origins, gateway.Staging.json có đủ | Program hiện tại chỉ dùng default configuration providers, **không tự nạp gateway.Staging.json**. Gộp đầy đủ vào appsettings.Staging.json; env vars có thể override. |
| Ảnh gateway còn DLL/EXE Hl25Gateway | Publish mới Gateway và dùng web.config trỏ Gateway.dll. Không chắp vá artifact cũ/mới. |
| Test URL hoalinh-staging + /hl25/config | Đây là sai cặp hostname/profile. Rule cũ đi thẳng ABP qua fallback; ARR/3.0 và HTTP200 không chứng minh YARP/quota hoạt động. Rule mới trả404. |
| JSON ABP dán thiếu dấu phẩy giữa SelfUrl và AppUrl | Sửa cú pháp dưới đây; nếu file thật y hệt, nó sẽ lỗi JSON trước validation. Log hiện tại không chứng minh bản dán chính là file đã nạp. |
| Port Gateway/ABP được mô tả bằng IP | Chưa xác nhận được port thực. Cấu hình theo bảng binding phía dưới hoặc xuất inventory. |

```json
"App": {
  "SelfUrl": "https://{0}-staging.genora.vn",
  "AppUrl": "https://staging.genora.vn"
}
```

Đây chỉ là hai dòng sửa dấu phẩy, giữ các App fields khác. `ASPNETCORE_ENVIRONMENT=Staging` đã được anh xác nhận. Kiểm tra giá trị này trong **web.config của cả Gateway và ABP**, và không có `DOTNET_ENVIRONMENT=Production`/env override khác. `launchSettings.json` không quyết định môi trường IIS. Source gateway web.config mặc định Production; script bên dưới sinh bản Staging đúng.

## 1. Binding và App Pool

Áp dụng khi cả ba site chạy trên **cùng Windows Server**. Không tạo thêm site ABP theo từng tenant/database.

| Site | Protocol / IP / Port | Hostname | Ghi chú |
|---|---|---|---|
| Genora.Ingress.Staging | HTTPS / 103.157.218.187 /443 | duocphamhoalinh-staging.genora.vn | SNI + certificate hợp lệ |
| Genora.Ingress.Staging | HTTPS / 103.157.218.187 /443 | hoalinh-staging.genora.vn | SNI + certificate hợp lệ |
| Genora.Ingress.Staging | HTTPS / 103.157.218.187 /443 | staging.genora.vn | Thêm nếu đưa Host quản trị qua ingress như template |
| Genora.Tenant.Gateway | HTTP / **127.0.0.1 /5088** | **để trống** | Chỉ nội bộ; Host request vẫn là tenant hostname |
| Genora.MultiTenancy | HTTP / **127.0.0.1 /8868** | **để trống** | ABP origin nội bộ |

Binding `All Unassigned:8868` có thể nhận loopback, nhưng cũng nghe trên IP public. Nên chuyển origin sang127.0.0.1 sau khi kiểm tra các hệ thống đang gọi port8868. Binding chỉ vào103.157.218.187:5088 **không** nhận kết nối127.0.0.1:5088. Không dùng IP làm giá trị Port.

Hai cách hợp lệ cho `staging.genora.vn`: (a) binding exact Host trên ingress và fallback về8868 như bảng; hoặc (b) giữ Host quản trị trên ABP443 hiện tại. Cách(b) vẫn hợp lệ nhưng Host không đi qua ingress. Không được để cùng một exact IP/port/hostname binding trên hai site. Binding ABP `All Unassigned:443` anh gửi chưa có hostname nên cần kiểm tra trước khi bỏ/đổi. Không xóa binding đang phục vụ tenant/module khác; sau khi chuyển qua ingress, bảo đảm hai hostname Mini App không còn đường public khác đi thẳng ABP để né gateway.

Gateway và ABP: App Pool riêng, No Managed Code, Integrated, .NET9 Hosting Bundle phù hợp, app pool identity đọc được artifact/config. Gateway **Maximum Worker Processes=1**; tránh overlapped recycle chạy hai process nhận traffic cùng lúc vì quota local mỗi process. Ingress có thể chỉ chứa web.config với ARR + URL Rewrite, App Pool riêng. Không chạy IISRESET toàn server16site để restart một app; recycle đúng pool theo cutover window.

Để xuất đúng binding/pool/env, chạy Windows PowerShell64bit trên server:

```powershell
powershell -NoProfile -File docs/tenant-gateway/iis/Get-IisInventory.ps1
```

Script chỉ đọc và không xuất secrets/connection strings. Các tên pool thật và ARR server setting vẫn chưa được cung cấp trong hội thoại.

## 2. Bật ARR và giữ Host

Trong IIS Manager:

1. Server → Application Request Routing Cache → Server Proxy Settings → **Enable Proxy**.
2. Server → Configuration Editor → `system.webServer/proxy` → **preserveHostHeader=true**. Đây là setting cấp server, review ảnh hưởng các ARR site khác trước khi đổi. YARP chọn route bằng Host thật; chỉ gửi X-Forwarded-Host không đủ.
3. Ingress → URL Rewrite → View Server Variables: cho phép `HTTP_X_FORWARDED_HOST` và `HTTP_X_FORWARDED_PROTO`. Nếu section bị khóa, cấu hình allowlist tại applicationHost.config theo quy trình IIS; không mở khóa toàn bộ sections tùy tiện. Thiếu allowlist có thể gây500.50.
4. Không thêm cache/retry/fallback cho429. IIS và ARR phải giữ nguyên status/body/Retry-After từ gateway. Templates có `httpErrors existingResponse="PassThrough"`; kiểm tra không có ARR disk/output caching cho hai prefix Mini App.
5. ABP `ReverseProxy:Enabled=true`; KnownProxies chứa127.0.0.1 và::1 cho topology này. IPpublic chỉ giữ nếu thực sự là trusted proxy kết nối vào ABP, không trust toàn Internet. Thay đổi tenant resolver của anh trong3commit gần nhất được giữ nguyên.

Template chỉ áp dụng cho **site ingress riêng**. Nó dùng `<clear/>` xóa các distributed rules kế thừa ở site đó để có thứ tự xác định, không sửa rule của các site khác; vẫn phải kiểm tra globalRules cấp server. Không đưa file này vào site ABP hoặc gateway.

Thứ tự rule mới: HL25 Admin→ABP; HL25 đúnghost→YARP; HLG đúnghost→YARP; hai prefix trên host sai→404; các đường khác của3host đã biết→ABP; host lạ→404. Regex bao gồm cả root prefix/đường có slash, ignoreCase; query giữ nguyên. SignalR/static/Account/Admin và `/api/abp/*` vẫn về ABP.

## 3. Tạo bộ cấu hình đúng và key mới

Chạy từ repository root **trên máy triển khai**, với thư mục output mới, ngoài Git và được giới hạn ACL:

```powershell
powershell -NoProfile -File docs/tenant-gateway/iis/New-DeploymentConfig.ps1 -Environment Staging -OutputDirectory C:\Genora\Deploy\prepared-staging-20260921
```

Script không deploy/restart/sửaIIS; mặc định Gateway5088, ABP8868 cùng máy. Nó sinh:

| File output | Sử dụng |
|---|---|
| gateway/appsettings.Staging.json | Toàn bộ TenantGateway config: đúng2GUID/domain staging, quota500/300, AllowedOrigins và **hai key mới khác nhau** |
| gateway/web.config | DLLGateway mới, environmentStaging, HTTPerrorsPassThrough |
| abp/guard.Staging.fragment.json | Hai key khớp gateway; legacyguardfalse, genericguardtrue; **merge các section vào file ABP hiện có** |
| ingress/web.config | Các rewrite rules đã sửa, theo hostname staging |

**Không dùng fragment làm toàn bộ appsettings ABP.** Không thay ConnectionStrings, AuthServer, Zalo, UrBox, DataProtection hay thiết lập nghiệp vụ bằng fragment. Cấu hình guard không gọi DB và không thay schema.

Base `appsettings.json` Gateway và env vars không được còn legacy `Hl25Gateway:TenantId` đã cấu hình khi bật TenantGateway. Các file `gateway.Staging.json` cũ chỉ nên lưu làm bản nháp bên ngoài deployment; runtime không đọc chúng. Nếu env vars đang đặt SharedKey, chúng override JSON: cập nhật đồng bộ hoặc bỏ override cũ. Không set key chung cấp machine cho cả hai tenant.

Key đã đưa vào hội thoại không đưa tiếp vào source hoặc dùng cho production. Script sinh key mới, không echo giá trị; chỉ cấp quyền đọc cho deployment owner và App Pool cần đọc. Password DB/API keys khác đã chia sẻ cần rotate theo quy trình riêng; **không tự đổi StringEncryption/DataProtection keys** khi chưa đánh giá dữ liệu/cookie bị ảnh hưởng.

## 4. Publish và cutover staging

```powershell
dotnet publish src/Genora.MultiTenancy.Gateway -c Release -o artifacts/gateway-iis
dotnet publish src/Genora.MultiTenancy.Web -c Release -o artifacts/abp-iis
```

Backup artifact/config và IIS bindings hiện hành. Deploy gateway vào thư mục version mới, đầy đủ deps/runtimeconfig/YARP DLLs, rồi overlay2file gateway vừa sinh; DLL và web.config phải cùng phiên bản mới. Không chỉ copy một DLL vào thư mục ảnh đang chứa Hl25Gateway. Với ABP, triển khai Web mới có validator giải thích lỗi, merge fragment vào appsettings.Staging.json, sửa dấu phẩy nếu lỗi tồn tại ở file thật. Copy ingress/web.config vào site ingress riêng.

Chốt binding/pool/ARR như trên và restart riêng các pool trong maintenance window. Khi guard bật, gọi ABP trực tiếp không có key sẽ403: đó là kết quả đúng, không tắt guard để xử lý403. Nếu startupfail, thông báo mới chỉ rõ đường dẫn như `TenantGatewayGuard:Tenants:hlg:SharedKey duplicates ...hl25:SharedKey`, không in key.

## 5. Test từng chặng, không follow redirect

Chạy trên server bằng `curl.exe` để không nhầm alias Invoke-WebRequest. Không dùng `--location` hoặc `-k` khi chẩn đoán.

```powershell
# Process Gateway hoạt động, chưa khẳng định ABP/DB:
curl.exe -i http://127.0.0.1:5088/health/live

# Gateway -> ABP, Host đúng:
curl.exe -i http://127.0.0.1:5088/api/mini-app/hl25/config -H "Host: duocphamhoalinh-staging.genora.vn"
curl.exe -i http://127.0.0.1:5088/api/mini-app/hlg/knowledge/categories -H "Host: hoalinh-staging.genora.vn"

# Chốt origin ABP: hai request KHÔNG gửi sharedkey phải403:
curl.exe -i http://127.0.0.1:8868/api/mini-app/hl25/config -H "Host: duocphamhoalinh-staging.genora.vn"
curl.exe -i http://127.0.0.1:8868/api/mini-app/hlg/knowledge/categories -H "Host: hoalinh-staging.genora.vn"

# Chuỗi public ARR -> YARP -> ABP:
curl.exe -i https://duocphamhoalinh-staging.genora.vn/api/mini-app/hl25/config
curl.exe -i https://hoalinh-staging.genora.vn/api/mini-app/hlg/knowledge/categories

# Sai hostname/profile: phải404 sau khi sửa ingress:
curl.exe -i https://hoalinh-staging.genora.vn/api/mini-app/hl25/config
```

Đọc response body: HTTP200 có thể là businesserror. HL25 cần success=true; HLG cần data đúng và không có error. `/health/live` gateway200, còn API502 thường là origin/binding/backend chưa chạy. API403 qua gateway: kiểm tra GUID resolution/key/env hiệu lực; không copykey lêncurl. Gateway404 cho đúngroute: kiểm tra incomingHost/preserveHostHeader, profile và DLL đang chạy. Public500.50/500.19: xem IIS substatus/allowedServerVariables/XML/sectionlocked. Tenant resolution sai: đối chiếu `/api/abp/application-configuration` trên **đúnghostname** với GUID trong mẫu; custom DatabaseHost resolver dùng ITenantStore.FindAsync(host) theo tenant name, còn HostTenant resolver hiện có tra host mapping. Không mặc định database name là tenant name/GUID.

Sau smoke, tạm quota2HL25/3HLG trong môi trường staging yên tải, restart gateway và gửi một burst nhỏ: phải có429/Retry-After, quay lại200 sau khoảng1giây, tenant còn lại không bị lấy mất quota. Trả500/300 sau test. Kiểm tra Admin/login/images/HLGSignalR trên tenant, Host quản trị và CORS từ Origin MiniApp thật. Dùng checklist/load scripts trong [runbook chung](../README.md) để đo250→500RPS HL25 và300RPS HLG, rồi mixed load. Các test regex/unit **không** thay cho IIS/ARR/SQL UAT.

## 6. Production sau khi staging đạt

- Chỉ chuyển khi cả3chặng test đúng, directorigin403, wronghost404,429hồi phục, Host/Admin/static/CORS đúng và load test đạt SLA. Hiện chưa có các kết quả này để xác nhận production-ready.
- Chạy lại generator với `-Environment Production`, outputdir mới. Nó dùng3domainproduction và GUIDproduction đã lưu trong mẫu; không copyGUID/key staging. Nếu deployserver khácmáy hoặc đổiports cần review topology, không dùng loopback của máy khác.
- Gateway production: environmentProduction, domain `duocpham-hoalinh.genora.vn` / `hoalinh.genora.vn`; Host `production.genora.vn`. Giữ1worker và cấu hìnhTLS/firewall; kiểm tra lạiGUID sau restoreDB. IISoriginproduction từng được xác định8868HTTP, cần smoke trên chính server trước cutover.
- Phần sửa này không apply migration/DB. Chưa chứng nhận500RPS/1000CCU; máy6vCPU16GB cùngSQL+16IIS vẫn cần đo tải thực.

## Tài liệu chính thức

[ARR + URL Rewrite](https://learn.microsoft.com/en-us/iis/extensions/url-rewrite-module/reverse-proxy-with-url-rewrite-v2-and-application-request-routing), [allowlist server variables](https://learn.microsoft.com/en-us/iis/extensions/url-rewrite-module/setting-http-request-headers-and-iis-server-variables), [ASP.NET Core configuration providers](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-9.0), [giữ hostname qua reverse proxy](https://learn.microsoft.com/en-us/azure/architecture/best-practices/host-name-preservation).
