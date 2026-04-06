using Genora.MultiTenancy.AppDtos.AppProCategories;
using Genora.MultiTenancy.DomainModels.AppProCategories;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppProCategories;

public class MiniAppProCategoryService : ApplicationService, IMiniAppProCategoryService
{
    private readonly IRepository<ProCategory, System.Guid> _repository;

    public MiniAppProCategoryService(IRepository<ProCategory, System.Guid> repository)
    {
        _repository = repository;
    }

    public async Task<MiniAppProCategoryListDto> GetListAsync()
    {
        var query = await _repository.GetQueryableAsync();

        var items = await AsyncExecuter.ToListAsync(
            query.Where(x => x.IsActive)
                 .OrderBy(x => x.SortOrder)
                 .ThenBy(x => x.Name));

        return new MiniAppProCategoryListDto
        {
            Error = 0,
            Message = "Success",
            Data = items.Select(x => new MiniAppProCategoryData
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                SortOrder = x.SortOrder
            }).ToList()
        };
    }
}
