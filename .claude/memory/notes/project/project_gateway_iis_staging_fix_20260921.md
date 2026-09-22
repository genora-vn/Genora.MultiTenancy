# IIS ARR / YARP / ABP staging audit — 2026-09-21

## Context thực tế
Branch hotfix/20260920, HEAD e4ec431. User đã commit gateway/rename và thêm các sửa proxy/tenant resolver: 8e8d814, 7ef7155, e4ec431. Đã đọc source hiện tại và giữ các thay đổi đó. Ban đầu Git chỉ còn logs của user thay đổi; không sửa logs, connection strings hoặc tenant routing nghiệp vụ.

User cung cấp log OptionsValidationException của TenantGatewayGuard, cấu hình3site, và hai ảnh `C:/Users/DPC/Downloads/check-api-proxy.png`, `gateway-site.png`; đã xem cả hai ảnh. Không copy secret/password/API key từ tin nhắn vào source, docs hoặc note này.

Binding user mô tả:
- Genora.Ingress.Staging: IP103.157.218.187, HTTPS443 với2hostname tenant staging; chưa liệt kê staging.genora.vn ở ingress.
- Gateway/ABP có2dòng Port viết thành địa chỉ IP, nên chưa xác định được port thật từ2dòng này.
- ABP có thêm All Unassigned:8868 và All Unassigned:443; dòng443 chưa rõ hostname.
- ASPNETCORE_ENVIRONMENT=Staging được user xác nhận; chưa có App Pool/worker/env override chi tiết.
- Config rewrite/backend user dùng127.0.0.1:5088 cho YARP và127.0.0.1:8868 cho ABP. Đây là topology3site cùng máy; không còn thiếu đích dự kiến, nhưng cần xác minh binding thực tế nhận loopback.

## Nguyên nhân / phát hiện
1. Hai tenant dùng cùng SharedKey. Validator ABP và gateway yêu cầu key khác nhau giữa tenants. Key phải giống giữa Gateway/ABP của cùng một tenant; không nới điều kiện.
2. Bản ABP mục3 bật cả legacy Hl25GatewayGuard và TenantGatewayGuard. Phải legacy=false, generic=true. Bản đầu user dán legacy=false vẫn lỗi do key trùng.
3. Program gateway hiện chỉ CreateBuilder/add services, không AddJsonFile gateway.Staging.json. File appsettings.Staging.json user dán thiếu SharedKey/AllowedOrigins; file custom có đủ nhưng không được tự nạp.
4. Ảnh gateway còn binary Hl25Gateway dù repository đã đổi tênGateway. Cần fresh publish đầy đủ và web.config đúngassembly/environment.
5. Ảnh curl gọi hoalinh-staging.genora.vn/api/mini-app/hl25/config: sai cặp host/profile. Rule cũ rơi vàoHLGfallback→ABP8868, không điYARP5088. ARR/3.0 +HTTP200 không chứng minhquota. Không biết businessbody từ ảnhheaders.
6. JSON ABP trong tin nhắn thiếu dấu phẩy giữa App:SelfUrl và AppUrl. Nếu file thật giống hệt thì sẽ lỗiJSON sớm hơn; không khẳng định đây là chính file đã được process nạp.
7. Cần preserveHostHeader=true ở ARR, allowlist2forwardedservervariables, bindingGateway nhậnloopback5088 vàABP8868, tránh publicbypass; AppPool/env/CORS/IISnative vẫn cầnUAT. Hostquảntrị có thể điingresshoặcABPtrực tiếp, nhưng phải biếtbindingexact và tránhtrùng/catchall vôý.

