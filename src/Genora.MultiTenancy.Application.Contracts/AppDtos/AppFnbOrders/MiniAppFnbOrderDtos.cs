using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppFnbOrders;
public class GetMiniAppFnbOrderListInput : PagedAndSortedResultRequestDto
{
    public Guid? CustomerId { get; set; }
    public string? BagTag { get; set; }
}

public class MiniAppFnbOrderListDto : ZaloBaseResponse
{
    public PagedResultDto<MiniAppFnbOrderData> Data { get; set; } = null!;
}

public class MiniAppFnbOrderDetailDto : ZaloBaseResponse
{
    public MiniAppFnbOrderData Data { get; set; } = null!;
}

public class MiniAppFnbOrderData
{
    public Guid Id { get; set; }
    public string OrderCode { get; set; } = null!;
    public string BagTag { get; set; } = null!;
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerPhoneMasked { get; set; }
    public decimal TotalAmount { get; set; }
    public FnbServiceStatus ServiceStatus { get; set; }
    public FnbPaymentStatus PaymentStatus { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? Note { get; set; }
    public DateTime CreationTime { get; set; }

    public int TotalQuantity { get; set; }
    public int ItemCount { get; set; }
    public string? CancelReason { get; set; }
    public string? CancelNote { get; set; }
    public string? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public List<MiniAppFnbOrderItemData> Items { get; set; } = new();
}

public class MiniAppFnbOrderItemData
{
    public Guid Id { get; set; }
    public Guid? ItemId { get; set; }
    public string ItemName { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public string? Note { get; set; }

    public decimal LineTotal { get; set; }
    public string? CategoryName { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsAvailable { get; set; }
    public bool? IsActive { get; set; }
}