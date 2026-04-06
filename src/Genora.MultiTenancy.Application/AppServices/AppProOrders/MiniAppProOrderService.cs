using Genora.MultiTenancy.AppDtos.AppProOrders;
using Genora.MultiTenancy.DomainModels.AppProOrderActivity;
using Genora.MultiTenancy.DomainModels.AppProOrders;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Realtime;
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
    private readonly ICurrentTenant _currentTenant;
    private readonly IProOrderRealtimeNotifier _notifier;

    public MiniAppProOrderService(
        IRepository<ProOrder, Guid> orderRepository,
        IRepository<ProOrderItem, Guid> orderItemRepository,
        IRepository<ProOrderActivity, Guid> activityRepository,
        ICurrentTenant currentTenant,
        IProOrderRealtimeNotifier notifier)
    {
        _orderRepository     = orderRepository;
        _orderItemRepository = orderItemRepository;
        _activityRepository  = activityRepository;
        _currentTenant       = currentTenant;
        _notifier            = notifier;
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

        var itemRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<DomainModels.AppProItems.ProItem, Guid>>();

        var orderItems = new List<ProOrderItem>();
        foreach (var itemInput in input.Items)
        {
            DomainModels.AppProItems.ProItem? proItem;
            try { proItem = await itemRepo.GetAsync(itemInput.ItemId); }
            catch { return Error($"Sản phẩm không tồn tại: {itemInput.ItemId}"); }

            orderItems.Add(new ProOrderItem(
                GuidGenerator.Create(), order.Id, proItem.Name, proItem.Price, itemInput.Quantity)
            {
                ItemId = itemInput.ItemId,
                Note   = itemInput.Note
            });
        }

        order.TotalAmount = orderItems.Sum(i => i.Price * i.Quantity);
        await _orderRepository.InsertAsync(order);
        await _orderItemRepository.InsertManyAsync(orderItems);
        await WriteActivityAsync(order.Id, "Created",
            "Đơn hàng được tạo từ Mini App",
            $"Mã túi: {order.BagTag} | {orderItems.Count} sản phẩm | {order.TotalAmount:N0} VND");
        await CurrentUnitOfWork!.SaveChangesAsync();

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

        return new MiniAppProOrderListDto
        {
            Error = 0,
            Message = "Success",
            Data = new PagedResultDto<MiniAppProOrderData>(total,
                orders.Select(o => ToData(o)).ToList())
        };
    }

    public async Task<MiniAppProOrderDetailDto> GetAsync(Guid id)
    {
        ProOrder order;
        try
        {
            order = await _orderRepository.GetAsync(
                x => x.Id == id,
                includeDetails: true);
        }
        catch
        {
            return Error("Không tìm thấy đơn hàng.");
        }

        return new MiniAppProOrderDetailDto
        {
            Error = 0,
            Message = "Success",
            Data = ToData(order)
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
    private MiniAppProOrderData ToData(ProOrder o)
    {
        var items = o.Items.Select(i => new MiniAppProOrderItemData
        {
            Id          = i.Id,
            ItemId      = i.ItemId,
            ItemName    = i.ItemName,
            Price       = i.Price,
            Quantity    = i.Quantity,
            Note        = i.Note,
            LineTotal   = i.Price * i.Quantity
        }).ToList();

        return new MiniAppProOrderData
        {
            Id            = o.Id,
            OrderCode     = o.OrderCode,
            BagTag        = o.BagTag,
            CustomerId    = o.CustomerId,
            CustomerName  = o.CustomerName,
            CustomerPhoneMasked = MaskPhone(o.CustomerPhone),
            TotalAmount   = o.TotalAmount,
            ServiceStatus = o.ServiceStatus,
            PaymentStatus = o.PaymentStatus,
            PaymentMethod = o.PaymentMethod,
            Note          = o.Note,
            CreationTime  = o.CreationTime,
            TotalQuantity = items.Sum(i => i.Quantity),
            ItemCount     = items.Count,
            CancelNote    = o.CancelNote,
            CancelledAt   = o.CancelledAt,
            Items         = items
        };
    }

    private async Task WriteActivityAsync(Guid orderId, string actionType, string title,
        string? description = null, bool isDanger = false)
    {
        var activity = new ProOrderActivity(
            GuidGenerator.Create(), orderId, actionType, title, description,
            Clock.Now, isDanger, _currentTenant.Id);
        await _activityRepository.InsertAsync(activity);
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
