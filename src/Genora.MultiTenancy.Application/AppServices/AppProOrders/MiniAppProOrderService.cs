using Genora.MultiTenancy.AppDtos.AppEmails;
using Genora.MultiTenancy.AppDtos.AppProOrders;
using Genora.MultiTenancy.AppServices.AppEmails;
using Genora.MultiTenancy.AppServices.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppProCategories;
using Genora.MultiTenancy.DomainModels.AppProItems;
using Genora.MultiTenancy.DomainModels.AppProOrderActivity;
using Genora.MultiTenancy.DomainModels.AppProOrders;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.AppEmails;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Realtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;

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
    private readonly ISettingProvider _settingProvider;
    private readonly IAppEmailSenderService _appEmailSenderService;
    private readonly IBackgroundJobManager _jobManager;

    public MiniAppProOrderService(
        IRepository<ProOrder, Guid> orderRepository,
        IRepository<ProOrderItem, Guid> orderItemRepository,
        IRepository<ProOrderActivity, Guid> activityRepository,
        IRepository<ProItem, Guid> proItemRepository,
        IRepository<ProCategory, Guid> proCategoryRepository,
        ICurrentTenant currentTenant,
        IProOrderRealtimeNotifier notifier,
        IConfiguration configuration,
        ISettingProvider settingProvider,
        IAppEmailSenderService appEmailSenderService,
        IBackgroundJobManager jobManager)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _activityRepository = activityRepository;
        _proItemRepository = proItemRepository;
        _proCategoryRepository = proCategoryRepository;
        _currentTenant = currentTenant;
        _notifier = notifier;
        _configuration = configuration;
        _settingProvider = settingProvider;
        _appEmailSenderService = appEmailSenderService;
        _jobManager = jobManager;
    }

    public async Task<MiniAppProOrderDetailDto> CreateAsync(CreateProOrderDto input)
    {
        if (!input.Items.Any())
            return Error("Vui lòng thêm ít nhất một sản phẩm.");

        var orderCode = await GenerateOrderCodeAsync();
        var order = new ProOrder(GuidGenerator.Create(), orderCode, input.BagTag.Trim(), _currentTenant.Id)
        {
            CustomerId = input.CustomerId,
            CustomerName = input.CustomerName?.Trim(),
            CustomerPhone = input.CustomerPhone?.Trim(),
            Note = input.Note,
            PaymentMethod = input.PaymentMethod,
            ServiceStatus = ProServiceStatus.Created, // Value = 1
            PaymentStatus = ProPaymentStatus.Unpaid,  // Value = 1
            TotalAmount = 0
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
                ItemId = null,
                Note = itemInput.Note
            });
        }

        order.TotalAmount = orderItems.Sum(i => i.Price * i.Quantity);

        foreach (var orderItem in orderItems)
            order.Items.Add(orderItem);

        await _orderRepository.InsertAsync(order, autoSave: true);

        await WriteActivityAsync(order.Id, "Created",
            "Đơn hàng được tạo từ Mini App",
            $"Mã túi: {order.BagTag} | {orderItems.Count} sản phẩm | {order.TotalAmount:N0} VND");

        try { await _notifier.OrderCreatedAsync(order.Id); }
        catch { /* Broadcast realtime không chặn luồng chính */ }

        var orderDetail = await GetAsync(order.Id);
        try
        {
            if (orderDetail?.Data != null)
            {
                var orderData = orderDetail.Data;

                // === GỬI ZBS CHO QUẢN TRỊ VIÊN THÔNG BÁO ĐƠN HÀNG MỚI ===
                var itemsSummary = string.Join(", ", orderData.Items?.Select(x => $"{x.ItemName}") ?? Array.Empty<string>());

                var zbsTemplateData = new
                {
                    customer_name = string.IsNullOrWhiteSpace(orderData.CustomerName) ? "Khách vãng lai" : orderData.CustomerName,
                    order_code = orderData.OrderCode,
                    bag_tag = orderData.BagTag,
                    order_time = orderData.CreationTime.ToString("HH:mm dd/MM/yyyy", CultureInfo.InvariantCulture),
                    item_count = orderData.ItemCount,
                    proshop_list = itemsSummary,
                    transfer_amount = orderData.TotalAmount > 0 ? Convert.ToInt32(orderData.TotalAmount) : 0,
                    bank_transfer_note = $"Thanh toán đặt Proshop Mã đơn {orderData.OrderCode}"
                };

                try
                {
                    // Lấy SĐT Admin FnB từ SettingProvider
                    var adminPhone = await _settingProvider.GetOrNullAsync(ZaloSettingNames.ZbsProshopOrderPhoneNumber);

                    if (!string.IsNullOrWhiteSpace(adminPhone))
                    {
                        await _jobManager.EnqueueAsync(
                            new ZbsSendJobArgs
                            {
                                TenantId = _currentTenant.Id,
                                TemplateKey = "ProshopOrder", // Hoặc Key template ZBS thông báo cho Admin
                                Phone = adminPhone,
                                TrackingId = $"ADMIN_{order.Id}",
                                TemplateData = zbsTemplateData
                            },
                            priority: BackgroundJobPriority.Normal
                        );
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(
                        ex,
                        "[ZBS] Enqueue FnbOrder admin notification failed. OrderId={OrderId}, OrderCode={OrderCode}, TenantId={TenantId}",
                        order.Id,
                        order.OrderCode,
                        _currentTenant.Id
                    );
                }

                // === GỬI EMAIL THÔNG BÁO ĐƠN HÀNG PROSHOP MỚI ===
                var emailModel = new ProOrderNewRequestEmailModelDto
                {
                    OrderCode = orderData.OrderCode ?? string.Empty,
                    BagTag = orderData.BagTag ?? string.Empty,
                    CustomerName = string.IsNullOrWhiteSpace(orderData.CustomerName) ? "Khách vãng lai" : orderData.CustomerName,
                    CustomerPhone = orderData.CustomerPhoneMasked ?? "N/A",

                    ServiceStatus = (int)orderData.ServiceStatus,
                    ServiceStatusText = GetProServiceStatusText(orderData.ServiceStatus),

                    PaymentStatus = (int)orderData.PaymentStatus,
                    PaymentStatusText = GetProPaymentStatusText(orderData.PaymentStatus),
                    PaymentMethodText = GetProPaymentMethodText(orderData.PaymentMethod),

                    Note = orderData.Note,
                    CancelReason = null,
                    CancelNote = orderData.CancelNote,

                    CreationTimeText = orderData.CreationTime.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                    TotalAmountText = MoneyText(orderData.TotalAmount),

                    Items = orderData.Items?.Select(x => new ProOrderItemEmailItemDto
                    {
                        ItemName = x.ItemName,
                        PriceText = MoneyText(x.Price),
                        Quantity = x.Quantity,
                        AmountText = MoneyText(x.LineTotal),
                        Note = x.Note
                    }).ToList() ?? new List<ProOrderItemEmailItemDto>()
                };

                var cfg = await EmailHelper.GetEmailConfigAsync(
                    _settingProvider,
                    AppEmailSettingNames.ProshopOrderNew_ToEmails,
                    AppEmailSettingNames.ProshopOrderNew_CcEmails,
                    AppEmailSettingNames.ProshopOrderNew_BccEmails,
                    AppEmailSettingNames.ProshopOrderNew_SubjectTemplate,
                    order.OrderCode,
                    fallbackTo: "tandv@baygolf.vn"
                );

                var subject = cfg.Subject?
                    .Replace("{OrderCode}", order.OrderCode)
                    .Replace("{0}", order.OrderCode);

                var templateData = new Dictionary<string, object>
                    {
                        { "model", emailModel },
                        { "order_code", emailModel.OrderCode },
                        { "OrderCode", emailModel.OrderCode },
                        { "bag_tag", emailModel.BagTag },
                        { "BagTag", emailModel.BagTag },
                        { "customer_name", emailModel.CustomerName },
                        { "CustomerName", emailModel.CustomerName },
                        { "customer_phone", emailModel.CustomerPhone },
                        { "CustomerPhone", emailModel.CustomerPhone },
                        { "creation_time_text", emailModel.CreationTimeText },
                        { "CreationTimeText", emailModel.CreationTimeText },
                        { "total_amount_text", emailModel.TotalAmountText },
                        { "TotalAmountText", emailModel.TotalAmountText },
                        { "items", emailModel.Items },
                        { "Items", emailModel.Items },
                        { "service_status", emailModel.ServiceStatus },
                        { "ServiceStatus", emailModel.ServiceStatus },
                        { "service_status_text", emailModel.ServiceStatusText },
                        { "ServiceStatusText", emailModel.ServiceStatusText },
                        { "payment_status", emailModel.PaymentStatus },
                        { "PaymentStatus", emailModel.PaymentStatus },
                        { "payment_status_text", emailModel.PaymentStatusText },
                        { "PaymentStatusText", emailModel.PaymentStatusText },
                        { "payment_method_text", emailModel.PaymentMethodText },
                        { "PaymentMethodText", emailModel.PaymentMethodText },
                        { "note", emailModel.Note },
                        { "Note", emailModel.Note }
                    };

                await _appEmailSenderService.EnqueueTemplateAsync(
                    templateName: AppEmailTemplateNames.ProshopOrderNewRequest,
                    model: templateData,
                    toEmails: cfg.To,
                    subject: subject,
                    cc: cfg.Cc,
                    bcc: cfg.Bcc,
                    bookingId: order.Id,
                    bookingCode: order.OrderCode
                );
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "[ProOrderEmail] Failed to enqueue order created email. OrderId={OrderId}, OrderCode={OrderCode}, TenantId={TenantId}",
                order.Id,
                order.OrderCode,
                _currentTenant.Id
            );
        }

        return orderDetail;
    }

    // ── Helper Methods hiển thị TextEnum ──────────────────────────────────────────

    private static string GetProServiceStatusText(ProServiceStatus status)
    {
        return status switch
        {
            ProServiceStatus.Created => "Đơn mới",         // 1
            ProServiceStatus.Processing => "Đang xử lý",       // 2
            ProServiceStatus.Ready => "Sẵn sàng giao",   // 3
            ProServiceStatus.Delivered => "Đã giao",         // 4
            ProServiceStatus.Cancelled => "Đã hủy",          // 5
            _ => "Đơn mới"
        };
    }

    private static string GetProPaymentStatusText(ProPaymentStatus status)
    {
        return status switch
        {
            ProPaymentStatus.Unpaid => "Chưa thanh toán",     // 1
            ProPaymentStatus.Paid => "Đã thanh toán",       // 2
            ProPaymentStatus.Failed => "Thanh toán thất bại", // 3
            _ => "Chưa thanh toán"
        };
    }

    private static string GetProPaymentMethodText(PaymentMethod? method)
    {
        return method switch
        {
            PaymentMethod.COD => "Thanh toán khi nhận hàng (COD)",
            PaymentMethod.Online => "Thanh toán trực tuyến",
            PaymentMethod.BankTransfer => "Chuyển khoản ngân hàng",
            _ => "Khác"
        };
    }

    private static string MoneyText(decimal amount)
    {
        return string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} VNĐ", amount);
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
