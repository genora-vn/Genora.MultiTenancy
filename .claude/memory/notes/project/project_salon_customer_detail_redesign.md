---
name: project-salon-customer-detail-redesign
description: "Salon Beauty Customer Detail page redesign with KPI cards (TotalDeposit/Visit/AOV/Loyalty), 4 tabs, MembershipLevelLabel, NextTier, deposit ledger"
metadata: 
  node_type: memory
  type: project
  originSessionId: b0157f35-01a5-463a-9e01-3b5eb1102803
---

# Salon Beauty - Customer Detail page redesign + admin rename

## Backend (`SalonBeautyCustomerAppService.cs`)

### DTO mới (`SalonBeautyCustomerDto`)
- `MembershipLevelLabel` (Mới/Thân thiết/Vàng/Kim cương)
- `NextTierThreshold` + `NextTierLabel` (cho card AOV)
- `TotalDeposit`, `MonthlyDepositCurrent`, `MonthlyDepositChangePercent`
- `VisitFrequencyLabel` ("TB N tuần/lần" hoặc "TB N tháng/lần")

### Tier ladder
- `NEW < 1.000.000` → `REGULAR ≥ 1tr` → `VIP ≥ 10tr` → `DIAMOND ≥ 30tr`
- `ResolveNextTier(totalSpent)` → trả về (Threshold, Label) cho hạng kế tiếp

### Stats helpers
- `BuildDepositStatsAsync(customerIds)` — chỉ tính `Status=2 (Success)`, so sánh tháng hiện tại vs tháng trước → percent. `prev=0 + current>0` → +100%.
- `BuildBookingStatsAsync` — tính thêm `VisitFrequencyLabel` từ avg gap giữa 2 lần `BookingDate`. `<14 ngày` → tuần, `<60 ngày` → tuần, còn lại → tháng.

### API mới
- `GetPurchaseHistoryAsync(id, max)` — match `ProOrder.CustomerId == id || ProOrder.CustomerPhone == customer.Phone`, group items, build `ItemsSummary` (top 3 + "+N").
- `GetDepositLedgerAsync(id, max)` — merge `SalonBeautyDepositTransaction` + `SalonBeautyCustomerLoyaltyTransaction` (skip Type=1 để tránh double-count với deposit table). Sort by `EntryDate desc`.

## UI (`Pages/SalonBeautyCustomers/Detail.cshtml`)

### Layout
1. Hero: profile card (avatar 128px + status pill góc dưới-phải avatar + name + code pill nhẹ + tier badge gold riêng dòng + 2 contact pill có icon primary) + Next booking card (link `/SalonBeautyBookings/Detail?id={guid}`)
2. KPI grid 4 cards (clean: title + value + foot 1 dòng):
   - **TỔNG SỐ TIỀN ĐÃ NẠP** — kpi-foot delta-up/down (xanh/đỏ) "+12% so với tháng trước"
   - **SỐ LẦN GHÉ THĂM** — kpi-foot-muted "TB N tuần/lần"
   - **AOV** — kpi-foot-primary "Ngưỡng hạng VIP/Vàng/Kim cương"
   - **ĐIỂM TÍCH LŨY** — kpi-foot-tier (amber) icon star + "Thành viên Vàng"; value cũng amber
   - Card có border-bottom 2px primary/20, KHÔNG có icon box
3. Detail info card (left col 3, nền surface-container) + Tabs card (right col 9):
   - **Lịch sử đặt lịch** — service+stylist+amount+localized status (Chờ xác nhận / Đã xác nhận / Đang thực hiện / Hoàn thành / Đã hủy)
   - **Lịch sử mua hàng** — từ `AppProOrders`
   - **Tích điểm & Voucher** — table 4 cột (Mã/Giá trị/Hết hạn/Trạng thái) + 1 dòng "Chưa có voucher" (per design image2)
   - **Lịch sử nạp & Tiêu điểm** — table 6 cột: Mã GD / Ngày & giờ / Loại GD / Giá trị (VND) / Điểm / Ghi chú (per design image3, KHÔNG có cột Status riêng)

### CSS (`customer-shared.css`)
- Material 3 vars: `--md-primary #006db3`, `--md-tertiary #b45309` (amber/gold), `--md-on-surface-variant #64748b`, surface tones slate-50/100/200
- Tier-badge (gold) — `tier-gold`: amber bg + amber text + amber border (KHÔNG dùng gradient hay icon-box)
- Salon-kpi-card: bottom border 2px primary/20, value 24px font-weight 800, unit nhỏ uppercase, foot 12px
- Salon-info-card: nền `--md-surface-container` (slate-100), không box-shadow nặng
- Note box: border-left 4px tertiary (amber) + italic + bg white + shadow nhẹ
- Tabs: nav-link active có border-bottom 2px primary, hover bg surface-low
- Inner table: thead `--md-surface-high` slate-200; row hover bg surface lowest

### Status & ledger pill colors
- Booking status badges (uppercase, rounded-full): warning/info/primary/success/danger/muted
- Ledger entry pills: deposit/earn=green emerald, redeem=amber, adjust=indigo, refund=cyan
- Ledger point: pos green-600, neg red-600

## Rename
- `vi.json` + `en.json`:
  - `Menu:SalonBeautyServiceCategories`: "Danh mục dịch vụ" → "Loại dịch vụ"
  - `SalonBeautyServiceCategories:PageTitle`: "Quản lý loại dịch vụ" → "Loại dịch vụ"
  - `SalonBeautyTimeSlots:AddTimeSlot`: "Thêm mới time slot" → "Thêm mới lịch làm việc"
  - `MenuGroup:SalonBeautyAndTeeTimes`: "Cơ sở & Giờ hẹn" → "Cơ sở & Lịch làm việc"

## How to apply
- Khi thêm tab/KPI mới ở Customer Detail → reuse `salon-kpi-card` (clean, no icon box) + tier-badge từ `Model.TierColorClass()`. Tier label luôn dùng `MembershipLevelLabel` từ DTO, không hardcode.
- Khi cần fetch deposit/loyalty cho khách → gọi `GetDepositLedgerAsync` (đã merge), không query trực tiếp 2 repo trong Razor.
- Voucher tab phải giữ table headers (4 cột) khi data rỗng — không dùng empty box.

Related: [[project-salon-deposit-loyalty]] [[project-salon-beauty-miniapp-payment-endpoints]] [[project-proorder-customer-soft-reference]]
