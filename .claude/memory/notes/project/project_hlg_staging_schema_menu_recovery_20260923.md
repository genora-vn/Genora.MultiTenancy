# HLG staging schema and menu recovery — 2026-09-23

Branch `feature/dev-hoalinh-gamification`, HEAD at start `22a126b`. Read `LOAD_CONTEXT.md`, `ACTIVE_CONTEXT.md`, `PROJECT_STATE.md`, `TASK_LOG.md`, `MEMORY.md`, rules, HLG architecture and prior host-seed/migration notes. Preserve unrelated Web log deletion/new log. No appsettings changes in this task.

## Verified root causes

- `MultiTenancyDbContextFactory` reads `DbMigrator/appsettings.json` Default. That points to host `GenoraMultiTenancy`. Host HLG schema existed with **0 objects**, yet history had five baseline HLG migrations. Consequently EF skipped table creation and `AddHlgDesignContent` failed at `ALTER TABLE HLG.AppHlgUserProfiles` (SQL 4902). Prior 2026-09-18 memory identified this drift; this task confirmed it again with read-only live SQL.
- Hoa Linh tenant `HoaLinhMienNam` had 13 baseline HLG tables and five history rows before this task. `Hlg.Management` is enabled for tenant `Hoa Linh Miền Nam`; HLG root role grants for `admin` exist in host/tenant. Plain EF CLI targets host, so the tenant needed a separate explicit connection.
- Source menu/pages exist in current branch; Git shows `b507697` introduced permissions before `20b9e1c` introduced admin Razor menu/pages. Both live staging hosts serve known HL25 JS (200) but return HLG admin/rewards JS (404); host HLG routes 404. Deployed Web is older/incomplete. No source permission change is warranted. Host and tenant need a current Web publish for menu to appear.

## Changes and execution

- Added guarded host repair SQL `docs/HLG_STAGING_SCHEMA_REPAIR_20260923.sql`, dry-run by default. Checks expected DB, empty HLG object set and exact five history rows; transactional backup to `dbo.HlgMigrationHistoryRepair_20260923`, then removes those five rows only. Dry-run returned exactly five rows. Apply branch executed once on host: backupRows=5, remainingHlgHistory=0 before replay.
- Added preflight SQL to `20260919112304_AddHlgDesignContent.Up` to fail clearly when HLG baseline tables are missing; partial drift is never ignored.
- Built EF project (0 errors); `dotnet ef database update --no-build` successfully replayed five baseline HLG migrations and applied design-content migration on host. Read-only host post-check: 17 HLG tables, 6 HLG history rows, 5 backup rows, PharmacyCode column present.
- Applied `AddHlgDesignContent` to `HoaLinhMienNam` via `dotnet ef database update --no-build --connection <tenant Default connection>` after verifying target name and baseline. Tenant read-only post-check: 17 HLG tables, 6 HLG migration rows, PharmacyCode present. No history reset on tenant.
- The task created a host history-row backup table, not a full SQL database backup; the server's normal backup policy was not inspected.
- Fixed a stale `GameDto` substitute in `HlgDesignWebTests` to use the current `GameDetailDto` return contract; 17 HLG Web tests now pass. Initial default Debug Web build hit locked HL25 JS output; isolated `HlgMenuValidation` Web build passed. Plain `dotnet ef database update` implicit build is blocked only in this sandbox by access to user NuGet.Config; explicit EF build and `--no-build` update worked.
- Published Web Release locally to ignored `artifacts/hlg-web-staging-20260923`; verified Web DLL, web.config and both HLG JS files in the artifact. This is deploy-ready output, not a live IIS deployment. Preserve/overlay each site's staging appsettings when deploying; do not ship local settings blindly.
- Web build `-c HlgMenuValidation --no-restore` passed (0 errors). Previous default Debug build failed on locked HL25 JS output; custom config avoids that local file lock. See `docs/HLG_STAGING_RECOVERY_20260923.md` for deployment and smoke steps.

## Remaining

- Publish current Web package to both staging IIS sites; verify HLG JS 200, authenticated HLG pages/menu on host and tenant, and disabled-feature tenant denial. No IIS deployment or signed-in UAT was possible from workspace; live sites still serve old UI at handoff.
- HLG Content permission is newer than the 18 permissions shown in supplied role screenshots; grant it separately if CMS Content page is needed. Other five menu groups have granted roots.
