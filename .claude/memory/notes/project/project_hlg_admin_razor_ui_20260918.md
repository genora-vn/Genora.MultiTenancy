# HLG Admin Razor UI — 2026-09-18

## Scope / baseline
- Resumed the 2026-08-21 checkpoint on dev, baseline c76c54b. Only Rewards admin service existed; no HLG Razor pages.
- Implemented all five groups: Rewards; Knowledge (categories + nested products/lessons); Ranking events; Games + nested questions/options; Users read-only.
- Separate HLG module / schema / Hlg.Management feature. No changes to HL Sales or HL25 business flows, entities, migrations, or database contents.

## Implementation
- Routes: /Hlg/Rewards, /Hlg/Categories, /Hlg/Ranking, /Hlg/Games, /Hlg/Users. Child routes /Hlg/Products?parentId={categoryId} and /Hlg/Questions?parentId={gameId} are opened from row actions.
- Each editable entity has Index.cshtml + PageModel + index.js + CreateModal/EditModal pages. Filters: keyword and active status; nested lists enforce parent filtering in UI. Server-side paging (10/25/50/100) and stable ordering.
- New Category/Product/Ranking/Game/Question admin CRUD services use FeatureProtectedCrudAppService with separate Create/Update DTOs, manual mapping and AsyncExecuter. Users service joins HLG profiles with Customer for read-only code/name/phone/Zalo/type/points/registration/status.
- HlgAdminPageModel enforces feature and current-side root/action permissions on direct page/modal GET and POST. Services independently check permission and feature. Menu order 49 includes only existing pages and permitted groups, with tenant feature gate and Host permissions.
- Shared Pages/Hlg/admin.js resolves appServices.hlg.admin or appDtos.hlg.admin explicitly; checks getList and displays a localized error if missing. Live generated proxy was NOT fetched; guarded resolution follows checkpoint's allowed fallback. No assumption that build verifies runtime proxy paths.
- List renderers HTML-escape stored text. ModalManager handles forms/antiforgery. VI/EN labels, enum choices, menu and missing Reward validation messages added.
- Questions list/get DTO has NO CorrectKey/options. GetEditorAsync checks Games.Edit before loading answers. Mini-app DTOs and response mapping unchanged.
- Question+options writes use transactional UoW and explicit parent autoSave before child writes (MARS pattern). Editing preserves existing option IDs; removes blank optional options and adds new options. Validates nonempty correct option, positive timing/multiplier, and duplicate question index.
- Once a game has sessions, question changes/deletion and game type/base-score changes are rejected to protect existing play/scoring. This is an application-level guard, not a load/concurrency proof. Other game metadata/status remain editable. Category/game deletion refuses existing child content/sessions; soft-deletion/history are preserved.
- Date interval validation on Ranking/Game; reward input range/length validation incl. nonnegative stock. Product images entered one URL per line, stored as JSON; optional Content may be null.

## Validation
- Web build with HlgAdminValidation configuration: 0 errors (existing project warnings remain).
- 13 Application tests pass: tenant feature gate, host permission mapping, secret editor authorization, parent/keyword/status/pagination and no secret serialization, parent-before-options writes/current tenant, missing correct option, invalid dates/stock, optional lesson content/image array, update/add/delete options, sessions guard and duplicate question index.
- 7 Node regression tests pass: proxy namespace resolution/missing proxy handling, filters incl. false/null, text escaping/unlimited stock, read-only users, action permissions, nested/modal/delete actions.
- Commands: dotnet test test/Genora.MultiTenancy.Application.Tests --configuration HlgAdminValidation --filter FullyQualifiedName~HlgAdminTests; node --test test/hlg-admin-ui-regressions.cjs; dotnet build src/Genora.MultiTenancy.Web --configuration HlgAdminValidation.
- No browser UAT, real-DB CRUD or deployment performed. Existing appsettings/log changes belong to user and were preserved.

## Next runtime check
- Restart/rebuild Web with these changes; use the HLG tenant with schema and Hlg.Management enabled, grant relevant permissions.
- Run checklist in docs/HLG_ADMIN_UAT_20260918.md. Verify real ABP generated proxy, modal save/reload and VI date/decimal input in browser.
- Host database was previously found without HLG tables despite migration history. This task does not repair that schema drift; test business CRUD in the configured HLG tenant. Host permission mapping has unit coverage, not an assertion that host schema is provisioned.
- Remaining separate workstreams: UrBox integration, mapping game↔reward at finish, profile accuracyPercent; actual tenant provisioning/migration status still requires runtime confirmation.
