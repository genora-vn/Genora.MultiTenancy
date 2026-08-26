using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.DomainModels.AppHl25;
using Genora.MultiTenancy.Features.AppHl25Features;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;

namespace Genora.MultiTenancy.AppServices.Hl25;

/// <summary>
/// AppService lịch sử tạo ảnh thiệp (read-only). Join Participant + Campaign để hiển thị tên.
/// </summary>
[Authorize]
public class Hl25FrameCreationAppService : ApplicationService, IHl25FrameCreationAppService
{
    private readonly IRepository<Hl25FrameCreation, Guid> _repository;
    private readonly IRepository<Hl25Participant, Guid> _participantRepository;
    private readonly IRepository<Hl25FrameCampaign, Guid> _campaignRepository;
    private readonly IFeatureChecker _featureChecker;

    public Hl25FrameCreationAppService(
        IRepository<Hl25FrameCreation, Guid> repository,
        IRepository<Hl25Participant, Guid> participantRepository,
        IRepository<Hl25FrameCampaign, Guid> campaignRepository,
        IFeatureChecker featureChecker)
    {
        _repository = repository;
        _participantRepository = participantRepository;
        _campaignRepository = campaignRepository;
        _featureChecker = featureChecker;
        LocalizationResource = typeof(MultiTenancyResource);
    }

    public async Task<PagedResultDto<Hl25FrameCreationDto>> GetListAsync(GetHl25FrameCreationListInput input)
    {
        await CheckViewPolicyAsync();

        var creationQueryable = await _repository.GetQueryableAsync();
        var participantQueryable = await _participantRepository.GetQueryableAsync();
        var campaignQueryable = await _campaignRepository.GetQueryableAsync();

        var query = from c in creationQueryable
                    join p in participantQueryable on c.ParticipantId equals p.Id into pg
                    from p in pg.DefaultIfEmpty()
                    join cp in campaignQueryable on c.CampaignId equals cp.Id into cpg
                    from cp in cpg.DefaultIfEmpty()
                    select new { c, p, cp };

        if (input.CampaignId.HasValue)
            query = query.Where(x => x.c.CampaignId == input.CampaignId.Value);

        if (input.ParticipantId.HasValue)
            query = query.Where(x => x.c.ParticipantId == input.ParticipantId.Value);

        if (input.SharePlatform.HasValue)
            query = query.Where(x => x.c.SharePlatform == input.SharePlatform.Value);

        if (!string.IsNullOrWhiteSpace(input.FilterText))
        {
            var f = input.FilterText.Trim();
            query = query.Where(x => x.p != null &&
                ((x.p.FullName != null && x.p.FullName.Contains(f)) ||
                 (x.p.PhoneNumber != null && x.p.PhoneNumber.Contains(f))));
        }

        if (input.CreatedFrom.HasValue)
            query = query.Where(x => x.c.CreatedTime >= input.CreatedFrom.Value);

        if (input.CreatedTo.HasValue)
            query = query.Where(x => x.c.CreatedTime <= input.CreatedTo.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? "c.CreatedTime DESC"
            : "c." + input.Sorting;

        var rows = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        var items = rows.Select(x =>
        {
            var dto = ObjectMapper.Map<Hl25FrameCreation, Hl25FrameCreationDto>(x.c);
            dto.ParticipantName = x.p?.FullName;
            dto.ParticipantPhone = x.p?.PhoneNumber;
            dto.CampaignName = x.cp?.Name;
            return dto;
        }).ToList();

        return new PagedResultDto<Hl25FrameCreationDto>(totalCount, items);
    }

    private async Task EnsureFeatureAsync()
    {
        if (!CurrentTenant.IsAvailable) return;
        if (!await _featureChecker.IsEnabledAsync(AppHl25Features.Management))
            throw new AbpAuthorizationException($"Feature '{AppHl25Features.Management}' is disabled for this tenant.");
    }

    private async Task CheckViewPolicyAsync()
    {
        var policy = CurrentTenant.IsAvailable
            ? MultiTenancyPermissions.AppHl25Frames.Default
            : MultiTenancyPermissions.HostAppHl25Frames.Default;
        await AuthorizationService.CheckAsync(policy);
        await EnsureFeatureAsync();
    }
}
