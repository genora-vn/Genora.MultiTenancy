# HL25 — Gateway riêng, 500 request/giây

> **Cập nhật 2026-09-21:** cấu hình nhiều tenant, staging/production và quota HL25=500 / HLG=300 ở [runbook mới](../tenant-gateway/README.md). Nội dung bên dưới mô tả chế độ legacy một tenant; không trộn hai chế độ cấu hình.

Ngày 2026-09-20; branch `hotfix/20260920`, baseline `060df7e`.
Source đã triển khai; chưa deploy/chuyển traffic hoặc chạy load test trên DB nghiệp vụ.

## Phạm vi và topology

```
Zalo Mini App → HTTPS duocpham-hoalinh.genora.vn (giữ URL hiện có)
             → gateway Ocelot hiện có: thêm đúng 1 route theo hostname + path
             → YARP riêng, 1 process → ABP IIS → tenant DB DuocPhamHoaLinh
```

YARP được triển khai như một ứng dụng ASP.NET Core riêng, có thể host bằng IIS một site/App Pool riêng. "Trước IIS" ở đây nghĩa là trước site ABP; không nhét proxy vào middleware business của ABP. Reuse máy/hạ tầng gateway hiện có; không chuyển các route Golf/Identity/khách hàng khác sang YARP. Không thay GlobalConfiguration.BaseUrl `api.baygolf.vn` của gateway cũ. User xác nhận lại dùng chính hostname `duocpham-hoalinh.genora.vn`, không tạo `api-hl25` và không dùng domain Baygolf cho Mini App. FE giữ nguyên baseURL; ingress phải giữ các đường dẫn Admin/static ngoài API HL25 về ABP.

`ocelot-route.example.json` là **một route fragment**, thêm vào `ReRoutes` của phiên bản Ocelot user đang dùng, không ghi đè file hiện có. Xác minh phiên bản/schema Ocelot (bản mới dùng `Routes`), `UpstreamHost`, thứ tự/priority khi staging. Repository này không có source gateway Ocelot. Mẫu đã giới hạn UpstreamHost=`duocpham-hoalinh.genora.vn`; cần xác minh ingress/DNS/TLS hiện có trước cutover, không tự thay DNS. Port YARP 5088 chỉ là đề xuất, cần kiểm tra chưa bị chiếm.

## Dữ kiện endpoint đã xác minh

- Public ABP tenant: `https://duocpham-hoalinh.genora.vn`; API prefix `/api/mini-app/hl25/`.
- Một GET công khai application-configuration trả tenant **209567fc-4850-44e9-11c8-3a23c58d15a4**, tên **Dược phẩm Hoa Linh**, isAvailable=true. DB DuocPhamHoaLinh do user xác nhận, không đọc connection string production.
- User cung cấp `https://103.157.218.174:8868/`; probe TLS thất bại `Cannot determine the frame size or a corrupted frame was received`.
- Probe **HTTP** cùng IP:8868 với Host `duocpham-hoalinh.genora.vn` trả 200, đúng TenantId, không redirect. Port quan sát đang phục vụ HTTP, không phải HTTPS. Không tắt kiểm tra chứng chỉ.
- Mẫu backend `http://127.0.0.1:8868/` chỉ dùng **khi YARP chạy trên chính máy ABP** và đã test binding loopback. Chưa xác minh loopback từ máy production. Nếu ở máy khác, dùng origin HTTPS chứng chỉ hợp lệ hoặc mạng riêng được bảo vệ; không gửi shared key qua HTTP public IP. Không dùng hostname public làm BackendAddress sau cutover vì có thể quay lại ingress tạo vòng lặp; dùng origin nội bộ đã xác minh.

## Hành vi limiter và giới hạn

