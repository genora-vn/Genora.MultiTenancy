using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Genora.MultiTenancy.AppDtos.SalonBeauties.MiniApps;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Localization;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.SalonBeauties.MiniApps;

public class MiniAppSalonBeautyCustomerAppService : ApplicationService, IMiniAppSalonBeautyCustomerAppService
{
    private readonly IRepository<SalonBeautyCustomer, Guid> _customerRepository;
    private readonly IRepository<SalonBeautyBooking, Guid> _bookingRepository;
    private readonly IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> _loyaltyRepository;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    public MiniAppSalonBeautyCustomerAppService(
        IRepository<SalonBeautyCustomer, Guid> customerRepository,
        IRepository<SalonBeautyBooking, Guid> bookingRepository,
        IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> loyaltyRepository,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _customerRepository = customerRepository;
        _bookingRepository = bookingRepository;
        _loyaltyRepository = loyaltyRepository;
        _l = l;
    }

    public async Task<PagedResultDto<SalonBeautyCustomerDto>> GetListMiniAppAsync(GetSalonBeautyListInput input)
    {
        input.MaxResultCount = input.MaxResultCount <= 0 ? 20 : Math.Min(input.MaxResultCount, 100);

        var query = await _customerRepository.GetQueryableAsync();

        query = query.Where(x => x.Status == 1);

        query = query.WhereIf(
            !input.FilterText.IsNullOrWhiteSpace(),
            x => x.Name.Contains(input.FilterText!) || (x.Phone != null && x.Phone.Contains(input.FilterText!))
        );

        var total = await AsyncExecuter.CountAsync(query);

        var items = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        var ids = items.Select(x => x.Id).ToList();
        var bookingStats = await BuildBookingStatsAsync(ids);
        var loyalty = await BuildLoyaltyStatsAsync(ids);

        return new PagedResultDto<SalonBeautyCustomerDto>(
            total,
            items.Select(x => Map(x, bookingStats, loyalty)).ToList()
        );
    }

    public async Task<SalonBeautyCustomerDto> GetMiniAppAsync(Guid id)
    {
        var customer = await _customerRepository.GetAsync(id);
        var bookingStats = await BuildBookingStatsAsync(new List<Guid> { id });
        var loyalty = await BuildLoyaltyStatsAsync(new List<Guid> { id });

        return Map(customer, bookingStats, loyalty);
    }

    public async Task<SalonBeautyCustomerDto?> GetByPhoneAsync(string phoneNumber, CancellationToken ct = default)
    {
        var normalized = NormalizePhone(phoneNumber);
        if (normalized.IsNullOrWhiteSpace())
        {
            throw new UserFriendlyException("Vui lòng nhập số điện thoại.");
        }

        var query = await _customerRepository.GetQueryableAsync();

        var customer = await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(x => x.Phone == normalized),
            ct
        );

        if (customer == null)
        {
            return null;
        }

        var bookingStats = await BuildBookingStatsAsync(new List<Guid> { customer.Id });
        var loyalty = await BuildLoyaltyStatsAsync(new List<Guid> { customer.Id });

