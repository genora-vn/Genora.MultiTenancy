using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.DomainModels.AppCaddie;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.Caddie;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.Caddies;

[Authorize]
public class CaddieRatingAppService : ApplicationService
{
    private readonly IRepository<AppCaddieRating, Guid> _ratingRepo;
    private readonly IRepository<AppCaddieRatingDetail, Guid> _ratingDetailRepo;
    private readonly IRepository<AppCaddie, Guid> _caddieRepo;
    private readonly IRepository<AppCaddieBooking, Guid> _bookingRepo;
    private readonly IRepository<AppCaddieSkill, Guid> _skillRepo;
    private readonly ICurrentTenant _currentTenant;
    private readonly IFeatureChecker _featureChecker;

    public CaddieRatingAppService(
        IRepository<AppCaddieRating, Guid> ratingRepo,
        IRepository<AppCaddieRatingDetail, Guid> ratingDetailRepo,
        IRepository<AppCaddie, Guid> caddieRepo,
        IRepository<AppCaddieBooking, Guid> bookingRepo,
        IRepository<AppCaddieSkill, Guid> skillRepo,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
    {
        _ratingRepo = ratingRepo;
        _ratingDetailRepo = ratingDetailRepo;
        _caddieRepo = caddieRepo;
        _bookingRepo = bookingRepo;
        _skillRepo = skillRepo;
        _currentTenant = currentTenant;
        _featureChecker = featureChecker;
        LocalizationResource = typeof(MultiTenancyResource);
    }

    private string P(string tenantPerm, string hostPerm)
        => _currentTenant.IsAvailable ? tenantPerm : hostPerm;

    private async Task EnsureFeatureAsync()
    {
        if (!_currentTenant.IsAvailable) return;
        if (!await _featureChecker.IsEnabledAsync(CaddieFeatures.Management))
            throw new AbpAuthorizationException($"Feature '{CaddieFeatures.Management}' is disabled for this tenant.");
    }

    public async Task<PagedResultDto<CaddieRatingDto>> GetListAsync(GetCaddieRatingListInput input)
    {
        await EnsureFeatureAsync();
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieRatings.Default, MultiTenancyPermissions.HostAppCaddieRatings.Default));

        var query = await _ratingRepo.GetQueryableAsync();

        if (input.CaddieId.HasValue)
            query = query.Where(x => x.CaddieId == input.CaddieId.Value);

        if (input.ApprovalStatus.HasValue)
            query = query.Where(x => x.ApprovalStatus == input.ApprovalStatus.Value);

        if (input.FromDate.HasValue)
            query = query.Where(x => x.CreationTime >= input.FromDate.Value);

        if (input.ToDate.HasValue)
            query = query.Where(x => x.CreationTime <= input.ToDate.Value.AddDays(1));

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "CreationTime DESC" : input.Sorting;
        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        // Load caddie names
        var caddieIds = items.Select(x => x.CaddieId).Distinct().ToList();
        var caddieQuery = (await _caddieRepo.GetQueryableAsync())
            .Where(x => caddieIds.Contains(x.Id))
            .Select(x => new { x.Id, x.CaddieName, x.CaddieCode });
        var caddies = await AsyncExecuter.ToListAsync(caddieQuery);

        // Load booking info
        var bookingIds = items.Select(x => x.BookingId).Distinct().ToList();
        var bookingQuery = (await _bookingRepo.GetQueryableAsync())
            .Where(x => bookingIds.Contains(x.Id))
            .Select(x => new { x.Id, x.BookingCode, x.BookingDate, x.StartTime, x.CustomerName });
        var bookings = await AsyncExecuter.ToListAsync(bookingQuery);

        var dtos = items.Select(x =>
        {
            var caddie = caddies.FirstOrDefault(c => c.Id == x.CaddieId);
            var booking = bookings.FirstOrDefault(b => b.Id == x.BookingId);
            return new CaddieRatingDto
            {
                Id = x.Id,
                BookingId = x.BookingId,
                BookingCode = booking?.BookingCode,
                CustomerId = x.CustomerId,
                CustomerName = booking?.CustomerName,
                CaddieId = x.CaddieId,
                CaddieName = caddie?.CaddieName,
                CaddieCode = caddie?.CaddieCode,
                OverallRating = x.OverallRating,
                Comment = x.Comment,
                ApprovalStatus = x.ApprovalStatus,
                ApprovalStatusText = GetApprovalStatusText(x.ApprovalStatus),
                ApprovedAt = x.ApprovedAt,
                ApprovedBy = x.ApprovedBy,
                RejectReason = x.RejectReason,
                CreationTime = x.CreationTime,
                BookingDate = booking?.BookingDate,
                BookingStartTime = booking?.StartTime
            };
        }).ToList();

