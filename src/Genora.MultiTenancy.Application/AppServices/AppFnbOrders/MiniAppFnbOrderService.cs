using Genora.MultiTenancy.AppDtos.AppFnbOrders;
using Genora.MultiTenancy.DomainModels.AppCustomers;
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
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<FnbOrderActivity, Guid> _orderActivityRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IFnbOrderRealtimeNotifier _notifier;
    private readonly IConfiguration _configuration;

    public MiniAppFnbOrderService(
        IRepository<FnbOrder, Guid> orderRepository,
        IRepository<FnbOrderItem, Guid> orderItemRepository,
        IRepository<FnbItem, Guid> itemRepository,
        IRepository<Customer, Guid> customerRepository,
        ICurrentTenant currentTenant,
        IFnbOrderRealtimeNotifier notifier,
        IRepository<FnbOrderActivity, Guid> orderActivityRepository,
        IConfiguration configuration)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _itemRepository = itemRepository;
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

            orderItems.Add(new FnbOrderItem(
                GuidGenerator.Create(),
                order.Id,
                item.Name,
                item.Price,
                row.Quantity)
            {
                ItemId = null,
                Note = string.IsNullOrWhiteSpace(row.Note) ? null : row.Note.Trim()
            });
        }

        order.TotalAmount = total;

        Logger.LogInformation(
            "MiniApp CreateAsync before save. TenantId={TenantId}, OrderId={OrderId}, ItemIds={ItemIds}",
            _currentTenant.Id,
            order.Id,
            string.Join(",", orderItems.Select(x => x.ItemId))
        );

        await _orderRepository.InsertAsync(order, autoSave: true);

        foreach (var orderItem in orderItems)
        {
            await _orderItemRepository.InsertAsync(orderItem, autoSave: false);
        }

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
            autoSave: false
        );

        await CurrentUnitOfWork.SaveChangesAsync();

        await _notifier.OrderCreatedAsync(order.Id);

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

        var itemIds = items
            .Where(x => x.ItemId.HasValue)
            .Select(x => x.ItemId!.Value)
            .Distinct()
            .ToList();

        var imageMap = new Dictionary<Guid, string?>();

        if (itemIds.Count > 0)
        {
            var itemQuery = await _itemRepository.GetQueryableAsync();
            var itemEntities = await AsyncExecuter.ToListAsync(
                itemQuery.Where(x => itemIds.Contains(x.Id))
            );

            imageMap = itemEntities.ToDictionary(
                x => x.Id,
                x => ImageHelper.NormalizeThumb(_configuration, x.ImageUrl)
            );
        }

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
                        Note = x.Note,
                        ImageUrl = x.ItemId.HasValue && imageMap.TryGetValue(x.ItemId.Value, out var imageUrl)
                            ? imageUrl
                            : null
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