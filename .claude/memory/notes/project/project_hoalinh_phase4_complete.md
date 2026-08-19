---
name: project-hoalinh-phase4-complete
description: "Phase 4 CRUD hoàn thành — HlOrderAppService + HlGiftExchangeAppService, DTOs, đọc/ghi DB Genora (AppHlOrders + AppHlGiftExchanges)"
metadata: 
  node_type: memory
  type: project
  originSessionId: dcf06de4-47b3-4551-89dd-8fabec86f6de
---

## Hoa Linh Phase 4 Complete — Admin CRUD Orders + GiftExchanges (2026-06-24)

### DTOs (Application.Contracts/AppDtos/HoaLinh/):
- HlOrderDtos.cs — HlOrderDto, HlOrderItemDto, HlOrderUpdateStatusDto, HlOrderCancelDto, HlOrderFilterDto
- HlGiftExchangeDtos.cs — HlGiftExchangeDto, HlGiftExchangeFilterDto, HlGiftExchangeApproveDto

### Interfaces (IHlOrderAppService.cs):
- IHlOrderAppService: GetListAsync, GetAsync, UpdateStatusAsync, CancelAsync
- IHlGiftExchangeAppService: GetListAsync, GetAsync, ApproveOrRejectAsync

### Implementations:

**HlOrderAppService:**
- GetListAsync — filter by text/deliveryStatus/paymentStatus/dateRange, paged
- GetAsync — WithDetails (eager load Items)
- UpdateStatusAsync — update delivery/payment status + append InternalNote
- CancelAsync — set Cancelled + CancelNote + CancelledBy + CancelledAt
- Permission: AppHlOrders.Default (read), AppHlOrders.Edit (update/cancel)

**HlGiftExchangeAppService:**
- GetListAsync — filter by text/status, paged
- GetAsync — single record
- ApproveOrRejectAsync — validate Pending → Approved/Rejected, set ApprovedBy/At
- Permission: AppHlGiftExchange.Default (read), AppHlGiftExchange.Edit (approve/reject)

### JS Proxy path:
```
genora.multiTenancy.appServices.hoaLinh.hlOrder.getList(input)
genora.multiTenancy.appServices.hoaLinh.hlOrder.get(id)
genora.multiTenancy.appServices.hoaLinh.hlOrder.updateStatus(input)
genora.multiTenancy.appServices.hoaLinh.hlOrder.cancel(input)

genora.multiTenancy.appServices.hoaLinh.hlGiftExchange.getList(input)
genora.multiTenancy.appServices.hoaLinh.hlGiftExchange.get(id)
genora.multiTenancy.appServices.hoaLinh.hlGiftExchange.approveOrReject(input)
```

**Why:** Orders + GiftExchanges là 2 entities duy nhất lưu DB Genora, cần CRUD. Các entities khác gọi trực tiếp API HL.
**How to apply:** Admin UI gọi proxy methods, Mini App (Phase 5) sẽ tạo records qua HoaLinhMiniAppController.

[[project-hoalinh-phase3-complete]] [[project-hoalinh-phase1-complete]]
