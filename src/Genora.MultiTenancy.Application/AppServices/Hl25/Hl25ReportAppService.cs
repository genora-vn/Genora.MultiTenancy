using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.DomainModels.AppHl25;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Hl25;
using Genora.MultiTenancy.Features.AppHl25Features;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;

namespace Genora.MultiTenancy.AppServices.Hl25;

/// <summary>
/// AppService báo cáo thống kê chương trình Hoa Linh 25 Năm (query tổng hợp qua AsyncExecuter).
/// </summary>
[Authorize]
public class Hl25ReportAppService : ApplicationService, IHl25ReportAppService
{
    private readonly IRepository<Hl25FrameCreation, Guid> _creationRepository;
    private readonly IRepository<Hl25SpinLog, Guid> _spinLogRepository;
    private readonly IRepository<Hl25SpinTurnLog, Guid> _spinTurnLogRepository;
    private readonly IRepository<Hl25Participant, Guid> _participantRepository;
    private readonly IRepository<Hl25Gift, Guid> _giftRepository;
    private readonly IFeatureChecker _featureChecker;

    public Hl25ReportAppService(
        IRepository<Hl25FrameCreation, Guid> creationRepository,
        IRepository<Hl25SpinLog, Guid> spinLogRepository,
        IRepository<Hl25SpinTurnLog, Guid> spinTurnLogRepository,
        IRepository<Hl25Participant, Guid> participantRepository,
        IRepository<Hl25Gift, Guid> giftRepository,
        IFeatureChecker featureChecker)
    {
        _creationRepository = creationRepository;
        _spinLogRepository = spinLogRepository;
        _spinTurnLogRepository = spinTurnLogRepository;
        _participantRepository = participantRepository;
        _giftRepository = giftRepository;
        _featureChecker = featureChecker;
        LocalizationResource = typeof(MultiTenancyResource);
    }

    // ===== Báo cáo 1: Đổi Frame =====
    public async Task<Hl25FrameStatsDto> GetFrameStatsAsync(Hl25ReportInput input)
    {
        await CheckViewPolicyAsync();
        Hl25AdminRules.ValidateDateRange(input.FromDate, input.ToDate);
        var toExclusive = input.ToDate?.Date.AddDays(1);

        var queryable = await _creationRepository.GetQueryableAsync();
        var query = queryable;

        if (input.CampaignId.HasValue)
            query = query.Where(x => x.CampaignId == input.CampaignId.Value);
        if (input.FromDate.HasValue)
            query = query.Where(x => x.CreatedTime >= input.FromDate.Value);
        if (input.ToDate.HasValue)
            query = query.Where(x => x.CreatedTime < toExclusive.Value);

        var items = await AsyncExecuter.ToListAsync(query);

        var result = new Hl25FrameStatsDto
        {
            TotalCreations = items.Count,
            TotalShared = items.Count(x => x.SharePlatform != Hl25SharePlatform.None),
            UniqueParticipants = items.Select(x => x.ParticipantId).Distinct().Count(),
            ByDate = items
                .GroupBy(x => x.CreatedTime.Date)
                .OrderBy(g => g.Key)
                .Select(g => new Hl25FrameStatsRowDto
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    CreationCount = g.Count(),
                    SharedCount = g.Count(x => x.SharePlatform != Hl25SharePlatform.None)
                })
                .ToList()
        };