## Thay đổi source và tài liệu
- Web TenantGatewayGuardOptions.GetValidationErrors + TenantGatewayGuardOptionsValidator, đăng kýIValidateOptions/ValidateOnStart trongMultiTenancyWebModule. Lỗi nêu rõ field/tenant (keytrùng/keythiếu/prefixsai/mixedguards), không insecret; IsValid vẫn có đểtươngthíchtests. Không thay guardruntime hoặc authorization.
- Gateway TenantGatewayOptions: tách lỗiSharedKeythiếu/khônghợp lệ vớiSharedKeytrùng, nêu pathcấuhìnhđúngvànhắc customgatewayJSONkhôngautoload. Các yêu cầusecuritygiữnguyên.
- Gateway web.config thêm httpErrors existingResponse=PassThrough để giữstatus/body403/429 khihostIIS.
- `docs/tenant-gateway/iis/ingress.Staging.web.config` vàProduction: đúnghost+profile→YARP, HL25Admin→ABP, wronghostprotectedAPI→404 trướcfallthrough, hostknownAdmin/static/SignalR→ABP, hostlạ→404. Mẫu dành riêngsiteIngress; khôngchạyIISconfig hoặc sửaweb.config trênserver.
- `New-DeploymentConfig.ps1`: sinhcấuhìnhGatewayappsettings.<env>.json, web.configđúngenv/assembly, ABPguardfragment, ingressweb.config; RNG32bytes keyriêngtenant vàghépđúng2bên. Chỉghioutputdir mới, khôngoverwrite/deploy/restart; khôngecho key.
- `Get-IisInventory.ps1`: chỉđọcbindings/pool/worker/envtrongwebconfig/ARRflags, khôngdumpappsettingssecrets. Chưa chạytrênIISserver.
- `docs/tenant-gateway/iis/README.md`: hướngdẫnsửalỗitheothứtự/binding3site/tạoartifact/curltừngchặng/quota/productiongate. Runbookchungtrỏtài liệunàyvànóirõdeploymenthiệntạilàARR, khôngcầnthêmOcelot.
- NguồnMicrosoftđãđốichiếu: URLRewrite+ARR, allowedservervariables, defaultASP.NETconfiguration vàhostnamepreservation. Khôngdùngbàiviếtbênthứbacủa searchlàm nguồnthựcthi.

## Verification thực chạy
1. `dotnet test test/Genora.MultiTenancy.Gateway.Tests/Genora.MultiTenancy.Gateway.Tests.csproj -c Release --no-restore --nologo --logger "console;verbosity=minimal"`: **60passed/0failed**.42existing+18new (1validationtest+17casesmẫuIIS). TestsIIS đọcXML/evaluateregex/order; khôngphảitestmoduleARRnative.
2. `dotnet test test/Genora.MultiTenancy.Web.Tests/Genora.MultiTenancy.Web.Tests.csproj -c Hl25Performance --filter FullyQualifiedName~Hl25 --no-restore --nologo --logger "console;verbosity=minimal" -clp:ErrorsOnly`: **42passed/0failed** (40existing+2validationregressions).
3. `dotnet build src/Genora.MultiTenancy.Web/Genora.MultiTenancy.Web.csproj -c Hl25Performance --no-restore --nologo -clp:ErrorsOnly`: PASS,0warnings/0errors (incremental).
4. `dotnet publish src/Genora.MultiTenancy.Gateway/Genora.MultiTenancy.Gateway.csproj -c Release --no-restore --nologo -o artifacts/gateway-iis-audit-20260921`: PASS. XMLpublish xácnhậnhttpErrorsPassThrough; khôngdeploy.
5. ChạyNew-DeploymentConfig bằngWindowsPowerShell5 choStagingvàProduction, checkGUID/keykhớp, keys>=32khácnhau, legacyfalse/generictrue, env/DLLđúng, backendloopback, origincó, vàrefuseoverwrite: **PASS cả2môi trường**. Keytestkhôngin;outputartifacts testđượcxóasaukhikiểmtrascopedpathbằngnativePowerShell.
6. ParseGet-IisInventory.ps1: PASS; chưachạyIISruntime. GitdiffcheckPASS. KhôngsửaJSnênkhôngchạylại12JS tests cũ.

## Giới hạn / bàn giao
Khôngmigration/DBbusinesschange. KhôngapplyIISconfig, deploystaging/production, chạyremoteHTTP/load/browserUAT. Khôngxácnhậnproduction-ready. Usercầnreview/điềubindingđúngvàAppPool, freshpublish, generatecấuhìnhtrênmáytriểnkhai, mergesafefragmentvàtestchuỗi.
Acceptance: gatewayhealth200; đúng2host/profile điYARP→ABP trảbusinesssuccess; directoriginthiếukey403; wronghost404; vượtquota429/RetryAfterhồiphục; HostAdmin/static/CORS/HLGSignalRgiữnguyên; measuredloadđạtSLA trướcproduction. Quota1process500/300; khôngcoitestunitlàchứngnhận1000CCU.
Secrets đã chia sẻ cầnrotation cókiểmsoát; khôngtựđổiStringEncryption/DataProtection làmhỏngdữliệu/cookie. Sourcegiữnguyêncácsửacủauserởtenantresolversvàforwardedheaders. Migrationrequest-time/cachelocal làrủirohiệnsẵnthamchiếurunbooktrước.
