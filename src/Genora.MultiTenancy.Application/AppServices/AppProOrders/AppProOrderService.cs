using Genora.MultiTenancy.AppDtos.AppProOrders;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppProOrderActivity;
using Genora.MultiTenancy.DomainModels.AppProOrders;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Permissions;
using Genora.MultiTenancy.Realtime;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppProOrders;

[Authorize]
public class AppProOrderService : ApplicationService, IAppProOrderService
{
    private readonly IRepository<ProOrder, Guid> _orderRepository;
    private readonly IRepository<ProOrderActivity, Guid> _activityRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IProOrderRealtimeNotifier _notifier;
    private readonly IConfiguration _configuration;
    private readonly IIdentityUserRepository _identityUserRepository;

    public AppProOrderService(
        IRepository<ProOrder, Guid> orderRepository,
        IRepository<ProOrderActivity, Guid> activityRepository,
        IRepository<Customer, Guid> customerRepository,
        ICurrentTenant currentTenant,
        IProOrderRealtimeNotifier notifier,
        IConfiguration configuration,
        IIdentityUserRepository identityUserRepository)
    {
        _orderRepository          = orderRepository;
        _activityRepository       = activityRepository;
        _customerRepository       = customerRepository;
        _currentTenant            = currentTenant;
        _notifier                 = notifier;
        _configuration            = configuration;
        _identityUserRepository   = identityUserRepository;
    }

    // ── Permission helpers: tự động chọn Tenant vs Host permission ──────────
    private string GetRootPermission()
        => CurrentTenant.IsAvailable
            ? MultiTenancyPermissions.AppProOrders.Default
            : MultiTenancyPermissions.HostAppProOrders.Default;

    private string GetCreatePermission()
        => CurrentTenant.IsAvailable
            ? MultiTenancyPermissions.AppProOrders.Create
            : MultiTenancyPermissions.HostAppProOrders.Create;

    private string GetEditPermission()
        => CurrentTenant.IsAvailable
            ? MultiTenancyPermissions.AppProOrders.Edit
            : MultiTenancyPermissions.HostAppProOrders.Edit;

    private async Task CheckPolicyAsync(string perm)
        => await AuthorizationService.CheckAsync(perm);

    public async Task<PagedResultDto<ProOrderDto>> GetListAsync(GetProOrderListInput input)
    {
        await CheckPolicyAsync(GetRootPermission());

        var query = await BuildQueryAsync(input);
        var total = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? "CreationTime desc" : input.Sorting;

        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<ProOrderDto>(total,
            ObjectMapper.Map<List<ProOrder>, List<ProOrderDto>>(items));
    }

