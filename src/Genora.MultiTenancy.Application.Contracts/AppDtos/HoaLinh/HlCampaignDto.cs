namespace Genora.MultiTenancy.AppDtos.HoaLinh;

/// <summary>
/// DTO chiến dịch khách hàng từ API Hoa Linh DMS
/// </summary>
public class HlCampaignDto
{
    public string? CampaignCode { get; set; }
    public string? CampaignName { get; set; }
    public string? CustCode { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }

    // Bổ sung trường từ API chi tiết chiến dịch (CustomerCampaigns/{custCode})
    public int? CampaignPeriod { get; set; }
    public string? DisplayType { get; set; }
    public decimal? AccumulatedSales { get; set; }
    public int? AccumulatedPoints { get; set; }
    public string? MembershipTier { get; set; }
    public string? VoucherCode { get; set; }
    public string? VoucherName { get; set; }

    // Bổ sung field voucher chi tiết (2026-07)
    public int? VoucherType { get; set; }
    public decimal? VoucherValue { get; set; }
}
