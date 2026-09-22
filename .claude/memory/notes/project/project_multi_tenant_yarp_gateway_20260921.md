# YARP nhiều tenant — 2026-09-21

> Cập nhật tên project ngày 21/09: dùng `Genora.MultiTenancy.Gateway` và `Genora.MultiTenancy.Gateway.Tests`. Các lệnh/tên cũ bên dưới là ghi nhận lịch sử; xem [note đổi tên](project_gateway_rename_20260921.md) và runbook hiện tại trước khi build/deploy.

## Yêu cầu / phạm vi
User yêu cầu mở rộng gateway riêng đã triển khai cho HL25 sang nhiều tenant ABP: HL25 tổng500request/s, HLG tổng300request/s, thêm tenant được qua cấu hình. Staging để test trước production. Giữ Host quản trị/DB/schema resolution thuộc ABP. Đây là task gateway, không tiếp tục corrective HLG design audit hoặc sửa cache/index nghiệp vụ.

Branch `hotfix/20260920`, HEAD `060df7e`. Không commit/push/deploy/cutover. Gateway/docs/tests phiên20/09 còn uncommitted, được mở rộng. Giữ nguyên appsettings Web/DbMigrator và Logs thay đổi của user. Workspace chỉ cho write Web, thay đổi sibling/root được thực hiện qua commands có escalation.

## Hostname/GUID thực tế
User cung cấp6public URLs. Read-only một GET `/api/abp/application-configuration?includeLocalizationResources=false` theo từng hostname 21/09, không in toàn configuration/secret:
- staging.genora.vn: Host, currentTenant.id=null, isAvailable=false.
- duocphamhoalinh-staging.genora.vn: `650ccd37-aeb4-63e7-bac7-3a23a72f9cbc`, Dược phẩm Hoa Linh, available=true.
- hoalinh-staging.genora.vn: `8cfc81eb-4693-2434-c5b2-3a21cccfe131`, Hoa Linh Miền Nam, available=true.
- production.genora.vn: Host, currentTenant.id=null, isAvailable=false.
- hoalinh.genora.vn: `27e348a9-036c-bef8-fa2b-3a22fc202c26`, Hoa Linh Miền Nam, available=true.
- HL25production duocpham-hoalinh.genora.vn GUID `209567fc-4850-44e9-11c8-3a23c58d15a4` dùng kết quả probe phiên20/09, không probe lại trong lần mở rộng này.
Không truy cập DB/connection strings. Tên HLG tenant khác tên module là dữ kiện, không suy ra DB name. Staging internal IIS origin/port vẫn chưa có; BackendAddress để trống bắt buộc điền, không suy diễn từ public URL/production8868. Production8868 được quan sát HTTP ở phiên trước; HTTPS thất bại, không tắt TLS validation.

## Implementation
- Giữ đường project/assembly `src/Genora.MultiTenancy.Hl25Gateway` (.NET9/YARP2.3.0), đổi Program gọi generic Add/UseTenantGateway. Không đưa ABP/SQL deps vào gateway.
- `TenantGatewayOptions.cs`: dictionary Tenants; Enabled, TenantId, PublicHosts, BackendAddress, BackendTenantOrigin, SharedKey, PermitLimit, AllowedOrigins, ApiProfiles, AdditionalRoutes. Tất cả profile/hostnamealias của mộtGUID cùng mộtquota; cácGUID độc lập; backend có thể khácnhau. Failstartup khi trùngGUID/host/key, unknownprofile, wildcard/admin/ambiguousroute, thiếu origin/key/positivequota. HTTPbackend chỉ loopback, ngoài loopback cầnHTTPS.
- `GatewayApiProfiles.cs`: explicit17method-pathHL25 (16paths) và24method-pathHLG đối chiếu controllers hiện tại. Không proxy Admin/Hub. AdditionalRoutes chỉ explicit MiniApp methods/paths; các API cầncookie/JWT cần thiết kế riêng vì forwarder bỏ auth của anonymousprofiles.
- `TenantGatewayExtensions.cs`: match exact incoming Host+path+method; shared slidingwindow1s/10segments, queue0, timer AutoReplenishment=true theo tenant. CORS riêngtenant, validpreflight không ănquota.429:RetryAfter/no-store, HL25errorstring/successfalse, HLGerrornumeric429/data=null. PinHost/XForwardedHost/Proto/tenantGUID; scrub overrides/cookie/auth/forwarded, fixedsecret+claimedTenant headers. Không retrybusinesswrites/cacheResponse.
- Regression phát hiện: mặc định YARP thêm OriginalHost(false) ở cuối transformlist, xóa Host đãpin khi bằng Hostincoming. Sửa **explicit RequestHeaderOriginalHost=false đứng trước pinHost**. Thử customtransform đơn lẻ không giải quyết do default vẫn cuối; bỏ thử nghiệm, code cuối dùng built-in transforms. Tests canonical+alias forwardedHost PASS.
- `Hl25GatewayExtensions.cs`: wrapper compatibility; legacy config còn hỗ trợ, không trộn configured legacy TenantId với genericmode. Legacy incomingHost giờ phải khớp originHost; Ocelotfragment cũ bổ sung Host transform.
- Web `Middlewares/TenantGatewayGuardMiddleware.cs`: opt-in genericguard, GUID+keydistinctpertenant/prefix, excludedAdminprefix. SauUseMultiTenancy, trướcTenantAutoMigrate. Require1key+1claimGUID khớp resolvedICurrentTenant; constanttimehashcompare; scrubheaders. Wrongtenant/host/out-of-scope markedrequest403; unrelatedunmarkedrequest giữhànhvi. Startupreject bật generic+legacyguardcùnglúc.
- `MultiTenancyWebModule.cs`: registerValidateOnStart genericguard, middleware trướclegacyguard. Không thay appsettings user, defaultguardoff.

