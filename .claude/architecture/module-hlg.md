# Architecture — Module Hoa Linh Gamification (HLG)

> Mini app MỚI, tách biệt hoàn toàn với module Hoa Linh (schema HL) hiện tại.
> Schema DB riêng: **HLG**. Tenant riêng: **"Hoa Linh Miền Nam Gamification"** (database-per-tenant).
> Controller: `HoaLinhGamificationController` route `api/mini-app/hlg`.
> Backend khớp CHÍNH XÁC API contract frontend đã cố định (Phase 1 frontend dùng data mock).

## Nguyên tắc khớp contract
- Mọi response bọc envelope `{ error?: number, message?: string, data: <payload> }` → `HlgApiResult<T>`.
- Các "enum" trong contract là **chuỗi lowercase cụ thể** (`"male"`, `"pending"`...) → DTO dùng field `string`, map tường minh ở service qua `HlgEnumMapper` (không phụ thuộc JSON enum converter toàn cục).
- Tên field + kiểu dữ liệu trùng 100% với contract. KHÔNG tự thiết kế lại data model, KHÔNG đổi tên field.

---

## Quyết định kiến trúc (đã chốt với stakeholder)

### AD-1 — Envelope sạch
Tạo `HlgApiResult<T>` chỉ gồm `{ error?, message?, data }`. KHÔNG tái dùng `HlApiResult` (class cũ có field thừa `success`). Lý do: khớp contract 100%.

### AD-2 — Điểm dùng chung `Customer.BonusPoint`
`points` của game lưu tại `dbo.AppCustomers.BonusPoint`. An toàn vì tenant HLG có **database riêng** → các luồng loyalty DMS cũ (FIFO batch, expire worker) KHÔNG chạy trên DB này.
- **Rủi ro (ghi nhận):** nếu sau này bật loyalty DMS cho tenant HLG, điểm game và điểm loyalty sẽ lẫn. Khi đó phải tách sang ledger riêng.
- Point history tái dùng ledger `HL.AppHlPointTransactions` (lọc theo CustomerId).

### AD-3 — User model: reuse `AppCustomers` + `HlgUserProfile`
Tái dùng `dbo.AppCustomers` (zalo/phone/code/BonusPoint) qua `CustomerId`; field đặc thù game (`CustomerType`, `IsRegistered`, `ZaloId` snapshot) đặt ở `HLG.AppHlgUserProfiles`. Tránh trùng entity, giữ dữ liệu game cô lập.

---

## 6 Quyết định nghiệp vụ (theo yêu cầu)

### BD-1 — Loại game cấu hình động
`GameType` hỗ trợ 5 loại, cấu hình động (admin bật/tắt, cấu hình từng game):
- `quiz` — Trả lời trắc nghiệm
- `Picture-to-Word Puzzle` — Đuổi hình bắt chữ
- `King of Vietnamese` — Vua tiếng Việt
- `Spin Wheel - Lucky Wheel` — Vòng quay may mắn
- `Tile Flip / Reveal the Image` — Lật mảnh ghép

Entity `HlgGame` giữ `Type`, `Rules`, `RewardDescription`, `Status` (upcoming/ongoing/ended), `StartAt/EndAt`, `TotalQuestions`. Câu hỏi ở `HlgQuestion` + `HlgAnswerOption` (áp dụng cho các loại có câu hỏi). Các loại không câu hỏi (spin wheel) cấu hình phần thưởng/xác suất riêng.
→ **Trạng thái: sẽ làm ở Phase 3.**

### BD-2 — Chấm điểm server-side (chống gian lận)
**QUAN TRỌNG:** `/games/answer` và `/games/sessions/{id}/finish` tự chấm điểm **server-side**. KHÔNG tin `totalScore` client gửi.
- Server lưu `AnswerKey` đúng ở `HlgQuestion` (không trả về client trong lúc chơi).
- Điểm mỗi câu = công thức nội bộ (tham chiếu frontend tạm `100 × 2.5`) × `scoreMultiplier` của câu + yếu tố thời gian (`timeSpentSec` vs `timeLimitSec`).
- `Question.scoreMultiplier` trả client CHỈ để hiển thị.
- `finish` đối soát lại tổng điểm từ `HlgSessionAnswer` đã ghi, bỏ qua giá trị client.
→ **Trạng thái: sẽ làm ở Phase 3.**