    public async Task<ProOrderDetailDto> GetAsync(Guid id)
    {
        await CheckPolicyAsync(GetRootPermission());

        // Dùng WithDetailsAsync để eager-load Items — GetAsync thông thường không include navigation property
        var query   = await _orderRepository.WithDetailsAsync(o => o.Items);
        var order   = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id))
                      ?? throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(ProOrder), id);

        var activities = await _activityRepository.GetListAsync(a => a.OrderId == id);

        var detail = ObjectMapper.Map<ProOrder, ProOrderDetailDto>(order);

        if (order.CustomerId.HasValue)
        {
            var customer = await _customerRepository.FindAsync(order.CustomerId.Value);
            if (customer != null)
            {
                detail.CustomerPhoneMasked = MaskPhone(customer.PhoneNumber);
            }
        }

        detail.Items = order.Items
            .Select(i => ObjectMapper.Map<ProOrderItem, ProOrderItemDto>(i)).ToList();

        // Populate ImageUrl cho từng item từ ProItem master data
        var itemIds = order.Items.Where(x => x.ItemId.HasValue).Select(x => x.ItemId!.Value).Distinct().ToList();
        if (itemIds.Count > 0)
        {
            var proItemRepo  = LazyServiceProvider.LazyGetRequiredService<IRepository<DomainModels.AppProItems.ProItem, Guid>>();
            var proItemQuery = await proItemRepo.GetQueryableAsync();
            var proItems     = await AsyncExecuter.ToListAsync(proItemQuery.Where(x => itemIds.Contains(x.Id)));
            var imageMap     = proItems.ToDictionary(
                x => x.Id,
                x => string.IsNullOrWhiteSpace(x.ImageUrl) ? null : ImageHelper.NormalizeThumb(_configuration, x.ImageUrl));
            foreach (var item in detail.Items)
                if (item.ItemId.HasValue && imageMap.TryGetValue(item.ItemId.Value, out var img))
                    item.ImageUrl = img;
        }

        detail.Activities = activities
            .OrderBy(a => a.ActionTime)
            .Select(a => new ProOrderActivityDto
            {
                Title       = a.Title,
                Description = a.Description ?? "",
                Time        = a.ActionTime,
                IsDanger    = a.IsDanger
            }).ToList();

        return detail;
    }

    public async Task<ProOrderDetailDto> CreateAsync(CreateProOrderDto input)
    {
        await CheckPolicyAsync(GetCreatePermission());

        if (!input.Items.Any())
            throw new UserFriendlyException("Vui lòng thêm ít nhất một sản phẩm.");

        var orderCode = await GenerateOrderCodeAsync();
        var order = new ProOrder(GuidGenerator.Create(), orderCode, input.BagTag.Trim(), _currentTenant.Id)
        {
            CustomerId     = input.CustomerId,
            CustomerName   = input.CustomerName?.Trim(),
            CustomerPhone  = input.CustomerPhone?.Trim(),
            Note           = input.Note,
            InternalNote   = input.InternalNote,
            PaymentMethod  = input.PaymentMethod,
            ServiceStatus  = ProServiceStatus.Created,
            PaymentStatus  = ProPaymentStatus.Unpaid,
            TotalAmount    = 0
        };

        foreach (var itemInput in input.Items)
        {
            var proItem = await GetProItemAsync(itemInput.ItemId);

            var orderItem = new ProOrderItem(
                GuidGenerator.Create(),
                order.Id,
                proItem.Name,
                proItem.Price,
                itemInput.Quantity)
            {
                ItemId = itemInput.ItemId,
                Note   = itemInput.Note
            };

            order.Items.Add(orderItem);
        }

        order.TotalAmount = order.Items.Sum(i => i.Price * i.Quantity);
        order = await _orderRepository.InsertAsync(order, autoSave: true);

        await WriteActivityAsync(order.Id, "Created", "Đơn hàng được tạo",
            $"Mã túi: {order.BagTag} | {order.Items.Count} sản phẩm | {order.TotalAmount:N0} VND");

        await _notifier.OrderCreatedAsync(order.Id);

        return await GetAsync(order.Id);
    }

    public async Task<ProOrderDto> UpdateServiceStatusAsync(Guid id, UpdateProOrderServiceStatusDto input)
    {
        await CheckPolicyAsync(GetEditPermission());

        var order = await _orderRepository.GetAsync(id);
        var oldStatus = order.ServiceStatus;

        if (order.ServiceStatus == ProServiceStatus.Cancelled)
            throw new UserFriendlyException("Không thể cập nhật đơn hàng đã hủy.");

        order.ServiceStatus = input.ServiceStatus;
        if (!string.IsNullOrWhiteSpace(input.InternalNote))
            order.InternalNote = input.InternalNote;

        await _orderRepository.UpdateAsync(order, autoSave: true);
        await WriteActivityAsync(order.Id, "ServiceStatusChanged",
            $"Trạng thái dịch vụ: {oldStatus} → {input.ServiceStatus}",
            input.InternalNote);

        await _notifier.OrderUpdatedAsync(order.Id);

        return ObjectMapper.Map<ProOrder, ProOrderDto>(order);
    }

    public async Task<ProOrderDto> UpdatePaymentStatusAsync(Guid id, UpdateProOrderPaymentStatusDto input)
    {
        await CheckPolicyAsync(GetEditPermission());

        var order = await _orderRepository.GetAsync(id);
        var oldStatus = order.PaymentStatus;

        if (order.ServiceStatus == ProServiceStatus.Cancelled)
            throw new UserFriendlyException("Không thể cập nhật đơn hàng đã hủy.");

        order.PaymentStatus = input.PaymentStatus;
        await _orderRepository.UpdateAsync(order, autoSave: true);
        await WriteActivityAsync(order.Id, "PaymentStatusChanged",
            $"Trạng thái thanh toán: {oldStatus} → {input.PaymentStatus}", null);

        await _notifier.OrderUpdatedAsync(order.Id);

        return ObjectMapper.Map<ProOrder, ProOrderDto>(order);
    }

    public async Task<ProOrderDto> CancelAsync(Guid id, CancelProOrderDto input)
    {
        await CheckPolicyAsync(GetEditPermission());

        var order = await _orderRepository.GetAsync(id);

        if (order.ServiceStatus == ProServiceStatus.Delivered)
            throw new UserFriendlyException("Không thể hủy đơn hàng đã giao.");

        if (order.ServiceStatus == ProServiceStatus.Cancelled)
            throw new UserFriendlyException("Đơn hàng đã được hủy trước đó.");

        order.ServiceStatus = ProServiceStatus.Cancelled;
        order.CancelReason  = input.CancelReason;
        order.CancelNote    = input.CancelNote;
        order.CancelledAt   = Clock.Now;
        order.CancelledBy   = CurrentUser.Id;

        await _orderRepository.UpdateAsync(order, autoSave: true);
        await WriteActivityAsync(order.Id, "Cancelled",
            $"Hủy đơn: {input.CancelReason}", input.CancelNote, isDanger: true);

        await _notifier.OrderUpdatedAsync(order.Id);

        return ObjectMapper.Map<ProOrder, ProOrderDto>(order);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private async Task<IQueryable<ProOrder>> BuildQueryAsync(GetProOrderListInput input)
    {
        var query = (await _orderRepository.WithDetailsAsync(o => o.Items)).AsQueryable();

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var f = input.FilterText.Trim();
            query = query.Where(x =>
                x.OrderCode.Contains(f) ||
                x.BagTag.Contains(f) ||
                (x.CustomerName != null && x.CustomerName.Contains(f)) ||
                (x.CustomerPhone != null && x.CustomerPhone.Contains(f)));
        }

        if (!input.BagTag.IsNullOrWhiteSpace())
            query = query.Where(x => x.BagTag == input.BagTag!.Trim());

        if (input.ServiceStatus.HasValue)
            query = query.Where(x => x.ServiceStatus == input.ServiceStatus.Value);

        if (input.PaymentStatus.HasValue)
            query = query.Where(x => x.PaymentStatus == input.PaymentStatus.Value);

        if (input.CreationTimeFrom.HasValue)
            query = query.Where(x => x.CreationTime >= input.CreationTimeFrom.Value);

        if (input.CreationTimeTo.HasValue)
            query = query.Where(x => x.CreationTime <= input.CreationTimeTo.Value);

        return query;
    }

    private async Task<DomainModels.AppProItems.ProItem> GetProItemAsync(Guid itemId)
    {
        var repo = LazyServiceProvider.LazyGetRequiredService<IRepository<DomainModels.AppProItems.ProItem, Guid>>();
        return await repo.GetAsync(itemId);
    }

    private async Task WriteActivityAsync(Guid orderId, string actionType, string title,
        string? description = null, bool isDanger = false)
    {
        var repo = LazyServiceProvider.LazyGetRequiredService<IRepository<ProOrderActivity, Guid>>();
        var activity = new ProOrderActivity(
            GuidGenerator.Create(), orderId, actionType, title, description,
            Clock.Now, isDanger, _currentTenant.Id);
        await repo.InsertAsync(activity, autoSave: true);
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

    private static string MaskPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return "";
        return phone.Length > 4
            ? phone[..^4].PadRight(phone.Length, '*')
            : "****";
    }

    public async Task<IRemoteStreamContent> ExportExcelAsync(GetProOrderListInput input)
    {
        await CheckPolicyAsync(GetRootPermission());

        // Áp dụng cùng filter như GetListAsync — không phân trang, lấy toàn bộ
        var query = await BuildQueryAsync(input);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime));

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Proshop Orders");

        // Header
        var headers = new[]
        {
            "MÃ ĐƠN", "BAG TAG", "TÊN KHÁCH", "SỐ ĐIỆN THOẠI",
            "TỔNG TIỀN", "TRẠNG THÁI PHỤC VỤ", "TRẠNG THÁI THANH TOÁN",
            "GHI CHÚ", "NỘI BỘ", "NGÀY TẠO"
        };
        for (var col = 1; col <= headers.Length; col++)
            ws.Cell(1, col).Value = headers[col - 1];

        var headerRange = ws.Range(1, 1, 1, headers.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#059669");
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        var row = 2;
        foreach (var o in items)
        {
            ws.Cell(row, 1).Value = o.OrderCode;
            ws.Cell(row, 2).Value = o.BagTag;
            ws.Cell(row, 3).Value = o.CustomerName ?? "";
            ws.Cell(row, 4).Value = o.CustomerPhone ?? "";
            ws.Cell(row, 5).Value = (double)o.TotalAmount;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
            ws.Cell(row, 6).Value = MapServiceStatus(o.ServiceStatus);
            ws.Cell(row, 7).Value = MapPaymentStatus(o.PaymentStatus);
            ws.Cell(row, 8).Value = o.Note ?? "";
            ws.Cell(row, 9).Value = o.InternalNote ?? "";
            ws.Cell(row, 10).Value = o.CreationTime.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            row++;
        }

        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents();

        return StreamToRemoteContent(workbook, $"Export_ProshopOrders_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    private static string MapServiceStatus(ProServiceStatus status) => status switch
    {
        ProServiceStatus.Created    => "Mới tạo",
        ProServiceStatus.Processing => "Đang xử lý",
        ProServiceStatus.Ready      => "Sẵn sàng giao",
        ProServiceStatus.Delivered  => "Đã giao",
        ProServiceStatus.Cancelled  => "Đã hủy",
        _                           => "Không xác định"
    };

    private static string MapPaymentStatus(ProPaymentStatus status) => status switch
    {
        ProPaymentStatus.Unpaid => "Chưa thanh toán",
        ProPaymentStatus.Paid   => "Đã thanh toán",
        ProPaymentStatus.Failed => "Thanh toán lỗi",
        _                       => "Không xác định"
    };

    public async Task<ProOrderHistoryPageDto> GetHistoryPageAsync(GetProOrderHistoryInput input)
    {
        await CheckPolicyAsync(GetRootPermission());

        if (input.MaxResultCount <= 0) input.MaxResultCount = 10;

        var order = await _orderRepository.GetAsync(input.OrderId);

        CustomerType? customerType = null;
        if (order.CustomerId.HasValue)
        {
            var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Id == order.CustomerId.Value);
            if (customer?.CustomerTypeId != null)
            {
                var customerTypeRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<CustomerType, Guid>>();
                customerType = await customerTypeRepo.FirstOrDefaultAsync(x => x.Id == customer.CustomerTypeId!.Value);
            }
        }

        var activityQuery = await _activityRepository.GetQueryableAsync();

        var allQ = activityQuery.Where(x => x.OrderId == input.OrderId);

        var filteredQ = string.IsNullOrWhiteSpace(input.ActionType)
            ? allQ
            : allQ.Where(x => x.ActionType == input.ActionType);

        filteredQ = filteredQ.OrderByDescending(x => x.ActionTime);

        var totalCount = await AsyncExecuter.CountAsync(filteredQ);

        var pagedEntities = await AsyncExecuter.ToListAsync(
            filteredQ.Skip(input.SkipCount).Take(input.MaxResultCount));

        var allEntities = await AsyncExecuter.ToListAsync(
            allQ.OrderByDescending(x => x.ActionTime));

        var creatorIds = pagedEntities
            .Where(x => x.CreatorId.HasValue)
            .Select(x => x.CreatorId!.Value)
            .Distinct()
            .ToList();

        Dictionary<Guid, string> userMap = new();
        if (creatorIds.Count > 0)
        {
            var users = await _identityUserRepository.GetListAsync();
            userMap = users
                .Where(x => creatorIds.Contains(x.Id))
                .ToDictionary(
                    x => x.Id,
                    x => !string.IsNullOrWhiteSpace(x.Name)
                        ? x.Name!
                        : (!string.IsNullOrWhiteSpace(x.UserName) ? x.UserName! : "Hệ thống"));
        }

        var items = pagedEntities.Select(x =>
        {
            var performedBy = "Hệ thống";
            if (x.CreatorId.HasValue && userMap.TryGetValue(x.CreatorId.Value, out var actorName))
                performedBy = actorName;

            return new ProOrderHistoryItemDto
            {
                Time            = x.ActionTime,
                PerformedBy     = performedBy,
                ActionType      = x.ActionType,
                ActionTypeText  = MapProActionTypeText(x.ActionType),
                ActionTypeClass = MapProActionTypeClass(x.ActionType, x.IsDanger),
                Title           = x.Title,
                Description     = x.Description ?? string.Empty,
                IsDanger        = x.IsDanger
            };
        }).ToList();

        var actionTypeOptions = new List<ProOrderHistoryActionTypeOptionDto>
        {
            new() { Value = "",                       Text = "Tất cả thao tác" },
            new() { Value = "Created",                Text = "Tạo đơn" },
            new() { Value = "ServiceStatusChanged",   Text = "Đổi trạng thái" },
            new() { Value = "PaymentStatusChanged",   Text = "Cập nhật thanh toán" },
            new() { Value = "Cancelled",              Text = "Hủy đơn" }
        };

        return new ProOrderHistoryPageDto
        {
            OrderId                = order.Id,
            OrderCode              = order.OrderCode,
            CustomerName           = order.CustomerName,
            CustomerPhoneMasked    = PhoneHelper.MaskPhone(order.CustomerPhone),
            CustomerTypeName       = customerType?.Name,
            BagTag                 = order.BagTag,
            ServiceStatus          = order.ServiceStatus,
            PaymentStatus          = order.PaymentStatus,
            CreationTime           = order.CreationTime,
            LastActivityTime       = allEntities.FirstOrDefault()?.ActionTime,
            TotalActions           = allEntities.Count,
            CurrentFilterActionType = input.ActionType,
            ActionTypeOptions      = actionTypeOptions,
            PagedActivities        = new PagedResultDto<ProOrderHistoryItemDto>(totalCount, items)
        };
    }

    private static string MapProActionTypeText(string actionType) => actionType switch
    {
        "Created"              => "Tạo đơn",
        "ServiceStatusChanged" => "Đổi trạng thái",
        "PaymentStatusChanged" => "Cập nhật thanh toán",
        "Cancelled"            => "Hủy đơn",
        _                      => "Thao tác"
    };

    private static string MapProActionTypeClass(string actionType, bool isDanger)
    {
        if (isDanger) return "danger";
        return actionType switch
        {
            "Created"              => "orange",
            "ServiceStatusChanged" => "blue",
            "PaymentStatusChanged" => "green",
            _                      => "gray"
        };
    }

    public async Task<List<ProBoardItemDto>> GetBoardAsync(GetProBoardInput input)
    {
        await CheckPolicyAsync(GetRootPermission());

        var orderQuery = await _orderRepository.GetQueryableAsync();

        var orders = await AsyncExecuter.ToListAsync(
            orderQuery
                .Where(x => x.ServiceStatus != ProServiceStatus.Cancelled)
                .OrderBy(x => x.CreationTime)
        );

        if (!string.IsNullOrWhiteSpace(input.FilterText))
        {
            var keyword = input.FilterText.Trim().ToLowerInvariant();
            orders = orders.Where(x =>
                x.OrderCode.ToLower().Contains(keyword) ||
                x.BagTag.ToLower().Contains(keyword) ||
                (!string.IsNullOrWhiteSpace(x.CustomerName) && x.CustomerName.ToLower().Contains(keyword))
            ).ToList();
        }

        if (input.ServiceStatus.HasValue)
            orders = orders.Where(x => x.ServiceStatus == input.ServiceStatus.Value).ToList();

        var orderIds     = orders.Select(x => x.Id).ToList();
        var customerIds  = orders.Where(x => x.CustomerId.HasValue).Select(x => x.CustomerId!.Value).Distinct().ToList();

        // Lấy order items qua WithDetailsAsync trên một số order
        var orderItemRepo     = LazyServiceProvider.LazyGetRequiredService<IRepository<ProOrderItem, Guid>>();
        var proItemRepo       = LazyServiceProvider.LazyGetRequiredService<IRepository<DomainModels.AppProItems.ProItem, Guid>>();
        var customerTypeRepo  = LazyServiceProvider.LazyGetRequiredService<IRepository<CustomerType, Guid>>();

        var orderItemQuery = await orderItemRepo.GetQueryableAsync();
        var orderItems = orderIds.Count == 0
            ? new List<ProOrderItem>()
            : await AsyncExecuter.ToListAsync(orderItemQuery.Where(x => orderIds.Contains(x.OrderId)));

        var itemIds = orderItems.Where(x => x.ItemId.HasValue).Select(x => x.ItemId!.Value).Distinct().ToList();

        var proItemQuery = await proItemRepo.GetQueryableAsync();
        var proItems = itemIds.Count == 0
            ? new List<DomainModels.AppProItems.ProItem>()
            : await AsyncExecuter.ToListAsync(proItemQuery.Where(x => itemIds.Contains(x.Id)));

        var customers = customerIds.Count == 0
            ? new List<Customer>()
            : await AsyncExecuter.ToListAsync(
                (await _customerRepository.GetQueryableAsync()).Where(x => customerIds.Contains(x.Id)));

        var customerTypeIds = customers.Where(x => x.CustomerTypeId.HasValue).Select(x => x.CustomerTypeId!.Value).Distinct().ToList();
        var ctQuery = await customerTypeRepo.GetQueryableAsync();
        var customerTypes = customerTypeIds.Count == 0
            ? new List<CustomerType>()
            : await AsyncExecuter.ToListAsync(ctQuery.Where(x => customerTypeIds.Contains(x.Id)));

        var activities = orderIds.Count == 0
            ? new List<ProOrderActivity>()
            : await AsyncExecuter.ToListAsync(
                (await _activityRepository.GetQueryableAsync()).Where(x => orderIds.Contains(x.OrderId)));

        var orderItemMap       = orderItems.GroupBy(x => x.OrderId).ToDictionary(x => x.Key, x => x.ToList());
        var proItemMap         = proItems.ToDictionary(x => x.Id, x => x);
        var customerMap        = customers.ToDictionary(x => x.Id, x => x);
        var customerTypeMap    = customerTypes.ToDictionary(x => x.Id, x => x);
        var latestActivityMap  = activities
            .GroupBy(x => x.OrderId)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(a => a.ActionTime).FirstOrDefault());

        return orders.Select(order =>
        {
            orderItemMap.TryGetValue(order.Id, out var orderItemList);
            orderItemList ??= new List<ProOrderItem>();

            CustomerType? customerType = null;
            if (order.CustomerId.HasValue && customerMap.TryGetValue(order.CustomerId.Value, out var cust))
                if (cust.CustomerTypeId.HasValue && customerTypeMap.TryGetValue(cust.CustomerTypeId.Value, out var ct))
                    customerType = ct;

            string? primaryImage = null;
            var itemNames = new List<string>();
            foreach (var oi in orderItemList)
            {
                itemNames.Add(oi.ItemName);
                if (primaryImage == null && oi.ItemId.HasValue && proItemMap.TryGetValue(oi.ItemId.Value, out var pi))
                    if (!string.IsNullOrWhiteSpace(pi.ImageUrl))
                        primaryImage = ImageHelper.NormalizeThumb(_configuration, pi.ImageUrl);
            }

            latestActivityMap.TryGetValue(order.Id, out var latestActivity);

            return new ProBoardItemDto
            {
                Id                       = order.Id,
                OrderCode                = order.OrderCode,
                BagTag                   = order.BagTag,
                CustomerName             = order.CustomerName,
                CustomerPhoneMasked      = MaskPhone(order.CustomerPhone),
                CustomerTypeName         = customerType?.Name,
                CustomerTypeColorCode    = customerType?.ColorCode,
                Note                     = order.Note,
                TotalAmount              = order.TotalAmount,
                TotalQuantity            = orderItemList.Sum(x => x.Quantity),
                CreationTime             = order.CreationTime,
                ServiceStatus            = order.ServiceStatus,
                PaymentStatus            = order.PaymentStatus,
                PrimaryImageUrl          = primaryImage ?? "/images/pro/default-product.png",
                ItemsSummary             = string.Join(", ", orderItemList.Select(x => $"{x.ItemName} x{x.Quantity}")),
                ItemNotesSummary         = string.Join(" • ", orderItemList.Where(x => !string.IsNullOrWhiteSpace(x.Note)).Select(x => $"{x.ItemName}: {x.Note}")),
                LatestActivityTitle      = latestActivity?.Title,
                LatestActivityDescription = latestActivity?.Description,
                ItemNames                = itemNames
            };
        })
        .OrderBy(x => x.CreationTime)
        .ToList();
    }

    private static IRemoteStreamContent StreamToRemoteContent(XLWorkbook workbook, string fileName)    {
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return new RemoteStreamContent(stream, fileName,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}
