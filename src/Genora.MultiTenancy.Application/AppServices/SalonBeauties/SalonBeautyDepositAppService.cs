using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyDeposits;
using Genora.MultiTenancy.AppServices.SalonBeauty;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;

namespace Genora.MultiTenancy.AppServices.SalonBeauties;

[Authorize]
public class SalonBeautyDepositAppService :
    FeatureProtectedCrudAppService<
        SalonBeautyDepositTransaction,
        SalonBeautyDepositDto,
        Guid,
        GetSalonBeautyDepositListInput,
        CreateSalonBeautyDepositDto,
        UpdateSalonBeautyDepositDto>,
    ISalonBeautyDepositAppService
{
    protected override string FeatureName => string.Empty;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.SalonBeautyDeposits.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostSalonBeautyDeposits.Default;

    private readonly IRepository<SalonBeautyDepositTransaction, Guid> _depositRepository;
    private readonly IRepository<SalonBeautyLoyaltyBonusTier, Guid> _bonusTierRepository;
    private readonly IRepository<SalonBeautyCustomer, Guid> _customerRepository;
    private readonly IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> _balanceRepository;
    private readonly IRepository<SalonBeautyCustomerLoyaltyTransaction, Guid> _ledgerRepository;
    private readonly ISettingProvider _settingProvider;

    public SalonBeautyDepositAppService(
        IRepository<SalonBeautyDepositTransaction, Guid> depositRepository,
        IRepository<SalonBeautyLoyaltyBonusTier, Guid> bonusTierRepository,
        IRepository<SalonBeautyCustomer, Guid> customerRepository,
        IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> balanceRepository,
        IRepository<SalonBeautyCustomerLoyaltyTransaction, Guid> ledgerRepository,
        ISettingProvider settingProvider,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(depositRepository, currentTenant, featureChecker)
    {
        _depositRepository = depositRepository;
        _bonusTierRepository = bonusTierRepository;
        _customerRepository = customerRepository;
        _balanceRepository = balanceRepository;
        _ledgerRepository = ledgerRepository;
        _settingProvider = settingProvider;
    }

    public override async Task<PagedResultDto<SalonBeautyDepositDto>> GetListAsync(GetSalonBeautyDepositListInput input)
    {
        await CheckDepositPolicyAsync(
            MultiTenancyPermissions.SalonBeautyDeposits.Default,
            MultiTenancyPermissions.HostSalonBeautyDeposits.Default);

        input.MaxResultCount = input.MaxResultCount <= 0 ? 10 : Math.Min(input.MaxResultCount, 100);

        var query = await _depositRepository.GetQueryableAsync();

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var keyword = input.FilterText!.Trim();
            query = query.Where(x => x.TransactionCode.Contains(keyword) ||
                (x.ReferenceCode != null && x.ReferenceCode.Contains(keyword)));
        }

        if (input.CustomerId.HasValue)
            query = query.Where(x => x.CustomerId == input.CustomerId.Value);

        if (input.Status.HasValue)
            query = query.Where(x => x.Status == input.Status.Value);

        if (input.PaymentMethod.HasValue)
            query = query.Where(x => x.PaymentMethod == input.PaymentMethod.Value);

        if (input.DateFrom.HasValue)
        {
            var from = input.DateFrom.Value.Date;
            query = query.Where(x => x.CreationTime >= from);
        }

        if (input.DateTo.HasValue)
        {
            var to = input.DateTo.Value.Date.AddDays(1);
            query = query.Where(x => x.CreationTime < to);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var deposits = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount));

        // Lookup dictionaries — tránh cross-aggregate join hang trên multi-tenant DB
        var customerIds = deposits.Select(x => x.CustomerId).Distinct().ToList();
        var tierIds = deposits.Where(x => x.BonusTierId.HasValue).Select(x => x.BonusTierId!.Value).Distinct().ToList();

        var customerQ = await _customerRepository.GetQueryableAsync();
        var customers = customerIds.Count == 0
            ? new List<SalonBeautyCustomer>()
            : await AsyncExecuter.ToListAsync(customerQ.Where(c => customerIds.Contains(c.Id)));
        var customerDict = customers.ToDictionary(x => x.Id);

        var tierQ = await _bonusTierRepository.GetQueryableAsync();
        var tiers = tierIds.Count == 0
            ? new List<SalonBeautyLoyaltyBonusTier>()
            : await AsyncExecuter.ToListAsync(tierQ.Where(t => tierIds.Contains(t.Id)));
        var tierDict = tiers.ToDictionary(x => x.Id);

        var dtos = deposits.Select(d => MapToDto(
            d,
            customerDict.GetValueOrDefault(d.CustomerId),
            d.BonusTierId.HasValue ? tierDict.GetValueOrDefault(d.BonusTierId.Value) : null
        )).ToList();

        return new PagedResultDto<SalonBeautyDepositDto>(totalCount, dtos);
    }

    public override async Task<SalonBeautyDepositDto> GetAsync(Guid id)
    {
        await CheckDepositPolicyAsync(
            MultiTenancyPermissions.SalonBeautyDeposits.Default,
            MultiTenancyPermissions.HostSalonBeautyDeposits.Default);

        var deposit = await _depositRepository.GetAsync(id);
        var customer = await _customerRepository.FindAsync(deposit.CustomerId);
        SalonBeautyLoyaltyBonusTier? tier = null;
        if (deposit.BonusTierId.HasValue)
            tier = await _bonusTierRepository.FindAsync(deposit.BonusTierId.Value);
        return MapToDto(deposit, customer, tier);
    }

    public async Task<DepositPreviewResultDto> PreviewAsync(decimal amount)
    {
        await CheckDepositPolicyAsync(
            MultiTenancyPermissions.SalonBeautyDeposits.Default,
            MultiTenancyPermissions.HostSalonBeautyDeposits.Default);

        if (amount < 1000)
            throw new UserFriendlyException("Số tiền nạp phải >= 1.000đ.");

        var rate = await GetExchangeRateAsync();
        var basePoint = (int)Math.Floor(amount / rate);
        var (bonusPoint, tier) = await ResolveBonusTierAsync(amount);

        return new DepositPreviewResultDto
        {
            ExchangeRate = rate,
            BasePoint = basePoint,
            BonusPoint = bonusPoint,
            TotalPoint = basePoint + bonusPoint,
            BonusTierId = tier?.Id,
            BonusTierName = tier?.Name
        };
    }

    public override async Task<SalonBeautyDepositDto> CreateAsync(CreateSalonBeautyDepositDto input)
    {
        await CheckDepositPolicyAsync(
            MultiTenancyPermissions.SalonBeautyDeposits.Create,
            MultiTenancyPermissions.HostSalonBeautyDeposits.Create);

        if (input.Amount < 1000)
            throw new UserFriendlyException("Số tiền nạp phải >= 1.000đ.");

        var customer = await _customerRepository.FindAsync(input.CustomerId)
            ?? throw new UserFriendlyException("Khách hàng không tồn tại.");

        var rate = await GetExchangeRateAsync();
        var basePoint = (int)Math.Floor(input.Amount / rate);
        var (bonusPoint, tier) = await ResolveBonusTierAsync(input.Amount);

        var deposit = new SalonBeautyDepositTransaction(
            GuidGenerator.Create(),
            await GenerateTransactionCodeAsync(),
            input.CustomerId,
            input.Amount)
        {
            TenantId = CurrentTenant.Id,
            ExchangeRate = rate,
            BasePoint = basePoint,
            BonusPoint = bonusPoint,
            TotalPoint = basePoint + bonusPoint,
            BonusTierId = tier?.Id,
            PaymentMethod = input.PaymentMethod,
            ReferenceCode = NullIfWhiteSpace(input.ReferenceCode),
            Note = NullIfWhiteSpace(input.Note),
            Status = (byte)DepositStatus.Pending
        };

        await _depositRepository.InsertAsync(deposit, autoSave: true);
        return MapToDto(deposit, customer, tier);
    }

    public override async Task<SalonBeautyDepositDto> UpdateAsync(Guid id, UpdateSalonBeautyDepositDto input)
    {
        await CheckDepositPolicyAsync(
            MultiTenancyPermissions.SalonBeautyDeposits.Edit,
            MultiTenancyPermissions.HostSalonBeautyDeposits.Edit);

        if (input.Amount < 1000)
            throw new UserFriendlyException("Số tiền nạp phải >= 1.000đ.");

        var deposit = await _depositRepository.GetAsync(id);
        if (deposit.Status != (byte)DepositStatus.Pending)
            throw new UserFriendlyException("Chỉ có thể cập nhật giao dịch ở trạng thái Chờ duyệt.");

        var rate = await GetExchangeRateAsync();
        var basePoint = (int)Math.Floor(input.Amount / rate);
        var (bonusPoint, tier) = await ResolveBonusTierAsync(input.Amount);

        deposit.Amount = input.Amount;
        deposit.ExchangeRate = rate;
        deposit.BasePoint = basePoint;
        deposit.BonusPoint = bonusPoint;
        deposit.TotalPoint = basePoint + bonusPoint;
        deposit.BonusTierId = tier?.Id;
        deposit.PaymentMethod = input.PaymentMethod;
        deposit.ReferenceCode = NullIfWhiteSpace(input.ReferenceCode);
        deposit.Note = NullIfWhiteSpace(input.Note);

        await _depositRepository.UpdateAsync(deposit, autoSave: true);

        var customer = await _customerRepository.FindAsync(deposit.CustomerId);
        return MapToDto(deposit, customer, tier);
    }

    public async Task<SalonBeautyDepositDto> ApproveAsync(Guid id)
    {
        await CheckDepositPolicyAsync(
            MultiTenancyPermissions.SalonBeautyDeposits.Approve,
            MultiTenancyPermissions.HostSalonBeautyDeposits.Approve);

        var deposit = await _depositRepository.GetAsync(id);
        if (deposit.Status != (byte)DepositStatus.Pending)
            throw new UserFriendlyException("Chỉ giao dịch Chờ duyệt mới có thể duyệt.");

        // Dùng ambient UoW của ApplicationService — không mở nested UoW vì gây isolate
        // không nhìn thấy data từ ngoài trong scope multi-tenant separate DB.

        // 1. Tìm/khởi tạo balance + cộng điểm
        var balance = await _balanceRepository.FirstOrDefaultAsync(x => x.CustomerId == deposit.CustomerId);
        int balanceBefore;
        if (balance == null)
        {
            balanceBefore = 0;
            balance = new SalonBeautyCustomerLoyaltyBalance
            {
                CustomerId = deposit.CustomerId,
                CurrentPoint = deposit.TotalPoint,
                TenantId = CurrentTenant.Id
            };
            await _balanceRepository.InsertAsync(balance, autoSave: true);
        }
        else
        {
            balanceBefore = balance.CurrentPoint;
            balance.CurrentPoint = balanceBefore + deposit.TotalPoint;
            await _balanceRepository.UpdateAsync(balance, autoSave: true);
        }

        // 2. Ghi ledger entry
        await _ledgerRepository.InsertAsync(new SalonBeautyCustomerLoyaltyTransaction
        {
            TenantId = CurrentTenant.Id,
            CustomerId = deposit.CustomerId,
            Type = (byte)LoyaltyTransactionType.Deposit,
            Point = deposit.TotalPoint,
            BalanceBefore = balanceBefore,
            BalanceAfter = balance.CurrentPoint,
            ReferenceType = (byte)LoyaltyReferenceType.Deposit,
            ReferenceId = deposit.Id,
            Description = $"Nạp tiền {deposit.TransactionCode}: +{deposit.TotalPoint}P"
        }, autoSave: true);

        // 3. Đổi status deposit
        deposit.Status = (byte)DepositStatus.Success;
        deposit.ApprovedBy = CurrentUser.Id;
        deposit.ApprovedAt = Clock.Now;
        await _depositRepository.UpdateAsync(deposit, autoSave: true);

        var customer = await _customerRepository.FindAsync(deposit.CustomerId);
        SalonBeautyLoyaltyBonusTier? tier = null;
        if (deposit.BonusTierId.HasValue)
            tier = await _bonusTierRepository.FindAsync(deposit.BonusTierId.Value);
        return MapToDto(deposit, customer, tier);
    }

    public async Task<SalonBeautyDepositDto> CancelAsync(Guid id, CancelDepositDto input)
    {
        await CheckDepositPolicyAsync(
            MultiTenancyPermissions.SalonBeautyDeposits.Cancel,
            MultiTenancyPermissions.HostSalonBeautyDeposits.Cancel);

        var deposit = await _depositRepository.GetAsync(id);
        if (deposit.Status != (byte)DepositStatus.Pending)
            throw new UserFriendlyException("Chỉ giao dịch Chờ duyệt mới có thể hủy.");

        deposit.Status = (byte)DepositStatus.Cancelled;
        deposit.CancelledBy = CurrentUser.Id;
        deposit.CancelledAt = Clock.Now;
        deposit.CancelReason = input.CancelReason.Trim();
        await _depositRepository.UpdateAsync(deposit, autoSave: true);

        var customer = await _customerRepository.FindAsync(deposit.CustomerId);
        SalonBeautyLoyaltyBonusTier? tier = null;
        if (deposit.BonusTierId.HasValue)
            tier = await _bonusTierRepository.FindAsync(deposit.BonusTierId.Value);
        return MapToDto(deposit, customer, tier);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDepositPolicyAsync(
            MultiTenancyPermissions.SalonBeautyDeposits.Delete,
            MultiTenancyPermissions.HostSalonBeautyDeposits.Delete);

        var deposit = await _depositRepository.GetAsync(id);
        if (deposit.Status == (byte)DepositStatus.Success)
            throw new UserFriendlyException("Không thể xóa giao dịch đã thành công.");

        await _depositRepository.DeleteAsync(id, autoSave: true);
    }

    private async Task<decimal> GetExchangeRateAsync()
    {
        var raw = await _settingProvider.GetOrNullAsync(SalonBeautyLoyaltySettingNames.ExchangeRate);
        if (decimal.TryParse(raw, out var rate) && rate > 0)
            return rate;
        return 1000m;
    }

    private async Task<(int BonusPoint, SalonBeautyLoyaltyBonusTier? Tier)> ResolveBonusTierAsync(decimal amount)
    {
        var query = await _bonusTierRepository.GetQueryableAsync();
        var tiers = await AsyncExecuter.ToListAsync(
            query.Where(x => x.IsActive && x.MinAmount <= amount)
                 .OrderByDescending(x => x.MinAmount)
                 .Take(1));

        var tier = tiers.FirstOrDefault();
        return (tier?.BonusPoint ?? 0, tier);
    }

    private async Task<string> GenerateTransactionCodeAsync()
    {
        var prefix = $"DEP{DateTime.Now:yyyyMMdd}";
        var query = await _depositRepository.GetQueryableAsync();
        var todayCount = await AsyncExecuter.CountAsync(
            query.Where(x => x.TransactionCode.StartsWith(prefix)));
        return $"{prefix}{(todayCount + 1):D4}";
    }

    private async Task CheckDepositPolicyAsync(string tenantPermission, string hostPermission)
    {
        var permission = CurrentTenant.IsAvailable ? tenantPermission : hostPermission;
        if (permission.IsNullOrWhiteSpace())
            throw new AbpAuthorizationException("Missing Salon Beauty deposit permission.");

        await AuthorizationService.CheckAsync(permission);
    }

    private static SalonBeautyDepositDto MapToDto(
        SalonBeautyDepositTransaction d,
        SalonBeautyCustomer? c,
        SalonBeautyLoyaltyBonusTier? t)
    {
        return new SalonBeautyDepositDto
        {
            Id = d.Id,
            TransactionCode = d.TransactionCode,
            CustomerId = d.CustomerId,
            CustomerName = c?.Name,
            CustomerCode = c?.CustomerCode,
            CustomerPhone = c?.Phone,
            Amount = d.Amount,
            ExchangeRate = d.ExchangeRate,
            BasePoint = d.BasePoint,
            BonusPoint = d.BonusPoint,
            TotalPoint = d.TotalPoint,
            BonusTierId = d.BonusTierId,
            BonusTierName = t?.Name,
            PaymentMethod = d.PaymentMethod,
            PaymentMethodText = d.PaymentMethod switch
            {
                (byte)DepositPaymentMethod.Cash => "Tiền mặt",
                (byte)DepositPaymentMethod.BankTransfer => "Chuyển khoản",
                (byte)DepositPaymentMethod.EWallet => "Ví điện tử",
                _ => "Khác"
            },
            ReferenceCode = d.ReferenceCode,
            Note = d.Note,
            Status = d.Status,
            StatusText = d.Status switch
            {
                (byte)DepositStatus.Pending => "Chờ duyệt",
                (byte)DepositStatus.Success => "Thành công",
                (byte)DepositStatus.Cancelled => "Đã hủy",
                _ => "Khác"
            },
            ApprovedBy = d.ApprovedBy,
            ApprovedAt = d.ApprovedAt,
            CancelledBy = d.CancelledBy,
            CancelledAt = d.CancelledAt,
            CancelReason = d.CancelReason,
            CreationTime = d.CreationTime,
            CreatorId = d.CreatorId,
            LastModificationTime = d.LastModificationTime,
            LastModifierId = d.LastModifierId
        };
    }

    private static string? NullIfWhiteSpace(string? value)
        => value.IsNullOrWhiteSpace() ? null : value!.Trim();
}
