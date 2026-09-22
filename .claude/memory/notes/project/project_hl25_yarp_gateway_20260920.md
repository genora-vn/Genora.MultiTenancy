# HL25 YARP gateway — 2026-09-20

> Cập nhật tên project ngày 21/09: dùng `Genora.MultiTenancy.Gateway` và `Genora.MultiTenancy.Gateway.Tests`. Các lệnh/tên cũ bên dưới là ghi nhận lịch sử; xem [note đổi tên](project_gateway_rename_20260921.md) và runbook hiện tại trước khi build/deploy.

## Agreed scope and environment

- Branch `hotfix/20260920`, baseline HEAD `060df7e` (previous cache/index hotfix committed). These gateway changes are not committed or deployed.
- HL25 only, not HLG/Sales. Customer baseline 250 RPS; desired headroom and shared public API quota 500 RPS for the tenant using DuocPhamHoaLinh (schema hl25, per user).
- User reports 6 vCPU / 16 GB RAM, SQL Server and 16 IIS sites on the SAME machine. ABP has its own site; actual worker counts are unverified. 1000 CCU is not equivalent to 1000 RPS and has not been measured.
- User chose a separate YARP gateway while reusing existing gateway infrastructure. Supplied ReRoutes configuration belongs to Ocelot serving other projects. Do not replace those routes or its Baygolf BaseUrl.
- User confirmed the public URL again as **https://duocpham-hoalinh.genora.vn**. Keep this hostname and Mini App base URL. Route public HL25 through YARP; keep Admin, Account, assets and other paths routed to ABP. Do not introduce api-hl25.genora.vn or move Mini App onto api.baygolf.vn.

## Observed endpoint checks (not load tests)

- One GET to the public ABP application-configuration endpoint returned tenant **209567fc-4850-44e9-11c8-3a23c58d15a4**, name **Dược phẩm Hoa Linh**, isAvailable=true. Only currentTenant was printed; no settings/secrets or direct SQL access.
- User supplied origin `https://103.157.218.174:8868/`. A GET with tenant Host failed TLS: `Cannot determine the frame size or a corrupted frame was received`.
- An HTTP GET on the same IP:8868, with Host `duocpham-hoalinh.genora.vn`, returned 200 without redirect and the same tenant GUID. This port was observed serving HTTP. Certificate validation was never disabled.
- Sample origin `http://127.0.0.1:8868/` is conditional on YARP running on the ABP machine and a verified loopback binding. That binding has NOT been verified on production. Remote origins must use valid HTTPS; code rejects non-loopback HTTP to protect the shared key. Do not use the public hostname as the backend after cutover, which could create a proxy loop.

## Implementation

- `src/Genora.MultiTenancy.Hl25Gateway/`: standalone .NET9 / Yarp.ReverseProxy2.3.0 without ABP/SQL dependencies. Gateway and test project added to solution; original AnyCPU configuration preserved.
- `Hl25GatewayExtensions.cs`: 16 public paths / 17 method-path combinations inventoried from HoaLinh25MiniAppController. One named policy and constant configured tenant partition across ALL routes. Sliding window 1 second / 10 segments; default500, configurable1..500; queue0. Configuration changes require gateway restart.
- Initial gateway tests: 17 passed /1 failed during quota recovery. GetSlidingWindowLimiter disables native AutoReplenishment; its100ms partition heartbeat can skip100ms segments due to timer jitter. Fixed using RateLimitPartition.Get with a native auto-replenishing SlidingWindowRateLimiter (one partition, one timer). Final19/19 passed.
- Excess requests receive HTTP429, Retry-After, no-store and HL25 envelope. Exact-origin CORS with credentials and exposed Retry-After. Preflight ends at gateway without business quota/backend access. Liveness is independent of SQL. Admin/unknown/unrelated routes are not proxied. No write retries or gateway response cache; JSON/multipart/status bodies forwarded.
- Fixed Host/X-Forwarded-Host/Proto from configured tenant origin; server GUID overwrites tenant header/query. Remove client __tenant, Forwarded, Authorization and Cookie on this anonymous Mini App surface. Zalo access tokens in request bodies remain intact.
- `Web/Middlewares/Hl25GatewayGuardMiddleware.cs` and 8 added WebModule lines: opt-in guard after tenant resolution, before TenantAutoMigrateMiddleware. Target tenant public HL25 requires a shared key; marked requests resolving to another tenant/host return403. Constant-time hash comparison; remove key before business middleware. Other modules/tenants and Admin remain unchanged. Default disabled; enable at cutover with matching GUID/key.
- `docs/hl25-gateway/`: deployment runbook, one Ocelot route fragment, gateway/ABP configuration examples. Empty secrets supplied outside Git. Documents same-host Admin/static fallback, origin HTTP/TLS, trusted proxies, one worker/recycle caveat, cutover/rollback, load plan and report criteria.
- `tests/load/hl25-gateway/`: gated k6 read script (one HTTP per iteration) and journey script (17 HTTP per successful journey plus one setup request). Explicit approved target, additional write gate, unique RUN_ID and PNG fixture. No automatic write retry. Node VM tests exercise gates/accounting/sequencing, not k6 runtime.
- No new migration, domain/DTO/controller/business API change or modification to the previous cache/index hotfix. Existing user edits to Web/DbMigrator appsettings and logs preserved. No commit, push, deployment or live restart performed.

