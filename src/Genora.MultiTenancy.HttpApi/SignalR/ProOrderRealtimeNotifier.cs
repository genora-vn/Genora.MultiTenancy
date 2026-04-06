using Genora.MultiTenancy.DomainModels.AppProOrderActivity;
using Genora.MultiTenancy.DomainModels.AppProOrders;
using Genora.MultiTenancy.DomainModels.AppProItems;
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

public class ProOrderRealtimeNotifier : IProOrderRealtimeNotifier
{
    private const string HostGroup = "pro-orders:host";

    private readonly IHubContext<ProOrderHub> _hubContext;
    private readonly IRepository<ProOrder, Guid> _orderRepository;
    private readonly IRepository<ProOrderItem, Guid> _orderItemRepository;
    private readonly IRepository<ProItem, Guid> _itemRepository;
    private readonly IRepository<ProOrderActivity, Guid> _activityRepository;
    private readonly ILogger<ProOrderRealtimeNotifier> _logger;
    private readonly IConfiguration _configuration;

    public ProOrderRealtimeNotifier(
        IHubContext<ProOrderHub> hubContext,
        IRepository<ProOrder, Guid> orderRepository,
        IRepository<ProOrderItem, Guid> orderItemRepository,
        IRepository<ProItem, Guid> itemRepository,
        IRepository<ProOrderActivity, Guid> activityRepository,
        ILogger<ProOrderRealtimeNotifier> logger,
        IConfiguration configuration)
    {
        _hubContext          = hubContext;
        _orderRepository     = orderRepository;
        _orderItemRepository = orderItemRepository;
        _itemRepository      = itemRepository;
        _activityRepository  = activityRepository;
        _logger              = logger;
        _configuration       = configuration;
    }

    public async Task OrderCreatedAsync(Guid orderId)
    {
        var order   = await _orderRepository.GetAsync(orderId);
        var payload = await BuildPayloadAsync(order);
        var group   = GetGroupName(order.TenantId);

        _logger.LogInformation(
            "Broadcast pro.order.created {OrderId} | TenantId={TenantId} | Group={Group}",
            payload.Id, order.TenantId, group);

        await _hubContext.Clients.Group(group).SendAsync("pro.order.created", payload);
    }

    public async Task OrderUpdatedAsync(Guid orderId)
    {
        var order   = await _orderRepository.GetAsync(orderId);
        var payload = await BuildPayloadAsync(order);
        var group   = GetGroupName(order.TenantId);

        _logger.LogInformation(
            "Broadcast pro.order.updated {OrderId} | TenantId={TenantId} | Group={Group}",
            payload.Id, order.TenantId, group);

        await _hubContext.Clients.Group(group).SendAsync("pro.order.updated", payload);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string GetGroupName(Guid? tenantId)
        => tenantId.HasValue ? $"pro-orders:{tenantId.Value:D}" : HostGroup;

    private async Task<ProOrderRealtimeDto> BuildPayloadAsync(ProOrder order)
    {
        var orderId    = order.Id;
        var orderItems = await _orderItemRepository.GetListAsync(x => x.OrderId == orderId);

        // Map item images
        var itemIds = orderItems
            .Where(x => x.ItemId.HasValue)
            .Select(x => x.ItemId!.Value)
            .Distinct()
            .ToList();

        var itemMap = new Dictionary<Guid, ProItem>();
        if (itemIds.Count > 0)
        {
            var itemsQuery = await _itemRepository.GetQueryableAsync();
            itemMap = itemsQuery
                .Where(x => itemIds.Contains(x.Id))
                .ToDictionary(x => x.Id, x => x);
        }

        var items = orderItems.Select(x =>
        {
            itemMap.TryGetValue(x.ItemId ?? Guid.Empty, out var proItem);
            return new ProOrderRealtimeItemDto
            {
                ItemId   = x.ItemId,
                ItemName = x.ItemName,
                Price    = x.Price,
                Quantity = x.Quantity,
                Note     = x.Note,
                ImageUrl = string.IsNullOrWhiteSpace(proItem?.ImageUrl)
                    ? null
                    : ImageHelper.NormalizeThumb(_configuration, proItem.ImageUrl)
            };
        }).ToList();

        // Recent activities
        var activityQuery = await _activityRepository.GetQueryableAsync();
        var recentActivities = activityQuery
            .Where(x => x.OrderId == orderId)
            .OrderByDescending(x => x.ActionTime)
            .Take(5)
            .ToList();

        var recentActivityDtos = recentActivities
            .Select(x => new ProOrderRealtimeActivityDto
            {
                Title       = x.Title,
                Description = x.Description,
                Time        = x.ActionTime,
                IsDanger    = x.IsDanger
            }).ToList();

        var latestActivity  = recentActivityDtos.FirstOrDefault();
        var primaryImage    = items.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.ImageUrl))?.ImageUrl;
        var itemsSummary    = string.Join(", ", items.Select(x => $"{x.ItemName} x{x.Quantity}"));

        return new ProOrderRealtimeDto
        {
            Id                      = order.Id,
            OrderCode               = order.OrderCode,
            BagTag                  = order.BagTag,
            CustomerName            = order.CustomerName,
            CustomerPhone           = order.CustomerPhone,
            CustomerPhoneMasked     = PhoneHelper.MaskPhone(order.CustomerPhone),
            Note                    = order.Note,
            TotalAmount             = order.TotalAmount,
            CreationTime            = order.CreationTime,
            CancelledAt             = order.CancelledAt,
            ServiceStatus           = (int)order.ServiceStatus,
            PaymentStatus           = (int)order.PaymentStatus,
            PrimaryImageUrl         = primaryImage ?? "/images/fnb/default-food.png",
            ItemsSummary            = itemsSummary,
            TotalQuantity           = items.Sum(x => x.Quantity),
            LatestActivityTitle     = latestActivity?.Title,
            LatestActivityDescription = latestActivity?.Description,
            RecentActivities        = recentActivityDtos,
            Items                   = items
        };
    }
}