        return new PagedResultDto<CaddieRatingDto>(totalCount, dtos);
    }

    public async Task<CaddieRatingDto> GetAsync(Guid id)
    {
        await EnsureFeatureAsync();
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieRatings.Default, MultiTenancyPermissions.HostAppCaddieRatings.Default));

        var rating = await _ratingRepo.GetAsync(id);
        var caddie = await _caddieRepo.GetAsync(rating.CaddieId);
        var booking = await _bookingRepo.GetAsync(rating.BookingId);

        // Load details
        var detailQuery = (await _ratingDetailRepo.GetQueryableAsync())
            .Where(x => x.RatingId == id);
        var details = await AsyncExecuter.ToListAsync(detailQuery);

        // Load skill names
        var skillIds = details.Select(x => x.SkillId).ToList();
        var skillQuery = (await _skillRepo.GetQueryableAsync())
            .Where(x => skillIds.Contains(x.Id))
            .Select(x => new { x.Id, x.SkillName });
        var skills = await AsyncExecuter.ToListAsync(skillQuery);

        return new CaddieRatingDto
        {
            Id = rating.Id,
            BookingId = rating.BookingId,
            BookingCode = booking.BookingCode,
            CustomerId = rating.CustomerId,
            CustomerName = booking.CustomerName,
            CaddieId = rating.CaddieId,
            CaddieName = caddie.CaddieName,
            CaddieCode = caddie.CaddieCode,
            OverallRating = rating.OverallRating,
            Comment = rating.Comment,
            ApprovalStatus = rating.ApprovalStatus,
            ApprovalStatusText = GetApprovalStatusText(rating.ApprovalStatus),
            ApprovedAt = rating.ApprovedAt,
            ApprovedBy = rating.ApprovedBy,
            RejectReason = rating.RejectReason,
            CreationTime = rating.CreationTime,
            BookingDate = booking.BookingDate,
            BookingStartTime = booking.StartTime,
            Details = details.Select(d =>
            {
                var skill = skills.FirstOrDefault(s => s.Id == d.SkillId);
                return new CaddieRatingDetailDto
                {
                    SkillId = d.SkillId,
                    SkillName = skill?.SkillName,
                    Score = d.Score
                };
            }).ToList()
        };
    }

    public async Task ApproveRejectAsync(Guid id, ApproveRejectRatingDto input)
    {
        await EnsureFeatureAsync();
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieRatings.Edit, MultiTenancyPermissions.HostAppCaddieRatings.Edit));

        var rating = await _ratingRepo.GetAsync(id);

        if (rating.ApprovalStatus == (byte)CaddieRatingApprovalStatus.Approved)
            throw new UserFriendlyException("Không thể thay đổi đánh giá đã được duyệt.");

        if (input.ApprovalStatus == (byte)CaddieRatingApprovalStatus.Rejected && string.IsNullOrWhiteSpace(input.RejectReason))
            throw new UserFriendlyException("Vui lòng nhập lý do từ chối.");

        rating.ApprovalStatus = input.ApprovalStatus;
        rating.ApprovedAt = DateTime.UtcNow;
        rating.ApprovedBy = CurrentUser.Id;

        if (input.ApprovalStatus == (byte)CaddieRatingApprovalStatus.Rejected)
            rating.RejectReason = input.RejectReason;

        await _ratingRepo.UpdateAsync(rating, autoSave: true);

        // Update caddie rating_avg if approved
        if (input.ApprovalStatus == (byte)CaddieRatingApprovalStatus.Approved)
            await UpdateCaddieRatingAvgAsync(rating.CaddieId);
    }

    public async Task DeleteAsync(Guid id)
    {
        await EnsureFeatureAsync();
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieRatings.Delete, MultiTenancyPermissions.HostAppCaddieRatings.Delete));

        var rating = await _ratingRepo.GetAsync(id);

        // Delete details first
        var detailQuery = (await _ratingDetailRepo.GetQueryableAsync())
            .Where(x => x.RatingId == id);
        var details = await AsyncExecuter.ToListAsync(detailQuery);
        if (details.Any())
            await _ratingDetailRepo.DeleteManyAsync(details, autoSave: true);

        await _ratingRepo.DeleteAsync(id);

        // Recalculate rating avg
        await UpdateCaddieRatingAvgAsync(rating.CaddieId);
    }

    private async Task UpdateCaddieRatingAvgAsync(Guid caddieId)
    {
        var approvedQuery = (await _ratingRepo.GetQueryableAsync())
            .Where(x => x.CaddieId == caddieId && x.ApprovalStatus == (byte)CaddieRatingApprovalStatus.Approved);

        var ratings = await AsyncExecuter.ToListAsync(approvedQuery.Select(x => x.OverallRating));

        var caddie = await _caddieRepo.GetAsync(caddieId);
        caddie.RatingAvg = ratings.Any() ? (decimal)ratings.Average() : 0;
        caddie.TotalBooking = ratings.Count;
        await _caddieRepo.UpdateAsync(caddie, autoSave: true);
    }

    private static string GetApprovalStatusText(byte status) => status switch
    {
        (byte)CaddieRatingApprovalStatus.Pending => "Chờ duyệt",
        (byte)CaddieRatingApprovalStatus.Approved => "Đã duyệt",
        (byte)CaddieRatingApprovalStatus.Rejected => "Từ chối",
        _ => "Khác"
    };
}
