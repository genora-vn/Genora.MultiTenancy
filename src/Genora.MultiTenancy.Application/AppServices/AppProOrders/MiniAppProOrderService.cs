using Genora.MultiTenancy.AppDtos.AppProOrders;
using Genora.MultiTenancy.DomainModels.AppProCategories;
using Genora.MultiTenancy.DomainModels.AppProItems;
using Genora.MultiTenancy.DomainModels.AppProOrderActivity;
using Genora.MultiTenancy.DomainModels.AppProOrders;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Realtime;
using Microsoft.Extensions.Configuration;
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

namespace Genora.MultiTenancy.AppServices.AppProOrders;

public class MiniAppProOrderService : ApplicationService, IMiniAppProOrderService
{
    private readonly IRepository<ProOrder, Guid> _orderRepository;
    private readonly IRepository<ProOrderItem, Guid> _orderItemRepository;
    private readonly IRepository<ProOrderActivity, Guid> _activityRepository;
    private readonly IRepository<ProItem, Guid> _proItemRepository;
    private readonly IRepository<ProCategory, Guid> _proCategoryRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IProOrderRealtimeNotifier _notifier;
    private readonly IConfiguration _configuration;

    public MiniAppProOrderService(
        IRepository<ProOrder, Guid> orderRepository,
        IRepository<ProOrderItem, Guid> orderItemRepository,
        IRepository<ProOrderActivity, Guid> activityRepository,
        IRepository<ProItem, Guid> proItemRepository,
        IRepository<ProCategory, Guid> proCategoryRepository,
        ICurrentTenant currentTenant,
        IProOrderRealtimeNotifier notifier,
        IConfiguration configuration)
    {
        _orderRepository       = orderRepository;
        _orderItemRepository   = orderItemRepository;
        _activityRepository    = activityRepository;
        _proItemRepository     = proItemRepository;
        _proCategoryRepository = proCategoryRepository;
        _currentTenant         = currentTenant;
        _notifier              = notifier;
        _configuration         = configuration;
    }

    public async Task<MiniAppProOrderDetailDto> CreateAsync(CreateProOrderDto input)
    {
        if (!input.Items.Any())
            return Error("Vui lòng thêm ít nhất một sản phẩm.");

        var orderCode = await GenerateOrderCodeAsync();
        var order = new ProOrder(GuidGenerator.Create(), orderCode, input.BagTag.Trim(), _currentTenant.Id)
        {
            CustomerId    = input.CustomerId,
            CustomerName  = input.CustomerName?.Trim(),
            CustomerPhone = input.CustomerPhone?.Trim(),
            Note          = input.Note,
            PaymentMethod = input.PaymentMethod,
            ServiceStatus = ProServiceStatus.Created,
            PaymentStatus = ProPaymentStatus.Unpaid,
            TotalAmount   = 0
        };

        var itemRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<ProItem, Guid>>();

        var orderItems = new List<ProOrderItem>();
        foreach (var itemInput in input.Items)
        {
            ProItem? proItem;
            try { proItem = await itemRepo.GetAsync(itemInput.ItemId); }
            catch { return Error($"Sản phẩm không tồn tại: {itemInput.ItemId}"); }

            orderItems.Add(new ProOrderItem(
                GuidGenerator.Create(), order.Id, proItem.Name, proItem.Price, itemInput.Quantity)
            {
                TenantId = _currentTenant.Id,
                ItemId = null, // FK cross-tenant: dùng ItemName để lookup ảnh
                Note   = itemInput.Note
            });
        }

        order.TotalAmount = orderItems.Sum(i => i.Price * i.Quantity);

        foreach (var orderItem in orderItems)
            order.Items.Add(orderItem);

        await _orderRepository.InsertAsync(order, autoSave: true);

        await WriteActivityAsync(order.Id, "Created",
            "Đơn hàng được tạo từ Mini App",
            $"Mã túi: {order.BagTag} | {orderItems.Count} sản phẩm | {order.TotalAmount:N0} VND");

        // Broadcast SignalR — staff nhận notify realtime
        try { await _notifier.OrderCreatedAsync(order.Id); }
        catch { /* SignalR broadcast không được làm thất bại luồng đặt hàng */ }

        return await GetAsync(order.Id);
    }

