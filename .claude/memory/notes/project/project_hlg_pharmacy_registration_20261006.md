# HLG pharmacy registration — 2026-10-06

## Scope / decisions

- Task implemented on `feature/dev-hoalinh-gamification`, starting HEAD `6fe42b8`. User confirms HLG and Sales share the HoaLinhMienNam tenant/database and AppCustomers. Earlier architecture assumption that Sales points cannot touch HLG DB is obsolete.
- User explicitly confirmed: preserve existing Sales CustomerCode, store HLG selected DMS branch separately. Implementation preserves all nonblank existing codes, including HLGKH codes; new owner receives selected DMS code, new employee receives unique HLGKH sequential code.
- Limit is **five total accounts including owner** (owner + four employees), by normalized owner phone across all branches. Existing Sales-only owner occupies one reserved place. IsActive=false linked customers still count; soft-deleted Customer/Profile rows do not count. Legacy null links are not guessed/backfilled.

## Implementation

- New GET `/api/mini-app/hlg/auth/{phone}?pharmaPhone={ownerPhone}`. `phone` is registrant, owner phone defaults to phone. DMS queried using owner phone through existing `IHlApiClientService`; all valid branches returned, including isGkhl=false. No Customer/Profile writes by preflight (DMS client logging unchanged).
- POST `/api/mini-app/hlg/customer/upsert` adds PharmaPhone + required selected CustomerCode. Revalidates DMS ownership/branch, tenant-local owner existence and account limit. Three requested error messages are exact constants in `HlgRegistrationRules`.
- `HlgProfileAppService.Registration.cs` owns registration flow; Application has no EF calls. `IHlgRegistrationLock` / EF `HlgRegistrationLock` takes transaction-owned SQL application lock per DB + tenant/Host. DMS call happens before lock; ABP transactional requiresNew UOW wraps local recheck and both writes. Tenant-wide lock protects HLGKH allocation across different pharmacies, releases on commit/rollback.
- Host null tenant explicitly scoped, not implicit ABP Host visibility. Phone aliases 0/84/+84 supported; duplicates fail friendly. Deleted phone/code reservations produce friendly conflict; generated codes include deleted records.
- PUT profile cannot complete initial registration or change a linked phone. Legacy profile creation during reads coordinates with the same lock, avoiding concurrent GET/POST duplicate profiles.
- `HlgUserProfile.PharmaPhone` nullable20 and `.DmsCustomerCode` nullable50; existing PharmacyCode/VgaCode preserved. Nonunique TenantId+PharmaPhone index. No Customer schema or index changes. Actual CustomerCode unique index filter is `[IsActive] = 1 AND [CustomerCode] IS NOT NULL`.
- GamificationUserDto adds PharmaPhone, CustomerCode (local identity), DmsCustomerCode (selected HLG branch). Request customerCode must be DMS branch even when response local code is HLGKH/Sales code.
- Existing HLG envelope retained, business errors HTTP200 with numeric `body.error` (404/403/409/400/503), no `success`. FE must inspect body.error.

## Verification actually run

- Release solution build PASS, 0 errors / 193 warnings in the final build (existing warnings outside registration changes). Baseline had an unrelated compile failure in HlgRankingShareImageTests due to missing IConfiguration constructor argument; added ConfigurationBuilder().Build() to unblock suite.
- HLG Application **100/100 PASS**, including 30 new registration tests. Deleted HLG profiles also return a restore-required error instead of violating the unique CustomerId link.
- HLG Web **26/26 PASS**, including 7 new HTTP TestServer route/query/JSON/envelope tests.
- EF model/migration **2/2 PASS** and real SQL Server LocalDB **5/5 PASS**; all 7 passed together. Real ABP service/repositories/UOW/SQL lock, DMS stubbed.
- SQL scenarios: owner + 12 concurrent employee requests yields exactly 4 successes/8 quota rejections for both Host and tenant; 8 duplicate owner requests; simultaneous two pharmacies with unique HLGKH codes; failed Profile INSERT rolls back Customer INSERT and releases lock; soft-deleted code/phone protection; concurrent legacy GET and registration produces one profile and preserves existing Sales code and both balances.
- SQL fixture builds only production-model Customer/Profile tables and FK dependencies, applies actual new migration operations to pre-change profile schema. Full-model EnsureCreated initially hit pre-existing Salon multiple cascade paths; fixture was scoped to relevant tables, no Salon code changed.
- All SQL test databases were unique `(localdb)\MSSQLLocalDB` `HlgRegistrationTest_<guid>` databases, dropped by fixture disposal. No appsettings used by SQL tests, no staging/production migration or registration, no real DMS call.
- EF `has-pending-model-changes`: clean. `git diff --check` for src/test passed. Logs in ignored `artifacts/hlg-pharmacy-registration-20261006/`.

## Migration / FE handoff

- New migration `20261006054856_AddHlgPharmacyRegistration`: only two nullable HLG columns and one index, no seed/data rewrite. Designer and snapshot included.
- [API/cURL/rollout guide](../../../../docs/HLG_PHARMACY_REGISTRATION_API_20261006.md), [incremental idempotent SQL](../../../../docs/HLG_PHARMACY_REGISTRATION_20261006.sql).
- **NOT APPLIED to application local DB/staging/production**. SQL assumes valid baseline through `20261001040131_AddHlgRankingResultSnapshots`; review/apply correct Host/tenant DB before serving new code. No deploy, merge or commit performed.
- Coordinate FE rollout: upsert without selected CustomerCode now fails400; use existing customer/by-phone for login/read. Incomplete profiles must register through upsert. FE integrates DMS real-data UAT after migration.
- No gateway, TenantAutoMigrateMiddleware, Sales auth, HL25 wheel, seed or points logic edited. Existing pre-task appsettings changes and deleted/new log files preserved. Do not stage those unrelated changes indiscriminately.

## Follow-up constraints

- Do not rename PharmacyCode or update existing Sales identities when changing HLG branch.
- New registration gate covers all customerType values; pharmacy/consumer selection alone cannot bypass DMS membership.
- Registration still uses existing phone/Zalo caller identity convention; this task does not redesign authentication. Checking DMS existence is a business eligibility check.
- `BonusPoint` remains shared between Sales/HLG; new upsert preserves it. Existing ranking-export reset/loyalty ledger policies are separate work, not modified here.
- Admin soft-delete/restore and changes to shared customer phones/codes outside HLG registration remain existing operator workflows; the SQL lock coordinates HLG registration and legacy HLG profile creation, not every Sales/admin write.
