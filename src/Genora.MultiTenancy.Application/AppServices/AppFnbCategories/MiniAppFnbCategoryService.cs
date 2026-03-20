using Genora.MultiTenancy.AppDtos.AppFnbItems;
using Genora.MultiTenancy.DomainModels.AppFnbCategories;
using Genora.MultiTenancy.DomainModels.AppFnbItems;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppFnbCategories;
public class MiniAppFnbItemService : ApplicationService, IMiniAppFnbItemService
{
    private readonly IRepository<FnbItem, Guid> _itemRepository;
    private readonly IRepository<FnbCategory, Guid> _categoryRepository;

    public MiniAppFnbItemService(
        IRepository<FnbItem, Guid> itemRepository,
        IRepository<FnbCategory, Guid> categoryRepository)
    {
        _itemRepository = itemRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<MiniAppFnbItemListDto> GetListAsync(GetMiniAppFnbItemListInput input)
    {
        var itemQuery = await _itemRepository.GetQueryableAsync();
        var categoryQuery = await _categoryRepository.GetQueryableAsync();

        var query =
            from item in itemQuery
            join category in categoryQuery on item.CategoryId equals category.Id
            where item.IsActive && category.IsActive
            select new { item, category };

        if (input.CategoryId.HasValue)
        {
            query = query.Where(x => x.item.CategoryId == input.CategoryId.Value);
        }

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var filter = input.FilterText.Trim();
            query = query.Where(x => x.item.Name.Contains(filter) || (x.item.Description != null && x.item.Description.Contains(filter)));
        }

        if (input.IsAvailable.HasValue)
        {
            query = query.Where(x => x.item.IsAvailable == input.IsAvailable.Value);
        }

        var total = await AsyncExecuter.CountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.category.SortOrder)
                 .ThenBy(x => x.item.SortOrder)
                 .ThenBy(x => x.item.Name)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount)
        );

        var data = rows.Select(x => new MiniAppFnbItemData
        {
            Id = x.item.Id,
            CategoryId = x.item.CategoryId,
            CategoryName = x.category.Name,
            Name = x.item.Name,
            Price = x.item.Price,
            ImageUrl = x.item.ImageUrl,
            Description = x.item.Description,
            IsAvailable = x.item.IsAvailable,
            SortOrder = x.item.SortOrder
        }).ToList();

        return new MiniAppFnbItemListDto
        {
            Error = 0,
            Message = "Success",
            Data = new PagedResultDto<MiniAppFnbItemData>(total, data)
        };
    }

    public async Task<MiniAppFnbItemDetailDto> GetAsync(Guid id)
    {
        var entity = await _itemRepository.GetAsync(id);
        var category = await _categoryRepository.GetAsync(entity.CategoryId);

        return new MiniAppFnbItemDetailDto
        {
            Error = 0,
            Message = "Success",
            Data = new MiniAppFnbItemData
            {
                Id = entity.Id,
                CategoryId = entity.CategoryId,
                CategoryName = category.Name,
                Name = entity.Name,
                Price = entity.Price,
                ImageUrl = entity.ImageUrl,
                Description = entity.Description,
                IsAvailable = entity.IsAvailable,
                SortOrder = entity.SortOrder
            }
        };
    }
}