        return Map(customer, bookingStats, loyalty);
    }

    /// <summary>
    /// Tạo mới/cập nhật khách hàng Salon Beauty từ Zalo Mini App.
    /// Idempotent theo số điện thoại. Nếu đã tồn tại Phone thì update mapping, chưa có thì tạo mới.
    /// </summary>
    public async Task<SalonBeautyCustomerDto> UpsertFromMiniAppAsync(MiniAppSalonBeautyUpsertCustomerRequest input, CancellationToken ct = default)
    {
        var phone = NormalizePhone(input.PhoneNumber ?? input.Phone);
        if (phone.IsNullOrWhiteSpace())
        {
            throw new UserFriendlyException("Vui lòng nhập số điện thoại.");
        }

        var name = NormalizeNullable(input.FullName ?? input.Name);
        if (name.IsNullOrWhiteSpace())
        {
            name = "Zalo User";
        }

        var query = await _customerRepository.GetQueryableAsync();

        var customer = await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(x => x.Phone == phone)
        );

        var isFollowOa = input.IsFollowOa ?? input.IsFollower ?? false;
        var birthday = input.Birthday ?? input.DateOfBirth;
        var avatar = NormalizeNullable(input.Avatar ?? input.AvatarUrl);
        var source = input.Source ?? SalonBeautyCustomerSource.Zalo;

        if (customer == null)
        {
            customer = new SalonBeautyCustomer
            {
                CustomerCode = await GenerateCustomerCodeMiniAppAsync(),
                Name = name!,
                Phone = phone,
                Email = NormalizeNullable(input.Email),
                Gender = ToDefinedEnumByte<SalonBeautyGender>(input.Gender),
                Birthday = birthday?.Date,
                Avatar = avatar,
                ZaloUserId = NormalizeNullable(input.ZaloUserId ?? input.ZaloFollowerId),
                IsFollowOa = isFollowOa,
                Source = (byte)source,
                Status = 1,
                Note = NormalizeNullable(input.Note)
            };

            customer = await _customerRepository.InsertAsync(customer, autoSave: true);
        }
        else
        {
            customer.Name = name.IsNullOrWhiteSpace() ? customer.Name : name!;
            customer.Email = NormalizeNullable(input.Email) ?? customer.Email;
            customer.Gender = ToDefinedEnumByte<SalonBeautyGender>(input.Gender) ?? customer.Gender;
            customer.Birthday = birthday?.Date ?? customer.Birthday;
            customer.Avatar = avatar ?? customer.Avatar;
            customer.ZaloUserId = NormalizeNullable(input.ZaloUserId ?? input.ZaloFollowerId) ?? customer.ZaloUserId;
            customer.IsFollowOa = input.IsFollowOa ?? input.IsFollower ?? customer.IsFollowOa;
            customer.Source = (byte)source;
            customer.Status = 1;

            var note = NormalizeNullable(input.Note);
            if (!note.IsNullOrWhiteSpace())
            {
                customer.Note = note;
            }

            customer = await _customerRepository.UpdateAsync(customer, autoSave: true);
        }

        return await GetMiniAppAsync(customer.Id);
    }


    private async Task<Dictionary<Guid, (int Count, decimal Total, DateTime? LastDate)>> BuildBookingStatsAsync(List<Guid> ids)
    {
        var result = ids.ToDictionary(x => x, _ => (Count: 0, Total: 0m, LastDate: (DateTime?)null));
        if (ids.Count == 0) return result;

        var bookings = await _bookingRepository.GetListAsync(x =>
            ids.Contains(x.CustomerId) && x.Status != SalonBeautyBookingStatus.Cancelled
        );

        foreach (var group in bookings.GroupBy(x => x.CustomerId))
        {
            result[group.Key] = (
                group.Count(),
                group.Sum(x => x.TotalAmount),
                group.Max(x => (DateTime?)x.BookingDate)
            );
        }

        return result;
    }

    private async Task<Dictionary<Guid, int>> BuildLoyaltyStatsAsync(List<Guid> ids)
    {
        var result = ids.ToDictionary(x => x, _ => 0);
        if (ids.Count == 0) return result;

        var rows = await _loyaltyRepository.GetListAsync(x => ids.Contains(x.CustomerId));
        foreach (var row in rows)
        {
            result[row.CustomerId] = row.CurrentPoint;
        }

        return result;
    }

    private SalonBeautyCustomerDto Map(
        SalonBeautyCustomer x,
        Dictionary<Guid, (int Count, decimal Total, DateTime? LastDate)> bookingStats,
        Dictionary<Guid, int> loyalty)
    {
        bookingStats.TryGetValue(x.Id, out var bs);
        loyalty.TryGetValue(x.Id, out var point);

        var gender = ToNullableEnum<SalonBeautyGender>(x.Gender);
        var source = ToNullableEnum<SalonBeautyCustomerSource>(x.Source);

        return new SalonBeautyCustomerDto
        {
            Id = x.Id,
            CustomerCode = x.CustomerCode,
            Name = x.Name,
            Phone = x.Phone,
            PhoneMasked = PhoneHelper.MaskPhone(x.Phone),
            Email = x.Email,

            // Entity đang lưu enum dạng byte?, DTO dùng enum nullable => cần convert rõ ràng.
            Gender = gender,
            GenderText = gender.HasValue ? LocalizeEnum(gender.Value) : null,

            Birthday = x.Birthday,
            Avatar = x.Avatar,
            ZaloUserId = x.ZaloUserId,
            IsFollowOa = x.IsFollowOa,

            // Entity đang lưu enum dạng byte?, DTO dùng enum nullable => cần convert rõ ràng.
            Source = source,
            SourceText = source.HasValue ? LocalizeEnum(source.Value) : null,

            Status = x.Status,
            StatusText = x.Status == 1
                ? _l["SalonBeautyCustomer:StatusActive"]
                : _l["SalonBeautyCustomer:StatusInactive"],
            Note = x.Note,
            MembershipLevel = bs.Total >= 20000000 ? "VIP" : (bs.Count > 0 ? "REGULAR" : "NEW"),
            TotalSpent = bs.Total,
            TotalBooking = bs.Count,
            AverageOrderValue = bs.Count > 0 ? bs.Total / bs.Count : 0,
            LoyaltyPoint = point,
            LastBookingDate = bs.LastDate,
            CreationTime = x.CreationTime,
            CreatorId = x.CreatorId,
            LastModificationTime = x.LastModificationTime,
            LastModifierId = x.LastModifierId
        };
    }

    private static string? NormalizeNullable(string? value)
        => value.IsNullOrWhiteSpace() ? null : value!.Trim();

    private static string? NormalizePhone(string? phone)
        => phone.IsNullOrWhiteSpace() ? null : Regex.Replace(phone!.Trim(), @"\s+|-|\.", "");

    private async Task<string> GenerateCustomerCodeMiniAppAsync()
    {
        var prefix = "SB" + Clock.Now.ToString("yyMMdd");
        var query = await _customerRepository.GetQueryableAsync();
        var countToday = await AsyncExecuter.CountAsync(query.Where(x => x.CustomerCode.StartsWith(prefix)));
        return $"{prefix}{countToday + 1:D4}";
    }


    private static byte? ToDefinedEnumByte<TEnum>(TEnum? value)
        where TEnum : struct, Enum
    {
        if (!value.HasValue)
        {
            return null;
        }

        return Enum.IsDefined(typeof(TEnum), value.Value)
            ? Convert.ToByte(value.Value)
            : null;
    }

    private static byte? ToDefinedEnumByte<TEnum>(byte? value)
        where TEnum : struct, Enum
    {
        if (!value.HasValue)
        {
            return null;
        }

        return Enum.IsDefined(typeof(TEnum), value.Value)
            ? value.Value
            : null;
    }

    private static TEnum? ToNullableEnum<TEnum>(byte? value)
        where TEnum : struct, Enum
    {
        if (!value.HasValue)
        {
            return null;
        }

        return Enum.IsDefined(typeof(TEnum), value.Value)
            ? (TEnum)Enum.ToObject(typeof(TEnum), value.Value)
            : null;
    }

    private string LocalizeEnum<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        var key = $"Enum:{typeof(TEnum).Name}.{value}";
        var text = _l[key].Value;
        return string.IsNullOrWhiteSpace(text) || text == key ? value.ToString() : text;
    }
}
