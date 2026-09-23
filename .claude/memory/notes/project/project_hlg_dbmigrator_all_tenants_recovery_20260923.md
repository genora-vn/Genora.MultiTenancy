# HLG DbMigrator all-tenant recovery — 2026-09-23

Branch `feature/dev-hoalinh-gamification`, starting HEAD `bc86f91`. User's `dotnet run -- --migrate-database` reached `Test1` and failed in `AddHlgDesignContent` guard 51023. Build warnings from Fnb DTOs/nullability are unrelated to this SQL exception.

## Diagnosis

- Direct read-only inventory: `Test1` had 0 HLG objects with the exact five baseline HLG migrations recorded; `Test2` had 13 HLG baseline tables/five history rows. Six other tenants and host already had design-content applied. Two tenant records may resolve to `Test1`, but there are nine distinct DBs including host.
- The `AddHlgDesignContent` guard is correct for drift: removing it would just fail at `ALTER TABLE` or leave absent tables. HLG feature availability cannot drive this migration because the shared EF migration set and history exist in feature-disabled tenant DBs too.

## Work done

- Created `docs/HLG_TEST1_SCHEMA_REPAIR_20260923.sql`, a dry-run-by-default copy of the guarded host repair pinned to `Test1`. Dry-run returned exactly five rows. Applied it once: `Test1.dbo.HlgMigrationHistoryRepair_20260923` contains five backed-up rows and HLG history was cleared for replay.
- EF CLI `--connection` on Test1 replayed five HLG baseline migrations plus design-content successfully. EF CLI on Test2 applied only design-content. No manual HLG data deletion or table drop.
- Updated `EntityFrameworkCoreMultiTenancyDbSchemaMigrator`: ping existing target DB first; master only on SQL missing-database errors 4060/911; `QUOTENAME` for new DB creation; remove connection string from `BusinessException` data; log database name/reason safely. In-sandbox attempt stopped at AmiHairSalon due machine TLS support, separate from schema.
- DbMigrator build 0 errors. The built `dotnet run --no-build -- --migrate-database` executed outside sandbox, migrated/seeded all listed DBs, printed `Completed database migrations.` and exited 0. Post-run SQL audit of nine distinct DBs: each has 17 HLG tables, 6 HLG history rows, `AddHlgDesignContent` present. Host and Test1 each have five history-backup rows.

## Remaining

- The separate Admin menu issue is still a Web deployment gap: both staging sites served HLG Admin assets/routes as 404 at last check. Local Release artifact was prepared in ignored `artifacts/hlg-web-staging-20260923`; IIS deployment/authenticated menu UAT remains outstanding. See [staging recovery](../../../../docs/HLG_STAGING_RECOVERY_20260923.md).
- Full SQL database backup policy was not verified; task backup tables contain only migration-history rows. New-database creation branch of the migrator was not exercised. Preserve unrelated log changes.

Detailed commands/findings: [runbook](../../../../docs/HLG_DBMIGRATOR_RECOVERY_20260923.md).
