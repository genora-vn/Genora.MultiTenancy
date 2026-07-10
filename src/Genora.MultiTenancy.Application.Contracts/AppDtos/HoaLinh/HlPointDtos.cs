using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.HoaLinh;

/// <summary>
/// Input đổi điểm/tiền từ chiến dịch (Mini App gọi vào).
/// </summary>
public class HlRedeemPointInput
{
    [Required]
    public string CustomerCode { get; set; } = null!;
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }

    /// <summary>Mã chiến dịch cần đổi</summary>
    [Required]
    public string CampaignCode { get; set; } = null!;

    /// <summary>Đơn vị đổi: 1=Point (điểm tích lũy), 2=Amount (tiền tích lũy)</summary>
    public int Unit { get; set; } = 1;
}

/// <summary>DTO lô điểm/tiền đã đổi.</summary>
public class HlPointBatchDto
{
    public Guid Id { get; set; }
    public string? BatchCode { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CampaignCode { get; set; }
    public string? CampaignName { get; set; }
    public int? CampaignPeriod { get; set; }
    public string? DisplayType { get; set; }
    public string? MembershipTier { get; set; }
    public int Unit { get; set; }
    public string? UnitText { get; set; }
    public decimal SourceValue { get; set; }
    public decimal ConvertedValue { get; set; }
    public decimal RemainingValue { get; set; }
    public int Status { get; set; }
    public string? StatusText { get; set; }
    public DateTime ExchangedAt { get; set; }
    public DateTime ExpireDate { get; set; }
}

/// <summary>DTO giao dịch điểm (sổ cái).</summary>
public class HlPointTransactionDto
{
    public Guid Id { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public int Type { get; set; }
    public string? TypeText { get; set; }
    public int Unit { get; set; }
    public string? UnitText { get; set; }
    public decimal Value { get; set; }
    public decimal BalancePointAfter { get; set; }
    public decimal BalanceAmountAfter { get; set; }
    public Guid? BatchId { get; set; }
    public string? RefCode { get; set; }
    public string? Description { get; set; }
    public DateTime CreationTime { get; set; }
}

/// <summary>Số dư điểm/tiền + danh sách lô còn hiệu lực (cho Mini App).</summary>
public class HlPointBalanceDto
{
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public decimal BonusPoint { get; set; }
    public decimal BonusAmount { get; set; }
    public List<HlPointBatchDto> ActiveBatches { get; set; } = new();
}

/// <summary>Filter lịch sử điểm (admin).</summary>
public class HlPointHistoryFilter
{
    public string? Search { get; set; }
    public int? Type { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}