        return result;
    }

    // ===== Báo cáo 2: Tham gia Vòng quay =====
    public async Task<Hl25WheelParticipationStatsDto> GetWheelParticipationStatsAsync(Hl25ReportInput input)
    {
        await CheckViewPolicyAsync();
        Hl25AdminRules.ValidateDateRange(input.FromDate, input.ToDate);
        var toExclusive = input.ToDate?.Date.AddDays(1);

        var spinQueryable = await _spinLogRepository.GetQueryableAsync();
        var spinQuery = spinQueryable;
        if (input.FromDate.HasValue)
            spinQuery = spinQuery.Where(x => x.SpinTime >= input.FromDate.Value);
        if (input.ToDate.HasValue)
            spinQuery = spinQuery.Where(x => x.SpinTime < toExclusive.Value);

        var spins = await AsyncExecuter.ToListAsync(spinQuery);

        var turnQueryable = await _spinTurnLogRepository.GetQueryableAsync();
        var turnQuery = turnQueryable;
        if (input.FromDate.HasValue)
            turnQuery = turnQuery.Where(x => x.GrantedTime >= input.FromDate.Value);
        if (input.ToDate.HasValue)
            turnQuery = turnQuery.Where(x => x.GrantedTime < toExclusive.Value);

        var totalTurnsGranted = await AsyncExecuter.SumAsync(turnQuery.Select(x => x.TurnsAdded));
        var adminTurnsGranted = await AsyncExecuter.SumAsync(
            turnQuery.Where(x => x.Source == Hl25SpinTurnSource.AdminGrant).Select(x => x.TurnsAdded));

        var participantQueryable = await _participantRepository.GetQueryableAsync();
        var participantsWithRemaining = await AsyncExecuter.CountAsync(
            participantQueryable.Where(x => x.RemainingSpinTurns > 0));

        return new Hl25WheelParticipationStatsDto
        {
            UniqueSpinners = spins.Select(x => x.ParticipantId).Distinct().Count(),
            TotalSpins = spins.Count,
            TotalTurnsGranted = totalTurnsGranted,
            AdminTurnsGranted = adminTurnsGranted,
            AutomaticTurnsGranted = totalTurnsGranted - adminTurnsGranted,
            TotalWins = spins.Count(x => x.GiftId != null &&
                (x.RewardStatus == Hl25RewardStatus.Won || x.RewardStatus == Hl25RewardStatus.Delivered)),
            ParticipantsWithRemainingTurns = participantsWithRemaining
        };
    }

    // ===== Báo cáo 3: Vòng quay theo Quà =====
    public async Task<Hl25WheelGiftStatsDto> GetWheelGiftStatsAsync(Hl25ReportInput input)
    {
        await CheckViewPolicyAsync();
        Hl25AdminRules.ValidateDateRange(input.FromDate, input.ToDate);
        var toExclusive = input.ToDate?.Date.AddDays(1);

        var giftQueryable = await _giftRepository.GetQueryableAsync();
        var gifts = await AsyncExecuter.ToListAsync(giftQueryable);

        var spinQueryable = await _spinLogRepository.GetQueryableAsync();
        var spinQuery = spinQueryable.Where(x => x.GiftId != null &&
            (x.RewardStatus == Hl25RewardStatus.Won || x.RewardStatus == Hl25RewardStatus.Delivered));
        if (input.FromDate.HasValue)
            spinQuery = spinQuery.Where(x => x.SpinTime >= input.FromDate.Value);
        if (input.ToDate.HasValue)
            spinQuery = spinQuery.Where(x => x.SpinTime < toExclusive.Value);

        var spins = await AsyncExecuter.ToListAsync(spinQuery);
        var totalWon = spins.Count;

        var wonByGift = spins
            .GroupBy(x => x.GiftId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());
        var deliveredByGift = spins
            .Where(x => x.RewardStatus == Hl25RewardStatus.Delivered)
            .GroupBy(x => x.GiftId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var rows = gifts.Select(gift =>
        {
            var won = wonByGift.TryGetValue(gift.Id, out var w) ? w : 0;
            var delivered = deliveredByGift.TryGetValue(gift.Id, out var d) ? d : 0;
            return new Hl25WheelGiftStatsRowDto
            {
                GiftId = gift.Id,
                GiftName = gift.Name,
                TotalQuantity = gift.TotalQuantity,
                RemainingQuantity = gift.RemainingQuantity,
                WonCount = won,
                DeliveredCount = delivered,
                WinRatePercent = totalWon > 0 ? Math.Round((decimal)won * 100 / totalWon, 2) : 0
            };
        })
        .OrderByDescending(x => x.WonCount)
        .ToList();

        return new Hl25WheelGiftStatsDto
        {
            TotalWon = totalWon,
            Rows = rows
        };
    }

    // ===== Báo cáo 4: Phân bổ nhóm tuổi (Delta 2026-09) =====
    public async Task<Hl25AgeGroupStatsDto> GetAgeGroupStatsAsync(Hl25ReportInput input)
    {
        await CheckViewPolicyAsync();
        Hl25AdminRules.ValidateDateRange(input.FromDate, input.ToDate);
        var toExclusive = input.ToDate?.Date.AddDays(1);

        var participantQueryable = await _participantRepository.GetQueryableAsync();
        var query = participantQueryable;

        if (input.FromDate.HasValue)
            query = query.Where(x => x.JoinedTime >= input.FromDate.Value);
        if (input.ToDate.HasValue)
            query = query.Where(x => x.JoinedTime < toExclusive.Value);

        var participants = await AsyncExecuter.ToListAsync(query);
        var total = participants.Count;

        var countByGroup = participants
            .GroupBy(x => x.AgeGroup)
            .ToDictionary(g => g.Key, g => g.Count());

        // Liệt kê đủ 4 nhóm tuổi (kể cả nhóm 0 người) theo thứ tự enum.
        var groups = new[]
        {
            Hl25AgeGroup.Age18To25,
            Hl25AgeGroup.Age26To35,
            Hl25AgeGroup.Age36To44,
            Hl25AgeGroup.Unknown
        };

        var rows = groups.Select(g =>
        {
            var count = countByGroup.TryGetValue(g, out var c) ? c : 0;
            return new Hl25AgeGroupStatsRowDto
            {
                AgeGroup = g,
                Label = AgeGroupLabel(g),
                Count = count,
                Percent = total > 0 ? Math.Round((decimal)count * 100 / total, 2) : 0
            };
        }).ToList();

        return new Hl25AgeGroupStatsDto
        {
            TotalParticipants = total,
            Rows = rows
        };
    }

    private static string AgeGroupLabel(Hl25AgeGroup ageGroup) => ageGroup switch
    {
        Hl25AgeGroup.Age18To25 => "18 - 25",
        Hl25AgeGroup.Age26To35 => "26 - 35",
        Hl25AgeGroup.Age36To44 => "36 - 44",
        _ => "Không xác định"
    };

    // ===== Báo cáo 5: Phân bổ giới tính =====
    public async Task<Hl25GenderStatsDto> GetGenderStatsAsync(Hl25ReportInput input)
    {
        await CheckViewPolicyAsync();
        Hl25AdminRules.ValidateDateRange(input.FromDate, input.ToDate);
        var toExclusive = input.ToDate?.Date.AddDays(1);

        var participantQueryable = await _participantRepository.GetQueryableAsync();
        var query = participantQueryable;

        if (input.FromDate.HasValue)
            query = query.Where(x => x.JoinedTime >= input.FromDate.Value);
        if (input.ToDate.HasValue)
            query = query.Where(x => x.JoinedTime < toExclusive.Value);

        var participants = await AsyncExecuter.ToListAsync(query);
        var total = participants.Count;

        var countByGender = participants
            .GroupBy(x => x.Gender)
            .ToDictionary(g => g.Key, g => g.Count());

        // Liệt kê đủ 4 giới tính (kể cả nhóm 0 người) theo thứ tự enum.
        var genders = new[]
        {
            Hl25Gender.Unknown,
            Hl25Gender.Male,
            Hl25Gender.Female,
            Hl25Gender.Other
        };

        var rows = genders.Select(g =>
        {
            var count = countByGender.TryGetValue(g, out var c) ? c : 0;
            return new Hl25GenderStatsRowDto
            {
                Gender = g,
                Label = GenderLabel(g),
                Count = count,
                Percent = total > 0 ? Math.Round((decimal)count * 100 / total, 2) : 0
            };
        }).ToList();

        return new Hl25GenderStatsDto
        {
            TotalParticipants = total,
            Rows = rows
        };
    }

    private static string GenderLabel(Hl25Gender gender) => gender switch
    {
        Hl25Gender.Male => "Nam",
        Hl25Gender.Female => "Nữ",
        Hl25Gender.Other => "Khác",
        _ => "Không xác định"
    };

    // ===== Helpers =====
    private async Task EnsureFeatureAsync()
    {
        if (!CurrentTenant.IsAvailable) return;
        if (!await _featureChecker.IsEnabledAsync(AppHl25Features.Management))
            throw new AbpAuthorizationException($"Feature '{AppHl25Features.Management}' is disabled for this tenant.");
    }

    private async Task CheckViewPolicyAsync()
    {
        var policy = CurrentTenant.IsAvailable
            ? MultiTenancyPermissions.AppHl25Reports.Default
            : MultiTenancyPermissions.HostAppHl25Reports.Default;
        await AuthorizationService.CheckAsync(policy);
        await EnsureFeatureAsync();
    }
}
