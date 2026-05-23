using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLoyalties;

public interface IMiniAppSalonBeautyLoyaltyAppService : IApplicationService
{
    Task<CustomerLoyaltyBalanceDto> GetBalanceMiniAppAsync(Guid customerId);
    Task<MiniAppCustomerLoyaltyDetailDto> GetDetailMiniAppAsync(Guid customerId, int maxResultCount = 10);
}

public class MiniAppCustomerLoyaltyDetailDto
{
    public Guid CustomerId { get; set; }
    public int CurrentPoint { get; set; }
    public List<MiniAppLoyaltyTransactionDto> RecentTransactions { get; set; } = new();
}

public class MiniAppLoyaltyTransactionDto
{
    public Guid Id { get; set; }
    public byte Type { get; set; }
    public string TypeText { get; set; } = string.Empty;
    public int Point { get; set; }
    public int BalanceBefore { get; set; }
    public int BalanceAfter { get; set; }
    public byte ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Description { get; set; }
    public DateTime CreationTime { get; set; }
}