using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLoyalties;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace Genora.MultiTenancy.AppServices.SalonBeauty;

public class SalonBeautyLoyaltyAppService : ApplicationService, ISalonBeautyLoyaltyAppService
{
    private readonly IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> _balanceRepository;
    private readonly IRepository<SalonBeautyCustomerLoyaltyTransaction, Guid> _transactionRepository;

    public SalonBeautyLoyaltyAppService(
        IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> balanceRepository,
        IRepository<SalonBeautyCustomerLoyaltyTransaction, Guid> transactionRepository)
    {
        _balanceRepository = balanceRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<CustomerLoyaltyBalanceDto> GetBalanceAsync(Guid customerId)
    {
        var balance = await _balanceRepository.FindAsync(x => x.CustomerId == customerId);
        if (balance == null)
        {
            balance = new SalonBeautyCustomerLoyaltyBalance
            {
                CustomerId = customerId,
                CurrentPoint = 0
            };
            await _balanceRepository.InsertAsync(balance);
        }

        return new CustomerLoyaltyBalanceDto
        {
            Id = balance.Id,
            CustomerId = balance.CustomerId,
            CurrentPoint = balance.CurrentPoint
        };
    }

    public async Task<CustomerLoyaltyBalanceDto> AddPointsAsync(Guid customerId, int points, string description)
    {
        var balance = await _balanceRepository.FindAsync(x => x.CustomerId == customerId);
        if (balance == null)
        {
            balance = new SalonBeautyCustomerLoyaltyBalance { CustomerId = customerId, CurrentPoint = 0 };
            balance.CurrentPoint += points;
            await _balanceRepository.InsertAsync(balance);
        }
        else
        {
            balance.CurrentPoint += points;
            await _balanceRepository.UpdateAsync(balance);
        }

        await _transactionRepository.InsertAsync(new SalonBeautyCustomerLoyaltyTransaction
        {
            CustomerId = customerId,
            Type = 1, // Add
            Point = points,
            Description = description
        });

        return new CustomerLoyaltyBalanceDto
        {
            Id = balance.Id,
            CustomerId = balance.CustomerId,
            CurrentPoint = balance.CurrentPoint
        };
    }

    public async Task<CustomerLoyaltyBalanceDto> DeductPointsAsync(Guid customerId, int points, string description)
    {
        var balance = await _balanceRepository.FindAsync(x => x.CustomerId == customerId)
            ?? throw new Volo.Abp.UserFriendlyException("Customer loyalty balance not found");

        if (balance.CurrentPoint < points)
        {
            throw new Volo.Abp.UserFriendlyException("Insufficient loyalty points");
        }

        balance.CurrentPoint -= points;
        await _balanceRepository.UpdateAsync(balance);

        await _transactionRepository.InsertAsync(new SalonBeautyCustomerLoyaltyTransaction
        {
            CustomerId = customerId,
            Type = 2, // Deduct
            Point = -points,
            Description = description
        });

        return new CustomerLoyaltyBalanceDto
        {
            Id = balance.Id,
            CustomerId = balance.CustomerId,
            CurrentPoint = balance.CurrentPoint
        };
    }
}
