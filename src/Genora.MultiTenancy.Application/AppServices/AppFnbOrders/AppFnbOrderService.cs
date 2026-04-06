using Genora.MultiTenancy.AppDtos.AppFnbOrders;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppFnbItems;
using Genora.MultiTenancy.DomainModels.AppFnbOrderActivity;
using Genora.MultiTenancy.DomainModels.AppFnbOrders;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.AppFnbFeatures;
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
using Volo.Abp.Features;
using Volo.Abp.Identity;
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
    private readonly IRepository<CustomerType, Guid> _customerTypeRepository;
    private readonly IRepository<FnbOrderActivity, Guid> _orderActivityRepository;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IConfiguration _configuration;
    private readonly ICurrentTenant _currentTenant;
    private readonly IFeatureChecker _featureChecker;
    private readonly ICurrentUser _currentUser;
    private readonly IFnbOrderRealtimeNotifier _notifier;

    public AppFnbOrderService(
        IRepository<FnbOrder, Guid> orderRepository,
        IRepository<FnbOrderItem, Guid> orderItemRepository,
        IRepository<FnbItem, Guid> itemRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<CustomerType, Guid> customerTypeRepository,
        IRepository<FnbOrderActivity, Guid> orderActivityRepository,
        IConfiguration configuration,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        ICurrentUser currentUser,
        IFnbOrderRealtimeNotifier notifier,
        IIdentityUserRepository identityUserRepository)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _itemRepository = itemRepository;
        _customerRepository = customerRepository;
        _customerTypeRepository = customerTypeRepository;
        _orderActivityRepository = orderActivityRepository;
        _configuration = configuration;
        _currentTenant = currentTenant;
        _featureChecker = featureChecker;
        _currentUser = currentUser;
        _notifier = notifier;
        _identityUserRepository = identityUserRepository;
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
        var items = await AsyncExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount)
        );

        return new PagedResultDto<FnbOrderDto>(
            totalCount,
            items.Select(MapOrderDto).ToList()
        );
    }

    public async Task<FnbOrderDetailDto> GetAsync(Guid id)
    {
        await CheckFeatureAndPolicyAsync(GetRootPermission());

        var order = await _orderRepository.GetAsync(id);
        var orderItems = await _orderItemRepository.GetListAsync(x => x.OrderId == id);

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

        Dictionary<Guid, string?> itemImageMap = new();

        if (itemIds.Count > 0)
        {
            var itemQuery = await _itemRepository.GetQueryableAsync();
            var items = await AsyncExecuter.ToListAsync(
                itemQuery.Where(x => itemIds.Contains(x.Id))
            );

            itemImageMap = items.ToDictionary(
                x => x.Id,
                x => string.IsNullOrWhiteSpace(x.ImageUrl)
                    ? null
                    : ImageHelper.NormalizeThumb(_configuration, x.ImageUrl)
            );
        }

        var activityQuery = await _orderActivityRepository.GetQueryableAsync();
        var activities = await AsyncExecuter.ToListAsync(
            activityQuery
                .Where(x => x.OrderId == id)
                .OrderByDescending(x => x.ActionTime)
        );

        return new FnbOrderDetailDto
        {
            Id = order.Id,
            TenantId = order.TenantId,
            OrderCode = order.OrderCode,
            BagTag = order.BagTag,
            CustomerId = order.CustomerId,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            CustomerPhoneMasked = PhoneHelper.MaskPhone(order.CustomerPhone),
            CustomerTypeName = customerType?.Name,
            CustomerTypeColorCode = customerType?.ColorCode,
            TotalAmount = order.TotalAmount,
            ServiceStatus = order.ServiceStatus,
            PaymentStatus = order.PaymentStatus,
            PaymentMethod = order.PaymentMethod,
            Note = order.Note,
            InternalNote = order.InternalNote,
            CancelReason = order.CancelReason,
            CancelNote = order.CancelNote,
            CancelledBy = order.CancelledBy,
            CancelledAt = order.CancelledAt,
            CreationTime = order.CreationTime,
            CreatorId = order.CreatorId,
            LastModificationTime = order.LastModificationTime,
            LastModifierId = order.LastModifierId,
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
                    Note = x.Note,
                    ImageUrl = x.ItemId.HasValue && itemImageMap.TryGetValue(x.ItemId.Value, out var imageUrl)
                        ? imageUrl
                        : null
                })
                .ToList(),
            Activities = activities
                .Select(x => new FnbOrderActivityDto
                {
                    Title = x.Title,
                    Description = x.Description ?? string.Empty,
                    Time = x.ActionTime,
                    IsDanger = x.IsDanger
                })
                .ToList()
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

            orderItems.Add(new FnbOrderItem(GuidGenerator.Create(), order.Id, item.Name, item.Price, row.Quantity)
            {
                ItemId = item.Id,
                Note = string.IsNullOrWhiteSpace(row.Note) ? null : row.Note.Trim()
            });
        }

        order.TotalAmount = total;

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

        var oldStatus = order.ServiceStatus;

        order.ServiceStatus = input.ServiceStatus;
        order.InternalNote = input.InternalNote;

        order = await _orderRepository.UpdateAsync(order, autoSave: true);

        await _orderActivityRepository.InsertAsync(
            new FnbOrderActivity(
                GuidGenerator.Create(),
                order.Id,
                "ServiceStatusChanged",
                $"Cập nhật trạng thái: {MapServiceStatus(input.ServiceStatus)}",
                $"Từ {MapServiceStatus(oldStatus)} sang {MapServiceStatus(input.ServiceStatus)}" +
                (!string.IsNullOrWhiteSpace(input.InternalNote) ? $". Ghi chú: {input.InternalNote}" : string.Empty),
                Clock.Now,
                false,
                _currentTenant.Id
            ),
            autoSave: true
        );

        await _notifier.OrderUpdatedAsync(order.Id);

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

        var oldStatus = order.PaymentStatus;

        order.PaymentStatus = input.PaymentStatus;
        order = await _orderRepository.UpdateAsync(order, autoSave: true);

        await _orderActivityRepository.InsertAsync(
            new FnbOrderActivity(
                GuidGenerator.Create(),
                order.Id,
                "PaymentStatusChanged",
                $"Cập nhật thanh toán: {MapPaymentStatus(input.PaymentStatus)}",
                $"Từ {MapPaymentStatus(oldStatus)} sang {MapPaymentStatus(input.PaymentStatus)}",
                Clock.Now,
                input.PaymentStatus == FnbPaymentStatus.Failed,
                _currentTenant.Id
            ),
            autoSave: true
        );

        await _notifier.OrderUpdatedAsync(order.Id);

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

        await _orderActivityRepository.InsertAsync(
            new FnbOrderActivity(
                GuidGenerator.Create(),
                order.Id,
                "Cancelled",
                "Hủy đơn hàng",
                $"Lý do hủy: {input.CancelReason}" +
                (!string.IsNullOrWhiteSpace(order.CancelNote) ? $". Ghi chú: {order.CancelNote}" : string.Empty),
                order.CancelledAt!.Value,
                true,
                _currentTenant.Id
            ),
            autoSave: true
        );

        await _notifier.OrderUpdatedAsync(order.Id);

        return MapOrderDto(order);
    }

    // Giữ overload cũ để các chỗ gọi cũ không vỡ
    public async Task<FnbOrderHistoryPageDto> GetHistoryPageAsync(Guid id)
    {
        return await GetHistoryPageAsync(new GetFnbOrderHistoryInput
        {
            OrderId = id,
            SkipCount = 0,
            MaxResultCount = 10
        });
    }

    public async Task<FnbOrderHistoryPageDto> GetHistoryPageAsync(GetFnbOrderHistoryInput input)
    {
        await CheckFeatureAndPolicyAsync(GetRootPermission());

        if (input.MaxResultCount <= 0)
        {
            input.MaxResultCount = 10;
        }

        var order = await _orderRepository.GetAsync(input.OrderId);

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

        var activityQuery = await _orderActivityRepository.GetQueryableAsync();

        var allActivitiesQuery = activityQuery
            .Where(x => x.OrderId == input.OrderId);

        var filteredQuery = allActivitiesQuery;

        if (!string.IsNullOrWhiteSpace(input.ActionType))
        {
            filteredQuery = filteredQuery.Where(x => x.ActionType == input.ActionType);
        }

        filteredQuery = filteredQuery.OrderByDescending(x => x.ActionTime);

        var totalCount = await AsyncExecuter.CountAsync(filteredQuery);

        var pagedActivityEntities = await AsyncExecuter.ToListAsync(
            filteredQuery.Skip(input.SkipCount).Take(input.MaxResultCount)
        );

        var allActivityEntities = await AsyncExecuter.ToListAsync(
            allActivitiesQuery.OrderByDescending(x => x.ActionTime)
        );

        var creatorIds = pagedActivityEntities
            .Where(x => x.CreatorId.HasValue)
            .Select(x => x.CreatorId!.Value)
            .Distinct()
            .ToList();

        Dictionary<Guid, string> userMap = new();

        if (creatorIds.Count > 0)
        {
            var users = await _identityUserRepository.GetListAsync();

            var filteredUsers = users
                .Where(x => creatorIds.Contains(x.Id))
                .ToList();

            userMap = filteredUsers.ToDictionary(
                x => x.Id,
                x => !string.IsNullOrWhiteSpace(x.Name)
                    ? x.Name!
                    : (!string.IsNullOrWhiteSpace(x.UserName) ? x.UserName! : "Hệ thống")
            );
        }

        var items = pagedActivityEntities.Select(x =>
        {
            var performedBy = "Hệ thống";

            if (x.CreatorId.HasValue && userMap.TryGetValue(x.CreatorId.Value, out var actorName))
            {
                performedBy = actorName;
            }

            return new FnbOrderHistoryItemDto
            {
                Time = x.ActionTime,
                PerformedBy = performedBy,
                ActionType = x.ActionType,
                ActionTypeText = MapActionTypeText(x.ActionType),
                ActionTypeClass = MapActionTypeClass(x.ActionType, x.IsDanger),
                Title = x.Title,
                Description = x.Description ?? string.Empty,
                IsDanger = x.IsDanger
            };
        }).ToList();

        var actionTypeOptions = new List<FnbOrderHistoryActionTypeOptionDto>
        {
            new() { Value = "", Text = "Tất cả thao tác" },
            new() { Value = "Created", Text = "Tạo đơn" },
            new() { Value = "ServiceStatusChanged", Text = "Đổi trạng thái" },
            new() { Value = "PaymentStatusChanged", Text = "Cập nhật thanh toán" },
            new() { Value = "Cancelled", Text = "Hủy đơn" }
        };

        return new FnbOrderHistoryPageDto
        {
            OrderId = order.Id,
            OrderCode = order.OrderCode,
            CustomerName = order.CustomerName,
            CustomerPhoneMasked = PhoneHelper.MaskPhone(order.CustomerPhone),
            CustomerTypeName = customerType?.Name,
            BagTag = order.BagTag,
            ServiceStatus = order.ServiceStatus,
            PaymentStatus = order.PaymentStatus,
            CreationTime = order.CreationTime,
            LastActivityTime = allActivityEntities.FirstOrDefault()?.ActionTime,
            TotalActions = allActivityEntities.Count,
            CurrentFilterActionType = input.ActionType,
            ActionTypeOptions = actionTypeOptions,
            Activities = items,
            PagedActivities = new PagedResultDto<FnbOrderHistoryItemDto>(totalCount, items)
        };
    }

    public async Task<List<FnbKitchenBoardItemDto>> GetKitchenBoardAsync(GetFnbKitchenBoardInput input)
    {
        await CheckFeatureAndPolicyAsync(GetRootPermission());

        var orderQuery = await _orderRepository.GetQueryableAsync();
        var orderItemQuery = await _orderItemRepository.GetQueryableAsync();
        var itemQuery = await _itemRepository.GetQueryableAsync();
        var customerQuery = await _customerRepository.GetQueryableAsync();
        var customerTypeQuery = await _customerTypeRepository.GetQueryableAsync();
        var activityQuery = await _orderActivityRepository.GetQueryableAsync();

        var orders = await AsyncExecuter.ToListAsync(
            orderQuery
                .Where(x => x.ServiceStatus != FnbServiceStatus.Cancelled)
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
        {
            orders = orders.Where(x => x.ServiceStatus == input.ServiceStatus.Value).ToList();
        }

        var orderIds = orders.Select(x => x.Id).ToList();
        var customerIds = orders.Where(x => x.CustomerId.HasValue).Select(x => x.CustomerId!.Value).Distinct().ToList();

        var orderItems = orderIds.Count == 0
            ? new List<FnbOrderItem>()
            : await AsyncExecuter.ToListAsync(orderItemQuery.Where(x => orderIds.Contains(x.OrderId)));

        var itemIds = orderItems.Where(x => x.ItemId.HasValue).Select(x => x.ItemId!.Value).Distinct().ToList();

        var items = itemIds.Count == 0
            ? new List<FnbItem>()
            : await AsyncExecuter.ToListAsync(itemQuery.Where(x => itemIds.Contains(x.Id)));

        var customers = customerIds.Count == 0
            ? new List<Customer>()
            : await AsyncExecuter.ToListAsync(customerQuery.Where(x => customerIds.Contains(x.Id)));

        var customerTypeIds = customers
            .Where(x => x.CustomerTypeId.HasValue)
            .Select(x => x.CustomerTypeId!.Value)
            .Distinct()
            .ToList();

        var customerTypes = customerTypeIds.Count == 0
            ? new List<CustomerType>()
            : await AsyncExecuter.ToListAsync(customerTypeQuery.Where(x => customerTypeIds.Contains(x.Id)));

        var activities = orderIds.Count == 0
            ? new List<FnbOrderActivity>()
            : await AsyncExecuter.ToListAsync(activityQuery.Where(x => orderIds.Contains(x.OrderId)));

        var orderItemMap = orderItems.GroupBy(x => x.OrderId).ToDictionary(x => x.Key, x => x.ToList());
        var itemMap = items.ToDictionary(x => x.Id, x => x);
        var customerMap = customers.ToDictionary(x => x.Id, x => x);
        var customerTypeMap = customerTypes.ToDictionary(x => x.Id, x => x);
        var latestActivityMap = activities
            .GroupBy(x => x.OrderId)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(a => a.ActionTime).FirstOrDefault());

        return orders.Select(order =>
        {
            orderItemMap.TryGetValue(order.Id, out var orderItemList);
            orderItemList ??= new List<FnbOrderItem>();

            Customer? customer = null;
            CustomerType? customerType = null;

            if (order.CustomerId.HasValue && customerMap.TryGetValue(order.CustomerId.Value, out var customerEntity))
            {
                customer = customerEntity;

                if (customer.CustomerTypeId.HasValue &&
                    customerTypeMap.TryGetValue(customer.CustomerTypeId.Value, out var customerTypeEntity))
                {
                    customerType = customerTypeEntity;
                }
            }

            string? primaryImage = null;
            var itemNames = new List<string>();

            foreach (var oi in orderItemList)
            {
                itemNames.Add(oi.ItemName);

                if (primaryImage == null && oi.ItemId.HasValue && itemMap.TryGetValue(oi.ItemId.Value, out var menuItem))
                {
                    if (!string.IsNullOrWhiteSpace(menuItem.ImageUrl))
                    {
                        primaryImage = ImageHelper.NormalizeThumb(_configuration, menuItem.ImageUrl);
                    }
                }
            }

            latestActivityMap.TryGetValue(order.Id, out var latestActivity);

            return new FnbKitchenBoardItemDto
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                BagTag = order.BagTag,
                CustomerName = order.CustomerName,
                CustomerPhoneMasked = PhoneHelper.MaskPhone(order.CustomerPhone),
                CustomerTypeName = customerType?.Name,
                CustomerTypeColorCode = customerType?.ColorCode,
                Note = order.Note,
                TotalAmount = order.TotalAmount,
                TotalQuantity = orderItemList.Sum(x => x.Quantity),
                CreationTime = order.CreationTime,
                ServiceStatus = order.ServiceStatus,
                PaymentStatus = order.PaymentStatus,
                PrimaryImageUrl = primaryImage ?? "/images/fnb/default-food.png",
                ItemsSummary = string.Join(", ", orderItemList.Select(x => $"{x.ItemName} x{x.Quantity}")),
                ItemNotesSummary = string.Join(" • ", orderItemList.Where(x => !string.IsNullOrWhiteSpace(x.Note)).Select(x => $"{x.ItemName}: {x.Note}")),
                LatestActivityTitle = latestActivity?.Title,
                LatestActivityDescription = latestActivity?.Description,
                ItemNames = itemNames
            };
        })
        .OrderBy(x => x.CreationTime)
        .ToList();
    }

    private static string MapActionTypeText(string actionType)
    {
        return actionType switch
        {
            "Created" => "Tạo đơn",
            "ServiceStatusChanged" => "Đổi trạng thái",
            "PaymentStatusChanged" => "Cập nhật thanh toán",
            "Cancelled" => "Hủy đơn",
            _ => "Thao tác"
        };
    }

    private static string MapActionTypeClass(string actionType, bool isDanger)
    {
        if (isDanger) return "danger";

        return actionType switch
        {
            "Created" => "orange",
            "ServiceStatusChanged" => "blue",
            "PaymentStatusChanged" => "green",
            _ => "gray"
        };
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
            CustomerPhone = order.CustomerPhone,
            CustomerPhoneMasked = PhoneHelper.MaskPhone(order.CustomerPhone),
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

    private static string MapServiceStatus(FnbServiceStatus status)
    {
        return status switch
        {
            FnbServiceStatus.Created => "Mới tạo",
            FnbServiceStatus.Preparing => "Đang xử lý",
            FnbServiceStatus.Delivering => "Đang giao",
            FnbServiceStatus.Served => "Đã phục vụ",
            FnbServiceStatus.Cancelled => "Đã hủy",
            _ => "Không xác định"
        };
    }

    private static string MapPaymentStatus(FnbPaymentStatus status)
    {
        return status switch
        {
            FnbPaymentStatus.Unpaid => "Chưa thanh toán",
            FnbPaymentStatus.Paid => "Đã thanh toán",
            FnbPaymentStatus.Failed => "Thanh toán lỗi",
            _ => "Không xác định"
        };
    }

    public async Task<IRemoteStreamContent> ExportExcelAsync(GetFnbOrderListInput input)
    {
        await CheckFeatureAndPolicyAsync(GetRootPermission());

        // Áp dụng cùng filter như GetListAsync — không phân trang, lấy toàn bộ
        var query = await _orderRepository.GetQueryableAsync();

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var f = input.FilterText.Trim();
            query = query.Where(x =>
                x.OrderCode.Contains(f) ||
                x.BagTag.Contains(f) ||
                (x.CustomerName != null && x.CustomerName.Contains(f)));
        }

        if (!input.BagTag.IsNullOrWhiteSpace())
            query = query.Where(x => x.BagTag.Contains(input.BagTag!.Trim()));

        if (input.ServiceStatus.HasValue)
            query = query.Where(x => x.ServiceStatus == input.ServiceStatus.Value);

        if (input.PaymentStatus.HasValue)
            query = query.Where(x => x.PaymentStatus == input.PaymentStatus.Value);

        if (input.CreationTimeFrom.HasValue)
            query = query.Where(x => x.CreationTime >= input.CreationTimeFrom.Value);

        if (input.CreationTimeTo.HasValue)
            query = query.Where(x => x.CreationTime <= input.CreationTimeTo.Value);

        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime));

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("FnB Orders");

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
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
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

        return StreamToRemoteContent(workbook, $"Export_FnBOrders_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    private static IRemoteStreamContent StreamToRemoteContent(XLWorkbook workbook, string fileName)
    {
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return new RemoteStreamContent(stream, fileName,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}