using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.DomainModels.AppHl25;
using Genora.MultiTenancy.Features.AppHl25Features;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
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
/// AppService lịch sử nhận lượt quay (read-only). Join với Participant để hiển thị tên/SĐT.
/// </summary>
[Authorize]
public class Hl25SpinTurnLogAppService : ApplicationService, IHl25SpinTurnLogAppService
{
    private readonly IRepository<Hl25SpinTurnLog, Guid> _repository;
    private readonly IRepository<Hl25Participant, Guid> _participantRepository;
    private readonly IFeatureChecker _featureChecker;

    public Hl25SpinTurnLogAppService(
        IRepository<Hl25SpinTurnLog, Guid> repository,
        IRepository<Hl25Participant, Guid> participantRepository,
        IFeatureChecker featureChecker)
    {
        _repository = repository;
        _participantRepository = participantRepository;
        _featureChecker = featureChecker;
        LocalizationResource = typeof(MultiTenancyResource);
    }

    public async Task<PagedResultDto<Hl25SpinTurnLogDto>> GetListAsync(GetHl25SpinTurnLogListInput input)
    {
        await CheckViewPolicyAsync();

        var logQueryable = await _repository.GetQueryableAsync();
        var query = logQueryable.AsQueryable();

        if (input.ParticipantId.HasValue)
            query = query.Where(x => x.ParticipantId == input.ParticipantId.Value);

        if (input.Source.HasValue)
            query = query.Where(x => x.Source == input.Source.Value);

        if (input.GrantedFrom.HasValue)
            query = query.Where(x => x.GrantedTime >= input.GrantedFrom.Value);

        if (input.GrantedTo.HasValue)
            query = query.Where(x => x.GrantedTime <= input.GrantedTo.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? "GrantedTime DESC"
            : input.Sorting;

        var logs = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        // Nạp thông tin participant riêng để tránh join phức tạp.
        var participantIds = logs.Select(x => x.ParticipantId).Distinct().ToList();
        var participantQueryable = await _participantRepository.GetQueryableAsync();
        var participants = await AsyncExecuter.ToListAsync(
            participantQueryable.Where(p => participantIds.Contains(p.Id)));
        var participantMap = participants.ToDictionary(p => p.Id);

        var items = logs.Select(x =>
        {
            var dto = ObjectMapper.Map<Hl25SpinTurnLog, Hl25SpinTurnLogDto>(x);
            if (participantMap.TryGetValue(x.ParticipantId, out var p))
            {
                dto.ParticipantName = p.FullName;
                dto.ParticipantPhone = p.PhoneNumber;
            }
            return dto;
        }).ToList();

        return new PagedResultDto<Hl25SpinTurnLogDto>(totalCount, items);
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
}
