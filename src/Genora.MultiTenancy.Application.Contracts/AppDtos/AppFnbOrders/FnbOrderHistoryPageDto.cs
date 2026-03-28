using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppFnbOrders;

public class FnbOrderHistoryPageDto
{
    public Guid OrderId { get; set; }
    public string OrderCode { get; set; } = default!;
    public string? CustomerName { get; set; }
    public string? CustomerPhoneMasked { get; set; }
    public string? CustomerTypeName { get; set; }
    public string BagTag { get; set; } = default!;
    public FnbServiceStatus ServiceStatus { get; set; }
    public FnbPaymentStatus PaymentStatus { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastActivityTime { get; set; }
    public int TotalActions { get; set; }

    public string? CurrentFilterActionType { get; set; }

    public List<FnbOrderHistoryActionTypeOptionDto> ActionTypeOptions { get; set; } = new();

    // Giữ lại để không vỡ code cũ nếu đang dùng
    public List<FnbOrderHistoryItemDto> Activities { get; set; } = new();

    public PagedResultDto<FnbOrderHistoryItemDto> PagedActivities { get; set; }
        = new PagedResultDto<FnbOrderHistoryItemDto>(0, new List<FnbOrderHistoryItemDto>());
}

public class FnbOrderHistoryItemDto
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

public class FnbOrderHistoryActionTypeOptionDto
{
    public string Value { get; set; } = default!;
    public string Text { get; set; } = default!;
}