using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppProOrders;

public class ProOrderHistoryPageDto
{
    public Guid OrderId { get; set; }
    public string OrderCode { get; set; } = default!;
    public string? CustomerName { get; set; }
    public string? CustomerPhoneMasked { get; set; }
    public string? CustomerTypeName { get; set; }
    public string BagTag { get; set; } = default!;
    public ProServiceStatus ServiceStatus { get; set; }
    public ProPaymentStatus PaymentStatus { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastActivityTime { get; set; }
    public int TotalActions { get; set; }

    public string? CurrentFilterActionType { get; set; }

    public List<ProOrderHistoryActionTypeOptionDto> ActionTypeOptions { get; set; } = new();

    public PagedResultDto<ProOrderHistoryItemDto> PagedActivities { get; set; }
        = new PagedResultDto<ProOrderHistoryItemDto>(0, new List<ProOrderHistoryItemDto>());
}

public class ProOrderHistoryItemDto
{
    public DateTime Time { get; set; }
    public string PerformedBy { get; set; } = default!;
    public string ActionType { get; set; } = default!;
    public string ActionTypeText { get; set; } = default!;
    public string ActionTypeClass { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public bool IsDanger { get; set; }
}

public class ProOrderHistoryActionTypeOptionDto
{
    public string Value { get; set; } = default!;
    public string Text { get; set; } = default!;
}
