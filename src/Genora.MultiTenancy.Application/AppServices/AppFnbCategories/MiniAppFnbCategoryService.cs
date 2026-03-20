using Genora.MultiTenancy.AppDtos.AppFnbCategories;
using Genora.MultiTenancy.DomainModels.AppFnbCategories;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppFnbCategories;
public class MiniAppFnbCategoryService : ApplicationService, IMiniAppFnbCategoryService
{
    private readonly IRepository<FnbCategory, System.Guid> _categoryRepository;

    public MiniAppFnbCategoryService(IRepository<FnbCategory, System.Guid> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<MiniAppFnbCategoryListDto> GetListAsync()
    {
        var query = await _categoryRepository.GetQueryableAsync();

        var entities = await AsyncExecuter.ToListAsync(
            query.Where(x => x.IsActive)
                 .OrderBy(x => x.SortOrder)
                 .ThenBy(x => x.Name)
        );

        var data = entities.Select(x => new MiniAppFnbCategoryData
        {
            Id = x.Id,
            Name = x.Name,
            Code = x.Code,
            SortOrder = x.SortOrder
        }).ToList();

        return new MiniAppFnbCategoryListDto
        {
            Error = 0,
            Message = "Success",
            Data = data
        };
    }
}