- 16 route path, 17 method+path hiện có của public HL25 cùng **một partition tenant**; GET/POST/PUT đều tính chung 500, không phải mỗi route 500. Options hạ được 1..500, thay đổi cần restart gateway.
- ASP.NET Core sliding window 1 giây/10 segment 100ms, queue=0. Đây là cửa sổ chia segment, có độ lệch ở biên/timer; **không cam kết tối đa đúng 500 trong mọi khoảng trượt bất kỳ 1.000ms**. Hạn mức đầu vào cũng không phải cam kết backend xử lý thành công 500 RPS.
- Một timer gốc cho partition duy nhất, tránh helper `GetSlidingWindowLimiter` tắt AutoReplenishment khiến heartbeat 100ms có thể bỏ segment do jitter (đã phát hiện bằng test hồi phục hạn mức).
- Vượt hạn mức: HTTP429, Retry-After >=1 giây, Cache-Control:no-store, envelope `{success:false,data:null,error:"Hl25:RateLimitExceeded",message:...}`. Request bị từ chối không gọi backend. CORS cho phép FE đọc Retry-After.
- CORS preflight hợp lệ kết thúc tại gateway, không tính quota business/không đến SQL. `/health/live` chỉ liveness, không chứng minh DB sẵn sàng. Admin/export, module khác và route lạ không được proxy.
- Một process YARP cho tenant này. Nhiều worker/replica hoặc overlapped recycle có thể nhân đôi quota; nếu cần global500 xuyên node/restart phải triển khai bộ đếm chung. Không thêm Redis trong phiên này. Chọn 1 worker; điều phối restart tránh overlap và warmup có kiểm soát.
- FE cần xử lý HTTP429, chờ Retry-After và thêm jitter để tránh retry đồng loạt; GET có thể thử lại có giới hạn. Không tự phát lại POST quay/tạo thiệp sau timeout khi chưa biết giao dịch đã commit hay chưa.
- Không tự retry POST/PUT, không cache response ở gateway; stock và logic quay vẫn do ABP/SQL xử lý. Không thay API business/DTO/schema/cache đã hotfix.
- Không có concurrency cap cho request nặng: 500 lượt upload/quay mỗi giây vẫn có thể quá tải máy. Ngưỡng concurrency phải đo bằng load test trước khi đặt thêm.

## Tenant và origin guard

ABP hiện chọn tenant theo Host trước, sau đó Domain `.local`, Header `tenant`, Query `tenant`. Vì vậy YARP cố định Host và X-Forwarded-Host về hostname tenant, X-Forwarded-Proto về scheme cấu hình; ghi đè header/query `tenant` bằng GUID đúng; bỏ `__tenant`, client Forwarded, cookie và Authorization của tuyến Mini App anonymous. AccessToken gọi Zalo trong body vẫn giữ nguyên. Gateway này không dùng cho Admin/login ABP.

Chốt `Hl25GatewayGuardMiddleware` chạy sau UseMultiTenancy và trước TenantAutoMigrateMiddleware. Khi Enabled=true:

- Public HL25 của target tenant phải có shared key; request gateway-marked phân giải thành host/tenant khác bị 403.
- So sánh hash constant-time, bỏ header secret trước middleware business/logging kế tiếp.
- Tenant/module khác giữ hành vi cũ. Admin nằm ngoài chốt này và không nằm trong route YARP.
- Chốt mặc định **tắt**, phải enable khi cutover; đây không thay cho firewall. Sai TenantId ở cấu hình là sai triển khai, cần xác minh GUID ở cả hai app trước khi bật.

Không coi quota là authentication. Mini App hiện nhận ZaloUserId từ client theo contract cũ; gateway không chứng minh danh tính Zalo và không sửa authorization đó.

## Cấu hình triển khai

