---
name: project_caddie_module_phase2_ui_redesign
description: "Caddie admin UI redesign — Booking/Rating Detail pages, ParseException fix, avatar/phone/stars enhancements"
metadata: 
  node_type: memory
  type: project
  originSessionId: 42c60f84-6497-4468-9a7a-4a2842937bc4
---

Caddie Module UI Redesign (updated June 05, 2026):

## Phase 1: Menu Rename ✅
- "Quản lý danh sách Caddy" → "Danh sách Caddy"
- "Kỹ năng Caddie" → "Kỹ năng Caddy"
- "Lịch Caddie" → "Lịch làm việc Caddy"
- "Đặt Caddie" → "Đặt Caddy"
- "Đánh giá Caddie" → "Đánh giá Caddy"

## Phase 2: Booking Pages ✅
- Index: redesigned filter bar, columns with initials avatar, badges
- Detail: Progress tracker, Golfer info (avatar + phone hover), Caddy info (avatar + phone hover + stars), Chi tiết đánh giá Caddy (skill ratings from DB), Đánh giá từ KH (comment), Chi tiết thanh toán, Lịch sử thao tác
- `Detail.cshtml.cs` inject repos + `IAsyncQueryableExecuter` to load rating/caddie/customer data server-side
- `detail.js`: phone hover pattern `.golfer-phone-hover`, `.caddie-phone-hover`

## Phase 3: Rating Pages ✅
- Index: KPI avg card uses `sum / totalCount` (approved only); star filter added; Caddie column shows avatar img if exists; Rating column floor-based stars + "Chưa đánh giá" for 0; Modal shows Golfer/Caddy avatars
- Detail: Golfer section — avatar, CustomerCode (Member ID), phone hover; Caddy section — avatar, CaddiePhone hover, CaddieRatingAvg stars; Skill notes via `GetSkillNote()` helper; Comment merged into skill card
- ParseException fix: `CaddieRatingAppService.GetListAsync` whitelist allowed sort fields

## Phase 4: Schedule ✅ (done earlier)
- Month/Day/Week views, filter, auto-generate, Excel import/export

## Backend DTO changes (June 05)
- `CaddieRatingDto` added: `CaddieAvatar`, `CaddiePhone`, `CaddieRatingAvg`
- `CaddieBookingDto` added: `BookingRatingAvg` (decimal?)
- `CaddieRatingAppService` caddie query includes Avatar/RatingAvg/Phone; sort whitelist
- `CaddieBookingAppService` enriches BookingRatingAvg from rating detail scores

## Key Patterns
- PageModel → inject `IAsyncQueryableExecuter` (not `AsyncExecuter` which is ApplicationService-only)
- Phone hover: `data-masked` + `data-full` attributes, mouseenter/mouseleave JS
- Floor-based stars: `Math.floor(avg)` for filled, rest empty (no half-stars)

**Why:** UI cần match design mockup; tách Detail page cho Booking/Rating.
**How to apply:** [[project_caddie_ui_fixes_june05]], [[project_caddie_avatar_refactor]]
