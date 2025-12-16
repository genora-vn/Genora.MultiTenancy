using Genora.MultiTenancy.AppDtos.AppNews;
using Genora.MultiTenancy.DomainModels.AppNews;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.AppNewsFeatures;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.AppNewsServices;

[Authorize]
public class AppNewsService :
    FeatureProtectedCrudAppService<
        News,
        AppNewsDto,
        Guid,
        GetNewsListInput,
        CreateUpdateAppNewsDto>,
    IAppNewsService
{
    // === GIỐNG PATTERN CỦA AppCalendarSlotService ===
    protected override string FeatureName => AppNewsFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppNews.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppNews.Default;

    public AppNewsService(
        IRepository<News, Guid> repository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(repository, currentTenant, featureChecker)
    {
        // policy TENANT (Host sẽ được FeatureProtectedCrudAppService map qua HostNews.*)
        GetPolicyName = MultiTenancyPermissions.AppNews.Default;
        GetListPolicyName = MultiTenancyPermissions.AppNews.Default;
        CreatePolicyName = MultiTenancyPermissions.AppNews.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppNews.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppNews.Delete;
    }

    // Nếu muốn tránh validate kỳ quặc như bên CustomerList thì có thể thêm DisableValidation
    [DisableValidation]
    public override async Task<PagedResultDto<AppNewsDto>> GetListAsync(GetNewsListInput input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();
        var query = queryable;

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var filter = input.FilterText.Trim();
            query = query.Where(x => x.Title.Contains(filter));
        }

        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == (byte)input.Status.Value);
        }

        if (input.PublishedAtFrom.HasValue)
        {
            query = query.Where(x => x.PublishedAt >= input.PublishedAtFrom.Value);
        }

        if (input.PublishedAtTo.HasValue)
        {
            query = query.Where(x => x.PublishedAt <= input.PublishedAtTo.Value);
        }

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(News.DisplayOrder) + " asc, " + nameof(News.PublishedAt) + " desc"
            : input.Sorting;

        query = query.OrderBy(sorting);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var items = await AsyncExecuter
            .ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtoList = ObjectMapper.Map<List<News>, List<AppNewsDto>>(items);

        return new PagedResultDto<AppNewsDto>(totalCount, dtoList);
    }

    public override async Task<AppNewsDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();

        var entity = await Repository.GetAsync(id);
        return ObjectMapper.Map<News, AppNewsDto>(entity);
    }

    public override async Task<AppNewsDto> CreateAsync(CreateUpdateAppNewsDto input)
    {
        await CheckCreatePolicyAsync();

        var entity = ObjectMapper.Map<CreateUpdateAppNewsDto, News>(input);

        // auto set thời gian publish nếu status = Published mà chưa set
        if (!entity.PublishedAt.HasValue && entity.Status == (byte)NewsStatus.Published)
        {
            entity.PublishedAt = Clock.Now;
        }

        entity = await Repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<News, AppNewsDto>(entity);
    }

    public override async Task<AppNewsDto> UpdateAsync(Guid id, CreateUpdateAppNewsDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await Repository.GetAsync(id);

        ObjectMapper.Map(input, entity);

        if (!entity.PublishedAt.HasValue && entity.Status == (byte)NewsStatus.Published)
        {
            entity.PublishedAt = Clock.Now;
        }

        entity = await Repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<News, AppNewsDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        await Repository.DeleteAsync(id);
    }
}