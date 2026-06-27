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
}
