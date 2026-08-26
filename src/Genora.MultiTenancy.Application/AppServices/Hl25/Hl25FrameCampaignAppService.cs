using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.DomainModels.AppHl25;
using Genora.MultiTenancy.Features.AppHl25Features;
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

namespace Genora.MultiTenancy.AppServices.Hl25;

/// <summary>
/// AppService quản lý chiến dịch ghép ảnh (Hl25FrameCampaign) — CRUD + đếm số mẫu frame.
/// </summary>
[Authorize]
public class Hl25FrameCampaignAppService :
    FeatureProtectedCrudAppService<Hl25FrameCampaign, Hl25FrameCampaignDto, Guid, GetHl25FrameCampaignListInput, CreateUpdateHl25FrameCampaignDto>,
    IHl25FrameCampaignAppService
{
    protected override string FeatureName => AppHl25Features.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppHl25Frames.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppHl25Frames.Default;

    private readonly IRepository<Hl25FrameTemplate, Guid> _templateRepository;

    public Hl25FrameCampaignAppService(
        IRepository<Hl25FrameCampaign, Guid> repository,
        IRepository<Hl25FrameTemplate, Guid> templateRepository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(repository, currentTenant, featureChecker)
    {
        GetPolicyName = MultiTenancyPermissions.AppHl25Frames.Default;
        GetListPolicyName = MultiTenancyPermissions.AppHl25Frames.Default;
        CreatePolicyName = MultiTenancyPermissions.AppHl25Frames.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppHl25Frames.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppHl25Frames.Delete;

        _templateRepository = templateRepository;
    }

    [DisableValidation]
    public override async Task<PagedResultDto<Hl25FrameCampaignDto>> GetListAsync(GetHl25FrameCampaignListInput input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();
        var query = queryable;

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var filter = input.FilterText.Trim();
            query = query.Where(x => x.Name.Contains(filter));
        }

        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? nameof(Hl25FrameCampaign.Name) : input.Sorting;

        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = ObjectMapper.Map<List<Hl25FrameCampaign>, List<Hl25FrameCampaignDto>>(items);

        // Đếm số mẫu frame cho mỗi chiến dịch.
        var campaignIds = items.Select(x => x.Id).ToList();
        if (campaignIds.Count > 0)
        {
            var templateQueryable = await _templateRepository.GetQueryableAsync();
            var counts = await AsyncExecuter.ToListAsync(
                templateQueryable
                    .Where(t => campaignIds.Contains(t.CampaignId))
                    .GroupBy(t => t.CampaignId)
                    .Select(g => new { CampaignId = g.Key, Count = g.Count() }));

            var countMap = counts.ToDictionary(x => x.CampaignId, x => x.Count);
            foreach (var dto in dtos)
            {
                dto.TemplateCount = countMap.TryGetValue(dto.Id, out var c) ? c : 0;
            }
        }

        return new PagedResultDto<Hl25FrameCampaignDto>(totalCount, dtos);
    }

    public override async Task<Hl25FrameCampaignDto> CreateAsync(CreateUpdateHl25FrameCampaignDto input)
    {
        await CheckCreatePolicyAsync();

        var entity = new Hl25FrameCampaign(GuidGenerator.Create(), input.Name, CurrentTenant.Id)
        {
            Description = input.Description,
            StartTime = input.StartTime,
            EndTime = input.EndTime,
            Status = input.Status
        };

        entity = await Repository.InsertAsync(entity, autoSave: true);
        return ObjectMapper.Map<Hl25FrameCampaign, Hl25FrameCampaignDto>(entity);
    }

    public override async Task<Hl25FrameCampaignDto> UpdateAsync(Guid id, CreateUpdateHl25FrameCampaignDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await Repository.GetAsync(id);
        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.StartTime = input.StartTime;
        entity.EndTime = input.EndTime;
        entity.Status = input.Status;

        entity = await Repository.UpdateAsync(entity, autoSave: true);
        return ObjectMapper.Map<Hl25FrameCampaign, Hl25FrameCampaignDto>(entity);
    }
}
