# HLG staging recovery — 2026-09-23

## Database finding and repair

`dotnet ef database update` in `src/Genora.MultiTenancy.EntityFrameworkCore` uses the **Default** connection in `src/Genora.MultiTenancy.DbMigrator/appsettings.json` through `MultiTenancyDbContextFactory`; on this staging checkout it targets host DB `GenoraMultiTenancy`, not the Hoa Linh tenant database. Read-only inspection found schema `HLG` with **zero objects**, although the first five HLG migrations were marked applied. This is schema/history drift, not an error in the six new nullable columns.

The guarded repair in `docs/HLG_STAGING_SCHEMA_REPAIR_20260923.sql` checks the exact database, empty HLG schema and exact five history rows. It backs the rows up in `dbo.HlgMigrationHistoryRepair_20260923` and removes only those rows so EF can replay their original DDL. It refuses partial schema drift or a second run. On 2026-09-23 the dry-run passed; the apply branch ran once on the host DB. `dotnet ef database update --no-build` then applied the five baseline HLG migrations and `20260919112304_AddHlgDesignContent` successfully. Read-only verification: **17 HLG tables, 6 HLG migration rows, 5 backup rows, PharmacyCode present**. No tenant DB data or history was changed by this host repair.

The backup table contains only the five history rows. This task did not create a full SQL database backup or verify the server's existing backup policy.

The tenant DB `HoaLinhMienNam` had 13 baseline HLG tables and the five baseline migration rows before this task. After the host repair, `dotnet ef database update --no-build --connection <tenant Default connection>` applied **only** `AddHlgDesignContent` to this tenant. Read-only verification: **17 HLG tables, 6 HLG migration rows, PharmacyCode present**. The plain EF CLI command without `--connection` still targets host. Do not reuse the host history-repair script on the tenant; its nonempty-schema guard will reject it.

`AddHlgDesignContent.Up` now checks required baseline tables first and raises a clear error on any database with missing HLG baseline. It does not hide or skip partial schema drift.

## Menu finding and release check

Read-only DB inspection found `Hlg.Management=True` for tenant `Hoa Linh Miền Nam`, and the `admin` role has the HLG root permissions for both host and tenant. Source at HEAD `22a126b` contains the HLG menu contributor and Razor pages. Git history shows the permission group was introduced in `b507697`, while menu/pages were introduced later in `20b9e1c`.

Both `staging.genora.vn` and `hoalinh-staging.genora.vn` currently return **404** for `/Pages/Hlg/admin.js` and `/Pages/Hlg/Rewards/index.js`; `/Pages/Hl25/Settings.js` returns **200** on both. `staging.genora.vn/Hlg/Rewards` and `/Hlg/Categories` also return **404**, while `/Hl25/Wheel` returns **200**. This establishes that the deployed Web packages omit the HLG Admin UI, regardless of the granted permissions. A source-only permission change cannot repair that deployed binary.

An IIS-ready Web publish has been built at `artifacts/hlg-web-staging-20260923` (`dotnet publish -c Release --no-restore`). The ignored local artifact contains `Genora.MultiTenancy.Web.dll`, `web.config`, `Pages/Hlg/admin.js` and `Pages/Hlg/Rewards/index.js`. Publish/deploy that complete build to **both** staging site instances using the existing IIS release procedure; restore/overlay each site's staging configuration from its current deployment rather than using the local artifact's appsettings as live configuration. Verify DLL and `Pages/Hlg` static assets are part of the same release, recycle the app pools and clear any old output. Before/after checks:

1. `/Pages/Hlg/admin.js` and `/Pages/Hlg/Rewards/index.js` return 200 on both hosts (they returned 404 before release).
2. With the authorized host `admin`, `/Hlg/Rewards` returns 200 and `Hoa Linh Gamification` appears in the sidebar. With the authorized tenant `admin`, repeat on `hoalinh-staging.genora.vn` while `Hlg.Management` remains enabled.
3. Confirm Rewards, Knowledge, Ranking, Games and Users links; Content needs its separate newer root permission if desired. Sign out/in or hard-refresh if an old browser menu remains cached.
4. Check a tenant with HLG feature disabled still has no HLG menu and direct pages/API remain denied. Do not grant host permission on a tenant to bypass the feature gate.

The authenticated sidebar check and live IIS deployment were not executed in this workspace. No IIS server credentials or physical deployment path were available here. The local publish artifact is not itself a deployed fix.
