# HL25 staging missing AgeGroup — corrective migration (2026-09-18)

## Evidence / cause
- User: local HL25 works; staging after deploy/migrate throws Invalid column name AgeGroup.
- Git initial commit 2ffacb7: 20260825160252_AddHl25Module creates BirthDate datetime2 nullable, no AgeGroup.
- Current migration with the SAME ID creates AgeGroup tinyint NOT NULL, no BirthDate. Snapshot/model already expect AgeGroup.
- Existing DBs that applied the original ID are not upgraded by editing that migration; newly created local DBs receive the edited schema. This explains the reported discrepancy. User confirmed dedicated tenant/database DuocPhamHoaLinh and DbMigrator visits all tenant databases. Read-only inspection of the currently configured host resolves tenant Dược phẩm Hoa Linh to DuocPhamHoaLinh; that configured database already has AgeGroup tinyint NOT NULL and all three preceding HL25 migrations. This is not verification of the deployed staging connection.

## Repair
- New migration **20260918093000_EnsureHl25ParticipantAgeGroup**, with full target-model metadata matching the previous migration; snapshot remains unchanged because current model already includes AgeGroup.
- Skip databases without hl25.AppHl25Participants so the all-tenant migration pipeline can continue across modules with different schemas.
- Add AgeGroup only when absent: tinyint NOT NULL DEFAULT(0) WITH VALUES, old rows become Hl25AgeGroup.Unknown=0.
- Existing AgeGroup values remain untouched; reject an existing nullable/wrong-type AgeGroup column for explicit review.
- Preserve legacy BirthDate and all participant data. No inferred age conversion/backfill from dates; keep source data for later business decision.
- Down intentionally leaves the column because it existed in the preceding model and may predate this repair on fresh local DBs; rollback must not delete participant data.
- No edits to already released migrations, no changes to HL Sales/HLG.
- Idempotent SQL generated via EF CLI at `.claude/docs/hl25_agegroup_repair_20260918.sql`, from 20260914111213_AddHl25GiftWheelImage to this migration, including migration-history recording.

## Validation / deployment
- EF CLI builds with separate Hl25MigrationValidation configuration to avoid running-Web DLL locks.
- EF recognizes new migration and generates the idempotent SQL. has-pending-model-changes confirms no model change versus snapshot.
- SQL Server engine validation on session-local tempdb tables: 4 cases pass — legacy rows backfilled to 0 with BirthDate preserved/repeated repair safe, existing AgeGroup values preserved, non-HL25 database skipped, incompatible-type/nullability guard. Reusable smoke script: `test/hl25-agegroup-repair-validation.ps1`, reads configured DbMigrator connection without exposing credentials and only creates session-local temporary tables.
- No business schema/data writes or staging migration run performed. Tempdb validation is not inspection of the actual staging target. Staging HL25 target is DuocPhamHoaLinh; actual staging schema/history still needs verification during deployment.
- Deploy backend containing the new migration, run DbMigrator for configured DBs, confirm the migration is recorded in the actual HL25 tenant DB and column exists, then restart host/retest Participants/Reports/Frames/Wheel. Use generated SQL only if previous migrations are already applied and after selecting the correct database.
- Existing participant rows remain Unknown until user/admin updates AgeGroup. Legacy BirthDate is retained.
