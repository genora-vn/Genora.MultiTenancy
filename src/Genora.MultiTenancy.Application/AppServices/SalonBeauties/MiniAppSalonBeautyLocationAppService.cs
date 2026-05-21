using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLocations;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.SalonBeauties.MiniApps;

public class MiniAppSalonBeautyLocationAppService : ApplicationService, IMiniAppSalonBeautyLocationAppService
{
    private readonly IRepository<SalonBeautyLocation, Guid> _repository;
    private readonly IConfiguration _configuration;

    public MiniAppSalonBeautyLocationAppService(
        IRepository<SalonBeautyLocation, Guid> repository,
        IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task<List<MiniAppSalonBeautyLocationDto>> GetListAsyncLocations(GetMiniAppLocationListInput input)
    {
        var query = await _repository.GetQueryableAsync();

        query = query.WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive!.Value);
        query = query.WhereIf(input.IsShowOnApp.HasValue, x => x.IsShowOnApp == input.IsShowOnApp!.Value);
        query = query.WhereIf(!input.Filter.IsNullOrWhiteSpace(), x =>
            x.Name.Contains(input.Filter!) ||
            x.Address.Contains(input.Filter!) ||
            (x.Phone != null && x.Phone.Contains(input.Filter!)));

        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name));

        return items.Select(x => new MiniAppSalonBeautyLocationDto
        {
            Id = x.Id,
            Name = x.Name,
            Address = x.Address,
            Phone = x.Phone,
            OpenTime = x.OpenTime.ToString(@"hh\:mm"),
            CloseTime = x.CloseTime.ToString(@"hh\:mm"),
            ImageUrl = ImageHelper.NormalizeThumb(_configuration, x.ImageUrl)
        }).ToList();
    }
}
