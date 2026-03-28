using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppFnbItems;
using Genora.MultiTenancy.DomainModels.AppFnbOrderActivity;
using Genora.MultiTenancy.DomainModels.AppFnbOrders;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Realtime;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.SignalR;

public class FnbOrderRealtimeNotifier : IFnbOrderRealtimeNotifier
{
    private readonly IHubContext<FnbOrderHub> _hubContext;
    private readonly IRepository<FnbOrder, Guid> _orderRepository;
    private readonly IRepository<FnbOrderItem, Guid> _orderItemRepository;
    private readonly IRepository<FnbItem, Guid> _itemRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<CustomerType, Guid> _customerTypeRepository;
    private readonly IRepository<FnbOrderActivity, Guid> _orderActivityRepository;
    private readonly ILogger<FnbOrderRealtimeNotifier> _logger;
    private readonly IConfiguration _configuration;

    public FnbOrderRealtimeNotifier(
        IHubContext<FnbOrderHub> hubContext,
        IRepository<FnbOrder, Guid> orderRepository,
        IRepository<FnbOrderItem, Guid> orderItemRepository,
        IRepository<FnbItem, Guid> itemRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<CustomerType, Guid> customerTypeRepository,
        IRepository<FnbOrderActivity, Guid> orderActivityRepository,
        ILogger<FnbOrderRealtimeNotifier> logger,
        IConfiguration configuration)
    {
        _hubContext = hubContext;
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _itemRepository = itemRepository;
        _customerRepository = customerRepository;
        _customerTypeRepository = customerTypeRepository;
        _orderActivityRepository = orderActivityRepository;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task OrderCreatedAsync(Guid orderId)
    {
        var payload = await BuildPayloadAsync(orderId);

        _logger.LogInformation(
            "Broadcast fnb.order.created {OrderId} | OrderCode={OrderCode} | Customer={CustomerName}",
            payload.Id,
            payload.OrderCode,
            payload.CustomerName);

        await _hubContext.Clients.All.SendAsync("fnb.order.created", payload);
    }

    public async Task OrderUpdatedAsync(Guid orderId)
    {
        var payload = await BuildPayloadAsync(orderId);

        _logger.LogInformation(
            "Broadcast fnb.order.updated {OrderId} | OrderCode={OrderCode} | Customer={CustomerName}",
            payload.Id,
            payload.OrderCode,
            payload.CustomerName);

        await _hubContext.Clients.All.SendAsync("fnb.order.updated", payload);
    }

    private async Task<FnbOrderRealtimeDto> BuildPayloadAsync(Guid orderId)
    {
        var order = await _orderRepository.GetAsync(orderId);
        var orderItems = await _orderItemRepository.GetListAsync(x => x.OrderId == orderId);

        Customer? customer = null;
        CustomerType? customerType = null;

        if (order.CustomerId.HasValue)
        {
            customer = await _customerRepository.FirstOrDefaultAsync(x => x.Id == order.CustomerId.Value);

            if (customer?.CustomerTypeId != null)
            {
                customerType = await _customerTypeRepository.FirstOrDefaultAsync(x => x.Id == customer.CustomerTypeId);
            }
        }

        var itemIds = orderItems
            .Where(x => x.ItemId.HasValue)
            .Select(x => x.ItemId!.Value)
            .Distinct()
            .ToList();

        Dictionary<Guid, FnbItem> itemMap = new();

        if (itemIds.Count > 0)
        {
            var itemsQuery = await _itemRepository.GetQueryableAsync();
            var itemEntities = itemsQuery
                .Where(x => itemIds.Contains(x.Id))
                .ToList();

            itemMap = itemEntities.ToDictionary(x => x.Id, x => x);
        }

        var items = orderItems.Select(x =>
        {
            itemMap.TryGetValue(x.ItemId ?? Guid.Empty, out var itemEntity);

            return new FnbOrderRealtimeItemDto
            {
                ItemId = x.ItemId,
                ItemName = x.ItemName,
                Price = x.Price,
                Quantity = x.Quantity,
                Note = x.Note,
                ImageUrl = string.IsNullOrWhiteSpace(itemEntity?.ImageUrl)
                    ? null
                    : ImageHelper.NormalizeThumb(_configuration, itemEntity.ImageUrl)
            };
        }).ToList();

        var activityQuery = await _orderActivityRepository.GetQueryableAsync();
        var recentActivities = activityQuery
            .Where(x => x.OrderId == orderId)
            .OrderByDescending(x => x.ActionTime)
            .Take(5)
            .ToList();

        var recentActivityDtos = recentActivities
            .Select(x => new FnbOrderRealtimeActivityDto
            {
                Title = x.Title,
                Description = x.Description,
                Time = x.ActionTime,
                IsDanger = x.IsDanger
            })
            .ToList();

        var latestActivity = recentActivityDtos.FirstOrDefault();

        var primaryImage = items.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.ImageUrl))?.ImageUrl;
        var itemsSummary = string.Join(", ", items.Select(x => $"{x.ItemName} x{x.Quantity}"));
        var itemNotesSummary = string.Join(" • ",
            items.Where(x => !string.IsNullOrWhiteSpace(x.Note))
                 .Select(x => $"{x.ItemName}: {x.Note}"));

        return new FnbOrderRealtimeDto
        {
            Id = order.Id,
            OrderCode = order.OrderCode,
            BagTag = order.BagTag,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            CustomerPhoneMasked = PhoneHelper.MaskPhone(order.CustomerPhone),
            CustomerTypeName = customerType?.Name,
            CustomerTypeColorCode = customerType?.ColorCode,
            Note = order.Note,
            TotalAmount = order.TotalAmount,
            CreationTime = order.CreationTime,
            CancelledAt = order.CancelledAt,
            ServiceStatus = (int)order.ServiceStatus,
            PaymentStatus = (int)order.PaymentStatus,
            PrimaryImageUrl = primaryImage ?? "/images/fnb/default-food.png",
            ItemsSummary = itemsSummary,
            ItemNotesSummary = itemNotesSummary,
            TotalQuantity = items.Sum(x => x.Quantity),
            LatestActivityTitle = latestActivity?.Title,
            LatestActivityDescription = latestActivity?.Description,
            RecentActivities = recentActivityDtos,
            Items = items
        };
    }
}