using System;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.MiniApps;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLoyalties;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Enums;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.SalonBeauties.MiniApps;

public class MiniAppSalonBeautyLoyaltyAppService : ApplicationService, IMiniAppSalonBeautyLoyaltyAppService
{
    private readonly IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> _balanceRepository;
    private readonly IRepository<SalonBeautyCustomerLoyaltyTransaction, Guid> _ledgerRepository;

    public MiniAppSalonBeautyLoyaltyAppService(
        IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> balanceRepository,
        IRepository<SalonBeautyCustomerLoyaltyTransaction, Guid> ledgerRepository)
    {
        _balanceRepository = balanceRepository;
        _ledgerRepository = ledgerRepository;
    }

    public async Task<CustomerLoyaltyBalanceDto> GetBalanceMiniAppAsync(Guid customerId)
    {
        var balance = await _balanceRepository.FirstOrDefaultAsync(x => x.CustomerId == customerId);
        return new CustomerLoyaltyBalanceDto
        {
            Id = balance?.Id ?? Guid.Empty,
            CustomerId = customerId,
            CurrentPoint = balance?.CurrentPoint ?? 0
        };
    }

    public async Task<MiniAppCustomerLoyaltyDetailDto> GetDetailMiniAppAsync(Guid customerId, int maxResultCount = 10)
    {
        if (maxResultCount <= 0) maxResultCount = 10;
        if (maxResultCount > 50) maxResultCount = 50;

        var balance = await _balanceRepository.FirstOrDefaultAsync(x => x.CustomerId == customerId);

        var ledgerQuery = await _ledgerRepository.GetQueryableAsync();
        var recent = await AsyncExecuter.ToListAsync(
            ledgerQuery
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.CreationTime)
                .Take(maxResultCount));

        return new MiniAppCustomerLoyaltyDetailDto
        {
            CustomerId = customerId,
            CurrentPoint = balance?.CurrentPoint ?? 0,
            RecentTransactions = recent.Select(x => new MiniAppLoyaltyTransactionDto
            {
                Id = x.Id,
                Type = x.Type,
                TypeText = x.Type switch
                {
                    (byte)LoyaltyTransactionType.Deposit => "Nạp tiền",
                    (byte)LoyaltyTransactionType.Earn => "Tặng điểm",
                    (byte)LoyaltyTransactionType.Redeem => "Đổi quà / dùng dịch vụ",
                    (byte)LoyaltyTransactionType.Adjust => "Điều chỉnh",
                    (byte)LoyaltyTransactionType.Refund => "Hoàn điểm",
                    _ => "Khác"
                },
                Point = x.Point,
                BalanceBefore = x.BalanceBefore,
                BalanceAfter = x.BalanceAfter,
                ReferenceType = x.ReferenceType,
                ReferenceId = x.ReferenceId,
                Description = x.Description,
                CreationTime = x.CreationTime
            }).ToList()
        };
    }
}
