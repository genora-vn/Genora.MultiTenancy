# HL25 Mini App read caching and indexes — 2026-09-20

## Scope and baseline
- Branch `hotfix/20260920`, starting HEAD `c76c54b`. This task is Hoa Linh 25 Năm (schema `hl25`), not HL Sales or HLG. Tenant DB name previously confirmed by user: DuocPhamHoaLinh; no live target connection/schema verification in this task.
- Existing uncommitted Web/DbMigrator appsettings changes and deleted/untracked Web logs were preserved. No commit, deployment, host restart or DB migration execution performed.
- Actual entity names: Hl25Participant / Hl25SpinLog / Hl25FrameCreation. Existing snapshot already had the participant filtered unique index and spin RewardStatus index; three additional composite indexes were missing.

## Implementation
- `Application/AppServices/Hl25/Hl25MiniAppCache.cs`: ABP singleton using process-local Microsoft MemoryCache; 20-minute absolute data lifetime. Keys are `hl25:config:{tenant}`, `hl25:frame-campaigns:{tenant}`, `hl25:frame-templates:{tenant}:{campaign-or-all}`, `hl25:gifts:{tenant}`. Null tenant is `host`.
- Cold fills double-check under bounded SemaphoreSlim locks (256 stripes keyed by tenant/catalog group). Warm reads bypass locks. Cache is limited to 4,096 entries; catalog version markers have sliding expiry. Template group invalidation covers every filter variant without enumerating campaign keys, including an old and new parent after moving a template. Invalidation waits for an in-flight fill and changes the version so it cannot republish stale data after invalidation completes.
- Cache stores only serialized public DTO snapshots; each call gets a new DTO. No tracked entities, personal profile, spin turns, WinRate or remaining gift quantities are cached. Relative asset paths are cached; gift URLs are expanded using the current HTTP request origin, template URLs use existing NormalizeThumb behavior per call.
- `MiniAppHl25Service`: cached GetConfigAsync/GetFrameCampaignsAsync/GetFrameTemplatesAsync/GetGiftsAsync, unchanged public signatures/routes/DTOs. Active-template counts now aggregate in the database rather than loading all active template entities to count in memory. Existing Active/Disabled filters and ordering semantics preserved.
- `Hl25MiniAppCacheInvalidator`: UoW OnCompleted callbacks; rollback does not invalidate committed data. Tenant ID is captured when scheduling. If no ambient UoW exists, invalidate immediately after the repository operation.
- AppConfig Admin update/initial auto-create and Gift/Campaign/Template Admin create/update/delete schedule invalidation. Frame changes invalidate campaigns/counts and all template variants. Inherited delete still performs its original permission/feature checks before invalidation.
- SpinAsync continues reading/checking/decrementing real stock inside its original transaction. When the last gift transitions to OutOfStock, invalidate the public gift catalog after commit so its Status is refreshed. Ordinary stock decrements do not invalidate the static catalog.
- No permissions, feature definitions, business turn/win rules, Mini App API contracts, packages, controller routes or appsettings were changed.

## Deployment topology limitation
This is the IMemoryCache option explicitly permitted by the task, with guarantees within ONE application process. It is not a distributed cache or distributed lock. Do not infer cross-instance invalidation or a single DB fill across multiple IIS workers/replicas. Before running multiple workers/replicas, replace this mechanism with a shared cache plus cross-node invalidation and locking. Restarting the process clears its cache. Out-of-band SQL writes are not invalidated automatically and expire with TTL.

## EF indexes and migration
- `EntityFrameworkCore/EntityFrameworkCore/MultiTenancyDbContextModelCreatingExtensionsHl25.cs` now explicitly specifies `[TenantId] IS NOT NULL AND [ZaloUserId] IS NOT NULL` on the existing unique participant index.
- Added `(TenantId, PhoneNumber, IsDeleted)` on AppHl25Participants, `(TenantId, ParticipantId, SpinTime)` on AppHl25SpinLogs, `(TenantId, ParticipantId, CreatedTime)` on AppHl25FrameCreations. Kept the existing RewardStatus index and other older indexes.
- EF generated `20260920100056_AddHl25MiniAppReadIndexes` + Designer + snapshot; snapshot diff is exactly the three indexes. Reviewed migration Up only creates these indexes. Guarded SQL skips databases without each HL25 table and indexes already present with the same name, for the project's all-tenant/mixed-module migrator. Down only removes these three named indexes if present. No tables/columns/data are dropped by Up.
- Idempotent SQL: `.claude/docs/hl25_read_indexes_20260920.sql`, generated from `20260918093000_EnsureHl25ParticipantAgeGroup` to the new migration. This incremental SQL assumes earlier migrations/history already exist. Prefer the normal DbMigrator pipeline for pending earlier migrations.
- Neither migration nor SQL was applied. Physical target indexes (including any manually created same-name indexes) still need deployment verification; name guards do not repair an existing differently-defined index. Do not edit past released migrations.

