using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLoyaltyConfigs;

public interface ISalonBeautyLoyaltyConfigAppService : IApplicationService
{
    Task<SalonBeautyLoyaltyConfigDto> GetAsync();
    Task<SalonBeautyLoyaltyConfigDto> UpdateAsync(SalonBeautyLoyaltyConfigDto input);
}

public class SalonBeautyLoyaltyConfigDto
{
    /// <summary>1 điểm = bao nhiêu VND. Default = 1000.</summary>
    [Range(1, 1_000_000)]
    public decimal ExchangeRate { get; set; } = 1000m;
}
