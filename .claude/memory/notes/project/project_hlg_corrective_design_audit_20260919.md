# HLG corrective design audit — 2026-09-19

Source and full evidence: [audit report](../../../docs/HLG_FULL_DESIGN_AUDIT_20260919.md), [machine-readable inventory/matrix](../../../docs/HLG_FULL_DESIGN_AUDIT_20260919.json).

31/31 PDF pages visually inspected in first AND second audit; 60 screens/components/states; 54 logical data groups. Before: 10 covered/3 partial/18 missing/3 unknown in34 CMS-or-unknown groups. After:31 covered/0 partial/0 missing/3 unknown;20 remaining static/user/derived groups separately classified. This is source coverage, not production acceptance.

Implemented industry→brand→product; typed ordered FAQ/media/related product aggregate; rich information; game CTA; Home content/share link; game banners/badges; per-game events/prize tiers/manual winner publication; redemption fulfillment; read-only user details/history; HLG PharmacyCode and additive retailer3; progress/game-history APIs. Existing mini routes preserved. New session-specific redeem can attach history to a finished owned game session. No automatic award model invented.

New migration 20260919112304_AddHlgDesignContent + idempotent SQL:4 HLG tables/6 nullable columns; additive Up reviewed; model check clean; NOT applied. No destructive schema operations.

Verification: Web build0 errors; Application42/42; Domain3/3; Web17/17; JS11/11. Installed ABP generator verified all13 HLG service names. Live browser UAT BLOCKED because browser tool returned no browser and empty list. No fresh live proxy HTTP/tenant CRUD run, no demonstrated DB connection failure. Repository substitute tests do not prove SQL concurrency.

Unknowns: spin mechanics/probabilities; automated winner eligibility/ties/allocation; award/deduction and type-specific fulfillment timing. Winners are manual publications and do not automatically create reward histories. Existing anonymous phone-based mini identity trust remains. Full checklist in UAT doc.

Prior claims of full design coverage/no migration are superseded. Earlier VgaCode→Customer golf identity mapping corrected: pharmacy code is stored on HlgUserProfile; legacy DTO alias retained. Do not overwrite user's pre-existing appsettings/log changes. Not committed/deployed.
