using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.AppServices.Caddies;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Web.Pages.AppCaddieReports;

public class IndexModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }

    public List<CaddiePerformanceRow> PerformanceData { get; set; } = new();
    public int TotalBookings { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalCancelled { get; set; }
    public decimal CompletionRate { get; set; }

    private readonly CaddieAppService _caddieService;
    private readonly CaddieBookingAppService _bookingService;
    private readonly CaddieRatingAppService _ratingService;

    public IndexModel(
        CaddieAppService caddieService,
        CaddieBookingAppService bookingService,
        CaddieRatingAppService ratingService)
    {
        _caddieService = caddieService;
        _bookingService = bookingService;
        _ratingService = ratingService;
    }

    public async Task OnGetAsync()
    {
        var today = DateTime.Today;
        FromDate ??= new DateTime(today.Year, today.Month, 1);
        ToDate ??= today;

        // Load all caddies
        var caddies = await _caddieService.GetListAsync(new GetCaddieListInput { MaxResultCount = 500 });

        // Load bookings in range
        var bookings = await _bookingService.GetListAsync(new GetCaddieBookingListInput
        {
            FromDate = FromDate,
            ToDate = ToDate,
            MaxResultCount = 5000
        });

        // Load ratings in range
        var ratings = await _ratingService.GetListAsync(new GetCaddieRatingListInput
        {
            FromDate = FromDate,
            ToDate = ToDate,
            MaxResultCount = 5000
        });

        TotalBookings = (int)bookings.TotalCount;
        TotalCompleted = bookings.Items.Count(b => b.Status == 3);
        TotalCancelled = bookings.Items.Count(b => b.Status == 4);
        CompletionRate = TotalBookings > 0 ? Math.Round((decimal)TotalCompleted / TotalBookings * 100, 1) : 0;

        // Build per-caddie performance
        PerformanceData = caddies.Items.Select(c =>
        {
            var caddieBookings = bookings.Items.Where(b => b.CaddieId == c.Id).ToList();
            var caddieRatings = ratings.Items.Where(r => r.CaddieId == c.Id).ToList();
            var completedCount = caddieBookings.Count(b => b.Status == 3);
            var cancelledCount = caddieBookings.Count(b => b.Status == 4);
            var avgRating = caddieRatings.Any() ? Math.Round((decimal)caddieRatings.Average(r => r.OverallRating), 1) : 0;

            return new CaddiePerformanceRow
            {
                CaddieId = c.Id,
                CaddieCode = c.CaddieCode,
                CaddieName = c.CaddieName,
                TotalBookings = caddieBookings.Count,
                CompletedBookings = completedCount,
                CancelledBookings = cancelledCount,
                CompletionRate = caddieBookings.Count > 0 ? Math.Round((decimal)completedCount / caddieBookings.Count * 100, 1) : 0,
                TotalRatings = caddieRatings.Count,
                AvgRating = avgRating,
                Status = c.Status
            };
        })
        .Where(x => x.TotalBookings > 0 || x.TotalRatings > 0)
        .OrderByDescending(x => x.TotalBookings)
        .ToList();
    }

    public class CaddiePerformanceRow
    {
        public Guid CaddieId { get; set; }
        public string CaddieCode { get; set; } = "";
        public string CaddieName { get; set; } = "";
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public decimal CompletionRate { get; set; }
        public int TotalRatings { get; set; }
        public decimal AvgRating { get; set; }
        public byte Status { get; set; }
    }
}
