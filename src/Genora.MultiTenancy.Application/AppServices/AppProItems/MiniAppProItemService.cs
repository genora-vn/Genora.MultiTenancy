using Genora.MultiTenancy.AppDtos.AppProItems;
using Genora.MultiTenancy.DomainModels.AppProCategories;
using Genora.MultiTenancy.DomainModels.AppProItems;
using Genora.MultiTenancy.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppProItems;

public class MiniAppProItemService : ApplicationService, IMiniAppProItemService
{
    private readonly IRepository<ProItem, Guid> _itemRepository;
    private readonly IRepository<ProCategory, Guid> _categoryRepository;
    private readonly IConfiguration _configuration;

    public MiniAppProItemService(
        IRepository<ProItem, Guid> itemRepository,
        IRepository<ProCategory, Guid> categoryRepository,
        IConfiguration configuration)
    {
        _itemRepository = itemRepository;
        _categoryRepository = categoryRepository;
        _configuration = configuration;
    }

    public async Task<MiniAppProItemListDto> GetListAsync(GetMiniAppProItemListInput input)
    {
        var itemQuery = await _itemRepository.GetQueryableAsync();
        var categoryQuery = await _categoryRepository.GetQueryableAsync();

        var query = from item in itemQuery
                    join cat in categoryQuery on item.CategoryId equals cat.Id
                    where item.IsActive && cat.IsActive
                    select new { item, cat };

        if (input.CategoryId.HasValue)
            query = query.Where(x => x.item.CategoryId == input.CategoryId.Value);

        if (input.IsAvailable.HasValue)
            query = query.Where(x => x.item.IsAvailable == input.IsAvailable.Value);

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var f = input.FilterText.Trim();
            query = query.Where(x => x.item.Name.Contains(f));
        }

        var total = await AsyncExecuter.CountAsync(query);

        var rows = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.cat.SortOrder)
                 .ThenBy(x => x.item.SortOrder)
                 .ThenBy(x => x.item.Name)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount));

        var data = rows.Select(r => ToData(r.item, r.cat.Name)).ToList();

        return new MiniAppProItemListDto
        {
            Error = 0,
            Message = "Success",
            Data = new PagedResultDto<MiniAppProItemData>(total, data)
        };
    }

    public async Task<MiniAppProItemDetailDto> GetAsync(Guid id)
    {
        var item = await _itemRepository.GetAsync(id);
        var cat = await _categoryRepository.GetAsync(item.CategoryId);

        return new MiniAppProItemDetailDto
        {
            Error = 0,
            Message = "Success",
            Data = ToData(item, cat.Name)
        };
    }

    private MiniAppProItemData ToData(ProItem item, string? categoryName)
        => new()
        {
            Id          = item.Id,
            CategoryId  = item.CategoryId,
            CategoryName= categoryName,
            Name        = item.Name,
            Price       = item.Price,
            ImageUrl    = ImageHelper.NormalizeThumb(_configuration, item.ImageUrl),
            Description = item.Description,
            IsAvailable = item.IsAvailable,
            SortOrder   = item.SortOrder
        };
}