### BD-3 — Cơ chế thưởng (physical | voucher)
`RewardType`: `physical` (quà vật lý) | `voucher` (eVoucher). Điều kiện nhận:
- Đạt ngưỡng điểm/hoàn thành game (cấu hình theo game).
- **Voucher:** phát qua UrBox (tái dùng `UrBoxService`), trừ điểm `BonusPoint`.
- **Physical:** cần địa chỉ giao hàng → luồng consumer (xem BD-6).
- Ghi `HlgRewardHistory` (status: pending/shipping/delivered/done).
→ **Trạng thái: sẽ làm ở Phase 4.**

### BD-4 — Live-feed realtime
Live-feed người chơi cùng đạt điểm: realtime qua **SignalR** (`HlgLiveFeedHub` + `IHlgLiveFeedNotifier`, copy pattern `ProOrderHub`, group tách theo tenant). Fallback polling qua endpoint `GET /games/{id}/live-feed`.
→ **Trạng thái: sẽ làm ở Phase 6.**

### BD-5 — Ranking reset theo sự kiện
`HlgRankingEvent` (chiến dịch có `StartAt/EndAt`). Điểm ranking tính trong khoảng sự kiện. `RankingEntry` tính `rank` (ORDER BY score) + `isCurrentUser` (đánh dấu vị trí user hiện tại để hiển thị trên mini app).
→ **Trạng thái: sẽ làm ở Phase 5.**

### BD-6 — customerType gán khi register
`CustomerType` (`pharmacy` | `consumer`) gán tại bước register (`POST /customer/upsert`), lưu ở `HlgUserProfile.CustomerType`. Quyết định luồng nhận quà sau game:
- `pharmacy`: nhận quà không cần địa chỉ ship.
- `consumer`: quà physical cần địa chỉ → endpoint MỚI `POST /games/sessions/{id}/shipping-address`.
→ **Trạng thái: gán khi register ĐÃ làm ở Phase 1; luồng shipping ở Phase 4.**

---

## Tiến độ triển khai
| Phase | Nội dung | Trạng thái |
|-------|----------|-----------|
| 0 | Hạ tầng: schema HLG, feature `Hlg.Management`, localization | ✅ Xong (build 0 errors) |
| 1 | Auth (3) + Profile (5): envelope, HlgUserProfile, service, controller | ✅ Xong (build 0 errors) |
| 2 | Knowledge base (5 endpoint) | ✅ Xong (build 0 errors) |
| 3 | Games engine (7 endpoint) — chấm điểm server-side | ✅ Xong (build 0 errors) |
| 4 | Rewards & Shipping (endpoint shipping-address mới) | ✅ Xong (build 0 errors) |
| 5 | Ranking (2 endpoint) | ✅ Xong (build 0 errors) |
| 6 | Live-feed realtime (SignalR) | ✅ Xong (Web build 0 errors) |
| 7 | Backend hoàn thiện: Permission provider ✅, sample data seeder ✅, tenant provisioning (hướng dẫn) ✅. **Admin Razor UI: chưa làm** (workstream lớn, làm theo từng nhóm entity) |

## Endpoints (contract — HoaLinhGamificationController)
Auth: `POST decode-phone`, `POST customer/upsert`, `GET customer/by-phone`
Profile: `PUT profile`, `GET profile/stats`, `GET profile/learning-history`, `GET profile/point-history`, `GET profile/reward-history`
Knowledge: `GET knowledge/categories`, `GET knowledge/categories/{id}`, `GET knowledge/categories/{id}/products`, `GET knowledge/products/{id}`, `POST knowledge/products/{id}/complete`
Games: `GET games`, `GET games/{id}`, `POST games/{id}/start`, `POST games/answer`, `POST games/sessions/{id}/finish`, `GET games/{id}/live-feed`, `POST games/sessions/{id}/shipping-address`
Ranking: `GET ranking/event`, `GET ranking/entries`

