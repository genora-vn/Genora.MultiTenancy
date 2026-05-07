using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.SalonBeauty;

public interface ISalonBeautyLoyaltyAppService : IApplicationService
{
    Task<CustomerLoyaltyBalanceDto> GetBalanceAsync(Guid customerId);
    Task<CustomerLoyaltyBalanceDto> AddPointsAsync(Guid customerId, int points, string description);
    Task<CustomerLoyaltyBalanceDto> DeductPointsAsync(Guid customerId, int points, string description);
}

public class CustomerLoyaltyBalanceDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public int CurrentPoint { get; set; }
}
