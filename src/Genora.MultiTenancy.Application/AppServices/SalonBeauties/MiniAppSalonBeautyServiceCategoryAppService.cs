using System;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServiceCategories;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Localization;
using Microsoft.Extensions.Localization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.SalonBeauties.MiniApps;

public class MiniAppSalonBeautyServiceCategoryAppService : ApplicationService, IMiniAppSalonBeautyServiceCategoryAppService
{
    private readonly IRepository<SalonBeautyServiceCategory, Guid> _repository;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    public MiniAppSalonBeautyServiceCategoryAppService(
        IRepository<SalonBeautyServiceCategory, Guid> repository,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _repository = repository;
        _l = l;
    }

    public async Task<PagedResultDto<SalonBeautyServiceCategoryDto>> GetListMiniAppAsync(GetSalonBeautyListInput input)
    {
        input.MaxResultCount = input.MaxResultCount <= 0 ? 20 : Math.Min(input.MaxResultCount, 100);

        var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.Status == 1);
        query = query.WhereIf(!input.FilterText.IsNullOrWhiteSpace(), x => x.Name.Contains(input.FilterText!));

        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<SalonBeautyServiceCategoryDto>(total, items.Select(Map).ToList());
    }

    public async Task<SalonBeautyServiceCategoryDto> GetMiniAppAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return Map(entity);
    }

    private SalonBeautyServiceCategoryDto Map(SalonBeautyServiceCategory entity)
    {
        return new SalonBeautyServiceCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            SortOrder = entity.SortOrder,
            Status = entity.Status,
            StatusText = entity.Status == 1 ? _l["SalonBeautyCustomer:StatusActive"] : _l["SalonBeautyCustomer:StatusInactive"],
            Note = entity.Note,
            CreationTime = entity.CreationTime,
            LastModificationTime = entity.LastModificationTime
        };
    }
}