## Entity đã tạo (schema HLG)
- `HlgUserProfile` (`HLG.AppHlgUserProfiles`) — hồ sơ game, FK mềm `CustomerId` → dbo.AppCustomers.
- `HlgKnowledgeCategory` (`HLG.AppHlgKnowledgeCategories`) — danh mục kiến thức.
- `HlgProduct` (`HLG.AppHlgProducts`) — bài học; FK cascade → Category; `ImagesJson` cho images[]; Content HTML.
- `HlgLearningProgress` (`HLG.AppHlgLearningProgress`) — tiến độ học per-user (isCompleted, progressPercent).
- `HlgGame` (`HLG.AppHlgGames`) — game config-driven theo `GameType`; `BaseScorePerQuestion` cho chấm điểm.
- `HlgQuestion` (`HLG.AppHlgQuestions`) — câu hỏi; **`CorrectKey` bí mật server-side (KHÔNG serialize ra client, BD-2)**.
- `HlgAnswerOption` (`HLG.AppHlgAnswerOptions`) — lựa chọn A/B/C/D (an toàn trả client).
- `HlgGameSession` (`HLG.AppHlgGameSessions`) — phiên chơi; `Score` là nguồn sự thật server-side.
- `HlgSessionAnswer` (`HLG.AppHlgSessionAnswers`) — câu trả lời đã chấm; nguồn đối soát khi /finish.
- `HlgReward` (`HLG.AppHlgRewards`) — quà đổi bằng điểm; `Type` physical/voucher; `StockQuantity`; `VoucherCode`.
- `HlgRewardHistory` (`HLG.AppHlgRewardHistories`) — lịch sử đổi quà per-user (pointDelta âm, status).
- `HlgShippingAddress` (`HLG.AppHlgShippingAddresses`) — địa chỉ giao hàng consumer (BD-6).
- `HlgRankingEvent` (`HLG.AppHlgRankingEvents`) — sự kiện xếp hạng (BD-5); điểm ranking tính động từ GameSession.Score.

## Permission (đã tạo)
- Tenant group `HlgManagement` (mọi permission `RequireFeatures(Hlg.Management)`) + Host group `HlgManagementHost`.
- 6 nhóm: `AppHlgUsers` (read), `AppHlgKnowledge`/`AppHlgGames`/`AppHlgRewards`/`AppHlgRanking` (CRUD), `AppHlgDashboard`.
- Host admin nhận tự động qua `MultiTenancyDataSeederContributor` (gán mọi permission Host cho role admin).
- Lưu ý: group Tenant chỉ hiện trong modal phân quyền khi feature `Hlg.Management` đã bật cho tenant.

## Files chính đã tạo
**Phase 0-1 (Auth & Profile):**
- Envelope: `Application.Contracts/AppDtos/Hlg/HlgApiResult.cs`
- Enums: `Domain.Shared/Enums/Hlg/{HlgGender,HlgCustomerType}.cs`
- Entity: `Domain/DomainModels/AppHlg/HlgUserProfile.cs`
- EF config: `EntityFrameworkCore/MultiTenancyDbContextModelCreatingExtensionsHlg.cs` (+ đăng ký DbContext)
- Feature: `Application.Contracts/Features/AppHlgFeatures/*`
- DTOs: `Application.Contracts/AppDtos/Hlg/{GamificationUserDto,ProfileDtos,HistoryDtos,HlgCustomerUpsertPayloadDto,HlgEnumMapper}.cs`
- Service: `Application/AppServices/Hlg/HlgProfileAppService.cs` (+ interface)
- Controller: `HttpApi/Controllers/HoaLinhGamificationController.cs`

**Phase 2 (Knowledge base):**
- Entities: `Domain/DomainModels/AppHlg/{HlgKnowledgeCategory,HlgProduct,HlgLearningProgress}.cs`
- DTOs: `Application.Contracts/AppDtos/Hlg/KnowledgeDtos.cs`
- Service: `Application/AppServices/Hlg/HlgKnowledgeAppService.cs` (+ interface)
- 5 endpoint knowledge trong controller; nối `profile/stats` + `learning-history` từ dữ liệu thật.

**Phase 3 (Games engine):**
- Enums: `Domain.Shared/Enums/Hlg/{HlgGameType,HlgGameStatus,HlgAnswerKey}.cs`
- Entities: `Domain/DomainModels/AppHlg/{HlgGame,HlgQuestion,HlgAnswerOption,HlgGameSession,HlgSessionAnswer}.cs`
- DTOs: `Application.Contracts/AppDtos/Hlg/GameDtos.cs`
- Service: `Application/AppServices/Hlg/HlgGameAppService.cs` (+ interface) — chấm điểm server-side (BD-2).
- 6 endpoint games trong controller.

**Phase 4 (Rewards & Shipping):**
- Enums: `Domain.Shared/Enums/Hlg/{HlgRewardType,HlgRewardHistoryStatus}.cs`
- Entities: `Domain/DomainModels/AppHlg/{HlgReward,HlgRewardHistory,HlgShippingAddress}.cs`
- DTOs: `Application.Contracts/AppDtos/Hlg/ShippingAddressPayloadDto.cs` (+ RewardDto/RewardHistoryItemDto từ Phase 3/1)
- Service: `Application/AppServices/Hlg/HlgRewardAppService.cs` (+ interface) — redeem ACID, phân luồng pharmacy/consumer.
- Endpoint shipping-address + rewards + redeem; nối `profile/reward-history` thật.