1. Publish riêng gateway bằng `dotnet publish src/Genora.MultiTenancy.Gateway -c Release -o artifacts/tenant-gateway`. Không publish đè site ABP hoặc gateway cũ.
2. Dùng các giá trị trong `gateway-settings.example.json` sau khi xác minh mạng. File này là mẫu, không tự được load. Chuyển từng key qua env hoặc appsettings deployment ngoài Git. Hostname Mini App Zalo được lấy từ CORS hiện có `https://h5.zdn.vn`; thêm exact origin khác chỉ sau khi xác minh FE.
3. Cấp cùng secret ngẫu nhiên >=32 ký tự ASCII qua `Hl25Gateway__SharedKey` và `Hl25GatewayGuard__SharedKey`, không đưa vào git/log/FE/Ocelot public headers. Các key tương ứng `Hl25Gateway__TenantId`, `Hl25Gateway__BackendAddress`, `Hl25Gateway__BackendTenantOrigin`, `Hl25Gateway__AllowedOrigins__0`, `Hl25Gateway__PermitLimit`.
4. Deploy ABP guard với `Hl25GatewayGuard__Enabled=false` trước. Khi sẵn sàng cutover, set Enabled=true, TenantId như mẫu ABP và secret giống YARP. Không sửa appsettings hiện có của user tự động.
5. YARP → ABP cần trusted ForwardedHeaders: `ReverseProxy:Enabled=true`, KnownProxies chỉ đúng IP gateway/loopback. Code hiện đã gọi UseForwardedHeaders trong Program.cs. Xác minh proxy trust và URL ảnh `/uploads/...` vẫn là HTTPS hostname tenant, không ra IP/port nội bộ.
6. IIS gateway App Pool riêng, No Managed Code, .NET Hosting Bundle phù hợp, Production; 1 worker, warmup, recycle ngoài khung chương trình; xem HTTP binding localhost/private. IIS hosting bỏ qua `Urls` khi bind site: phải cấu hình binding/firewall đúng, không tin chỉ appsettings. Tắt overlap nếu dùng một quota local và chấp nhận khoảng gián đoạn ngắn khi recycle. YARP web.config cho body tối đa20MB tương ứng ABP; service upload vẫn kiểm tra ảnh tối đa5MB.
7. Giữ hostname `duocpham-hoalinh.genora.vn`/TLS tại ingress, thêm route theo host+path. Giữ tuyến `/api/mini-app/hl25/admin/{everything}` về ABP với ưu tiên cao hơn và fallback các path khác (Admin Razor, Account, API Admin, uploads, static, SignalR...) về ABP như hiện có. Nếu chuyển cả hostname vào Ocelot, phải dựng/kiểm thử các tuyến fallback này trước; không chỉ thêm một route HL25 rồi chuyển DNS. Fallback phải giữ Host/X-Forwarded-Host của tenant, scheme HTTPS, cookie/Authorization và websocket; không qua YARP anonymous. Các cấu hình fallback phụ thuộc ingress/Ocelot version thực tế nên không tự ghi đè bằng mẫu đoán. Ocelot/YARP/ABP giữ nguyên body multipart/JSON, không retry writes, không cache429 và không rewrite Location/error tùy tiện. Bật CORS credentialed cho exact origins (khớp FE cũ); cookie/bearer không chuyển tiếp vào ABP trên tuyến anonymous này.
8. Chặn origin bypass bằng guard + network ACL cho port8868. Nếu origin vẫn public, admin/static assets được giữ theo site hiện có nhưng public HL25 targettenant phải403 khi gọi thẳng. Không chặn toàn ABP khiến module khác mất dịch vụ. Chặn đường HTTP public mang shared key; dùng TLS hoặc loopback/private đã bảo vệ.
9. Giữ baseURL `https://duocpham-hoalinh.genora.vn`; xác minh domain allowlist phía Zalo Mini App không bị ảnh hưởng. Asset URLs vẫn dùng hostname tenant ABP, không thêm upload/static vào quota API.
10. Kiểm tra ngoài API: đăng nhập Admin, menu, JS proxy, upload/static, callback và SignalR như trước. Warmup tuần tự trước mở traffic, kiểm tra config/campaign/templates/gifts + một hành trình dữ liệu test. Bật guard, kiểm tra direct403/gateway200 rồi mới mở lại. Nếu rollback, đóng chương trình/traffic trước, revert riêng route+guard theo kế hoạch; không mở origin không giới hạn trong khi đang bị tải cao.

