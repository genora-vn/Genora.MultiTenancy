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
        var participantQueryable = await _participantRepository.GetQueryableAsync();

        // Join log -> participant để lấy tên/SĐT.
        var query = from log in logQueryable
                    join p in participantQueryable on log.ParticipantId equals p.Id into pg
                    from p in pg.DefaultIfEmpty()
                    select new { log, p };

        if (input.ParticipantId.HasValue)
            query = query.Where(x => x.log.ParticipantId == input.ParticipantId.Value);

        if (input.Source.HasValue)
            query = query.Where(x => x.log.Source == input.Source.Value);

        if (!string.IsNullOrWhiteSpace(input.FilterText))
        {
            var f = input.FilterText.Trim();
            query = query.Where(x => x.p != null &&
                ((x.p.FullName != null && x.p.FullName.Contains(f)) ||
                 (x.p.PhoneNumber != null && x.p.PhoneNumber.Contains(f))));
        }

        if (input.GrantedFrom.HasValue)
            query = query.Where(x => x.log.GrantedTime >= input.GrantedFrom.Value);

        if (input.GrantedTo.HasValue)
            query = query.Where(x => x.log.GrantedTime <= input.GrantedTo.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? "log.GrantedTime DESC"
            : "log." + input.Sorting;

        var rows = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        var items = rows.Select(x =>
        {
            var dto = ObjectMapper.Map<Hl25SpinTurnLog, Hl25SpinTurnLogDto>(x.log);
            dto.ParticipantName = x.p?.FullName;
            dto.ParticipantPhone = x.p?.PhoneNumber;
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
