using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Genora.MultiTenancy.Enums;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.HoaLinh;

public class HlConfirmGiftInput
{
    [Required, StringLength(20)] public string PhoneNumber { get; set; } = null!;
    [Required, StringLength(100)] public string CustCode { get; set; } = null!;
    [Required, StringLength(100)] public string CampaignCode { get; set; } = null!;
    [Range(0, int.MaxValue)] public int CampaignPeriod { get; set; }
    [Required, StringLength(100)] public string VoucherCode { get; set; } = null!;
    [StringLength(1000)] public string? Note { get; set; }
}

public class HlGiftReceiptDto : EntityDto<Guid>
{
    public string ReceiptCode { get; set; } = null!;
    public string CustCode { get; set; } = null!;
    public string CustName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Address { get; set; }
    public string CampaignCode { get; set; } = null!;
    public string? CampaignName { get; set; }
    public int CampaignPeriod { get; set; }
    public DateTime? CampaignStartDate { get; set; }
    public DateTime? CampaignEndDate { get; set; }
    public string VoucherCode { get; set; } = null!;
    public string VoucherName { get; set; } = null!;
    public int VoucherType { get; set; }
    public decimal VoucherValue { get; set; }
    public int Quantity { get; set; }
    public HlGiftReceiptStatus Status { get; set; }
    public bool IsConfirmed => Status == HlGiftReceiptStatus.Confirmed;
    public DateTime ConfirmedAt { get; set; }
    public string? MembershipTier { get; set; }
    public decimal? AccumulatedSales { get; set; }
    public int? AccumulatedPoints { get; set; }
    public string? DsrCode { get; set; }
    public string? DsrName { get; set; }
    public string? DistributorCode { get; set; }
    public string? DistributorName { get; set; }
    public string Source { get; set; } = null!;
    public string? Note { get; set; }
    public DateTime CreationTime { get; set; }
}

public class HlGiftReceiptFilter : PagedResultRequestDto
{
    [StringLength(250)] public string? Filter { get; set; }
    [StringLength(100)] public string? CustCode { get; set; }
    [StringLength(20)] public string? PhoneNumber { get; set; }
    [StringLength(100)] public string? CampaignCode { get; set; }
    [StringLength(100)] public string? VoucherCode { get; set; }
    [Range(0, int.MaxValue)] public int? CampaignPeriod { get; set; }
    public HlGiftReceiptStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class HlGiftReceiptHistoryInput : PagedResultRequestDto
{
    [Required, StringLength(20)] public string PhoneNumber { get; set; } = null!;
    [Required, StringLength(100)] public string CustCode { get; set; } = null!;
    [StringLength(100)] public string? CampaignCode { get; set; }
    [Range(0, int.MaxValue)] public int? CampaignPeriod { get; set; }
}

public interface IMiniAppHlGiftReceiptService : IApplicationService
{
    Task<HlGiftReceiptDto> ConfirmAsync(HlConfirmGiftInput input);
    Task<PagedResultDto<HlGiftReceiptDto>> GetHistoryAsync(HlGiftReceiptHistoryInput input);
}

public interface IHlGiftReceiptAdminAppService : IApplicationService
{
    Task<PagedResultDto<HlGiftReceiptDto>> GetListAsync(HlGiftReceiptFilter input);
    Task<HlGiftReceiptDto> GetAsync(Guid id);
    Task<IRemoteStreamContent> ExportAsync(HlGiftReceiptFilter input);
}
