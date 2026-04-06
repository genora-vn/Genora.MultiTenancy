using Genora.MultiTenancy.AppDtos.AppFnbOrders;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppFnbCategories;
using Genora.MultiTenancy.DomainModels.AppFnbItems;
using Genora.MultiTenancy.DomainModels.AppFnbOrderActivity;
using Genora.MultiTenancy.DomainModels.AppFnbOrders;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Realtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.AppFnbOrders;

public class MiniAppFnbOrderService : ApplicationService, IMiniAppFnbOrderService
{
    private readonly IRepository<FnbOrder, Guid> _orderRepository;
    private readonly IRepository<FnbOrderItem, Guid> _orderItemRepository;
    private readonly IRepository<FnbItem, Guid> _itemRepository;
    private readonly IRepository<FnbCategory, Guid> _categoryRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<FnbOrderActivity, Guid> _orderActivityRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IFnbOrderRealtimeNotifier _notifier;
    private readonly IConfiguration _configuration;

    public MiniAppFnbOrderService(
        IRepository<FnbOrder, Guid> orderRepository,
        IRepository<FnbOrderItem, Guid> orderItemRepository,
        IRepository<FnbItem, Guid> itemRepository,
        IRepository<FnbCategory, Guid> categoryRepository,
        IRepository<Customer, Guid> customerRepository,
        ICurrentTenant currentTenant,
        IFnbOrderRealtimeNotifier notifier,
        IRepository<FnbOrderActivity, Guid> orderActivityRepository,
        IConfiguration configuration)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _itemRepository = itemRepository;
        _categoryRepository = categoryRepository;
        _customerRepository = customerRepository;
        _currentTenant = currentTenant;
        _notifier = notifier;
        _orderActivityRepository = orderActivityRepository;
        _configuration = configuration;
    }

    public async Task<MiniAppFnbOrderDetailDto> CreateAsync(CreateFnbOrderDto input)
    {
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

        var order = new FnbOrder(
            GuidGenerator.Create(),
            await GenerateOrderCodeAsync(),
            input.BagTag.Trim(),
            _currentTenant.Id)
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

            var orderItem = new FnbOrderItem(
                GuidGenerator.Create(),
                order.Id,
                item.Name,
                item.Price,
                row.Quantity)
            {
                ItemId = null, // FK cross-tenant: dùng ItemName để lookup ảnh
                Note = string.IsNullOrWhiteSpace(row.Note) ? null : row.Note.Trim()
            };

            orderItems.Add(orderItem);
        }

        order.TotalAmount = total;

        try
        {
            await _orderRepository.InsertAsync(order, autoSave: true);
            await _orderItemRepository.InsertManyAsync(orderItems, autoSave: true);

            await _orderActivityRepository.InsertAsync(
                new FnbOrderActivity(
                    GuidGenerator.Create(),
                    order.Id,
                    "Created",
                    "Đơn hàng được khởi tạo",
                    $"Đơn hàng {order.OrderCode} đã được tạo.",
                    Clock.Now,
                    false,
                    _currentTenant.Id
                ),
                autoSave: true
            );

            await _notifier.OrderCreatedAsync(order.Id);

            return await GetAsync(order.Id);
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "FNB_ORDER_SAVE_FAILED | TenantId={TenantId} | OrderId={OrderId}",
                _currentTenant.Id,
                order.Id
            );

            throw;
        }
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

        var orders = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount)
        );

        var orderIds = orders.Select(x => x.Id).ToList();
        var orderItems = await _orderItemRepository.GetListAsync(x => orderIds.Contains(x.OrderId));

        var linkedItemIds = orderItems
            .Where(x => x.ItemId.HasValue)
            .Select(x => x.ItemId!.Value)
            .Distinct()
            .ToList();

        var itemQuery = await _itemRepository.GetQueryableAsync();
        var linkedItems = linkedItemIds.Count > 0
            ? await AsyncExecuter.ToListAsync(itemQuery.Where(x => linkedItemIds.Contains(x.Id)))
            : new List<FnbItem>();

        var orderItemNames = orderItems
            .Select(x => x.ItemName?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var fallbackItemsByName = orderItemNames.Count > 0
            ? await AsyncExecuter.ToListAsync(itemQuery.Where(x => orderItemNames.Contains(x.Name)))
            : new List<FnbItem>();

        var allItemEntities = linkedItems
            .Concat(fallbackItemsByName)
            .GroupBy(x => x.Id)
            .Select(g => g.First())
            .ToList();

        var itemById = allItemEntities.ToDictionary(x => x.Id, x => x);
        var itemByName = allItemEntities
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .GroupBy(x => x.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var categoryIds = allItemEntities
            .Select(x => x.CategoryId)
            .Distinct()
            .ToList();

        var categories = categoryIds.Count > 0
            ? await _categoryRepository.GetListAsync(x => categoryIds.Contains(x.Id))
            : new List<FnbCategory>();

        var categoryDict = categories.ToDictionary(x => x.Id, x => x.Name);

        var cancelledByIds = orders
            .Where(x => x.CancelledBy.HasValue)
            .Select(x => x.CancelledBy!.Value)
            .Distinct()
            .ToList();

        var cancelledCustomers = cancelledByIds.Count > 0
            ? await _customerRepository.GetListAsync(x => cancelledByIds.Contains(x.Id))
            : new List<Customer>();

        var cancelledCustomerDict = cancelledCustomers.ToDictionary(x => x.Id, x => x);

        var data = orders.Select(order =>
        {
            var itemsOfOrder = orderItems
                .Where(x => x.OrderId == order.Id)
                .OrderBy(x => x.Id)
                .Select(x =>
                {
                    FnbItem? itemEntity = null;

                    if (x.ItemId.HasValue && itemById.TryGetValue(x.ItemId.Value, out var byId))
                    {
                        itemEntity = byId;
                    }
                    else if (!string.IsNullOrWhiteSpace(x.ItemName) && itemByName.TryGetValue(x.ItemName.Trim(), out var byName))
                    {
                        itemEntity = byName;
                    }

                    return new MiniAppFnbOrderItemData
                    {
                        Id = x.Id,
                        ItemId = itemEntity?.Id,
                        ItemName = x.ItemName,
                        Price = x.Price,
                        Quantity = x.Quantity,
                        Note = x.Note,
                        LineTotal = x.Price * x.Quantity,
                        ImageUrl = !string.IsNullOrWhiteSpace(itemEntity?.ImageUrl)
                            ? ImageHelper.NormalizeThumb(_configuration, itemEntity.ImageUrl)
                            : null,
                        CategoryName = itemEntity != null && categoryDict.TryGetValue(itemEntity.CategoryId, out var categoryName)
                            ? categoryName
                            : null,
                        SortOrder = itemEntity?.SortOrder,
                        IsAvailable = itemEntity?.IsAvailable,
                        IsActive = itemEntity?.IsActive
                    };
                })
                .ToList();

            string? cancelledByDisplay = null;
            if (order.CancelledBy.HasValue)
            {
                if (cancelledCustomerDict.TryGetValue(order.CancelledBy.Value, out var cancelCustomer))
                {
                    cancelledByDisplay = !string.IsNullOrWhiteSpace(cancelCustomer.FullName)
                        ? cancelCustomer.FullName
                        : cancelCustomer.Id.ToString();
                }
                else
                {
                    cancelledByDisplay = order.CancelledBy.Value.ToString();
                }
            }

            return new MiniAppFnbOrderData
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
                TotalQuantity = itemsOfOrder.Sum(x => x.Quantity),
                ItemCount = itemsOfOrder.Count,
                CancelReason = order.CancelReason?.ToString(),
                CancelNote = order.CancelNote,
                CancelledBy = cancelledByDisplay,
                CancelledAt = order.CancelledAt,
                Items = itemsOfOrder
            };
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

        var linkedItemIds = items
            .Where(x => x.ItemId.HasValue)
            .Select(x => x.ItemId!.Value)
            .Distinct()
            .ToList();

        var itemQuery = await _itemRepository.GetQueryableAsync();
        var linkedItems = linkedItemIds.Count > 0
            ? await AsyncExecuter.ToListAsync(itemQuery.Where(x => linkedItemIds.Contains(x.Id)))
            : new List<FnbItem>();

        var itemNames = items
            .Select(x => x.ItemName?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var fallbackItemsByName = itemNames.Count > 0
            ? await AsyncExecuter.ToListAsync(itemQuery.Where(x => itemNames.Contains(x.Name)))
            : new List<FnbItem>();

        var allItemEntities = linkedItems
            .Concat(fallbackItemsByName)
            .GroupBy(x => x.Id)
            .Select(g => g.First())
            .ToList();

        var itemById = allItemEntities.ToDictionary(x => x.Id, x => x);
        var itemByName = allItemEntities
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .GroupBy(x => x.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var categoryIds = allItemEntities
            .Select(x => x.CategoryId)
            .Distinct()
            .ToList();

        var categories = categoryIds.Count > 0
            ? await _categoryRepository.GetListAsync(x => categoryIds.Contains(x.Id))
            : new List<FnbCategory>();

        var categoryDict = categories.ToDictionary(x => x.Id, x => x.Name);

        Customer? cancelledCustomer = null;
        if (order.CancelledBy.HasValue)
        {
            cancelledCustomer = await _customerRepository.FirstOrDefaultAsync(x => x.Id == order.CancelledBy.Value);
        }

        var itemDtos = items
            .OrderBy(x => x.Id)
            .Select(x =>
            {
                FnbItem? itemEntity = null;

                if (x.ItemId.HasValue && itemById.TryGetValue(x.ItemId.Value, out var byId))
                {
                    itemEntity = byId;
                }
                else if (!string.IsNullOrWhiteSpace(x.ItemName) && itemByName.TryGetValue(x.ItemName.Trim(), out var byName))
                {
                    itemEntity = byName;
                }

                return new MiniAppFnbOrderItemData
                {
                    Id = x.Id,
                    ItemId = itemEntity?.Id,
                    ItemName = x.ItemName,
                    Price = x.Price,
                    Quantity = x.Quantity,
                    Note = x.Note,
                    LineTotal = x.Price * x.Quantity,
                    ImageUrl = !string.IsNullOrWhiteSpace(itemEntity?.ImageUrl)
                        ? ImageHelper.NormalizeThumb(_configuration, itemEntity.ImageUrl)
                        : null,
                    CategoryName = itemEntity != null && categoryDict.TryGetValue(itemEntity.CategoryId, out var categoryName)
                        ? categoryName
                        : null,
                    SortOrder = itemEntity?.SortOrder,
                    IsAvailable = itemEntity?.IsAvailable,
                    IsActive = itemEntity?.IsActive
                };
            })
            .ToList();

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
                TotalQuantity = itemDtos.Sum(x => x.Quantity),
                ItemCount = itemDtos.Count,
                CancelReason = order.CancelReason?.ToString(),
                CancelNote = order.CancelNote,
                CancelledBy = cancelledCustomer?.FullName ?? order.CancelledBy?.ToString(),
                CancelledAt = order.CancelledAt,
                Items = itemDtos
            }
        };
    }

    public async Task<MiniAppFnbOrderDetailDto> CancelAsync(Guid id, CancelMiniAppFnbOrderDto input)
    {
        var order = await _orderRepository.GetAsync(id);

        if (input.CustomerId.HasValue && order.CustomerId.HasValue && input.CustomerId.Value != order.CustomerId.Value)
        {
            throw new UserFriendlyException("Bạn không có quyền hủy đơn này.");
        }

        if (order.ServiceStatus == FnbServiceStatus.Cancelled)
        {
            throw new UserFriendlyException("Đơn hàng đã được hủy trước đó.");
        }

        var cancelReason = ParseCancelReason(input.CancelReason);

        Customer? cancelledCustomer = null;
        if (input.CustomerId.HasValue)
        {
            cancelledCustomer = await _customerRepository.FirstOrDefaultAsync(x => x.Id == input.CustomerId.Value);
        }
        else if (order.CustomerId.HasValue)
        {
            cancelledCustomer = await _customerRepository.FirstOrDefaultAsync(x => x.Id == order.CustomerId.Value);
        }

        order.ServiceStatus = FnbServiceStatus.Cancelled;
        order.CancelReason = cancelReason;
        order.CancelNote = string.IsNullOrWhiteSpace(input.CancelNote) ? null : input.CancelNote.Trim();
        order.CancelledAt = Clock.Now;
        order.CancelledBy = input.CustomerId ?? order.CustomerId;

        await _orderRepository.UpdateAsync(order, autoSave: true);

        await _orderActivityRepository.InsertAsync(
            new FnbOrderActivity(
                GuidGenerator.Create(),
                order.Id,
                "Cancelled",
                "Đơn hàng đã bị hủy",
                $"Lý do: {(order.CancelReason?.ToString() ?? "Không có")}"
                    + (!string.IsNullOrWhiteSpace(order.CancelNote) ? $" | Ghi chú: {order.CancelNote}" : ""),
                Clock.Now,
                true,
                _currentTenant.Id
            ),
            autoSave: true
        );

        await _notifier.OrderUpdatedAsync(order.Id);

        return await GetAsync(order.Id);
    }

    private static FnbCancelReason? ParseCancelReason(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var value = raw.Trim();

        if (Enum.TryParse<FnbCancelReason>(value, true, out var byName))
        {
            return byName;
        }

        if (int.TryParse(value, out var intValue) && Enum.IsDefined(typeof(FnbCancelReason), intValue))
        {
            return (FnbCancelReason)intValue;
        }

        throw new UserFriendlyException("Lý do hủy không hợp lệ.");
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