## Actual verification commands (repository root)

```text
dotnet build src/Genora.MultiTenancy.Hl25Gateway/Genora.MultiTenancy.Hl25Gateway.csproj -c Release --no-restore --nologo -v:minimal
dotnet build src/Genora.MultiTenancy.Web/Genora.MultiTenancy.Web.csproj -c Hl25Performance --no-restore --nologo -v:quiet -clp:ErrorsOnly
dotnet test test/Genora.MultiTenancy.Hl25Gateway.Tests/Genora.MultiTenancy.Hl25Gateway.Tests.csproj -c Release --no-restore --nologo --logger "console;verbosity=minimal"
dotnet test test/Genora.MultiTenancy.Web.Tests/Genora.MultiTenancy.Web.Tests.csproj -c Hl25Performance --filter FullyQualifiedName~Hl25 --nologo --logger "console;verbosity=minimal" --no-restore
node --test tests/load/hl25-gateway/scripts.test.cjs
```

- Gateway and Web build PASS. Final incremental commands:0 warnings/0 errors; initial Web build had existing unrelated warnings.
- Gateway: **19 passed,0 failed**, covering shared budget, recovery, tenant spoofing, response429, CORS including credentialed preflight/429, multipart/JSON,503 without write retry, excluded routes/health and invalid settings including public HTTP origin rejection.
- Web HL25: **24 passed,0 failed** (14 existing +10 guard cases including valid/invalid key, wrong/host tenant, duplicate key, disabled guard and unrelated/Admin paths).
- Node script tests: **7 passed,0 failed**. Both k6 scripts passed syntax checks. **k6 is absent from PATH; no actual k6 load run.**
- `git diff --check` PASS. Solution change contains only14 added lines for the2 projects/config/nesting.
- Gateway test input is ASP.NET TestServer; backend is Kestrel loopback stub. Web guard tests mock ICurrentTenant. Not SQL capacity or IIS/Ocelot browser UAT.
- The81.NET/3JS results of the cache/index task are historical, not added to this turn's counts.

## Remaining work and limits

1. Existing Ocelot source/version and deployed ingress configuration are unavailable. Review same-host routing and preserve Admin/static/SignalR/Account fallback before cutover. One HL25 route fragment alone is insufficient to move an entire hostname.
2. Confirm YARP placement, loopback8868 or HTTPS origin, deployment secret, trusted proxy addresses and one-worker configuration. No guard activation, network ACL changes or traffic cutover performed.
3. Run staged SQL-backed tests on an approved clone/test environment; real registration/upload/share/spin concurrency, stock and Zalo integrations remain unverified. Runbook covers250→500RPS, overload,45minute soak, metrics/stop criteria and report. Shared production server must not be stressed without its own agreed window.
4. Existing TenantAutoMigrateMiddleware still calls MigrateAsync on30minute cache miss without stampede locking; tenant-active GetOrCreateAsync also lacks single-flight. Four cached API handlers do not bypass these middleware costs. Not changed here; propose moving migrations out of request processing in a separate task.
5. Gateway quota and existing ABP cache are process-local. Multiple workers/replicas or overlapped recycle can multiply quota; use shared mechanisms before scaling out. Segmented windows are not a strict cap for every arbitrary1000ms interval.500RPS admission is not proof of500 successful business requests/s or1000CCU.
6. Admin export is outside this public limiter/guard, with existing authorization unchanged. Verify Admin permissions and routing separately.

Full operational guide: `docs/hl25-gateway/README.md`.
