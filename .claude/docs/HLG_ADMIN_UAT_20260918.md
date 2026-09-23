# HLG Admin — UAT 2026-09-18

## Staging update 2026-09-23

Host `GenoraMultiTenancy` schema/history drift was repaired and `AddHlgDesignContent` applied; tenant `HoaLinhMienNam` also received this migration via explicit connection. Each now has 17 HLG tables/six HLG migration rows. However, both staging Web sites still return 404 for HLG Admin JS/routes while known HL25 assets return 200, so the current Web build must be published before browser UAT or menu verification. See [recovery runbook](../../docs/HLG_STAGING_RECOVERY_20260923.md). Historical status statements below describe their original dates.

## Verification follow-up 2026-09-19

- Automated PASS in this session: Web build 0 errors (52 warnings); 46 Application, 3 Domain, 17 Web and 11 JS tests; EF pending-model check reports no changes.
- Corrective fixes: shipping-address now requires query `phone` and rejects foreign/un-finished sessions; Admin image URL validation now also covers category, reward, question and legacy product image lists.
- Browser UAT remains BLOCKED: Computer Use returned no apps/browsers and in-app browser creation failed with `Browser is not available: iab`. No host startup, live proxy fetch, authenticated CRUD or database write occurred.
- Migration `20260919112304_AddHlgDesignContent` remains NOT APPLIED.
- Add to runtime UAT: call `POST /api/mini-app/hlg/games/sessions/{sessionId}/shipping-address?phone=...`; verify missing phone, another customer's session and unfinished session are rejected, while the owner's finished session succeeds.

Prerequisites: rebuild/restart Web; select the HLG tenant (not HL Sales or HL25), ensure HLG schema is present, enable Hlg.Management, grant the corresponding AppHlg* permissions. Use test records.

1. Menu Hoa Linh Gamification shows Rewards, Knowledge categories, Ranking, Games and Users. Tenant feature disabled: menu hidden and direct routes/API denied.
2. /Hlg/Rewards: create physical/voucher rewards, edit point cost/stock/order/active, search name/voucher code and active status, delete with confirmation. Blank stock = unlimited; negative stock/points and overlong fields rejected.
3. /Hlg/Categories: create a category; row action opens Products for that category. Create/edit lessons including optional content, thumbnail and image URLs (one per line). Reload verifies values. Deleting a category containing lessons is rejected.
4. /Hlg/Ranking: create/edit title, description, start/end and active status; reversed/equal interval rejected. Check input and display with VI browser/request culture.
5. /Hlg/Games: configure game type/status/start/end/rules/reward description/base score. Open Questions row action. Create A/B plus optional C/D, select correct answer; edit/add/remove optional options, verify modal reload. Empty correct option, duplicate index and invalid time/multiplier rejected. Verify multiplier 1.25 round-trips correctly with VI culture.
6. CorrectKey must be absent from list/get and all mini-app game responses. Account with Games.Default but without Games.Edit can list questions but cannot call GetEditor or open EditModal. Create/Delete privileges are independent.
7. Game with sessions: reject question edits/deletes and changes to type/base score; descriptive fields and status can still change. Game with child questions/sessions cannot be deleted.
8. /Hlg/Users: only HLG-linked profiles shown, with code/name/phone/Zalo/type/points/registered/active; search/paging/status work, no mutation actions.
9. Each group: verify create/edit/delete denied both via direct API and page/modal when missing permission; successful save refreshes table, invalid save retains entered values; cancel makes no changes.
10. Browser console/network: service proxy resolves, GET lists are paged, nested requests include parentId, filter false remains false, new modal/save works, no raw HTML executes from stored names/content. Open /Abp/ServiceProxyScript if the localized missing-proxy message appears.

## Earlier 2026-09-19 checkpoint (historical; superseded by corrective audit below)

