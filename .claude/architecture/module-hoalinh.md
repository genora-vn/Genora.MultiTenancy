# Architecture — Module Hoa Linh (Dược phẩm)

> Nguồn: `project_hoalinh_brd_overview.md`, `project_hoalinh_data_integration_pattern.md`,
> `project_hoalinh_phase1..7_complete.md`, `project_hl_*.md`, `project_urbox_integration.md`,
> `project_zalo_oa_articles.md`. Chi tiết đầy đủ trong `../memory/notes/project/`.

## Phạm vi (BRD)
Mini App (8 module) + Admin Portal (10 module) + tích hợp API DMS Hoa Linh. Prefix entity: `AppHl`.

## Data Integration Pattern (CỐT LÕI)
Phân biệt rõ **data ownership** để tránh conflict khi sync:
1. **Master từ Hoa Linh DMS** → Pull về Genora → **Read-only** trên Admin.
2. **Genora tạo ra** (Orders, Đổi quà, Users, Tin tức, Banner) → **Full CRUD**.
3. Dữ liệu Mini App lưu tại Genora, push sang Hoa Linh khi cần.

### 10 nhóm dữ liệu
| Entity | Nguồn | Chiều | Admin quyền |
|--------|-------|-------|-------------|
| AppHlProductCategories | HL API | Pull | Read-only |
| AppHlProducts | HL API | Pull | Read-only |
| AppHlCustomers | HL API | Pull | Read-only |
| AppHlCustomerBranches | HL API | Pull | Read-only |
| AppHlOrders | Genora (MiniApp) | Push to HL | Full CRUD |
| AppHlMiniAppUsers | Genora | Local | Full CRUD |
| AppHlLoyaltyPoints | HL API | Pull | Read-only |
| AppHlLoyaltyTiers | HL API | Pull | Read-only |
| AppHlPointHistories | HL API | Pull | Read-only |
| AppHlGiftExchanges | Genora | Local | Full CRUD |

- **Sync:** Pull qua scheduled job/manual (ghi `SyncLog`); Push real-time khi tạo đơn.
- Entities từ HL luôn có ExternalId/ExternalCode mapping; Admin UI read-only.

## Các phase
- **Phase 1:** 4 enum, 4 entity (schema HL), feature `AllowHoaLinhModule`, 7 permission pair, menu order 50.
- **Phase 2:** `IHlApiClientService`, HttpClient "HoaLinhDms", X-API-Key, log `AppHlApiLogs`.
- **Phase 3:** Admin Portal UI 6 page + Dashboard, `HlAdminAppService` read-only real-time.
- **Phase 4:** CRUD `HlOrderAppService` + `HlGiftExchangeAppService` (đọc DB Genora).
- **Phase 5:** `HoaLinhMiniAppController` 13 endpoints.
- **Phase 7:** Dashboard + data-level auth `HlDataAccessService` (user→dsr_code, AppSettings `HoaLinh.UserDsrCode.{userId}`).

## Loyalty (điểm thưởng)
- `HlPointBatch` (lô điểm +1 năm, FIFO) + `HlPointTransaction` (sổ cái) + `Customer.BonusAmount`.
- `HlPointExpireWorker` chạy mỗi giờ; gift-exchange gọi `SpendAsync` trừ điểm FIFO.
- Tỉ lệ `HlLoyaltyOptions` (PointRate/AmountRate). Migration 20260709064009.
- MiniApp: `loyalty/redeem` + `balance` + `history`; Admin "Lịch sử điểm thưởng".

## Tích hợp
- **UrBox eVoucher:** `IUrBoxService` (tra cứu GET, cartPayVoucher POST + Signature RSA-SHA256 .NET9); redeem lưu `HlGiftExchange`, success trừ `Customer.BonusAmount`. `HlGiftExchangeStatus`: 0=Failed/1=Success/2=Processing/3=Used.
- **Zalo OA Articles:** `GetArticleList/DetailAsync` (GET openapi.zalo.me, token ZaloAuth fallback), cache per-tenant `IDistributedCache` (`Zalo:NewsCacheMinutes=5`); endpoint `api/mini-app/hl/news` + `news/{id}`.
- **Customer registration:** `HlCustomerAppService` upsert theo phone; HL DMS→CustCode+HoaLinh(source=5), chưa có→HLKH{D6}+ZaloMiniApp.