**Phase 5 (Ranking):**
- Entity: `Domain/DomainModels/AppHlg/HlgRankingEvent.cs`
- DTOs: `Application.Contracts/AppDtos/Hlg/RankingDtos.cs`
- Service: `Application/AppServices/Hlg/HlgRankingAppService.cs` (+ interface) — rank + isCurrentUser theo sự kiện (BD-5), tính từ GameSession.Score. Dùng tuple (tránh dynamic footgun).
- 2 endpoint ranking trong controller.

**Phase 6 (Live-feed realtime SignalR, BD-4):**
- Interface: `Application.Contracts/Realtime/IHlgLiveFeedNotifier.cs`
- Hub: `HttpApi/SignalR/HlgLiveFeedHub.cs` (AllowAnonymous, group theo gameId, `JoinGame`/`LeaveGame`)
- Notifier: `HttpApi/SignalR/HlgLiveFeedNotifier.cs` (event `hlg.live-feed.activity`)
- Đăng ký: DI trong `MultiTenancyHttpApiModule.cs`; MapHub `/signalr-hubs/hlg-live-feed` trong `Web/Program.cs`.
- Broadcast: `HlgGameAppService.AnswerAsync` (đúng) + `FinishAsync`, bọc try/catch (feedback_signalr_try_catch).

**Phase 7 (Backend hoàn thiện):**
- Sample data seeder: `Domain/AppHlg/HlgDataSeedContributor.cs` — seed dữ liệu mẫu (1 danh mục + 2 bài học, 1 game quiz + 3 câu hỏi, 2 quà, 1 sự kiện xếp hạng). Gate bằng feature `Hlg.Management`, idempotent (chỉ seed khi bảng Games rỗng), chạy trong context tenant.
- Permission provider (đã xong đợt trước).
- **CHƯA làm: Admin Razor UI** (CRUD Games/Questions, Knowledge, Rewards, Ranking, Users) — workstream lớn, làm theo từng nhóm entity ở các lượt sau. Menu contributor nên làm cùng lúc với admin pages (menu item phải trỏ tới page tồn tại).
- Ghi chú: các HLG AppService map thủ công (KHÔNG dùng AutoMapper profile riêng).

**Permission:** `Application.Contracts/Permissions/MultiTenancyPermissions.cs` (region HLG) + `MultiTenancyPermissionDefinitionProvider.cs` (region HLG) + localization en/vi.

## Migration đã sinh
- `AddHlgModule` — schema HLG + `AppHlgUserProfiles`.
- `AddHlgKnowledge` — 3 bảng knowledge (Categories/Products/LearningProgress).
- `AddHlgGames` — 5 bảng game (Games/Questions/AnswerOptions/GameSessions/SessionAnswers).
- `AddHlgRewards` — 3 bảng reward (Rewards/RewardHistories/ShippingAddresses).
- `AddHlgRanking` — 1 bảng ranking event (RankingEvents).
- SQL script idempotent tại `EntityFrameworkCore/Migrations/Scripts/{AddHlgModule,AddHlgKnowledge,AddHlgGames,AddHlgRewards,AddHlgRanking}.sql`.

## Việc runtime còn lại (không phải code)
- Áp migration `AddHlgKnowledge` + `AddHlgGames` + `AddHlgRewards` + `AddHlgRanking` (review SQL script trước — DB config là server dùng chung từ xa).
- Tạo tenant "Hoa Linh Miền Nam Gamification" qua `TenantProvisioningAppService` (database-per-tenant) + bật feature `Hlg.Management` → seeder tự chạy tạo dữ liệu mẫu.
- Re-seed host admin để nhận permission `HostAppHlg*` (restart DbMigrator/Web).

## Còn lại (workstream lớn, chưa làm)
- **Admin Razor UI**: trang CRUD nội bộ cho Games/Questions, Knowledge, Rewards, Ranking, Users (mỗi nhóm: Index cshtml + .cs + JS + create/edit modal). Kèm menu contributor + menu localization keys.
- Tích hợp UrBox thật khi phát voucher (hiện dùng `HlgReward.VoucherCode` sẵn có).
- Mapping game↔reward để `GameResult.reward` + `requiresShippingAddress` trả giá trị thật khi finish.
