using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.DomainModels.AppHl25;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.AppHl25Features;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;

namespace Genora.MultiTenancy.AppServices.Hl25;

/// <summary>
/// AppService lịch sử lượt quay. Read-only list + cập nhật trạng thái trao thưởng (2 bước: Won → Delivered).
/// Khi chuyển sang Delivered: set DeliveredTime. (Việc trừ kho quà đã thực hiện lúc quay trúng.)
/// </summary>
[Authorize]
public class Hl25SpinLogAppService : ApplicationService, IHl25SpinLogAppService
{
    private readonly IRepository<Hl25SpinLog, Guid> _repository;
    private readonly IRepository<Hl25Participant, Guid> _participantRepository;
    private readonly IFeatureChecker _featureChecker;

    public Hl25SpinLogAppService(
        IRepository<Hl25SpinLog, Guid> repository,
        IRepository<Hl25Participant, Guid> participantRepository,
        IFeatureChecker featureChecker)
    {
        _repository = repository;
        _participantRepository = participantRepository;
        _featureChecker = featureChecker;
        LocalizationResource = typeof(MultiTenancyResource);
    }

    public async Task<PagedResultDto<Hl25SpinLogDto>> GetListAsync(GetHl25SpinLogListInput input)
    {
        await CheckViewPolicyAsync();

        var logQueryable = await _repository.GetQueryableAsync();
        var participantQueryable = await _participantRepository.GetQueryableAsync();

        var query = from log in logQueryable
                    join p in participantQueryable on log.ParticipantId equals p.Id into pg
                    from p in pg.DefaultIfEmpty()
                    select new { log, p };

        if (input.ParticipantId.HasValue)
            query = query.Where(x => x.log.ParticipantId == input.ParticipantId.Value);

        if (input.GiftId.HasValue)
            query = query.Where(x => x.log.GiftId == input.GiftId.Value);

        if (input.RewardStatus.HasValue)
            query = query.Where(x => x.log.RewardStatus == input.RewardStatus.Value);

        if (!string.IsNullOrWhiteSpace(input.FilterText))
        {
            var f = input.FilterText.Trim();
            query = query.Where(x => x.p != null &&
                ((x.p.FullName != null && x.p.FullName.Contains(f)) ||
                 (x.p.PhoneNumber != null && x.p.PhoneNumber.Contains(f))));
        }

        if (input.SpinFrom.HasValue)
            query = query.Where(x => x.log.SpinTime >= input.SpinFrom.Value);

        if (input.SpinTo.HasValue)
            query = query.Where(x => x.log.SpinTime <= input.SpinTo.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? "log.SpinTime DESC"
            : "log." + input.Sorting;

        var rows = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        var items = rows.Select(x =>
        {
            var dto = ObjectMapper.Map<Hl25SpinLog, Hl25SpinLogDto>(x.log);
            dto.ParticipantName = x.p?.FullName;
            dto.ParticipantPhone = x.p?.PhoneNumber;
            return dto;
        }).ToList();

        return new PagedResultDto<Hl25SpinLogDto>(totalCount, items);
    }

    public async Task<Hl25SpinLogDto> UpdateRewardStatusAsync(Guid id, Hl25RewardStatus status)
    {
        await CheckEditPolicyAsync();

        var entity = await _repository.GetAsync(id);
        if (status != Hl25RewardStatus.Delivered)
            throw new BusinessException("Hl25:InvalidRewardTransition");

        if (entity.RewardStatus == Hl25RewardStatus.Delivered)
            return ObjectMapper.Map<Hl25SpinLog, Hl25SpinLogDto>(entity);

        entity.ConfirmDelivery(Clock.Now);

        entity = await _repository.UpdateAsync(entity, autoSave: true);
        return ObjectMapper.Map<Hl25SpinLog, Hl25SpinLogDto>(entity);
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
            ? MultiTenancyPermissions.AppHl25Wheel.Default
            : MultiTenancyPermissions.HostAppHl25Wheel.Default;
        await AuthorizationService.CheckAsync(policy);
        await EnsureFeatureAsync();
    }

    private async Task CheckEditPolicyAsync()
    {
        var policy = CurrentTenant.IsAvailable
            ? MultiTenancyPermissions.AppHl25Wheel.Edit
            : MultiTenancyPermissions.HostAppHl25Wheel.Edit;
        await AuthorizationService.CheckAsync(policy);
        await EnsureFeatureAsync();
    }
}
