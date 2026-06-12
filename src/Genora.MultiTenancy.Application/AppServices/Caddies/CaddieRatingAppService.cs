using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.DomainModels.AppCaddie;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.Caddie;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.BackgroundJobs;
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
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly ICurrentTenant _currentTenant;
    private readonly IFeatureChecker _featureChecker;
    private readonly IBackgroundJobManager _backgroundJobManager;

    public CaddieRatingAppService(
        IRepository<AppCaddieRating, Guid> ratingRepo,
        IRepository<AppCaddieRatingDetail, Guid> ratingDetailRepo,
        IRepository<AppCaddie, Guid> caddieRepo,
        IRepository<AppCaddieBooking, Guid> bookingRepo,
        IRepository<AppCaddieSkill, Guid> skillRepo,
        IRepository<Customer, Guid> customerRepo,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        IBackgroundJobManager backgroundJobManager)
    {
        _ratingRepo = ratingRepo;
        _ratingDetailRepo = ratingDetailRepo;
        _caddieRepo = caddieRepo;
        _bookingRepo = bookingRepo;
        _skillRepo = skillRepo;
        _customerRepo = customerRepo;
        _currentTenant = currentTenant;
        _featureChecker = featureChecker;
        _backgroundJobManager = backgroundJobManager;
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

        // Filter by caddie name/code
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var keyword = input.Filter.Trim().ToLower();
            var filterCaddieQuery = (await _caddieRepo.GetQueryableAsync())
                .Where(c => c.CaddieName.ToLower().Contains(keyword) || c.CaddieCode.ToLower().Contains(keyword))
                .Select(c => c.Id);
            var matchingCaddieIds = await AsyncExecuter.ToListAsync(filterCaddieQuery);
            query = query.Where(x => matchingCaddieIds.Contains(x.CaddieId));
        }

        // Filter by customer/golfer name
        if (!string.IsNullOrWhiteSpace(input.CustomerFilter))
        {
            var custKeyword = input.CustomerFilter.Trim().ToLower();
            var filterBookingQuery = (await _bookingRepo.GetQueryableAsync())
                .Where(b => b.CustomerName.ToLower().Contains(custKeyword))
                .Select(b => b.Id);
            var matchingBookingIds = await AsyncExecuter.ToListAsync(filterBookingQuery);
            query = query.Where(x => matchingBookingIds.Contains(x.BookingId));
        }

        if (input.ApprovalStatus.HasValue)
            query = query.Where(x => x.ApprovalStatus == input.ApprovalStatus.Value);

        // Filter by star rating
        if (input.OverallRating.HasValue)
        {
            var ratingFilter = input.OverallRating.Value;
            if (ratingFilter == 2)
            {
                // "Dưới 3 sao" → OverallRating < 3
                query = query.Where(x => x.OverallRating < 3);
            }
            else
            {
                // Exact star value (3, 4, 5)
                query = query.Where(x => x.OverallRating == ratingFilter);
            }
        }

        if (input.FromDate.HasValue)
            query = query.Where(x => x.CreationTime >= input.FromDate.Value);

        if (input.ToDate.HasValue)
            query = query.Where(x => x.CreationTime <= input.ToDate.Value.AddDays(1));

        var totalCount = await AsyncExecuter.CountAsync(query);

        // Only allow sorting by properties that exist on AppCaddieRating entity
        var allowedSortFields = new[] { "CreationTime", "OverallRating", "ApprovalStatus", "CaddieId", "CustomerId" };
        var sorting = "CreationTime DESC";
        if (!input.Sorting.IsNullOrWhiteSpace())
        {
            var sortField = input.Sorting.Split(' ')[0];
            if (allowedSortFields.Any(f => f.Equals(sortField, StringComparison.OrdinalIgnoreCase)))
                sorting = input.Sorting;
        }
        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        // Load caddie names + avatars
        var caddieIds = items.Select(x => x.CaddieId).Distinct().ToList();
        var caddieQuery = (await _caddieRepo.GetQueryableAsync())
            .Where(x => caddieIds.Contains(x.Id))
            .Select(x => new { x.Id, x.CaddieName, x.CaddieCode, x.Avatar, x.RatingAvg, x.Phone });
        var caddies = await AsyncExecuter.ToListAsync(caddieQuery);

        // Load booking info
        var bookingIds = items.Select(x => x.BookingId).Distinct().ToList();
        var bookingQuery = (await _bookingRepo.GetQueryableAsync())
            .Where(x => bookingIds.Contains(x.Id))
            .Select(x => new { x.Id, x.BookingCode, x.BookingDate, x.StartTime, x.CustomerName });
        var bookings = await AsyncExecuter.ToListAsync(bookingQuery);

        // Load customer avatars
        var customerIds = items.Select(x => x.CustomerId).Distinct().ToList();
        var customerQuery = (await _customerRepo.GetQueryableAsync())
            .Where(x => customerIds.Contains(x.Id))
            .Select(x => new { x.Id, x.AvatarUrl });
        var customers = await AsyncExecuter.ToListAsync(customerQuery);

        // Load rating details to compute actual average per rating
        var ratingIds = items.Select(x => x.Id).ToList();
        var ratingDetailQuery = (await _ratingDetailRepo.GetQueryableAsync())
            .Where(x => ratingIds.Contains(x.RatingId))
            .Select(x => new { x.RatingId, x.Score });
        var allRatingDetails = await AsyncExecuter.ToListAsync(ratingDetailQuery);

        var dtos = items.Select(x =>
        {
            var caddie = caddies.FirstOrDefault(c => c.Id == x.CaddieId);
            var booking = bookings.FirstOrDefault(b => b.Id == x.BookingId);
            var customer = customers.FirstOrDefault(c => c.Id == x.CustomerId);

            // Compute actual rating from skill details (not from OverallRating which may be inaccurate)
            var details = allRatingDetails.Where(d => d.RatingId == x.Id).ToList();
            var computedRating = details.Count > 0
                ? Math.Round((decimal)details.Average(d => d.Score), 1)
                : (decimal)x.OverallRating;

            return new CaddieRatingDto
            {
                Id = x.Id,
                BookingId = x.BookingId,
                BookingCode = booking?.BookingCode,
                CustomerId = x.CustomerId,
                CustomerName = booking?.CustomerName,
                CustomerAvatar = customer?.AvatarUrl,
                CaddieId = x.CaddieId,
                CaddieName = caddie?.CaddieName,
                CaddieCode = caddie?.CaddieCode,
                CaddieAvatar = caddie?.Avatar,
                CaddiePhone = caddie?.Phone,
                CaddieRatingAvg = caddie?.RatingAvg ?? 0,
                OverallRating = (int)Math.Round(computedRating),
                ComputedRating = computedRating,
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

        // Load customer avatar
        string? customerAvatar = null;
        try
        {
            var customer = await _customerRepo.FindAsync(rating.CustomerId);
            customerAvatar = customer?.AvatarUrl;
        }
        catch { /* customer may not exist */ }

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

        // Compute actual rating from skill details
        var computedRating = details.Count > 0
            ? Math.Round((decimal)details.Average(d => d.Score), 1)
            : (decimal)rating.OverallRating;

        return new CaddieRatingDto
        {
            Id = rating.Id,
            BookingId = rating.BookingId,
            BookingCode = booking.BookingCode,
            CustomerId = rating.CustomerId,
            CustomerName = booking.CustomerName,
            CustomerAvatar = customerAvatar,
            CaddieId = rating.CaddieId,
            CaddieName = caddie.CaddieName,
            CaddieCode = caddie.CaddieCode,
            CaddieAvatar = caddie.Avatar,
            CaddiePhone = caddie.Phone,
            CaddieRatingAvg = caddie.RatingAvg,
            OverallRating = (int)Math.Round(computedRating),
            ComputedRating = computedRating,
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

        // Enqueue background job to recalculate caddie rating avg
        if (input.ApprovalStatus == (byte)CaddieRatingApprovalStatus.Approved ||
            input.ApprovalStatus == (byte)CaddieRatingApprovalStatus.Rejected)
        {
            await _backgroundJobManager.EnqueueAsync(new RecalculateCaddieRatingArgs { CaddieId = rating.CaddieId, TenantId = _currentTenant.Id });
        }
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

        // Enqueue background job to recalculate caddie rating avg
        await _backgroundJobManager.EnqueueAsync(new RecalculateCaddieRatingArgs { CaddieId = rating.CaddieId, TenantId = _currentTenant.Id });
    }

    private static string GetApprovalStatusText(byte status) => status switch
    {
        (byte)CaddieRatingApprovalStatus.Pending => "Chờ duyệt",
        (byte)CaddieRatingApprovalStatus.Approved => "Đã duyệt",
        (byte)CaddieRatingApprovalStatus.Rejected => "Từ chối",
        _ => "Khác"
    };
}