- **PASS (automated):** solution build; 13 HLG Application tests; 7 HLG Admin JavaScript tests.
- **PASS (runtime proxy):** `/Abp/ServiceProxyScript` on the locally started Web host contains `genora.multiTenancy.appServices.hlg.admin.hlgRewardAdmin` and the other five HLG admin services.
- **PASS (design evidence):** PDF export was text-extracted and rendered; all 31 of 31 pages were visually inspected.
- **BLOCKED (browser/real DB):** no HLG tenant database, feature/permission setup, or test credentials were available. Do not mark checklist items 1–10 as browser PASS until run against that tenant.
- **Additional UAT:** API must reject Upcoming/Ended/before-StartAt/after-EndAt games. Verify profile accuracy equals `sum(CorrectCount) / sum(TotalQuestions)` over completed sessions, rounded to a whole percent. Verify `/ranking/event` is null with no active event in its time window.

Automated: Web build 0 errors, 13 Application and 7 JS tests pass. Browser/real-database UAT is pending. This change adds no migrations and does not configure tenant databases. The previously observed host HLG schema drift is a separate deployment concern.

## Corrective audit 2026-09-19 — current acceptance status

The historical 13/7 tests and "no migration" summary above predate this correction. The prior statement "no tenant DB available" was not established by this corrective session; the verified browser blocker is no connected browser. Prior live proxy checkpoint had7 original admin services (not "other five"); current13 source service names are verified with the installed ABP generator, not fresh browser HTTP.

| Check | Status | Evidence / acceptance steps |
|---|---|---|
| PDF full coverage, both visual audits | PASS |31/31 pages each;60 states;54 data groups; inventory/matrix in HLG_FULL_DESIGN_AUDIT_20260919.md/json |
| Build and automated tests | PASS |Web build0 errors;42 Application,3 Domain,17 Web,11 JS tests pass |
| Migration review/model consistency | PASS |4 new HLG tables/6 nullable columns; additive Up/idempotent SQL; no pending model change; NOT applied |
| Tenant menu / feature disabled / direct API denial | BLOCKED |No connected browser; verify existing groups plus Content. Host uses Host permissions; tenant feature off hides/denies all HLG admin |
| Industry/brand/product CRUD | BLOCKED |Create industry, brands and products; parent mismatch rejected; active-parent hides descendants in public API; verify multi-product filter |
| Product tabs/content/media | BLOCKED |Save/reopen HTML, ordered FAQ, images, generic video/poster, placement by tab, related products/reorder, badge and game CTA. Test image upload and invalid URLs |
| Home CMS | BLOCKED |Manage carousel/order/active/card text/badges/share URL; inspect GET content; singleton slots reject duplicates |
| Ranking/prizes/winners | BLOCKED |Per-game scores; valid period; prize quantity; duplicate/wrong-tenant winner rejected; publish after end; test concurrent capacity guard with real SQL |
| Rewards/fulfillment | BLOCKED |Search/paging; existing stock behavior; recipient scope; valid forward type-specific status changes; session-origin history; no automatic winner-to-order expectation |
| Games/questions/options | BLOCKED |Banner/badge/rules; session edit guards; date status; VI decimal multiplier; view-only GetEditor denied; no correct answer in list/get/Mini DTO |
| Users read-only | BLOCKED |Search/type/registration filter; retailer3; PharmacyCode; stats/learning/games/rewards; no mutation; latest100 games label |
| Localization/responsive/forms | BLOCKED |VI/EN labels and validation; modals on desktop/tablet; no raw keys; lower-case public reward status localizes correctly; rich HTML uses intended safe rendering |
| Authenticated browser flows | BLOCKED |Browser getForUrl failed "No browser is available"; list returned[]. No full app/job startup or database mutations performed |

Three business UNKNOWNs remain: spin mechanics, automatic prize eligibility/ties/allocation, automatic reward timing/recipient policy. They are explicitly excluded from the completed CMS source coverage, not silently marked PASS. FE integration and real tenant acceptance are still required.
