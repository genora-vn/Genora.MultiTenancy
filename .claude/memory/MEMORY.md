# Memory Index — Genora.MultiTenancy

Latest Sales handoff (2026-10-11): [Blouse Excel style/size columns](notes/project/project_hl_blouse_excel_style_sizes_20261011.md). Q:X male/female M/L/XL/2XL, sums free+exchange from order snapshots, 71 Sales tests pass, Web build pass; deploy/UAT pending, no migration.

Latest HLG handoff (2026-10-06): [pharmacy registration](notes/project/project_hlg_pharmacy_registration_20261006.md). DMS branches, owner-first/max5, preserve Sales identity in shared DB, nullable HLG links; 133 tests pass, application database migration pending.

Đây là nơi lưu trữ project memory (kiến thức đặc thù dự án) được version-control cùng source code.
Nguồn chân lý duy nhất cho memory của các module. Không lưu secret hay config cá nhân ở đây.

Latest DbMigrator handoff (2026-09-23): [HLG all-tenant recovery](notes/project/project_hlg_dbmigrator_all_tenants_recovery_20260923.md). Nine DBs migrated/verified; staging Web menu release remains.

Latest HLG handoff (2026-09-23): [staging schema/menu recovery](notes/project/project_hlg_staging_schema_menu_recovery_20260923.md). Host and tenant HLG migrations applied; staging Web publish remains.

## Modules

### Salon Beauty
- [Final Implementation](modules/salon-beauty/final-implementation.md) — Bản implementation hoàn chỉnh: full CRUD, entities, APIs, permissions, features, localization
- [Implementation Progress](modules/salon-beauty/implementation-progress.md) — Nhật ký tiến độ triển khai theo từng phase (completed / in-progress / pending)