    public async Task<MiniAppProOrderListDto> GetListAsync(GetMiniAppProOrderListInput input)
    {
        var query = (await _orderRepository.WithDetailsAsync(o => o.Items)).AsQueryable();

        if (input.CustomerId.HasValue)
            query = query.Where(x => x.CustomerId == input.CustomerId.Value);

        if (!input.BagTag.IsNullOrWhiteSpace())
            query = query.Where(x => x.BagTag == input.BagTag!.Trim());

        var total = await AsyncExecuter.CountAsync(query);
        var orders = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount));

        var allOrderItems = orders.SelectMany(o => o.Items).ToList();
        var itemDict = await BuildItemDictAsync(allOrderItems);
        var categoryDict = await BuildCategoryDictAsync(itemDict.Values.ToList());

        return new MiniAppProOrderListDto
        {
            Error = 0,
            Message = "Success",
            Data = new PagedResultDto<MiniAppProOrderData>(total,
                orders.Select(o => ToData(o, itemDict, categoryDict)).ToList())
        };
    }

    public async Task<MiniAppProOrderDetailDto> GetAsync(Guid id)
    {
        ProOrder? order;
        try
        {
            var query = (await _orderRepository.WithDetailsAsync(o => o.Items)).AsQueryable();
            order = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));
        }
        catch
        {
            return Error("Không tìm thấy đơn hàng.");
        }

        if (order == null)
            return Error("Không tìm thấy đơn hàng.");

        var itemDict = await BuildItemDictAsync(order.Items.ToList());
        var categoryDict = await BuildCategoryDictAsync(itemDict.Values.ToList());

        return new MiniAppProOrderDetailDto
        {
            Error = 0,
            Message = "Success",
            Data = ToData(order, itemDict, categoryDict)
        };
    }

    public async Task<MiniAppProOrderDetailDto> CancelAsync(Guid id, CancelMiniAppProOrderDto input)
    {
        ProOrder order;
        try { order = await _orderRepository.GetAsync(id); }
        catch { return Error("Không tìm thấy đơn hàng."); }

        if (input.CustomerId.HasValue && order.CustomerId != input.CustomerId)
            return Error("Bạn không có quyền hủy đơn hàng này.");

        if (order.ServiceStatus == ProServiceStatus.Delivered)
            return Error("Không thể hủy đơn hàng đã giao.");

        if (order.ServiceStatus == ProServiceStatus.Cancelled)
            return Error("Đơn hàng đã được hủy trước đó.");

        order.ServiceStatus = ProServiceStatus.Cancelled;
        order.CancelNote    = input.CancelNote;
        order.CancelledAt   = Clock.Now;

        await _orderRepository.UpdateAsync(order, autoSave: true);
        await WriteActivityAsync(order.Id, "Cancelled",
            "Khách hủy đơn từ Mini App", input.CancelReason, isDanger: true);

        try { await _notifier.OrderUpdatedAsync(order.Id); }
        catch { /* SignalR broadcast không được làm thất bại luồng hủy đơn */ }

        return await GetAsync(id);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Build lookup dict từ ProItem. Key = Id.ToString() hoặc "name:{ItemName}" (fallback).
    /// </summary>
    private async Task<Dictionary<string, ProItem>> BuildItemDictAsync(List<ProOrderItem> orderItems)
    {
        var linkedItemIds = orderItems
            .Where(x => x.ItemId.HasValue)
            .Select(x => x.ItemId!.Value)
            .Distinct()
            .ToList();

        var itemQuery = await _proItemRepository.GetQueryableAsync();

        var linkedItems = linkedItemIds.Count > 0
            ? await AsyncExecuter.ToListAsync(itemQuery.Where(x => linkedItemIds.Contains(x.Id)))
            : new List<ProItem>();

        var orderItemNames = orderItems
            .Select(x => x.ItemName?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var fallbackItems = orderItemNames.Count > 0
            ? await AsyncExecuter.ToListAsync(itemQuery.Where(x => orderItemNames.Contains(x.Name)))
            : new List<ProItem>();

        var allItems = linkedItems
            .Concat(fallbackItems)
            .GroupBy(x => x.Id)
            .Select(g => g.First())
            .ToList();

        var dict = new Dictionary<string, ProItem>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in allItems)
        {
            dict[item.Id.ToString()] = item;
            if (!string.IsNullOrWhiteSpace(item.Name))
                dict.TryAdd("name:" + item.Name.Trim(), item);
        }

        return dict;
    }

    private async Task<Dictionary<Guid, string>> BuildCategoryDictAsync(List<ProItem> items)
    {
        var categoryIds = items.Select(x => x.CategoryId).Distinct().ToList();
        if (!categoryIds.Any()) return new Dictionary<Guid, string>();

        var categories = await _proCategoryRepository.GetListAsync(x => categoryIds.Contains(x.Id));
        return categories.ToDictionary(x => x.Id, x => x.Name);
    }

    private ProItem? ResolveItem(ProOrderItem orderItem, Dictionary<string, ProItem> itemDict)
    {
        if (orderItem.ItemId.HasValue && itemDict.TryGetValue(orderItem.ItemId.Value.ToString(), out var byId))
            return byId;

        if (!string.IsNullOrWhiteSpace(orderItem.ItemName) &&
            itemDict.TryGetValue("name:" + orderItem.ItemName.Trim(), out var byName))
            return byName;

        return null;
    }

    private MiniAppProOrderData ToData(ProOrder o, Dictionary<string, ProItem> itemDict, Dictionary<Guid, string> categoryDict)
    {
        var items = o.Items.Select(i =>
        {
            var proItem = ResolveItem(i, itemDict);
            return new MiniAppProOrderItemData
            {
                Id           = i.Id,
                ItemId       = proItem?.Id ?? i.ItemId,
                ItemName     = i.ItemName,
                Price        = i.Price,
                Quantity     = i.Quantity,
                Note         = i.Note,
                LineTotal    = i.Price * i.Quantity,
                ImageUrl     = !string.IsNullOrWhiteSpace(proItem?.ImageUrl)
                    ? ImageHelper.NormalizeThumb(_configuration, proItem.ImageUrl)
                    : null,
                CategoryName = proItem != null && categoryDict.TryGetValue(proItem.CategoryId, out var catName)
                    ? catName
                    : null,
                SortOrder    = proItem?.SortOrder,
                IsActive     = proItem?.IsActive,
                IsAvailable  = proItem?.IsAvailable
            };
        }).ToList();

        return new MiniAppProOrderData
        {
            Id                  = o.Id,
            OrderCode           = o.OrderCode,
            BagTag              = o.BagTag,
            CustomerId          = o.CustomerId,
            CustomerName        = o.CustomerName,
            CustomerPhoneMasked = MaskPhone(o.CustomerPhone),
            TotalAmount         = o.TotalAmount,
            ServiceStatus       = o.ServiceStatus,
            PaymentStatus       = o.PaymentStatus,
            PaymentMethod       = o.PaymentMethod,
            Note                = o.Note,
            CreationTime        = o.CreationTime,
            TotalQuantity       = items.Sum(i => i.Quantity),
            ItemCount           = items.Count,
            CancelNote          = o.CancelNote,
            CancelledAt         = o.CancelledAt,
            Items               = items
        };
    }

    private async Task WriteActivityAsync(Guid orderId, string actionType, string title,
        string? description = null, bool isDanger = false)
    {
        var activity = new ProOrderActivity(
            GuidGenerator.Create(), orderId, actionType, title, description,
            Clock.Now, isDanger, _currentTenant.Id);
        await _activityRepository.InsertAsync(activity, autoSave: true);
    }

    private async Task<string> GenerateOrderCodeAsync()
    {
        var today = Clock.Now.ToString("ddMMyy");
        var prefix = $"PS{today}";
        var query = await _orderRepository.GetQueryableAsync();
        var count = await AsyncExecuter.CountAsync(
            query.Where(x => x.OrderCode.StartsWith(prefix)));
        return $"{prefix}{(count + 1):D3}";
    }

    private static MiniAppProOrderDetailDto Error(string msg)
        => new() { Error = 1, Message = msg, Data = null! };

    private static string MaskPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return "";
        return phone.Length > 4
            ? phone[..^4].PadRight(phone.Length, '*')
            : "****";
    }
}
