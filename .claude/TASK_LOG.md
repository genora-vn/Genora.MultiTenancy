# TASK LOG — Nhật ký công việc

> Nhật ký các đợt task theo dòng thời gian, suy ra từ hậu tố ngày/round trên tên note
> trong `.claude/memory/notes/`. Sắp theo thứ tự gần đây nhất ở trên.
> Khi hoàn thành task mới, thêm 1 dòng vào đầu bảng.

## Cách đọc
- Mỗi dòng là 1 đợt làm việc. "Note" trỏ tới file chi tiết trong `memory/notes/project/`.
- Ngày lấy từ tên file (`_juneXX`, `_YYYY_MM_DD`) hoặc thứ tự phase/round.

## Nhật ký (mới → cũ)

| Mốc | Module | Nội dung | Note gốc |
|-----|--------|----------|----------|
| 2026-07-25 | Caddie | Fee + multi-caddie admin + upsert/unassign (migration 20260725062150) | `project_caddie_fee_upsert_unassign` |
| 2026-07-24 | Caddie | Booking gắn vào golf players (migration 20260724091716) | `project_caddie_booking_linked_to_golf_players` |
| 2026-07-09 | Hoa Linh | Loyalty đổi điểm/tiền + ledger FIFO + worker hết hạn (migration 20260709064009) | `project_hl_loyalty_points_redeem` |
| 2026-08-21 | HLG | ⏸️ **TẠM DỪNG** module HLG. Đã bắt đầu Admin Razor UI: xong **Rewards admin CrudAppService** (`HlgRewardAdminAppService` + DTOs + interface, build 0 errors). Dở phần Razor Pages cho Rewards. CHẶN: cần xác minh JS proxy path runtime (`appServices.hlg.admin.hlgRewardAdmin`?) trước khi wire DataTables. Chi tiết điểm dừng ở `ACTIVE_CONTEXT.md`. | `.claude/ACTIVE_CONTEXT.md`, `.claude/architecture/module-hlg.md` |
| 2026-08-20 | HLG | Phase 7 (Backend hoàn thiện): `HlgDataSeedContributor` seed dữ liệu mẫu (1 danh mục + 2 bài học, 1 game quiz + 3 câu hỏi, 2 quà, 1 sự kiện xếp hạng), gate bằng feature `Hlg.Management`, idempotent. Permission provider đã xong trước đó. Web build 0 errors. **Admin Razor UI: chưa làm** (workstream lớn riêng). | `.claude/architecture/module-hlg.md` |
| 2026-08-20 | HLG | Phase 6 (Live-feed realtime SignalR, BD-4): `IHlgLiveFeedNotifier` + `HlgLiveFeedHub` (AllowAnonymous, group theo gameId) + `HlgLiveFeedNotifier` (event `hlg.live-feed.activity`). Đăng ký DI (HttpApiModule) + MapHub `/signalr-hubs/hlg-live-feed` (Program.cs). Broadcast từ HlgGameAppService answer(đúng)/finish, bọc try/catch (feedback_signalr_try_catch). Web build 0 errors. | `.claude/architecture/module-hlg.md` |
| 2026-08-20 | HLG | Phase 5 (Ranking): entity `HlgRankingEvent` + 2 endpoint (ranking/event, ranking/entries). Ranking reset theo sự kiện (BD-5): điểm tính động = sum(GameSession.Score) các phiên finish trong [StartAt,EndAt]; gán rank + isCurrentUser theo phone; luôn kèm dòng user hiện tại nếu ngoài top. Migration `AddHlgRanking` (1 bảng). Build 0 errors. Sửa footgun dynamic→tuple. | `.claude/architecture/module-hlg.md` |
| 2026-08-20 | HLG | Phase 4 (Rewards & Shipping): 2 enum (RewardType/RewardHistoryStatus) + 3 entity (`HlgReward`/`HlgRewardHistory`/`HlgShippingAddress`), endpoint shipping-address (thứ 7 Games) + rewards + redeem. Redeem ACID trừ `Customer.BonusPoint` + phân luồng pharmacy vs consumer (BD-3, BD-6). Nối `profile/reward-history` thật. Migration `AddHlgRewards` (3 bảng). Build 0 errors. | `.claude/architecture/module-hlg.md` |
| 2026-08-20 | HLG | Phase 3 (Games engine): 3 enum (GameType/GameStatus/AnswerKey) + 5 entity (`HlgGame`/`HlgQuestion`/`HlgAnswerOption`/`HlgGameSession`/`HlgSessionAnswer`), 6 endpoint games. **Chấm điểm SERVER-SIDE chống gian lận (BD-2)**: /answer chấm theo CorrectKey bí mật, /finish đối soát từ answer đã ghi (bỏ qua totalScore client), cộng Customer.BonusPoint. Migration `AddHlgGames` (5 bảng). Build 0 errors. | `.claude/architecture/module-hlg.md` |
| 2026-08-19 | HLG | Phase 2 (Knowledge base): 3 entity `HlgKnowledgeCategory`/`HlgProduct`/`HlgLearningProgress`, 5 endpoint knowledge, DTO khớp contract (images[] qua ImagesJson, isCompleted per-user), nối `profile/stats` + `learning-history` từ dữ liệu thật. Migration `AddHlgKnowledge`. Build 0 errors. | `.claude/architecture/module-hlg.md` |
| 2026-08-19 | HLG | Permission provider: nhóm `HlgManagement` (Tenant, RequireFeatures) + `HlgManagementHost` (Host) cho 6 nhóm (Users/Knowledge/Games/Rewards/Ranking/Dashboard), localization en/vi. Host admin nhận tự động qua data seeder. | `MultiTenancyPermissionDefinitionProvider.cs` |
| 2026-08-19 | HLG | Module Hoa Linh Gamification MỚI — Phase 0 (hạ tầng schema HLG, feature `Hlg.Management`, localization) + Phase 1 (Auth 3 + Profile 5 endpoint, envelope `HlgApiResult`, entity `HlgUserProfile`). Build 0 errors. | `.claude/architecture/module-hlg.md` |
| 2026-06-26 | Hoa Linh | UI update batch 3 (Dashboard/Brands/Products/Customers/Orders/GiftExchanges) | `project_hoalinh_ui_update3_june26` |
| 2026-06-25 | Hoa Linh | UI update batch 1-2 + API endpoints mới (Brands/ProductGroups/OrderHeaders) | `project_hoalinh_ui_update_june25`, `_ui_update2_june25` |
| 2026-06-16 | Caddie | Import + Email Cc/Bcc + Feature toggle | `project_caddie_email_feature_fixes_june16` |
| 2026-06-05 | Caddie | UI fixes June 05 (batch 1-3) | `project_caddie_ui_fixes_june05`, `_batch2` |
| 2026-06-03 | Caddie | UI fixes June 03 (Select2, flatpickr) | `project_caddie_ui_fixes_june03` |
| 2026-05-25 | Golf/Pro | ProOrder.CustomerId soft reference (migration 20260525102341) | `project_proorder_customer_soft_reference` |
| 2026-05-20 | Salon | Stylist/Booking LocationId + slot config (migrations 20260520052000, 20260520100420) | `project_salon_stylist_booking_locationid`, `_location_slot_config` |
| — | Hoa Linh | Phase 1-7 complete (foundation → dashboard/data-auth) | `project_hoalinh_phase1..7_complete` |
| — | Caddie | Phase 1-7 complete (SRS → final) | `project_caddie_module_phase1..final_complete` |
| — | Salon Beauty | Backend + UI complete | `memory/modules/salon-beauty/`, `project_salon_*` |
| — | Documents | Online docs site + seeder 11 section | `project_app_documents_*` |

> Ghi chú: Danh sách đầy đủ 88 note project nằm ở `memory/notes/project/`. Bảng này chỉ
> tổng hợp các mốc chính; xem [MEMORY.md](MEMORY.md) để có index đầy đủ theo chủ đề.

## Sự cố đáng nhớ
- **Prod antiforgery SSL** — proxy terminate TLS, `SecurePolicy=Always` gây lỗi login. → `project_prod_antiforgery_ssl_incident`
- **AppCustomers permission leak** — dropdown load qua AppService `[Authorize]` throw AbpAuthorizationException. → `project_appcustomers_page_customertype_permission_leak`

## Migration đã ghi nhận
- 20260520052000, 20260520100420 (Salon location/slot)
- 20260525102341 (ProOrder drop FK)
- 20260709064009 (HL loyalty)
- 20260724091716, 20260725062150 (Caddie players link + fee)
