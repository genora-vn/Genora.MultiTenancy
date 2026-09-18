# HLG host seeder blocks DbMigrator (2026-09-18)

## Root cause verified
- Workspace branch `dev`, HEAD `b8d0c06` (Sales exports already committed).
- User log: migrate GenoraMultiTenancy OK, then HlgDataSeedContributor line 68 AnyAsync fails with SQL 208 Invalid object name HLG.AppHlgGames, before tenant migration loop.
- Read-only SQL check using DbMigrator configured connection: DB GenoraMultiTenancy has no HLG tables. __EFMigrationsHistory nevertheless contains AddHlgModule, AddHlgKnowledge, AddHlgGames, AddHlgRewards, AddHlgRanking (20260819–20260820). History and physical schema have drifted; MigrateAsync does not recreate tables for recorded migrations. Cause of the drift (manual deletion/restore/etc.) not established.
- Seeder previously unconditionally seeded sample HLG data for host; this contradicts tenant-specific HLG deployment. Sales/schema HL is unrelated to the failing query.

## Fix
- Host DataSeedContext(null): return before feature checks or any HLG repository query.
- Tenant: enter ICurrentTenant.Change(context.TenantId), check Hlg.Management, then retain the existing idempotent AnyAsync and sample-data insert flow.
- Enabled HLG tenants still surface missing-schema errors; no catch-and-ignore of SQL exceptions.
- Host permissions/admin are still seeded by their own contributors.
- No migration/schema/history edits, no database writes or rerun of full DbMigrator against shared DBs in this session.
- Test: 3 Domain regression tests pass (host, non-HLG tenant, enabled tenant scoping/schema error propagation); DbMigrator build succeeds with 0 errors.
- Existing tracked DbMigrator log deletions and untracked current log preserved.

## Next checks
- Re-run dotnet run --project src/Genora.MultiTenancy.DbMigrator from repo root after rebuild; verify it proceeds from host to each tenant. This command migrates all configured tenant DBs.
- Host HLG history/schema drift still exists physically; bypassing host sample seeding does not restore tables. If HLG tables are needed on host in future, first review backup/history and a dedicated repair plan; do not blindly delete history entries or rerun old migrations.
- If an enabled HLG tenant also has missing tables, inspect that tenant's schema/history before repair. No diagnosis on tenant DBs was performed here.
- SalonBeautyTimeSlot.Status default/sentinel and Customer.BonusAmount precision warnings are separate model warnings; not the SQL 208 failure, left unchanged in this fix.