## Kế hoạch kiểm thử tải trên server 6vCPU/16GB + SQL +16 IIS sites

Không thể kết luận cấu hình máy đủ tải chỉ từ core/RAM. Load generator chạy máy khác. Ưu tiên DB clone và site test cấu hình tương đương; seed kho quà giả, tắt tích hợp gửi quà/tin nhắn ra ngoài, không sửa dữ liệu người dùng thật. Nếu chỉ có production: cần lịch maintenance/phê duyệt riêng cho test ghi; cô lập dữ liệu/ảnh theo RUN_ID và kế hoạch thu hồi. Chưa thực hiện việc này.

Scripts tại `tests/load/hl25-gateway/`:

- `read-rps.js`: 1 iteration = **1 HTTP request** luân phiên4API cache; arrival rate do RPS điều khiển, redirect=0. Gate `HL25_LOAD_APPROVED=yes`, BASE_URL và CONFIRM_TARGET phải trùng. Mode `capacity` coi429 là lỗi; `overload` tách429 thành metric riêng. Test 500RPS đúng biên có thể có429 do segment/burst; báo cả offered/admitted/business-success RPS.
- `journey.js`: tạo ID người dùng mới theo RUN_ID + iteration, dùng template/campaign thật của DB test; đăng ký, profile read, upload, tạo thiệp, quay, chia sẻ, quay, histories. Mỗi hành trình thành công17request; **JOURNEYS_PER_SECOND không phải RPS**. Không tự retry writes. Bắt buộc thêm `HL25_ALLOW_WRITES=yes`, RUN_ID, IMAGE_FILE là ảnh PNG mẫu đại diện. API ghép/canvas FE không chạy trên server, upload mới được đo; Zalo decode-phone/OA thật cần UAT riêng bằng token hợp lệ và giới hạn nhà cung cấp.

Mẫu lệnh PowerShell (chỉ chạy sau khi đã chốt target test):

```powershell
$env:BASE_URL = 'https://REPLACE_WITH_APPROVED_TEST_GATEWAY'
$env:CONFIRM_TARGET = $env:BASE_URL
$env:HL25_LOAD_APPROVED = 'yes'
$env:RPS = '50'
$env:DURATION = '2m'
k6 run --summary-export artifacts/hl25-read-50.json tests/load/hl25-gateway/read-rps.js
```

Tăng 50→100→250 (15phút)→400→500 (15phút), sau đó soak250 ít nhất45phút qua TTL cache20phút/migrate30phút; mỗi mức có cooldown và kiểm tra ảnh hưởng15site còn lại. Đo hot/cold cache riêng, cold phải restart app test theo lịch, không flush production tùy tiện. Test overload700–1000RPS ngắn 30–60giây sau khi mức thấp ổn định, kiểm tra excess429 không làm SQL nhận quá tải. Theo dõi aborted/dropped_iterations, không gọi dropped là pass.

Hành trình: bắt đầu1journey/s, tăng theo số17request thành công, ví dụ14journey/s≈238RPS,29≈493RPS khi ổn định, **đo lại actual http_reqs**, không suy ra mục tiêu đạt từ rate cấu hình. Think time mặc định mô phỏng người dùng; preAllocatedVUs/maxVUs phải đủ (k6 dropped_iterations=0). Muốn đánh giá1000CCU: dùng thêm cohort1000VU với think time thực tế; không đánh đồng1000VU với1000request cùng một giây. Test burst1000request vào limiter500 phải chấp nhận phần429, không thể yêu cầu cả1000 thành công trong1s với quota này.

Acceptance đề xuất trước mở lại:

