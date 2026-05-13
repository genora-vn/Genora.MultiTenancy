using System;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Genora.MultiTenancy.AppDtos.SalonBeauties.MiniApps;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyStylists;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Localization;
using Microsoft.Extensions.Localization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.SalonBeauties.MiniApps;

public class MiniAppSalonBeautyStylistAppService : ApplicationService, IMiniAppSalonBeautyStylistAppService
{
    private readonly IRepository<SalonBeautyStylist, Guid> _repository;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    public MiniAppSalonBeautyStylistAppService(IRepository<SalonBeautyStylist, Guid> repository, IStringLocalizer<MultiTenancyResource> l)
    {
        _repository = repository;
        _l = l;
    }

    public async Task<PagedResultDto<SalonBeautyStylistDto>> GetListMiniAppAsync(GetSalonBeautyListInput input)
    {
        input.MaxResultCount = input.MaxResultCount <= 0 ? 20 : Math.Min(input.MaxResultCount, 100);
        var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.Status == 1 && x.IsShowOnApp && x.Avatar != null && x.Avatar != "");
        query = query.WhereIf(input.Role.HasValue, x => x.Role == input.Role.Value);
        query = query.WhereIf(input.Level.HasValue, x => x.Level == input.Level.Value);
        query = query.WhereIf(!input.FilterText.IsNullOrWhiteSpace(), x => x.DisplayName.Contains(input.FilterText!));
        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.SortOrder).ThenByDescending(x => x.RatingAvg).ThenBy(x => x.DisplayName).Skip(input.SkipCount).Take(input.MaxResultCount));
        return new PagedResultDto<SalonBeautyStylistDto>(total, items.Select(Map).ToList());
    }

    public async Task<SalonBeautyStylistDto> GetMiniAppAsync(Guid id) => Map(await _repository.GetAsync(id));

    private SalonBeautyStylistDto Map(SalonBeautyStylist x) => new()
    {
        Id = x.Id,
        DisplayName = x.DisplayName,
        Avatar = x.Avatar,
        Phone = x.Phone,
        PhoneMasked = PhoneHelper.MaskPhone(x.Phone),
        Gender = x.Gender,
        GenderText = LocalizeEnum<SalonBeautyGender>(x.Gender),
        Role = x.Role,
        RoleText = LocalizeEnum<SalonBeautyStylistRole>(x.Role),
        Level = x.Level,
        LevelText = LocalizeEnum<SalonBeautyStylistLevel>(x.Level),
        ExperienceYear = x.ExperienceYear,
        RatingAvg = x.RatingAvg,
        TotalBooking = x.TotalBooking,
        Status = x.Status,
        StatusText = x.Status == 1 ? _l["SalonBeautyCustomer:StatusActive"] : _l["SalonBeautyCustomer:StatusInactive"],
        IsShowOnApp = x.IsShowOnApp,
        IsShowOnAppText = x.IsShowOnApp ? _l["Yes"] : _l["No"],
        Note = x.Note,
        SortOrder = x.SortOrder
    };

    private string? LocalizeEnum<TEnum>(byte? value) where TEnum : struct, Enum
    {
        if (!value.HasValue || !Enum.IsDefined(typeof(TEnum), value.Value)) return null;
        var enumValue = (TEnum)Enum.ToObject(typeof(TEnum), value.Value);
        var key = $"Enum:{typeof(TEnum).Name}.{enumValue}";
        var text = _l[key].Value;
        return string.IsNullOrWhiteSpace(text) || text == key ? enumValue.ToString() : text;
    }
}
