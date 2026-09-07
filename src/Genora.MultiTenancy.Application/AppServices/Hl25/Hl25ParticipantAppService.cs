using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.DomainModels.AppHl25;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.AppHl25Features;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;

namespace Genora.MultiTenancy.AppServices.Hl25;

/// <summary>
/// AppService quản lý người tham gia chương trình (Hl25Participant).
/// Xem/sửa thông tin, cộng lượt quay thủ công, xuất Excel. KHÔNG cho tạo/xóa thủ công.
/// </summary>
[Authorize]
public class Hl25ParticipantAppService : ApplicationService, IHl25ParticipantAppService
{
    private readonly IRepository<Hl25Participant, Guid> _repository;
    private readonly IRepository<Hl25SpinTurnLog, Guid> _spinTurnLogRepository;
    private readonly IFeatureChecker _featureChecker;
    private readonly Hl25ParticipantExcelExporter _excelExporter;

    public Hl25ParticipantAppService(
        IRepository<Hl25Participant, Guid> repository,
        IRepository<Hl25SpinTurnLog, Guid> spinTurnLogRepository,
        IFeatureChecker featureChecker,
        Hl25ParticipantExcelExporter excelExporter)
    {
        _repository = repository;
        _spinTurnLogRepository = spinTurnLogRepository;
        _featureChecker = featureChecker;
        _excelExporter = excelExporter;
        LocalizationResource = typeof(MultiTenancyResource);
    }

    public async Task<Hl25ParticipantDto> GetAsync(Guid id)
    {
        await CheckViewPolicyAsync();
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<Hl25Participant, Hl25ParticipantDto>(entity);
    }

    public async Task<PagedResultDto<Hl25ParticipantDto>> GetListAsync(GetHl25ParticipantListInput input)
    {
        await CheckViewPolicyAsync();

        var query = await BuildQueryAsync(input);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(Hl25Participant.JoinedTime) + " DESC"
            : input.Sorting;

        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<Hl25ParticipantDto>(
            totalCount,
            ObjectMapper.Map<List<Hl25Participant>, List<Hl25ParticipantDto>>(items));
    }

    public async Task<Hl25ParticipantDto> UpdateAsync(Guid id, CreateUpdateHl25ParticipantDto input)
    {
        await CheckEditPolicyAsync();

        var entity = await _repository.GetAsync(id);
        entity.FullName = input.FullName;
        entity.PhoneNumber = input.PhoneNumber;
        entity.AgeGroup = input.AgeGroup;
        entity.Gender = input.Gender;
        entity.ReceiveAddress = input.ReceiveAddress;
        entity.IsFollowingOa = input.IsFollowingOa;

        // Ghi mốc consent khi Admin bật (nếu trước đó chưa đồng ý).
        if (input.HasConsent && !entity.HasConsent)
            entity.ConsentTime = DateTime.Now;
        entity.HasConsent = input.HasConsent;

        entity = await _repository.UpdateAsync(entity, autoSave: true);
        return ObjectMapper.Map<Hl25Participant, Hl25ParticipantDto>(entity);
    }

    public async Task<Hl25ParticipantDto> GrantSpinTurnAsync(Guid id, int turns, string? note)
    {
        await CheckEditPolicyAsync();

        if (turns <= 0)
            throw new BusinessException("Hl25:GrantTurnsInvalid").WithData("Turns", turns);

        var entity = await _repository.GetAsync(id);

        // Cộng lượt thủ công (KHÔNG áp trần MaxSpinTurnsPerUser vì đây là Admin cấp).
        entity.RemainingSpinTurns += turns;
        entity.TotalSpinTurns += turns;
        await _repository.UpdateAsync(entity, autoSave: true);

        // Ghi sổ nhận lượt.
        var log = new Hl25SpinTurnLog(GuidGenerator.Create(), entity.Id, Hl25SpinTurnSource.AdminGrant, turns, CurrentTenant.Id)
        {
            Note = note
        };
        await _spinTurnLogRepository.InsertAsync(log, autoSave: true);

        return ObjectMapper.Map<Hl25Participant, Hl25ParticipantDto>(entity);
    }

    public async Task<IRemoteStreamContent> ExportExcelAsync(GetHl25ParticipantListInput input)
    {
        await CheckViewPolicyAsync();

        var query = await BuildQueryAsync(input);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(Hl25Participant.JoinedTime) + " DESC"
            : input.Sorting;

        var items = await AsyncExecuter.ToListAsync(query.OrderBy(sorting));
        var dtos = ObjectMapper.Map<List<Hl25Participant>, List<Hl25ParticipantDto>>(items);

        return _excelExporter.Export(dtos);
    }

    private async Task<IQueryable<Hl25Participant>> BuildQueryAsync(GetHl25ParticipantListInput input)
    {
        var queryable = await _repository.GetQueryableAsync();
        var query = queryable;

        if (!string.IsNullOrWhiteSpace(input.FilterText))
        {
            var f = input.FilterText.Trim();
            query = query.Where(x =>
                (x.FullName != null && x.FullName.Contains(f)) ||
                (x.PhoneNumber != null && x.PhoneNumber.Contains(f)));
        }

        if (input.IsFollowingOa.HasValue)
            query = query.Where(x => x.IsFollowingOa == input.IsFollowingOa.Value);

        if (input.HasConsent.HasValue)
            query = query.Where(x => x.HasConsent == input.HasConsent.Value);

        if (input.JoinedFrom.HasValue)
            query = query.Where(x => x.JoinedTime >= input.JoinedFrom.Value);

        if (input.JoinedTo.HasValue)
            query = query.Where(x => x.JoinedTime <= input.JoinedTo.Value);

        return query;
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
            ? MultiTenancyPermissions.AppHl25Participants.Default
            : MultiTenancyPermissions.HostAppHl25Participants.Default;
        await AuthorizationService.CheckAsync(policy);
        await EnsureFeatureAsync();
    }

    private async Task CheckEditPolicyAsync()
    {
        var policy = CurrentTenant.IsAvailable
            ? MultiTenancyPermissions.AppHl25Participants.Edit
            : MultiTenancyPermissions.HostAppHl25Participants.Edit;
        await AuthorizationService.CheckAsync(policy);
        await EnsureFeatureAsync();
    }
}
