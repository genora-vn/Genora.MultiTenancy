---
name: Calendar Slot Pricing API with PlayerNumber
description: GET /api/mini-app/get-calendar-slots/{id} now recalculates prices based on playerNumber parameter
type: project
originSessionId: 53ccfe3b-152e-4ab6-90ed-16ee467b8469
---
## API Enhancement Summary

Updated `/api/mini-app/get-calendar-slots/{id}` endpoint to support dynamic price calculation based on player count.

## Query Parameters

- `id` (route) - Calendar slot ID
- `customerId` (query) - Customer ID to determine customer type
- `numberHoles` (query) - Number of holes (9/18/27/36), default 18
- `playerNumber` (query) - Number of players, default 1

## Response Fields Added

**Pricing Fields:**
- `customerTypeCode` - Code of current customer type (MB, MBG, VIS, etc.)
- `customerTypePrice` - Unit price for customer's type
- `originalPrice` - Original/base unit price
- `visitorPrice` - Visitor type unit price
- `discountPercent` - Discount percentage
- `memberGuestPrice` - Member guest unit price (when applicable)

**Calculated Bill Totals (based on playerNumber):**
- `customerBillTotalPrice` - Total amount customer pays
- `originalBillTotalPrice` - Total original price
- `discountTotalPrice` - Total discount amount

**Member Configuration:**
- `isMemberSupported` - Whether golf course supports membership
- `maxMemberGuest` - Max member guests allowed

## Pricing Logic

### When IsMemberSupported = true AND customer is Member (MB)
```
CustomerBillTotalPrice = 
    MB_price 
    + (MBG_price × min(maxMemberGuest, playerNumber-1)) 
    + (visitorPrice × max(0, playerNumber - maxMemberGuest - 1))
```

### When IsMemberSupported = false OR customer is not Member
```
CustomerBillTotalPrice = customerTypePrice × playerNumber
OriginalBillTotalPrice = currentCustomerType.OriginalPrice × playerNumber
```

## Key Implementation Details

**InputClass:** `GetMiniAppCalendarSlotDetailInput`
- Contains: Id, CustomerId, NumberHoles, PlayerNumber

**DTOs Updated:**
- `AppCalendarSlotDto` - Extended with price calculation fields

**Service Methods:**
- `GetMiniAppAsync(Guid id)` - Original overload (no price calc)
- `GetMiniAppAsync(GetMiniAppCalendarSlotDetailInput input)` - New overload with price calc

**Controller Endpoint (MiniAppController):**
```csharp
[HttpGet("get-calendar-slots/{id}")]
public Task<AppCalendarSlotDto> GetCalendarSlotAsync(Guid id, 
    [FromQuery] Guid? customerId, 
    [FromQuery] short? numberHoles = 18, 
    [FromQuery] int playerNumber = 1)
```

## Important: Non-Member Price Calculation Fix

**Issue:** When IsMemberSupported=false, API was always using VisitorPrice instead of customer's actual price.

**Solution:** Use `currentCustomerType.OriginalPrice` with fallback to VIS price
- MB customer uses MB.OriginalPrice (not VIS)
- VIS customer uses VIS.OriginalPrice
- Other types use their respective OriginalPrice

**Applied To:** Both list (`GetListMiniAppAsync`) and detail (`GetMiniAppAsync`) APIs

## Example Response

```json
{
  "id": "89213006-f956-fc66-a95a-3a205eaf2ced",
  "customerTypeCode": "MB",
  "customerTypePrice": 1800000,
  "originalPrice": 2000000,
  "visitorPrice": 2400000,
  "memberGuestPrice": 2200000,
  "isMemberSupported": true,
  "maxMemberGuest": 3,
  
  "customerBillTotalPrice": 8600000,  // 1.8M + 2×2.2M + 0×2.4M (4 players)
  "originalBillTotalPrice": 9400000,   // 2M + 2×2.2M + 0×2.8M
  "discountTotalPrice": 800000         // 9.4M - 8.6M
}
```

## Commits

- `9c2c67e` - Adjust API to recalculate slot prices based on player count
- `cf274f1` - Add customerTypeCode field to GET /api/mini-app/get-calendar-slots/{id} response
- `ec51ebb` - Fix pricing calculation when IsMemberSupported=false
