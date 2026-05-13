using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Genora.MultiTenancy.AppDtos.SalonBeauties.MiniApps;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServices;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Localization;
using Microsoft.Extensions.Localization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.SalonBeauties.MiniApps;

public class MiniAppSalonBeautyServiceAppService : ApplicationService, IMiniAppSalonBeautyServiceAppService
{
    private readonly IRepository<SalonBeautyService, Guid> _repository;
    private readonly IRepository<SalonBeautyServiceCategory, Guid> _categoryRepository;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    public MiniAppSalonBeautyServiceAppService(IRepository<SalonBeautyService, Guid> repository, IRepository<SalonBeautyServiceCategory, Guid> categoryRepository, IStringLocalizer<MultiTenancyResource> l)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _l = l;
    }

    public async Task<PagedResultDto<SalonBeautyServiceDto>> GetListMiniAppAsync(GetSalonBeautyListInput input)
    {
        input.MaxResultCount = input.MaxResultCount <= 0 ? 20 : Math.Min(input.MaxResultCount, 100);
        var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.Status == 1 && x.IsShowOnApp);
        query = query.WhereIf(input.CategoryId.HasValue && input.CategoryId.Value != Guid.Empty, x => x.CategoryId == input.CategoryId!.Value);
        query = query.WhereIf(!input.FilterText.IsNullOrWhiteSpace(), x => x.Name.Contains(input.FilterText!));

        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name).Skip(input.SkipCount).Take(input.MaxResultCount));
        var categoryIds = items.Select(x => x.CategoryId).Distinct().ToList();
        var categories = categoryIds.Count == 0 ? new List<SalonBeautyServiceCategory>() : await _categoryRepository.GetListAsync(x => categoryIds.Contains(x.Id));
        var dict = categories.ToDictionary(x => x.Id, x => x.Name);
        return new PagedResultDto<SalonBeautyServiceDto>(total, items.Select(x => Map(x, dict.TryGetValue(x.CategoryId, out var cat) ? cat : null)).ToList());
    }

    public async Task<SalonBeautyServiceDto> GetMiniAppAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        var category = await _categoryRepository.FindAsync(entity.CategoryId);
        return Map(entity, category?.Name);
    }

    private SalonBeautyServiceDto Map(SalonBeautyService x, string? categoryName) => new()
    {
        Id = x.Id,
        Name = x.Name,
        CategoryId = x.CategoryId,
        CategoryName = categoryName,
        Price = x.Price,
        PriceText = string.Format(CultureInfo.CurrentCulture, "{0:N0}", x.Price),
        Duration = x.Duration,
        DurationText = string.Format(CultureInfo.CurrentCulture, "{0} phút", x.Duration),
        ApplicableRole = x.ApplicableRole,
        ApplicableRoleText = LocalizeEnum<SalonBeautyStylistRole>(x.ApplicableRole),
        ApplicableLevel = x.ApplicableLevel,
        ApplicableLevelText = LocalizeEnum<SalonBeautyStylistLevel>(x.ApplicableLevel),
        Status = x.Status,
        StatusText = x.Status == 1 ? _l["SalonBeautyCustomer:StatusActive"] : _l["SalonBeautyCustomer:StatusInactive"],
        IsShowOnApp = x.IsShowOnApp,
        IsShowOnAppText = x.IsShowOnApp ? _l["Yes"] : _l["No"],
        Note = x.Note,
        SortOrder = x.SortOrder,
        CreationTime = x.CreationTime,
        LastModificationTime = x.LastModificationTime
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