| Chỉ số | Baseline250RPS / headroom500RPS |
|---|---|
| Business success | >=99%; báo riêng429, HTTP5xx, HTTP200 success=false và timeout |
| p95 | GET config <=500ms; đăng ký/tạo record/quay <=2s; upload <=5s (ảnh đại diện); chốt lại theo SLA |
| p99 | API business <=5s; upload báo riêng theo kích thước |
| HTTP5xx | <0,1%; không có deadlock/duplicate đăng ký chưa xử lý hoặc SQL timeout |
| Dữ liệu | không trừ lượt âm, không vượt tồn kho, <=1 quà/người, tạo/chia sẻ không cấp lặp; đối soát DB |
| Tài nguyên | CPU tổng không duy trì>80%; còn RAM khả dụng>=2GB, không paging liên tục; theo dõi SQL waits/IO |
| Các site khác | không tăng lỗi503/timeout hoặc suy giảm SLA của dịch vụ đang chạy |
| Limiter | tổng routes cùng quota; read và write đều tham gia; excess429 hồi phục; directorigin403 khi bật guard |

Thu PerfMon mỗi5s: CPU từng w3wp/dotnet/sqlservr và tổng máy, availableMB, committedbytes/pages/sec, disk latency/queue/IOPS, NIC; .NET ThreadPoolqueue/GC/requests active; IIS queue/503/time-taken; SQL waits/blocking/deadlock/queryduration/connections. QueryStore/DMV chỉ đọc và hạn chế tần suất. Kiểm tra max server memory SQL cùng ngân sách RAM16site trước khi đổi, không gán con số đoán.

Stop test ngay nếu CPU>90% kéo dài60s, availableRAM<1GB/paging kéo dài, 5xx/timeout>1% trong1phút, SQLblocking kéo dài hoặc site khác mất SLA. Hạ tải/cooldown, giữ báo cáo nguyên trạng, không lặp stress cho đến khi tìm nguyên nhân.

Mẫu báo cáo từng run: timestamp/timezone/commit/build/config/migration đã áp, topology+worker count, target tenant, data/ảnh/RUN_ID, offered/admitted/successRPS, 429/5xx/business errors, p50/p95/p99 từngendpoint, dropped/iterations, CPU/RAM/IIS/SQL/IO, đối soát dữ liệu, tác độngsitekhác, kết luận PASS/FAIL và ngưỡng an toàn. Chỉ mở theo ngưỡng đã đo, chưa mặc định500.

## Rủi ro nguồn hiện có cần đo trước go-live

- `TenantAutoMigrateMiddleware` vẫn chạy MigrateAsync khi cache tenant-migrated hết30phút; không khóa chống các request cùng migrate. Đọc IsActive dùng MemoryCache GetOrCreateAsync cũng không khóa. Cache4API không bỏ qua middleware này. Chưa sửa trong task gateway; nên đưa migration ra deployment và kiểm tra trước release bằng task riêng.
- Đăng ký/spin có tính cạnh tranh và tác động SQL; index/cache không chứng minh mọi race đã giải quyết. Cần test đăng ký trùng ZaloUserId, share/spin lặp đồng thời và stock cuối cùng với SQL thật.
- Cache hotfix vẫn local mỗi process. Nhiều ABPworker cần cache/invalidation đồng bộ trước khi nhân instance.
- Source logging SQL có thể log nhiều ở Information; productionweb.config trong repo có cấu hình Development/stdout trước đó. Xác minh artifact thực tế, SQL parameter/PII logging và logvolume trước test. Không sửa appsettings của user trong phiên này.

## Verification

Lệnh đã chạy và kết quả cụ thể ghi tại `.claude/memory/notes/project/project_hl25_yarp_gateway_20260920.md`.
Gateway test sử dụng ASP.NET TestServer ở đầu vào và Kestrel loopback ở đích, không có SQL. Web guard tests dùng tenant giả lập. Không gọi đây là IIS/Ocelot production UAT hay chứng nhận500RPS.

Tài liệu chính thức: [YARP route rate-limiting](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/rate-limiting?view=aspnetcore-9.0), [ASP.NET Core limiter](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-9.0), [k6 constant arrival rate](https://grafana.com/docs/k6/latest/using-k6/scenarios/executors/constant-arrival-rate/).
