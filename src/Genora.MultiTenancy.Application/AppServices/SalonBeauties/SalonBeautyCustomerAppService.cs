using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;
using Genora.MultiTenancy.DomainModels.AppProOrders;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Genora.MultiTenancy.AppServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.SalonBeauties;

[Authorize]
public class SalonBeautyCustomerAppService :
    FeatureProtectedCrudAppService<
        SalonBeautyCustomer,
        SalonBeautyCustomerDto,
        Guid,
        GetSalonBeautyListInput,
        CreateSalonBeautyCustomerDto,
        UpdateSalonBeautyCustomerDto>,
    ISalonBeautyCustomerAppService
{
    protected override string FeatureName => string.Empty;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.SalonBeautyCustomers.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostSalonBeautyCustomers.Default;
    private readonly IRepository<SalonBeautyCustomer, Guid> _customerRepository;
    private readonly IRepository<SalonBeautyBooking, Guid> _bookingRepository;
    private readonly IRepository<SalonBeautyBookingService, Guid> _bookingServiceRepository;
    private readonly IRepository<SalonBeautyService, Guid> _serviceRepository;
    private readonly IRepository<SalonBeautyStylist, Guid> _stylistRepository;
    private readonly IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> _loyaltyBalanceRepository;
    private readonly IRepository<SalonBeautyCustomerLoyaltyTransaction, Guid> _loyaltyTransactionRepository;
    private readonly IRepository<SalonBeautyDepositTransaction, Guid> _depositRepository;
    private readonly IRepository<ProOrder, Guid> _proOrderRepository;
    private readonly IRepository<ProOrderItem, Guid> _proOrderItemRepository;
    private const long MaxAvatarBytes = 2 * 1024 * 1024;
    private static readonly string[] AvatarAllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly IStringLocalizer<MultiTenancyResource> _l;
    private readonly IManageImageService _manageImageService;

    public SalonBeautyCustomerAppService(
        IRepository<SalonBeautyCustomer, Guid> customerRepository,
        IRepository<SalonBeautyBooking, Guid> bookingRepository,
        IRepository<SalonBeautyBookingService, Guid> bookingServiceRepository,
        IRepository<SalonBeautyService, Guid> serviceRepository,
        IRepository<SalonBeautyStylist, Guid> stylistRepository,
        IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> loyaltyBalanceRepository,
        IRepository<SalonBeautyCustomerLoyaltyTransaction, Guid> loyaltyTransactionRepository,
        IRepository<SalonBeautyDepositTransaction, Guid> depositRepository,
        IRepository<ProOrder, Guid> proOrderRepository,
        IRepository<ProOrderItem, Guid> proOrderItemRepository,
        IStringLocalizer<MultiTenancyResource> l,
        IManageImageService manageImageService,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(customerRepository, currentTenant, featureChecker)
    {
        _customerRepository = customerRepository;
        _bookingRepository = bookingRepository;
        _bookingServiceRepository = bookingServiceRepository;
        _serviceRepository = serviceRepository;
        _stylistRepository = stylistRepository;
        _loyaltyBalanceRepository = loyaltyBalanceRepository;
        _loyaltyTransactionRepository = loyaltyTransactionRepository;
        _depositRepository = depositRepository;
        _proOrderRepository = proOrderRepository;
        _proOrderItemRepository = proOrderItemRepository;
        _l = l;
        _manageImageService = manageImageService;
    }

    public override async Task<PagedResultDto<SalonBeautyCustomerDto>> GetListAsync(GetSalonBeautyListInput input)
    {
        await CheckCustomerPolicyAsync(
            MultiTenancyPermissions.SalonBeautyCustomers.Default,
            MultiTenancyPermissions.HostSalonBeautyCustomers.Default);

        NormalizeListInput(input);

        var customersQuery = await _customerRepository.GetQueryableAsync();

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var filter = input.FilterText!.Trim();
            customersQuery = customersQuery.Where(x =>
                x.Name.Contains(filter) ||
                x.CustomerCode.Contains(filter) ||
                (x.Phone != null && x.Phone.Contains(filter)) ||
                (x.Email != null && x.Email.Contains(filter)));
        }

        if (input.DateFrom.HasValue)
        {
            var from = input.DateFrom.Value.Date;
            customersQuery = customersQuery.Where(x => x.CreationTime >= from);
        }

        if (input.DateTo.HasValue)
        {
            var to = input.DateTo.Value.Date.AddDays(1).AddTicks(-1);
            customersQuery = customersQuery.Where(x => x.CreationTime <= to);
        }

        if (input.Source.HasValue)
            customersQuery = customersQuery.Where(x => x.Source == input.Source.Value);

        if (input.Status.HasValue)
            customersQuery = customersQuery.Where(x => x.Status == input.Status.Value);

        var customers = await AsyncExecuter.ToListAsync(customersQuery);
        var customerIds = customers.Select(x => x.Id).ToList();

        var bookingStats = await BuildBookingStatsAsync(customerIds);
        var loyaltyStats = await BuildLoyaltyStatsAsync(customerIds);
        var depositStats = await BuildDepositStatsAsync(customerIds);

        var dtoList = customers.Select(x => MapToCustomerDto(x, bookingStats, loyaltyStats, depositStats)).ToList();

        if (!input.CustomerGroup.IsNullOrWhiteSpace())
        {
            var group = input.CustomerGroup!.Trim();
            dtoList = dtoList
                .Where(x => string.Equals(x.MembershipLevel, group, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        dtoList = ApplySorting(dtoList, input.Sorting);

        var totalCount = dtoList.Count;
        var pagedItems = dtoList
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<SalonBeautyCustomerDto>(totalCount, pagedItems);
    }

    public override async Task<SalonBeautyCustomerDto> GetAsync(Guid id)
    {
        await CheckCustomerPolicyAsync(
            MultiTenancyPermissions.SalonBeautyCustomers.Default,
            MultiTenancyPermissions.HostSalonBeautyCustomers.Default);

        var customer = await _customerRepository.GetAsync(id);
        var bookingStats = await BuildBookingStatsAsync(new List<Guid> { id });
        var loyaltyStats = await BuildLoyaltyStatsAsync(new List<Guid> { id });
        var depositStats = await BuildDepositStatsAsync(new List<Guid> { id });

        return MapToCustomerDto(customer, bookingStats, loyaltyStats, depositStats);
    }

    public async Task<List<SalonBeautyCustomerBookingHistoryDto>> GetBookingHistoryAsync(Guid id, int maxResultCount = 20)
    {
        await CheckCustomerPolicyAsync(
            MultiTenancyPermissions.SalonBeautyCustomers.Default,
            MultiTenancyPermissions.HostSalonBeautyCustomers.Default);

        maxResultCount = Math.Clamp(maxResultCount, 1, 100);

        var query = await _bookingRepository.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(
            query
                .Where(x => x.CustomerId == id)
                .OrderByDescending(x => x.BookingDate)
                .ThenByDescending(x => x.StartTime)
                .Take(maxResultCount));

        if (items.Count == 0)
            return new List<SalonBeautyCustomerBookingHistoryDto>();

        var bookingIds = items.Select(x => x.Id).ToList();
        var stylistIds = items.Select(x => x.StylistId).Distinct().ToList();
        var bookingServiceQuery = await _bookingServiceRepository.GetQueryableAsync();
        var bookingServices = await AsyncExecuter.ToListAsync(
            bookingServiceQuery.Where(x => bookingIds.Contains(x.BookingId)));

        var serviceIds = bookingServices.Select(x => x.ServiceId)
            .Concat(items.Select(x => x.ServiceId))
            .Distinct().ToList();

        var serviceQuery = await _serviceRepository.GetQueryableAsync();
        var services = serviceIds.Count == 0
            ? new List<SalonBeautyService>()
            : await AsyncExecuter.ToListAsync(serviceQuery.Where(x => serviceIds.Contains(x.Id)));
        var serviceMap = services.ToDictionary(x => x.Id, x => x.Name);

        var stylistQuery = await _stylistRepository.GetQueryableAsync();
        var stylists = stylistIds.Count == 0
            ? new List<SalonBeautyStylist>()
            : await AsyncExecuter.ToListAsync(stylistQuery.Where(x => stylistIds.Contains(x.Id)));
        var stylistMap = stylists.ToDictionary(x => x.Id, x => x.DisplayName);

        var bookingServiceMap = bookingServices
            .GroupBy(x => x.BookingId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ServiceId).ToList());

        return items.Select(x =>
        {
            var serviceName = ResolveServiceName(x, bookingServiceMap, serviceMap);
            var stylistName = stylistMap.TryGetValue(x.StylistId, out var sname)
                ? sname
                : $"{T("SalonBeautyCustomer:StylistFallback", "Stylist")} #{ShortId(x.StylistId)}";

            return new SalonBeautyCustomerBookingHistoryDto
            {
                Id = x.Id,
                BookingCode = x.BookingCode,
                BookingDate = x.BookingDate,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                ServiceName = serviceName,
                StylistName = stylistName,
                Amount = x.TotalAmount,
                Status = x.Status.ToString()
            };
        }).ToList();
    }

    private string ResolveServiceName(
        SalonBeautyBooking booking,
        Dictionary<Guid, List<Guid>> bookingServiceMap,
        Dictionary<Guid, string> serviceMap)
    {
        if (bookingServiceMap.TryGetValue(booking.Id, out var ids) && ids.Count > 0)
        {
            var names = ids
                .Where(serviceMap.ContainsKey)
                .Select(sid => serviceMap[sid])
                .ToList();
            if (names.Count > 0)
                return string.Join(", ", names);
        }

        if (serviceMap.TryGetValue(booking.ServiceId, out var single))
            return single;

        return $"{T("SalonBeautyCustomer:ServiceFallback", "Dịch vụ")} #{ShortId(booking.ServiceId)}";
    }

    public async Task<List<SalonBeautyCustomerLoyaltyTransactionDto>> GetLoyaltyTransactionsAsync(Guid id, int maxResultCount = 20)
    {
        await CheckCustomerPolicyAsync(
            MultiTenancyPermissions.SalonBeautyCustomers.Default,
            MultiTenancyPermissions.HostSalonBeautyCustomers.Default);

        maxResultCount = Math.Clamp(maxResultCount, 1, 100);

        var query = await _loyaltyTransactionRepository.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(
            query
                .Where(x => x.CustomerId == id)
                .OrderByDescending(x => x.CreationTime)
                .Take(maxResultCount));

        return items.Select(x => new SalonBeautyCustomerLoyaltyTransactionDto
        {
            Id = x.Id,
            Type = x.Type,
            TypeText = LoyaltyTypeText(x.Type),
            Point = x.Point,
            Description = x.Description,
            CreatedAt = x.CreationTime
        }).ToList();
    }

    public async Task<List<SalonBeautyCustomerPurchaseHistoryDto>> GetPurchaseHistoryAsync(Guid id, int maxResultCount = 20)
    {
        await CheckCustomerPolicyAsync(
            MultiTenancyPermissions.SalonBeautyCustomers.Default,
            MultiTenancyPermissions.HostSalonBeautyCustomers.Default);

        maxResultCount = Math.Clamp(maxResultCount, 1, 100);

        var customer = await _customerRepository.FindAsync(id);
        if (customer == null)
            return new List<SalonBeautyCustomerPurchaseHistoryDto>();

        var phone = customer.Phone;
        var orderQuery = await _proOrderRepository.GetQueryableAsync();
        var ordersQ = orderQuery.Where(x => x.CustomerId == id ||
            (phone != null && phone != "" && x.CustomerPhone == phone));

        var orders = await AsyncExecuter.ToListAsync(
            ordersQ.OrderByDescending(x => x.CreationTime).Take(maxResultCount));

        if (orders.Count == 0)
            return new List<SalonBeautyCustomerPurchaseHistoryDto>();

        var orderIds = orders.Select(x => x.Id).ToList();
        var itemQuery = await _proOrderItemRepository.GetQueryableAsync();
        var allItems = await AsyncExecuter.ToListAsync(itemQuery.Where(x => orderIds.Contains(x.OrderId)));

        var itemMap = allItems.GroupBy(x => x.OrderId).ToDictionary(g => g.Key, g => g.ToList());

        return orders.Select(o =>
        {
            itemMap.TryGetValue(o.Id, out var items);
            items ??= new List<ProOrderItem>();
            return new SalonBeautyCustomerPurchaseHistoryDto
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                OrderDate = o.CreationTime,
                ItemCount = items.Sum(i => i.Quantity),
                ItemsSummary = items.Count == 0
                    ? "--"
                    : string.Join(", ", items.Take(3).Select(i => $"{i.ItemName} x{i.Quantity}"))
                        + (items.Count > 3 ? $", +{items.Count - 3}" : string.Empty),
                Amount = o.TotalAmount,
                ServiceStatus = o.ServiceStatus.ToString(),
                ServiceStatusText = ProServiceStatusText(o.ServiceStatus),
                PaymentStatus = o.PaymentStatus.ToString(),
                PaymentStatusText = ProPaymentStatusText(o.PaymentStatus)
            };
        }).ToList();
    }

    public async Task<List<SalonBeautyCustomerLedgerDto>> GetDepositLedgerAsync(Guid id, int maxResultCount = 20)
    {
        await CheckCustomerPolicyAsync(
            MultiTenancyPermissions.SalonBeautyCustomers.Default,
            MultiTenancyPermissions.HostSalonBeautyCustomers.Default);

        maxResultCount = Math.Clamp(maxResultCount, 1, 200);

        var depositQ = await _depositRepository.GetQueryableAsync();
        var deposits = await AsyncExecuter.ToListAsync(
            depositQ.Where(x => x.CustomerId == id)
                    .OrderByDescending(x => x.CreationTime));

        var loyaltyQ = await _loyaltyTransactionRepository.GetQueryableAsync();
        var loyalties = await AsyncExecuter.ToListAsync(
            loyaltyQ.Where(x => x.CustomerId == id && x.Type != 1) // Type=1 (Deposit) đã có ở deposit table
                    .OrderByDescending(x => x.CreationTime));

        var ledger = new List<SalonBeautyCustomerLedgerDto>();

        foreach (var d in deposits)
        {
            ledger.Add(new SalonBeautyCustomerLedgerDto
            {
                Id = d.Id,
                EntryType = "DEPOSIT",
                EntryTypeText = T("SalonBeautyCustomers:LedgerTypeDeposit", "Nạp tiền"),
                EntryDate = d.CreationTime,
                Code = d.TransactionCode,
                Description = d.Note ?? T("SalonBeautyCustomers:LedgerDepositDefault", "Nạp tiền vào tài khoản"),
                Amount = d.Amount,
                Point = d.TotalPoint,
                Status = DepositStatusKey(d.Status),
                StatusText = DepositStatusText(d.Status)
            });
        }

        foreach (var x in loyalties)
        {
            ledger.Add(new SalonBeautyCustomerLedgerDto
            {
                Id = x.Id,
                EntryType = LoyaltyTypeKey(x.Type),
                EntryTypeText = LoyaltyTypeText(x.Type),
                EntryDate = x.CreationTime,
                Code = $"LP-{x.Id.ToString("N").Substring(0, 6).ToUpperInvariant()}",
                Description = x.Description ?? T("SalonBeautyCustomers:LedgerLoyaltyDefault", "Giao dịch điểm"),
                Amount = null,
                Point = x.Point,
                Status = "DONE",
                StatusText = T("SalonBeautyCustomers:LedgerStatusDone", "Hoàn tất")
            });
        }

        return ledger
            .OrderByDescending(x => x.EntryDate)
            .Take(maxResultCount)
            .ToList();
    }

    private string LoyaltyTypeKey(byte type) => type switch
    {
        1 => "DEPOSIT",
        2 => "EARN",
        3 => "REDEEM",
        4 => "ADJUST",
        5 => "REFUND",
        _ => "OTHER"
    };

    private string LoyaltyTypeText(byte type) => type switch
    {
        1 => T("SalonBeautyCustomers:LedgerTypeDeposit", "Nạp tiền"),
        2 => T("SalonBeautyCustomers:LedgerTypeEarn", "Tích điểm"),
        3 => T("SalonBeautyCustomers:LedgerTypeRedeem", "Tiêu điểm"),
        4 => T("SalonBeautyCustomers:LedgerTypeAdjust", "Điều chỉnh"),
        5 => T("SalonBeautyCustomers:LedgerTypeRefund", "Hoàn điểm"),
        _ => T("SalonBeautyCustomers:LedgerTypeOther", "Khác")
    };

    private string DepositStatusKey(byte status) => status switch
    {
        1 => "PENDING",
        2 => "SUCCESS",
        3 => "CANCELLED",
        _ => "UNKNOWN"
    };

    private string DepositStatusText(byte status) => status switch
    {
        1 => T("SalonBeautyCustomers:DepositStatusPending", "Chờ duyệt"),
        2 => T("SalonBeautyCustomers:DepositStatusSuccess", "Thành công"),
        3 => T("SalonBeautyCustomers:DepositStatusCancelled", "Đã hủy"),
        _ => T("SalonBeautyCustomers:DepositStatusUnknown", "Không rõ")
    };

    private string ProServiceStatusText(ProServiceStatus status) => status switch
    {
        ProServiceStatus.Created => T("SalonBeautyCustomers:ProServiceStatusCreated", "Mới tạo"),
        ProServiceStatus.Processing => T("SalonBeautyCustomers:ProServiceStatusProcessing", "Đang xử lý"),
        ProServiceStatus.Ready => T("SalonBeautyCustomers:ProServiceStatusReady", "Sẵn sàng"),
        ProServiceStatus.Delivered => T("SalonBeautyCustomers:ProServiceStatusDelivered", "Đã giao"),
        ProServiceStatus.Cancelled => T("SalonBeautyCustomers:ProServiceStatusCancelled", "Đã hủy"),
        _ => status.ToString()
    };

    private string ProPaymentStatusText(ProPaymentStatus status) => status switch
    {
        ProPaymentStatus.Unpaid => T("SalonBeautyCustomers:ProPaymentStatusUnpaid", "Chưa thanh toán"),
        ProPaymentStatus.Paid => T("SalonBeautyCustomers:ProPaymentStatusPaid", "Đã thanh toán"),
        ProPaymentStatus.Failed => T("SalonBeautyCustomers:ProPaymentStatusFailed", "Thất bại"),
        _ => status.ToString()
    };


    public override async Task<SalonBeautyCustomerDto> CreateAsync(CreateSalonBeautyCustomerDto input)
    {
        await CheckCustomerPolicyAsync(
            MultiTenancyPermissions.SalonBeautyCustomers.Create,
            MultiTenancyPermissions.HostSalonBeautyCustomers.Create);

        await ValidateCustomerInputAsync(input.Name, input.Phone, input.Email, input.Birthday, null);

        var avatarUrl = await ResolveAvatarAsync(input.Avatar, input.Images, input.IsUploadImage);

        var customer = new SalonBeautyCustomer
        {
            CustomerCode = input.CustomerCode.IsNullOrWhiteSpace()
                ? await GenerateCustomerCodeAsync()
                : input.CustomerCode!.Trim(),
            Name = input.Name.Trim(),
            Phone = NormalizePhone(input.Phone),
            Email = NormalizeNullable(input.Email),
            Gender = input.Gender.HasValue ? (byte)input.Gender.Value : null,
            Birthday = input.Birthday?.Date,
            Avatar = NormalizeNullable(avatarUrl),
            ZaloUserId = NormalizeNullable(input.ZaloUserId),
            IsFollowOa = input.IsFollowOa,
            Source = input.Source.HasValue ? (byte)input.Source.Value : (byte)SalonBeautyCustomerSource.Zalo,
            Status = input.Status,
            Note = NormalizeNullable(input.Note)
        };

        var created = await _customerRepository.InsertAsync(customer, autoSave: true);
        return await GetAsync(created.Id);
    }

    public override async Task<SalonBeautyCustomerDto> UpdateAsync(Guid id, UpdateSalonBeautyCustomerDto input)
    {
        await CheckCustomerPolicyAsync(
            MultiTenancyPermissions.SalonBeautyCustomers.Edit,
            MultiTenancyPermissions.HostSalonBeautyCustomers.Edit);

        await ValidateCustomerInputAsync(input.Name, input.Phone, input.Email, input.Birthday, id);

        var customer = await _customerRepository.GetAsync(id);
        var avatarUrl = await ResolveAvatarAsync(input.Avatar, input.Images, input.IsUploadImage, customer.Avatar);
        if (input.IsUploadImage && input.Images != null && (input.Images.ContentLength ?? 0) > 0)
        {
            await DeleteOldAvatarIfLocalAsync(customer.Avatar);
        }
        customer.Name = input.Name.Trim();
        customer.Phone = NormalizePhone(input.Phone);
        customer.Email = NormalizeNullable(input.Email);
        customer.Gender = input.Gender.HasValue ? (byte)input.Gender.Value : null;
        customer.Birthday = input.Birthday?.Date;
        customer.Avatar = NormalizeNullable(avatarUrl);
        customer.ZaloUserId = NormalizeNullable(input.ZaloUserId);
        customer.IsFollowOa = input.IsFollowOa;
        customer.Source = input.Source.HasValue ? (byte)input.Source.Value : null;
        customer.Status = input.Status;
        customer.Note = NormalizeNullable(input.Note);

        await _customerRepository.UpdateAsync(customer, autoSave: true);
        return await GetAsync(customer.Id);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckCustomerPolicyAsync(
            MultiTenancyPermissions.SalonBeautyCustomers.Delete,
            MultiTenancyPermissions.HostSalonBeautyCustomers.Delete);

        await _customerRepository.DeleteAsync(id);
    }

    private async Task CheckCustomerPolicyAsync(string tenantPermission, string hostPermission)
    {
        var permission = CurrentTenant.IsAvailable ? tenantPermission : hostPermission;
        if (permission.IsNullOrWhiteSpace())
            throw new AbpAuthorizationException("Missing Salon Beauty customer permission.");

        await AuthorizationService.CheckAsync(permission);
    }

    private static void NormalizeListInput(GetSalonBeautyListInput input)
    {
        if (input.MaxResultCount <= 0)
            input.MaxResultCount = 10;

        if (input.MaxResultCount > 100)
            input.MaxResultCount = 100;

        if (input.SkipCount < 0)
            input.SkipCount = 0;
    }


    private async Task<string?> ResolveAvatarAsync(
        string? avatarUrl,
        IRemoteStreamContent? imageFile,
        bool isUploadImage,
        string? currentAvatar = null)
    {
        if (!isUploadImage)
        {
            return NormalizeNullable(avatarUrl) ?? NormalizeNullable(currentAvatar);
        }

        if (imageFile == null || (imageFile.ContentLength ?? 0) <= 0)
        {
            return NormalizeNullable(avatarUrl) ?? NormalizeNullable(currentAvatar);
        }

        ValidateAvatarFile(imageFile);

        return await _manageImageService.UploadImageAsync(
            imageFile,
            "salon-customers",
            allowedExtensions: AvatarAllowedExtensions);
    }

    private void ValidateAvatarFile(IRemoteStreamContent file)
    {
        if ((file.ContentLength ?? 0) > MaxAvatarBytes)
            throw new UserFriendlyException("Ảnh đại diện tối đa 2MB.");

        var extension = System.IO.Path.GetExtension(file.FileName ?? string.Empty).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !AvatarAllowedExtensions.Contains(extension))
            throw new UserFriendlyException("Chỉ hỗ trợ ảnh JPG, PNG hoặc WebP.");

        if (!string.IsNullOrWhiteSpace(file.ContentType) && !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new UserFriendlyException("File tải lên không phải định dạng ảnh hợp lệ.");
    }

    private async Task DeleteOldAvatarIfLocalAsync(string? oldAvatar)
    {
        if (!oldAvatar.IsNullOrWhiteSpace() && oldAvatar!.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            try { await _manageImageService.DeleteFileAsync(oldAvatar); } catch { }
        }
    }

    private async Task ValidateCustomerInputAsync(string? name, string? phone, string? email, DateTime? birthday, Guid? editingId)
    {
        if (name.IsNullOrWhiteSpace())
            throw new BusinessException("SalonBeautyCustomer:NameRequired");

        var normalizedPhone = NormalizePhone(phone);
        if (normalizedPhone.IsNullOrWhiteSpace())
            throw new BusinessException("SalonBeautyCustomer:PhoneRequired");

        if (!Regex.IsMatch(normalizedPhone!, @"^(0\d{9,10}|84\d{9,10})$"))
            throw new BusinessException("SalonBeautyCustomer:PhoneInvalid").WithData("Phone", phone);

        if (!email.IsNullOrWhiteSpace() && !Regex.IsMatch(email!.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new BusinessException("SalonBeautyCustomer:EmailInvalid").WithData("Email", email);

        if (birthday.HasValue && birthday.Value.Date > Clock.Now.Date)
            throw new BusinessException("SalonBeautyCustomer:BirthdayInvalid");

        var query = await _customerRepository.GetQueryableAsync();
        var duplicate = await AsyncExecuter.AnyAsync(query.Where(x =>
            x.Phone == normalizedPhone && (!editingId.HasValue || x.Id != editingId.Value)));

        if (duplicate)
            throw new BusinessException("SalonBeautyCustomer:PhoneDuplicated").WithData("Phone", normalizedPhone);
    }

    private async Task<string> GenerateCustomerCodeAsync()
    {
        var prefix = "SB" + Clock.Now.ToString("yyMMdd");
        var query = await _customerRepository.GetQueryableAsync();
        var countToday = await AsyncExecuter.CountAsync(query.Where(x => x.CustomerCode.StartsWith(prefix)));
        return $"{prefix}{countToday + 1:D4}";
    }

    private async Task<Dictionary<Guid, (decimal TotalSpent, int TotalBooking, DateTime? LastBookingDate, string VisitFrequencyLabel)>> BuildBookingStatsAsync(List<Guid> customerIds)
    {
        var result = new Dictionary<Guid, (decimal TotalSpent, int TotalBooking, DateTime? LastBookingDate, string VisitFrequencyLabel)>();
        if (customerIds == null || customerIds.Count == 0) return result;

        var query = await _bookingRepository.GetQueryableAsync();
        var bookings = await AsyncExecuter.ToListAsync(query.Where(x => customerIds.Contains(x.CustomerId)));

        foreach (var group in bookings.GroupBy(x => x.CustomerId))
        {
            var dates = group.Select(x => x.BookingDate.Date).OrderBy(d => d).ToList();
            result[group.Key] = (
                group.Sum(x => x.TotalAmount),
                group.Count(),
                dates.Count > 0 ? dates.Last() : (DateTime?)null,
                ResolveVisitFrequencyLabel(dates)
            );
        }

        return result;
    }

    private string ResolveVisitFrequencyLabel(List<DateTime> orderedDates)
    {
        if (orderedDates == null || orderedDates.Count < 2)
            return T("SalonBeautyCustomers:VisitFrequencyNoData", "Chưa có dữ liệu");

        var gaps = new List<double>();
        for (int i = 1; i < orderedDates.Count; i++)
        {
            var gap = (orderedDates[i] - orderedDates[i - 1]).TotalDays;
            if (gap > 0) gaps.Add(gap);
        }

        if (gaps.Count == 0)
            return T("SalonBeautyCustomers:VisitFrequencyNoData", "Chưa có dữ liệu");

        var avgDays = gaps.Average();
        if (avgDays < 14)
        {
            var weeks = Math.Max(1, Math.Round(avgDays / 7d, MidpointRounding.AwayFromZero));
            return string.Format(T("SalonBeautyCustomers:VisitFrequencyWeeks", "TB {0} tuần/lần"), weeks);
        }
        if (avgDays < 60)
        {
            var weeks = Math.Max(2, Math.Round(avgDays / 7d, MidpointRounding.AwayFromZero));
            return string.Format(T("SalonBeautyCustomers:VisitFrequencyWeeks", "TB {0} tuần/lần"), weeks);
        }
        var months = Math.Max(2, Math.Round(avgDays / 30d, MidpointRounding.AwayFromZero));
        return string.Format(T("SalonBeautyCustomers:VisitFrequencyMonths", "TB {0} tháng/lần"), months);
    }

    private async Task<Dictionary<Guid, (decimal TotalDeposit, decimal MonthlyCurrent, decimal MonthlyChangePercent)>> BuildDepositStatsAsync(List<Guid> customerIds)
    {
        var result = new Dictionary<Guid, (decimal TotalDeposit, decimal MonthlyCurrent, decimal MonthlyChangePercent)>();
        if (customerIds == null || customerIds.Count == 0) return result;

        var now = Clock.Now;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1);
        var nextMonthStart = thisMonthStart.AddMonths(1);
        var prevMonthStart = thisMonthStart.AddMonths(-1);

        var query = await _depositRepository.GetQueryableAsync();
        // Status=2 (Success)
        var deposits = await AsyncExecuter.ToListAsync(query.Where(x =>
            customerIds.Contains(x.CustomerId) && x.Status == 2));

        foreach (var group in deposits.GroupBy(x => x.CustomerId))
        {
            var total = group.Sum(x => x.Amount);
            var current = group.Where(x => x.CreationTime >= thisMonthStart && x.CreationTime < nextMonthStart)
                               .Sum(x => x.Amount);
            var prev = group.Where(x => x.CreationTime >= prevMonthStart && x.CreationTime < thisMonthStart)
                            .Sum(x => x.Amount);

            decimal changePercent;
            if (prev > 0)
                changePercent = Math.Round((current - prev) / prev * 100m, 1);
            else if (current > 0)
                changePercent = 100m;
            else
                changePercent = 0m;

            result[group.Key] = (total, current, changePercent);
        }

        return result;
    }

    private async Task<Dictionary<Guid, int>> BuildLoyaltyStatsAsync(List<Guid> customerIds)
    {
        var result = new Dictionary<Guid, int>();
        if (customerIds == null || customerIds.Count == 0) return result;

        var query = await _loyaltyBalanceRepository.GetQueryableAsync();
        var balances = await AsyncExecuter.ToListAsync(query.Where(x => customerIds.Contains(x.CustomerId)));

        foreach (var group in balances.GroupBy(x => x.CustomerId))
        {
            result[group.Key] = Math.Max(0, group.Sum(x => x.CurrentPoint));
        }

        return result;
    }

    private SalonBeautyCustomerDto MapToCustomerDto(
        SalonBeautyCustomer customer,
        Dictionary<Guid, (decimal TotalSpent, int TotalBooking, DateTime? LastBookingDate, string VisitFrequencyLabel)> bookingStats,
        Dictionary<Guid, int> loyaltyStats,
        Dictionary<Guid, (decimal TotalDeposit, decimal MonthlyCurrent, decimal MonthlyChangePercent)> depositStats)
    {
        bookingStats.TryGetValue(customer.Id, out var stat);
        loyaltyStats.TryGetValue(customer.Id, out var loyaltyPoint);
        depositStats.TryGetValue(customer.Id, out var deposit);

        var totalBooking = stat.TotalBooking;
        var totalSpent = stat.TotalSpent;
        var visitFrequencyLabel = string.IsNullOrWhiteSpace(stat.VisitFrequencyLabel)
            ? T("SalonBeautyCustomers:VisitFrequencyNoData", "Chưa có dữ liệu")
            : stat.VisitFrequencyLabel;

        var gender = ToNullableEnum<SalonBeautyGender>(customer.Gender);
        var source = ToNullableEnum<SalonBeautyCustomerSource>(customer.Source);

        var membershipLevel = ResolveMembershipLevel(totalSpent);
        var (nextThreshold, nextLabel) = ResolveNextTier(totalSpent);

        return new SalonBeautyCustomerDto
        {
            Id = customer.Id,
            CreationTime = customer.CreationTime,
            CreatorId = customer.CreatorId,
            LastModificationTime = customer.LastModificationTime,
            LastModifierId = customer.LastModifierId,
            CustomerCode = customer.CustomerCode,
            Name = customer.Name,
            Phone = customer.Phone,
            PhoneMasked = PhoneHelper.MaskPhone(customer.Phone),
            Email = customer.Email,
            Gender = gender,
            GenderText = gender.HasValue ? EnumText(gender.Value) : T("SalonBeautyCustomer:NotUpdated", "Chưa cập nhật"),
            Birthday = customer.Birthday,
            Avatar = customer.Avatar,
            ZaloUserId = customer.ZaloUserId,
            IsFollowOa = customer.IsFollowOa,
            Source = source,
            SourceText = source.HasValue ? EnumText(source.Value) : T("SalonBeautyCustomer:NotUpdated", "Chưa cập nhật"),
            Status = customer.Status,
            StatusText = StatusText(customer.Status),
            Note = customer.Note,
            TotalSpent = totalSpent,
            TotalBooking = totalBooking,
            AverageOrderValue = totalBooking > 0 ? totalSpent / totalBooking : 0,
            LoyaltyPoint = Math.Max(0, loyaltyPoint),
            LastBookingDate = stat.LastBookingDate,
            MembershipLevel = membershipLevel,
            MembershipLevelLabel = MembershipLevelLabel(membershipLevel),
            NextTierThreshold = nextThreshold,
            NextTierLabel = nextLabel,
            TotalDeposit = deposit.TotalDeposit,
            MonthlyDepositCurrent = deposit.MonthlyCurrent,
            MonthlyDepositChangePercent = deposit.MonthlyChangePercent,
            VisitFrequencyLabel = visitFrequencyLabel
        };
    }

    private static List<SalonBeautyCustomerDto> ApplySorting(List<SalonBeautyCustomerDto> items, string? sorting)
    {
        if (sorting.IsNullOrWhiteSpace())
            return items.OrderByDescending(x => x.TotalSpent).ThenBy(x => x.Name).ToList();

        var s = sorting!.Trim().ToLowerInvariant();
        var desc = s.Contains(" desc");

        if (s.Contains("name"))
            return desc ? items.OrderByDescending(x => x.Name).ToList() : items.OrderBy(x => x.Name).ToList();
        if (s.Contains("creationtime") || s.Contains("created"))
            return desc ? items.OrderByDescending(x => x.CreationTime).ToList() : items.OrderBy(x => x.CreationTime).ToList();
        if (s.Contains("totalbooking"))
            return desc ? items.OrderByDescending(x => x.TotalBooking).ToList() : items.OrderBy(x => x.TotalBooking).ToList();
        if (s.Contains("lastbookingdate"))
            return desc ? items.OrderByDescending(x => x.LastBookingDate).ToList() : items.OrderBy(x => x.LastBookingDate).ToList();

        return desc ? items.OrderByDescending(x => x.TotalSpent).ToList() : items.OrderBy(x => x.TotalSpent).ToList();
    }

    private static string ResolveMembershipLevel(decimal totalSpent)
    {
        if (totalSpent >= 30000000m) return "DIAMOND";
        if (totalSpent >= 10000000m) return "VIP";
        if (totalSpent > 0m) return "REGULAR";
        return "NEW";
    }

    private string MembershipLevelLabel(string level) => level switch
    {
        "DIAMOND" => T("SalonBeautyCustomers:MembershipDiamond", "Thành viên Kim cương"),
        "VIP" => T("SalonBeautyCustomers:MembershipGold", "Thành viên Vàng"),
        "REGULAR" => T("SalonBeautyCustomers:MembershipRegular", "Thành viên Thân thiết"),
        _ => T("SalonBeautyCustomers:MembershipNew", "Khách hàng mới")
    };

    private (decimal Threshold, string? Label) ResolveNextTier(decimal totalSpent)
    {
        if (totalSpent < 1000000m)
            return (1000000m - totalSpent, T("SalonBeautyCustomers:MembershipRegular", "Thành viên Thân thiết"));
        if (totalSpent < 10000000m)
            return (10000000m - totalSpent, T("SalonBeautyCustomers:MembershipGold", "Thành viên Vàng"));
        if (totalSpent < 30000000m)
            return (30000000m - totalSpent, T("SalonBeautyCustomers:MembershipDiamond", "Thành viên Kim cương"));
        return (0m, null);
    }

    private static string? NormalizePhone(string? phone)
        => phone.IsNullOrWhiteSpace() ? null : Regex.Replace(phone!.Trim(), @"\s+|-|\.", "");

    private static string? NormalizeNullable(string? value)
        => value.IsNullOrWhiteSpace() ? null : value!.Trim();

    private string StatusText(byte status)
        => status == 1
            ? T("SalonBeautyCustomer:StatusActive", "Đang hoạt động")
            : T("SalonBeautyCustomer:StatusInactive", "Ngừng hoạt động");

    private string EnumText<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var key = $"Enum:{typeof(TEnum).Name}.{value}";
        return T(key, value.ToString());
    }

    private string T(string key, string fallback)
    {
        var text = _l[key].Value;
        return string.IsNullOrWhiteSpace(text) || text.Equals(key, StringComparison.OrdinalIgnoreCase)
            ? fallback
            : text;
    }

    private static TEnum? ToNullableEnum<TEnum>(byte? value) where TEnum : struct, Enum
    {
        if (!value.HasValue)
            return null;

        return Enum.IsDefined(typeof(TEnum), value.Value)
            ? (TEnum)Enum.ToObject(typeof(TEnum), value.Value)
            : null;
    }

    private static string ShortId(Guid id)
    {
        var s = id.ToString("N");
        return s.Length <= 6 ? s : s.Substring(0, 6).ToUpperInvariant();
    }
}