## Hướng dẫn / test harness
`docs/tenant-gateway/README.md`: cấu hình theo tenant, topology/IIS/secret/proxytrust, publicHostquảntrị/fallbackAdmin/static/HLGSignalR, nhiềuinstance/recyclelimitations, thêmtenant/profile, chuyển/rollbacklegacy, stagesloadtest/acceptance/blockers.
6JSONexamples: gateway/ABPguard/Ocelotfragments choStaging vàProduction; điềnGUID/publicdomain/500+300, secrets/backendorigintrống. Cần Ocelot giữ **Host thực**; XForwardedHost không đủ. Không overwritecácroutekháchkhác/GlobalBaseUrl. VersionOcelot không cótrongrepo nên integration cầnUAT.
`docs/tenant-gateway/smoke-staging.ps1`: explicit-confirm, 4readGET staging (2tenantconfig+2businessread), chưa chạy; syntaxparsePASS.
`tests/load/tenant-gateway/read-rps.js`: 2scenario riêngtenant/constant-arrival, mỗiiteration1GET, gatedapproved+confirmedtargets, capacity/overload, metrics tagtenant/businessfail429/successLatency; all429 không pass do positiveSuccessCountthreshold. Không writeAPI. NodeVMtestskiểm tra guardrail/envelopes/quotaoptions/configexamples, không phải k6runtimebenchmark. ScriptHL25journeycũgiữnguyên.
`docs/hl25-gateway/README.md` thêm linkrunbookmới, phầncũgiữchếđộlegacy/historicalverification.

## Verification thực chạy
Từ repositoryroot:
1. `dotnet build src/Genora.MultiTenancy.Hl25Gateway/Genora.MultiTenancy.Hl25Gateway.csproj -c Release --no-restore --nologo` → PASS,0warnings/0errors.
2. `dotnet build src/Genora.MultiTenancy.Web/Genora.MultiTenancy.Web.csproj -c Hl25Performance --no-restore --nologo -clp:ErrorsOnly` → PASS,0warnings/0errors (incremental build). OutputriêngđểtránhDLLWebđangchạylock.
3. `dotnet test test/Genora.MultiTenancy.Hl25Gateway.Tests/Genora.MultiTenancy.Hl25Gateway.Tests.csproj -c Release --no-restore --nologo --logger "console;verbosity=minimal"` → **42passed/0failed** (19legacy+23multi). TestServer→Kestrel loopback, khôngSQL. TrướcfixHost cófailure, saufixchạyfullsuitePASS.
4. `dotnet test test/Genora.MultiTenancy.Web.Tests/Genora.MultiTenancy.Web.Tests.csproj -c Hl25Performance --filter FullyQualifiedName~Hl25 --no-restore --nologo --logger "console;verbosity=minimal" -clp:ErrorsOnly` → **40passed/0failed** (14existing+10legacyguard+16genericguard). Tenantmocks, khôngrealDBtenantUAT.
5. `node --test tests/load/hl25-gateway/scripts.test.cjs tests/load/tenant-gateway/scripts.test.cjs` → **12passed/0failed** (7existing+5new).
6. PowerShell Parser.ParseFile smoke-staging.ps1 → PASS, khônggọismokescript thật.
7. Git diff --check → PASS; reviewsource/scopeddiff và newfiles, khôngrevertunrelatedwork.
`Get-Command k6` không có executable. Không chạy k6, không realIIS/Ocelot/browserUAT/SQLloadtest/1000CCU hoặc benchmark500/300RPS. Runbook ghiBLOCKED các bước cầndeploy, không gọi build/test là chứng nhậncapacity.

## Không đổi / compatibility
Không migration/entity/DTO/businessendpoint/cache/index changes. Existing MiniApp payload và Admin permission/feature giữnguyên. Rate-limitedrequests thêmHTTP429; origin bị403 khi enableguard là chủđích bảo vệ. Không nâng package. Secrets chỉ dummytestconstants hoặc placeholderstrống; khôngsecretthậttrongrepo/memory.

## Còn lại / bước tiếp theo
1. Điền IISorigin staging thực, per-tenant secrets, exactCORS/knownproxies/bindings/TLS; xác minh saucloneTenantId vẫnđúng.
2. Deploy gateway+ABPguard tại staging, route đúng publicHost, bảo toànAdmin/static/Host/SignalR; khôngretry/fallback429sangABP. Sharedkeyrotation/cutover đồngbộ.
3. Smoke/lowquota2-3 tests để xác nhận ingress+429+recovery+directorigin403+tenantisolation; sauđó tải đo riêngvàkết hợp. TảiHL25writes dùngjourneycũ, HLGwriteflow chưa có loadscript riêng.
4. Hạnmức localmộtprocess;1worker, tránhoverlappedrecycle;HA/nhiềunodecầnsharedcounterchưalàm.500+300khôngchứngminh máy6vCPU16GB_SQL+16IISchịu800RPS.
5. Request-time auto-migrationcache30phút chưa khóa và cacheHL25mỗiprocess là rủi ro cũ chưa xửlý; cần đo/đưa migration ra deployment qua task riêng.
6. Chưa commit/push; không publishProduction hoặc stressrealtenanttrongphiênnày.
