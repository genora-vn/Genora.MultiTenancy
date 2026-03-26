using Genora.MultiTenancy.AppDtos.AppFnbOrders;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppFnbItems;
using Genora.MultiTenancy.DomainModels.AppFnbOrders;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Realtime;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppFnbOrders;
public class MiniAppFnbOrderService : ApplicationService, IMiniAppFnbOrderService
{
    private readonly IRepository<FnbOrder, Guid> _orderRepository;
    private readonly IRepository<FnbOrderItem, Guid> _orderItemRepository;
    private readonly IRepository<FnbItem, Guid> _itemRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IFnbOrderRealtimeNotifier _notifier;

    public MiniAppFnbOrderService(
        IRepository<FnbOrder, Guid> orderRepository,
        IRepository<FnbOrderItem, Guid> orderItemRepository,
        IRepository<FnbItem, Guid> itemRepository,
        IRepository<Customer, Guid> customerRepository,
        ICurrentTenant currentTenant,
        IFnbOrderRealtimeNotifier notifier)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _itemRepository = itemRepository;
        _customerRepository = customerRepository;
        _currentTenant = currentTenant;
        _notifier = notifier;
    }

    public async Task<MiniAppFnbOrderDetailDto> CreateAsync(CreateFnbOrderDto input)
    {
        if (input.Items == null || input.Items.Count == 0)
        {
            throw new UserFriendlyException("Đơn hàng phải có ít nhất 1 món.");
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

        Logger.LogInformation("Order saved: {OrderId}", order.Id);

        await _notifier.OrderCreatedAsync(order.Id);

        Logger.LogInformation("Realtime notified: {OrderId}", order.Id);

        return await GetAsync(order.Id);
    }

    public async Task<MiniAppFnbOrderListDto> GetListAsync(GetMiniAppFnbOrderListInput input)
    {
        var query = await _orderRepository.GetQueryableAsync();

        if (input.CustomerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == input.CustomerId.Value);
        }

        if (!input.BagTag.IsNullOrWhiteSpace())
        {
            var bagTag = input.BagTag.Trim();
            query = query.Where(x => x.BagTag.Contains(bagTag));
        }

        var total = await AsyncExecuter.CountAsync(query);

        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount)
        );

        var data = items.Select(x => new MiniAppFnbOrderData
        {
            Id = x.Id,
            OrderCode = x.OrderCode,
            BagTag = x.BagTag,
            CustomerId = x.CustomerId,
            CustomerName = x.CustomerName,
            CustomerPhoneMasked = MaskPhone(x.CustomerPhone),
            TotalAmount = x.TotalAmount,
            ServiceStatus = x.ServiceStatus,
            PaymentStatus = x.PaymentStatus,
            PaymentMethod = x.PaymentMethod,
            Note = x.Note,
            CreationTime = x.CreationTime
        }).ToList();

        return new MiniAppFnbOrderListDto
        {
            Error = 0,
            Message = "Success",
            Data = new PagedResultDto<MiniAppFnbOrderData>(total, data)
        };
    }

    public async Task<MiniAppFnbOrderDetailDto> GetAsync(Guid id)
    {
        var order = await _orderRepository.GetAsync(id);
        var items = await _orderItemRepository.GetListAsync(x => x.OrderId == id);

        return new MiniAppFnbOrderDetailDto
        {
            Error = 0,
            Message = "Success",
            Data = new MiniAppFnbOrderData
            {
                Id = order.Id,
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
                CreationTime = order.CreationTime,
                Items = items
                    .OrderBy(x => x.Id)
                    .Select(x => new MiniAppFnbOrderItemData
                    {
                        Id = x.Id,
                        ItemId = x.ItemId,
                        ItemName = x.ItemName,
                        Price = x.Price,
                        Quantity = x.Quantity,
                        Note = x.Note
                    }).ToList()
            }
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
}
