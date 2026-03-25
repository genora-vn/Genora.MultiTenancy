using Genora.MultiTenancy.AppDtos.AppFnbOrders;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppFnbItems;
using Genora.MultiTenancy.DomainModels.AppFnbOrders;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.AppFnbFeatures;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.AppFnbOrders;

[Authorize]
public class AppFnbOrderService : ApplicationService, IAppFnbOrderService
{
    private readonly IRepository<FnbOrder, Guid> _orderRepository;
    private readonly IRepository<FnbOrderItem, Guid> _orderItemRepository;
    private readonly IRepository<FnbItem, Guid> _itemRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IFeatureChecker _featureChecker;
    private readonly ICurrentUser _currentUser;

    public AppFnbOrderService(
        IRepository<FnbOrder, Guid> orderRepository,
        IRepository<FnbOrderItem, Guid> orderItemRepository,
        IRepository<FnbItem, Guid> itemRepository,
        IRepository<Customer, Guid> customerRepository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        ICurrentUser currentUser)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _itemRepository = itemRepository;
        _customerRepository = customerRepository;
        _currentTenant = currentTenant;
        _featureChecker = featureChecker;
        _currentUser = currentUser;
    }

    public async Task<PagedResultDto<FnbOrderDto>> GetListAsync(GetFnbOrderListInput input)
    {
        await CheckFeatureAndPolicyAsync(GetRootPermission());

        var query = await _orderRepository.GetQueryableAsync();

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var filter = input.FilterText.Trim();
            query = query.Where(x =>
                x.OrderCode.Contains(filter) ||
                x.BagTag.Contains(filter) ||
                (x.CustomerName != null && x.CustomerName.Contains(filter)));
        }

        if (!input.BagTag.IsNullOrWhiteSpace())
        {
            var bagTag = input.BagTag.Trim();
            query = query.Where(x => x.BagTag.Contains(bagTag));
        }

        if (input.ServiceStatus.HasValue)
        {
            query = query.Where(x => x.ServiceStatus == input.ServiceStatus.Value);
        }

        if (input.PaymentStatus.HasValue)
        {
            query = query.Where(x => x.PaymentStatus == input.PaymentStatus.Value);
        }

        if (input.CreationTimeFrom.HasValue)
        {
            query = query.Where(x => x.CreationTime >= input.CreationTimeFrom.Value);
        }

        if (input.CreationTimeTo.HasValue)
        {
            query = query.Where(x => x.CreationTime <= input.CreationTimeTo.Value);
        }

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(FnbOrder.CreationTime) + " desc"
            : input.Sorting;

        query = query.OrderBy(sorting);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<FnbOrderDto>(totalCount, items.Select(MapOrderDto).ToList());
    }

    public async Task<FnbOrderDetailDto> GetAsync(Guid id)
    {
        await CheckFeatureAndPolicyAsync(GetRootPermission());

        var order = await _orderRepository.GetAsync(id);
        var orderItems = await _orderItemRepository.GetListAsync(x => x.OrderId == id);

        return new FnbOrderDetailDto
        {
            Id = order.Id,
            TenantId = order.TenantId,
            OrderCode = order.OrderCode,
            BagTag = order.BagTag,
            CustomerId = order.CustomerId,
            CustomerName = order.CustomerName,
            CustomerPhoneMasked = MaskPhone(order.CustomerPhone),
            TotalAmount = order.TotalAmount,
            ServiceStatus = order.ServiceStatus,
            PaymentStatus = order.PaymentStatus,
            PaymentMethod = order.PaymentMethod,
            Note = order.Note,
            InternalNote = order.InternalNote,
            CancelReason = order.CancelReason,
            CancelNote = order.CancelNote,
            CreationTime = order.CreationTime,
            CreatorId = order.CreatorId,
            LastModificationTime = order.LastModificationTime,
            LastModifierId = order.LastModifierId,
            IsDeleted = order.IsDeleted,
            DeleterId = order.DeleterId,
            DeletionTime = order.DeletionTime,
            Items = orderItems
                .OrderBy(x => x.Id)
                .Select(x => new FnbOrderItemDto
                {
                    Id = x.Id,
                    OrderId = x.OrderId,
                    ItemId = x.ItemId,
                    ItemName = x.ItemName,
                    Price = x.Price,
                    Quantity = x.Quantity,
                    Note = x.Note
                }).ToList()
        };
    }

    public async Task<FnbOrderDetailDto> CreateAsync(CreateFnbOrderDto input)
    {
        await CheckFeatureAndPolicyAsync(GetCreatePermission());

        if (input.Items == null || input.Items.Count == 0)
        {
            throw new UserFriendlyException("Đơn hàng phải có ít nhất 1 món.");
        }

        if (input.Items.Any(x => x.Quantity <= 0))
        {
            throw new AbpValidationException("Validation failed");
        }

        Customer? customer = null;
        if (input.CustomerId.HasValue)
        {
            customer = await _customerRepository.FirstOrDefaultAsync(x => x.Id == input.CustomerId.Value);
        }

        var itemIds = input.Items.Select(x => x.ItemId).Distinct().ToList();
        var itemQuery = await _itemRepository.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(itemQuery.Where(x => itemIds.Contains(x.Id)));

        if (items.Count != itemIds.Count)
        {
            throw new UserFriendlyException("Có món không tồn tại.");
        }

        var invalidItem = items.FirstOrDefault(x => !x.IsActive || !x.IsAvailable);
        if (invalidItem != null)
        {
            throw new UserFriendlyException($"Món '{invalidItem.Name}' hiện không khả dụng.");
        }

        var order = new FnbOrder(GuidGenerator.Create(), await GenerateOrderCodeAsync(), input.BagTag.Trim(), _currentTenant.Id)
        {
            CustomerId = input.CustomerId,
            CustomerName = !string.IsNullOrWhiteSpace(input.CustomerName) ? input.CustomerName.Trim() : customer?.FullName,
            CustomerPhone = !string.IsNullOrWhiteSpace(input.CustomerPhone) ? input.CustomerPhone.Trim() : customer?.PhoneNumber,
            Note = string.IsNullOrWhiteSpace(input.Note) ? null : input.Note.Trim(),
            InternalNote = string.IsNullOrWhiteSpace(input.InternalNote) ? null : input.InternalNote.Trim(),
            PaymentMethod = input.PaymentMethod,
            ServiceStatus = FnbServiceStatus.Created,
            PaymentStatus = FnbPaymentStatus.Unpaid
        };

        decimal total = 0;
        var orderItems = new List<FnbOrderItem>();

        foreach (var row in input.Items)
        {
            var item = items.First(x => x.Id == row.ItemId);

            total += item.Price * row.Quantity;

            orderItems.Add(new FnbOrderItem(GuidGenerator.Create(), order.Id, item.Name, item.Price, row.Quantity)
            {
                ItemId = item.Id,
                Note = string.IsNullOrWhiteSpace(row.Note) ? null : row.Note.Trim()
            });
        }

        order.TotalAmount = total;

        await _orderRepository.InsertAsync(order, autoSave: true);
        await _orderItemRepository.InsertManyAsync(orderItems, autoSave: true);

        return await GetAsync(order.Id);
    }

    public async Task<FnbOrderDto> UpdateServiceStatusAsync(Guid id, UpdateFnbOrderServiceStatusDto input)
    {
        await CheckFeatureAndPolicyAsync(GetEditPermission());

        var order = await _orderRepository.GetAsync(id);

        if (order.ServiceStatus == FnbServiceStatus.Served)
        {
            throw new UserFriendlyException("Đơn đã hoàn thành, không thể cập nhật tiếp.");
        }

        if (order.ServiceStatus == FnbServiceStatus.Cancelled)
        {
            throw new UserFriendlyException("Đơn đã hủy, không thể cập nhật trạng thái phục vụ.");
        }

        var isValid = (order.ServiceStatus, input.ServiceStatus) switch
        {
            (var current, var next) when current == next => true,

            (FnbServiceStatus.Created, FnbServiceStatus.Preparing) => true,
            (FnbServiceStatus.Preparing, FnbServiceStatus.Delivering) => true,
            (FnbServiceStatus.Delivering, FnbServiceStatus.Served) => true,

            _ => false
        };

        if (!isValid)
        {
            throw new UserFriendlyException("Không thể nhảy trạng thái phục vụ.");
        }

        order.ServiceStatus = input.ServiceStatus;
        order.InternalNote = input.InternalNote;
        order = await _orderRepository.UpdateAsync(order, autoSave: true);

        return MapOrderDto(order);
    }

    public async Task<FnbOrderDto> UpdatePaymentStatusAsync(Guid id, UpdateFnbOrderPaymentStatusDto input)
    {
        await CheckFeatureAndPolicyAsync(GetEditPermission());

        var order = await _orderRepository.GetAsync(id);

        if (order.PaymentStatus == FnbPaymentStatus.Paid && input.PaymentStatus != FnbPaymentStatus.Paid)
        {
            throw new UserFriendlyException("Đơn đã thanh toán thì không được chuyển ngược trạng thái.");
        }

        order.PaymentStatus = input.PaymentStatus;
        order = await _orderRepository.UpdateAsync(order, autoSave: true);

        return MapOrderDto(order);
    }

    public async Task<FnbOrderDto> CancelAsync(Guid id, CancelFnbOrderDto input)
    {
        await CheckFeatureAndPolicyAsync(GetEditPermission());

        var order = await _orderRepository.GetAsync(id);

        if (order.ServiceStatus != FnbServiceStatus.Created &&
            order.ServiceStatus != FnbServiceStatus.Preparing)
        {
            throw new UserFriendlyException("Chỉ được hủy đơn ở trạng thái Mới tạo hoặc Đang chuẩn bị.");
        }

        if (input.CancelReason == 0)
        {
            throw new UserFriendlyException("Vui lòng chọn lý do hủy đơn.");
        }

        if (!string.IsNullOrWhiteSpace(input.CancelNote) && input.CancelNote.Length > 500)
        {
            throw new UserFriendlyException("Ghi chú hủy tối đa 500 ký tự.");
        }

        order.ServiceStatus = FnbServiceStatus.Cancelled;
        order.CancelReason = input.CancelReason;
        order.CancelNote = string.IsNullOrWhiteSpace(input.CancelNote) ? null : input.CancelNote.Trim();
        order.CancelledBy = _currentUser.Id;
        order.CancelledAt = Clock.Now;

        order = await _orderRepository.UpdateAsync(order, autoSave: true);
        return MapOrderDto(order);
    }

    private FnbOrderDto MapOrderDto(FnbOrder order)
    {
        return new FnbOrderDto
        {
            Id = order.Id,
            TenantId = order.TenantId,
            OrderCode = order.OrderCode,
            BagTag = order.BagTag,
            CustomerId = order.CustomerId,
            CustomerName = order.CustomerName,
            CustomerPhoneMasked = MaskPhone(order.CustomerPhone),
            TotalAmount = order.TotalAmount,
            ServiceStatus = order.ServiceStatus,
            PaymentStatus = order.PaymentStatus,
            PaymentMethod = order.PaymentMethod,
            Note = order.Note,
            InternalNote = order.InternalNote,
            CancelReason = order.CancelReason,
            CancelNote = order.CancelNote,
            CreationTime = order.CreationTime,
            CreatorId = order.CreatorId,
            LastModificationTime = order.LastModificationTime,
            LastModifierId = order.LastModifierId,
            IsDeleted = order.IsDeleted,
            DeleterId = order.DeleterId,
            DeletionTime = order.DeletionTime
        };
    }

    private string? MaskPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return phone;

        var clean = phone.Trim();
        if (clean.Length <= 7)
            return clean;

        var first = clean.Substring(0, Math.Min(4, clean.Length));
        var last = clean.Substring(clean.Length - 3, 3);
        return $"{first}***{last}";
    }

    private async Task<string> GenerateOrderCodeAsync()
    {
        var prefix = $"FNB{Clock.Now:yyMMdd}";
        var query = await _orderRepository.GetQueryableAsync();
        var count = await AsyncExecuter.CountAsync(query.Where(x => x.OrderCode.StartsWith(prefix)));
        return $"{prefix}{(count + 1):D4}";
    }

    private string GetRootPermission()
        => CurrentTenant.IsAvailable
            ? MultiTenancyPermissions.AppFnbOrders.Default
            : MultiTenancyPermissions.HostAppFnbOrders.Default;

    private string GetCreatePermission()
        => CurrentTenant.IsAvailable
            ? MultiTenancyPermissions.AppFnbOrders.Create
            : MultiTenancyPermissions.HostAppFnbOrders.Create;

    private string GetEditPermission()
        => CurrentTenant.IsAvailable
            ? MultiTenancyPermissions.AppFnbOrders.Edit
            : MultiTenancyPermissions.HostAppFnbOrders.Edit;

    private async Task CheckFeatureAndPolicyAsync(string permissionName)
    {
        if (CurrentTenant.IsAvailable)
        {
            await _featureChecker.CheckEnabledAsync(AppFnbFeatures.Management);
        }

        await AuthorizationService.CheckAsync(permissionName);
    }
}