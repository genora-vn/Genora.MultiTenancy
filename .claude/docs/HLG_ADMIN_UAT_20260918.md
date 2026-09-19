# HLG Admin — UAT 2026-09-18

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

Automated: Web build 0 errors, 13 Application and 7 JS tests pass. Browser/real-database UAT is pending. This change adds no migrations and does not configure tenant databases. The previously observed host HLG schema drift is a separate deployment concern.
