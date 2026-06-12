using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.DomainModels.AppCaddie;
using Genora.MultiTenancy.Enums;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Uow;

namespace Genora.MultiTenancy.AppServices.Caddies;

/// <summary>
/// Background job args for recalculating caddie rating average.
/// Triggered when a rating is approved, rejected, or deleted.
/// </summary>
[Serializable]
public class RecalculateCaddieRatingArgs
{
    public Guid CaddieId { get; set; }
    public Guid? TenantId { get; set; }
}

/// <summary>
/// Background job that recalculates AppCaddie.RatingAvg from all approved ratings.
/// Uses skill detail scores average per rating, then averages across all ratings.
/// </summary>
public class RecalculateCaddieRatingJob : AsyncBackgroundJob<RecalculateCaddieRatingArgs>, ITransientDependency
{
    private readonly IRepository<AppCaddieRating, Guid> _ratingRepo;
    private readonly IRepository<AppCaddieRatingDetail, Guid> _ratingDetailRepo;
    private readonly IRepository<AppCaddie, Guid> _caddieRepo;
    private readonly IAsyncQueryableExecuter _asyncExecuter;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public RecalculateCaddieRatingJob(
        IRepository<AppCaddieRating, Guid> ratingRepo,
        IRepository<AppCaddieRatingDetail, Guid> ratingDetailRepo,
        IRepository<AppCaddie, Guid> caddieRepo,
        IAsyncQueryableExecuter asyncExecuter,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _ratingRepo = ratingRepo;
        _ratingDetailRepo = ratingDetailRepo;
        _caddieRepo = caddieRepo;
        _asyncExecuter = asyncExecuter;
        _unitOfWorkManager = unitOfWorkManager;
    }

    [UnitOfWork]
    public override async Task ExecuteAsync(RecalculateCaddieRatingArgs args)
    {
        var caddieId = args.CaddieId;

        try
        {
            using var uow = _unitOfWorkManager.Begin(requiresNew: true);

            // Get all approved ratings for this caddie
            var approvedQuery = (await _ratingRepo.GetQueryableAsync())
                .Where(x => x.CaddieId == caddieId && x.ApprovalStatus == (byte)CaddieRatingApprovalStatus.Approved);
            var approvedRatings = await _asyncExecuter.ToListAsync(approvedQuery);

            var caddie = await _caddieRepo.GetAsync(caddieId);

            if (!approvedRatings.Any())
            {
                caddie.RatingAvg = 0;
                caddie.TotalBooking = 0;
                await _caddieRepo.UpdateAsync(caddie, autoSave: true);
                await uow.CompleteAsync();
                return;
            }

            // For each rating, compute avg from skill details
            var ratingIds = approvedRatings.Select(r => r.Id).ToList();
            var allDetails = await _asyncExecuter.ToListAsync(
                (await _ratingDetailRepo.GetQueryableAsync()).Where(d => ratingIds.Contains(d.RatingId)));

            var perBookingAvgs = new List<decimal>();
            foreach (var rating in approvedRatings)
            {
                var details = allDetails.Where(d => d.RatingId == rating.Id).ToList();
                if (details.Any())
                    perBookingAvgs.Add((decimal)details.Average(d => d.Score));
                else
                    perBookingAvgs.Add(rating.OverallRating);
            }

            caddie.RatingAvg = Math.Round(perBookingAvgs.Average(), 2);
            caddie.TotalBooking = approvedRatings.Count;
            await _caddieRepo.UpdateAsync(caddie, autoSave: true);

            await uow.CompleteAsync();

            Logger.LogInformation("Recalculated RatingAvg for Caddie {CaddieId}: {RatingAvg} ({TotalBooking} ratings)",
                caddieId, caddie.RatingAvg, caddie.TotalBooking);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to recalculate rating for Caddie {CaddieId}", caddieId);
            throw;
        }
    }
}
