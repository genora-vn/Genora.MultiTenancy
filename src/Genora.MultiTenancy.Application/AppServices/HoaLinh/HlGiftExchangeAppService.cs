using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.DomainModels.AppHlGiftExchanges;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

public class HlGiftExchangeAppService : ApplicationService, IHlGiftExchangeAppService
{
    private readonly IRepository<HlGiftExchange, Guid> _giftRepo;
    private readonly ICurrentTenant _currentTenant;
    private readonly IAuthorizationService _authService;

    public HlGiftExchangeAppService(
        IRepository<HlGiftExchange, Guid> giftRepo,
        ICurrentTenant currentTenant,
        IAuthorizationService authService)
    {
        _giftRepo = giftRepo;
        _currentTenant = currentTenant;
        _authService = authService;
    }

    private string P(string tenantPerm, string hostPerm)
        => _currentTenant.Id.HasValue ? tenantPerm : hostPerm;

    private async Task CheckPermissionAsync(string tenantPerm, string hostPerm)
    {
        var perm = P(tenantPerm, hostPerm);
        var result = await _authService.AuthorizeAsync(perm);
        if (!result.Succeeded)
            throw new Volo.Abp.Authorization.AbpAuthorizationException($"Permission denied: {perm}");
    }

    public async Task<PagedResultDto<HlGiftExchangeDto>> GetListAsync(HlGiftExchangeFilterDto input)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlGiftExchange.Default, MultiTenancyPermissions.HostAppHlGiftExchange.Default);

        var queryable = await _giftRepo.GetQueryableAsync();

        queryable = queryable
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => x.ExchangeCode.Contains(input.Filter!) ||
                     x.CustomerName!.Contains(input.Filter!) ||
                     x.GiftName.Contains(input.Filter!))
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status);

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        return new PagedResultDto<HlGiftExchangeDto>(totalCount, items.Select(MapToDto).ToList());
    }

    public async Task<HlGiftExchangeDto> GetAsync(Guid id)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlGiftExchange.Default, MultiTenancyPermissions.HostAppHlGiftExchange.Default);

        var entity = await _giftRepo.GetAsync(id);
        return MapToDto(entity);
    }

    public async Task<HlGiftExchangeDto> ApproveOrRejectAsync(HlGiftExchangeApproveDto input)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlGiftExchange.Edit, MultiTenancyPermissions.HostAppHlGiftExchange.Edit);

        var entity = await _giftRepo.GetAsync(input.Id);

        if (entity.Status != HlGiftExchangeStatus.Pending)
            throw new UserFriendlyException("Chỉ có thể duyệt/từ chối yêu cầu đang chờ xử lý");

        entity.Status = input.IsApproved
            ? HlGiftExchangeStatus.Approved
            : HlGiftExchangeStatus.Rejected;

        entity.ApprovedBy = CurrentUser.Id;
        entity.ApprovedAt = DateTime.Now;

        if (!string.IsNullOrWhiteSpace(input.InternalNote))
            entity.InternalNote = input.InternalNote;

        await _giftRepo.UpdateAsync(entity, autoSave: true);
        return MapToDto(entity);
    }

    private static HlGiftExchangeDto MapToDto(HlGiftExchange entity)
    {
        return new HlGiftExchangeDto
        {
            Id = entity.Id,
            ExchangeCode = entity.ExchangeCode,
            CustomerCode = entity.CustomerCode,
            CustomerName = entity.CustomerName,
            CustomerPhone = entity.CustomerPhone,
            GiftName = entity.GiftName,
            GiftCode = entity.GiftCode,
            GiftImageUrl = entity.GiftImageUrl,
            PointsRequired = entity.PointsRequired,
            Quantity = entity.Quantity,
            TotalPointsUsed = entity.TotalPointsUsed,
            Status = entity.Status,
            Note = entity.Note,
            InternalNote = entity.InternalNote,
            UrBoxVoucherCode = entity.UrBoxVoucherCode,
            DeliveryAddress = entity.DeliveryAddress,
            CreationTime = entity.CreationTime,
            ApprovedAt = entity.ApprovedAt
        };
    }
}
