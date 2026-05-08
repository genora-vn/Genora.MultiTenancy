using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos.SalonBeautyCustomerDtos;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Genora.MultiTenancy.SalonBeauty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.Application.SalonBeauty;

[Authorize]
public class SalonBeautyCustomerAppService : ApplicationService, ISalonBeautyCustomerAppService
{
    private readonly IRepository<SalonBeautyCustomer, Guid> _customerRepository;
    private readonly IRepository<SalonBeautyBooking, Guid> _bookingRepository;
    private readonly IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> _loyaltyBalanceRepository;
    private readonly IRepository<SalonBeautyCustomerLoyaltyTransaction, Guid> _loyaltyTransactionRepository;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    public SalonBeautyCustomerAppService(
        IRepository<SalonBeautyCustomer, Guid> customerRepository,
        IRepository<SalonBeautyBooking, Guid> bookingRepository,
        IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> loyaltyBalanceRepository,
        IRepository<SalonBeautyCustomerLoyaltyTransaction, Guid> loyaltyTransactionRepository,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _customerRepository = customerRepository;
        _bookingRepository = bookingRepository;
        _loyaltyBalanceRepository = loyaltyBalanceRepository;
        _loyaltyTransactionRepository = loyaltyTransactionRepository;
        _l = l;
    }

    public async Task<PagedResultDto<SalonBeautyCustomerDto>> GetListAsync(GetSalonBeautyListInput input)
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

        var dtoList = customers.Select(x => MapToCustomerDto(x, bookingStats, loyaltyStats)).ToList();

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

    public async Task<SalonBeautyCustomerDto> GetAsync(Guid id)
    {
        await CheckCustomerPolicyAsync(
            MultiTenancyPermissions.SalonBeautyCustomers.Default,
            MultiTenancyPermissions.HostSalonBeautyCustomers.Default);

        var customer = await _customerRepository.GetAsync(id);
        var bookingStats = await BuildBookingStatsAsync(new List<Guid> { id });
        var loyaltyStats = await BuildLoyaltyStatsAsync(new List<Guid> { id });

        return MapToCustomerDto(customer, bookingStats, loyaltyStats);
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

        return items.Select(x => new SalonBeautyCustomerBookingHistoryDto
        {
            Id = x.Id,
            BookingCode = x.BookingCode,
            BookingDate = x.BookingDate,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            ServiceName = $"{T("SalonBeautyCustomer:ServiceFallback", "Dịch vụ")} #{ShortId(x.ServiceId)}",
            StylistName = $"{T("SalonBeautyCustomer:StylistFallback", "Stylist")} #{ShortId(x.StylistId)}",
            Amount = x.TotalAmount,
            Status = x.Status.ToString()
        }).ToList();
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
            TypeText = x.Type == 1 ? "EARN" : (x.Type == 2 ? "REDEEM" : x.Type.ToString()),
            Point = x.Point,
            Description = x.Description,
            CreatedAt = x.CreationTime
        }).ToList();
    }

    public async Task<SalonBeautyCustomerDto> CreateAsync(CreateSalonBeautyCustomerDto input)
    {
        await CheckCustomerPolicyAsync(
            MultiTenancyPermissions.SalonBeautyCustomers.Create,
            MultiTenancyPermissions.HostSalonBeautyCustomers.Create);

        await ValidateCustomerInputAsync(input.Name, input.Phone, input.Email, input.Birthday, null);

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
            Avatar = NormalizeNullable(input.Avatar),
            ZaloUserId = NormalizeNullable(input.ZaloUserId),
            IsFollowOa = input.IsFollowOa,
            Source = input.Source.HasValue ? (byte)input.Source.Value : (byte)SalonBeautyCustomerSource.Zalo,
            Status = input.Status,
            Note = NormalizeNullable(input.Note)
        };

        var created = await _customerRepository.InsertAsync(customer, autoSave: true);
        return await GetAsync(created.Id);
    }

    public async Task<SalonBeautyCustomerDto> UpdateAsync(Guid id, UpdateSalonBeautyCustomerDto input)
    {
        await CheckCustomerPolicyAsync(
            MultiTenancyPermissions.SalonBeautyCustomers.Edit,
            MultiTenancyPermissions.HostSalonBeautyCustomers.Edit);

        await ValidateCustomerInputAsync(input.Name, input.Phone, input.Email, input.Birthday, id);

        var customer = await _customerRepository.GetAsync(id);
        customer.Name = input.Name.Trim();
        customer.Phone = NormalizePhone(input.Phone);
        customer.Email = NormalizeNullable(input.Email);
        customer.Gender = input.Gender.HasValue ? (byte)input.Gender.Value : null;
        customer.Birthday = input.Birthday?.Date;
        customer.Avatar = NormalizeNullable(input.Avatar);
        customer.ZaloUserId = NormalizeNullable(input.ZaloUserId);
        customer.IsFollowOa = input.IsFollowOa;
        customer.Source = input.Source.HasValue ? (byte)input.Source.Value : null;
        customer.Status = input.Status;
        customer.Note = NormalizeNullable(input.Note);

        await _customerRepository.UpdateAsync(customer, autoSave: true);
        return await GetAsync(customer.Id);
    }

    public async Task DeleteAsync(Guid id)
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

    private async Task ValidateCustomerInputAsync(string? name, string? phone, string? email, DateTime? birthday, Guid? editingId)
    {
        if (name.IsNullOrWhiteSpace())
            throw new BusinessException("SalonBeautyCustomer:NameRequired");

        var normalizedPhone = NormalizePhone(phone);
        if (normalizedPhone.IsNullOrWhiteSpace())
            throw new BusinessException("SalonBeautyCustomer:PhoneRequired");

        if (!Regex.IsMatch(normalizedPhone!, @"^0\d{9,10}$"))
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

    private async Task<Dictionary<Guid, (decimal TotalSpent, int TotalBooking, DateTime? LastBookingDate)>> BuildBookingStatsAsync(List<Guid> customerIds)
    {
        var result = new Dictionary<Guid, (decimal TotalSpent, int TotalBooking, DateTime? LastBookingDate)>();
        if (customerIds == null || customerIds.Count == 0) return result;

        var query = await _bookingRepository.GetQueryableAsync();
        var bookings = await AsyncExecuter.ToListAsync(query.Where(x => customerIds.Contains(x.CustomerId)));

        foreach (var group in bookings.GroupBy(x => x.CustomerId))
        {
            result[group.Key] = (
                group.Sum(x => x.TotalAmount),
                group.Count(),
                group.Select(x => (DateTime?)x.BookingDate).Max()
            );
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
        Dictionary<Guid, (decimal TotalSpent, int TotalBooking, DateTime? LastBookingDate)> bookingStats,
        Dictionary<Guid, int> loyaltyStats)
    {
        bookingStats.TryGetValue(customer.Id, out var stat);
        loyaltyStats.TryGetValue(customer.Id, out var loyaltyPoint);

        var totalBooking = stat.TotalBooking;
        var totalSpent = stat.TotalSpent;

        var gender = ToNullableEnum<SalonBeautyGender>(customer.Gender);
        var source = ToNullableEnum<SalonBeautyCustomerSource>(customer.Source);

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
            MembershipLevel = ResolveMembershipLevel(totalSpent)
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
        if (totalSpent >= 10000000m) return "VIP";
        if (totalSpent > 0m) return "REGULAR";
        return "NEW";
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
