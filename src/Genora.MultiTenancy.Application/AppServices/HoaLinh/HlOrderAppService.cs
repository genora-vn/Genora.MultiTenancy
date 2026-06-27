using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.DomainModels.AppHlOrders;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

public class HlOrderAppService : ApplicationService, IHlOrderAppService
{
    private readonly IRepository<HlOrder, Guid> _orderRepo;
    private readonly ICurrentTenant _currentTenant;
    private readonly IAuthorizationService _authService;

    public HlOrderAppService(
        IRepository<HlOrder, Guid> orderRepo,
        ICurrentTenant currentTenant,
        IAuthorizationService authService)
    {
        _orderRepo = orderRepo;
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

    public async Task<PagedResultDto<HlOrderDto>> GetListAsync(HlOrderFilterDto input)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlOrders.Default, MultiTenancyPermissions.HostAppHlOrders.Default);

        var queryable = await _orderRepo.WithDetailsAsync();

        queryable = queryable
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => x.OrderCode.Contains(input.Filter!) ||
                     x.CustomerName!.Contains(input.Filter!) ||
                     x.CustomerPhone!.Contains(input.Filter!))
            .WhereIf(input.DeliveryStatus.HasValue, x => x.DeliveryStatus == input.DeliveryStatus)
            .WhereIf(input.PaymentStatus.HasValue, x => x.PaymentStatus == input.PaymentStatus)
            .WhereIf(input.DateFrom.HasValue, x => x.CreationTime >= input.DateFrom)
            .WhereIf(input.DateTo.HasValue, x => x.CreationTime <= input.DateTo!.Value.AddDays(1));

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        return new PagedResultDto<HlOrderDto>(totalCount, items.Select(MapToDto).ToList());
    }

    public async Task<HlOrderDto> GetAsync(Guid id)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlOrders.Default, MultiTenancyPermissions.HostAppHlOrders.Default);

        var queryable = await _orderRepo.WithDetailsAsync(x => x.Items);
        var order = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id))
                    ?? throw new UserFriendlyException("Không tìm thấy đơn hàng");
        return MapToDto(order);
    }

    public async Task<HlOrderDto> UpdateStatusAsync(HlOrderUpdateStatusDto input)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlOrders.Edit, MultiTenancyPermissions.HostAppHlOrders.Edit);

        var order = await _orderRepo.GetAsync(input.Id);

        if (input.DeliveryStatus.HasValue)
            order.DeliveryStatus = input.DeliveryStatus.Value;

        if (input.PaymentStatus.HasValue)
            order.PaymentStatus = input.PaymentStatus.Value;

        if (!string.IsNullOrWhiteSpace(input.InternalNote))
            order.InternalNote = (order.InternalNote ?? "") + "\n" + input.InternalNote;

        await _orderRepo.UpdateAsync(order, autoSave: true);
        return MapToDto(order);
    }

    public async Task<HlOrderDto> CancelAsync(HlOrderCancelDto input)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlOrders.Edit, MultiTenancyPermissions.HostAppHlOrders.Edit);

        var order = await _orderRepo.GetAsync(input.Id);

        if (order.DeliveryStatus == HlOrderDeliveryStatus.Cancelled)
            throw new UserFriendlyException("Đơn hàng đã bị hủy trước đó");

        order.DeliveryStatus = HlOrderDeliveryStatus.Cancelled;
        order.CancelNote = input.CancelNote;
        order.CancelledBy = CurrentUser.Id;
        order.CancelledAt = DateTime.Now;

        await _orderRepo.UpdateAsync(order, autoSave: true);
        return MapToDto(order);
    }

    private static HlOrderDto MapToDto(HlOrder order)
    {
        return new HlOrderDto
        {
            Id = order.Id,
            OrderCode = order.OrderCode,
            CustomerCode = order.CustomerCode,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            BranchCode = order.BranchCode,
            BranchName = order.BranchName,
            DeliveryAddress = order.DeliveryAddress,
            ReceiverName = order.ReceiverName,
            ReceiverPhone = order.ReceiverPhone,
            SubTotal = order.SubTotal,
            DiscountCode = order.DiscountCode,
            DiscountAmount = order.DiscountAmount,
            SystemDiscount = order.SystemDiscount,
            TotalAmount = order.TotalAmount,
            DeliveryStatus = order.DeliveryStatus,
            PaymentStatus = order.PaymentStatus,
            PaymentMethod = order.PaymentMethod,
            Note = order.Note,
            InternalNote = order.InternalNote,
            CancelNote = order.CancelNote,
            ExternalOrderCode = order.ExternalOrderCode,
            IsSyncedToHl = order.IsSyncedToHl,
            SyncedAt = order.SyncedAt,
            CreationTime = order.CreationTime,
            Items = order.Items?.Select(i => new HlOrderItemDto
            {
                Id = i.Id,
                ProductCode = i.ProductCode,
                ProductName = i.ProductName,
                ProductGroupName = i.ProductGroupName,
                BrandName = i.BrandName,
                ProductUnit = i.ProductUnit,
                ImageUrl = i.ImageUrl,
                Price = i.Price,
                OriginalPrice = i.OriginalPrice,
                Quantity = i.Quantity,
                Amount = i.Amount,
                Note = i.Note
            }).ToList() ?? new()
        };
    }
}