## Executed verification
All commands launched from `src/Genora.MultiTenancy.Web`; build configuration `Hl25Performance` keeps outputs separate from normal Debug files.

```powershell
dotnet build Genora.MultiTenancy.Web.csproj -c Hl25Performance --no-restore -v quiet
dotnet test ../../test/Genora.MultiTenancy.Application.Tests/Genora.MultiTenancy.Application.Tests.csproj -c Hl25Performance --no-restore --filter FullyQualifiedName~Hl25
dotnet test ../../test/Genora.MultiTenancy.Domain.Tests/Genora.MultiTenancy.Domain.Tests.csproj -c Hl25Performance --no-build --no-restore --filter FullyQualifiedName~Hl25
dotnet test ../../test/Genora.MultiTenancy.EntityFrameworkCore.Tests/Genora.MultiTenancy.EntityFrameworkCore.Tests.csproj -c Hl25Performance --no-restore --filter FullyQualifiedName~Hl25ReadIndexTests
dotnet test ../../test/Genora.MultiTenancy.Web.Tests/Genora.MultiTenancy.Web.Tests.csproj -c Hl25Performance --no-restore --filter FullyQualifiedName~Hl25
node --test ../../test/hl25-ui-regressions.cjs
dotnet ef migrations has-pending-model-changes --project ../Genora.MultiTenancy.EntityFrameworkCore --startup-project . --configuration Hl25Performance --no-build
dotnet ef migrations script 20260918093000_EnsureHl25ParticipantAgeGroup 20260920100056_AddHl25MiniAppReadIndexes --idempotent --project ../Genora.MultiTenancy.EntityFrameworkCore --startup-project . --configuration Hl25Performance --no-build --output ../../.claude/docs/hl25_read_indexes_20260920.sql
```

- Web/dependency build PASS, 0 errors (baseline build has existing warnings; final incremental build 0 warnings).
- Application HL25: **43 passed / 0 failed** (17 existing + 26 new). Domain HL25: **18 passed / 0 failed**. EF SQL Server model/migration: **6 passed / 0 failed**, no DB connection. Web HL25: **14 passed / 0 failed**. Total .NET: **81 passed**. JS: **3 passed / 0 failed**.
- New tests: 7,000 simultaneous helper calls per catalog group, both cold and after simulated expiry, one factory call per wave; tenant/host isolation; all template filter variants; fill/invalidation race; failed-load recovery; DTO copy isolation; commit/rollback behavior; Admin CRUD invalidation; config auto-create; template reparenting; unauthorized/feature-disabled writes; per-request gift URLs; live stock during Spin and refresh when out of stock. Existing constructor-based tests adjusted for the cache dependency.
- `has-pending-model-changes`: no changes since last migration. SQL generated and reviewed; git diff check passed for changed source/tests.
- Test details/logs are temporary under `%TEMP%/hl25-performance-results` and `%TEMP%/hl25-*-*.log`, not committed.
- Browser/API UAT and real SQL-backed load test: **NOT RUN**. No claim that 1,000 CCU or 7,000 real HTTP users has been benchmarked. Concurrency tests use the actual cache helper with controlled factories/in-memory test repositories.

## Deployment / remaining verification
1. Deploy/rebuild, run the normal DbMigrator against the intended tenant databases, verify migration history and all five requested index definitions in DuocPhamHoaLinh. No new database columns required.
2. Use one application process for this cache implementation. Smoke-test the four GETs with Host and two tenant contexts, then Admin save/create/delete and template move between campaigns; verify committed data changes immediately and rollback does not expose changes.
3. Check response image origins, gift Status after last stock consumption, and that Spin uses live inventory. Use test participants/gifts for write tests.
4. Run staged HTTP load testing at the intended deployment topology up to 1,000 CCU; measure error rate, p95/p99, CPU/memory, SQL queries/locks/connection pool and actual query plans. Cache helper concurrency tests cannot establish whole-system capacity.
