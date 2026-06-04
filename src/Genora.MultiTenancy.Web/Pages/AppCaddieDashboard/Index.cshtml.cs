using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.AppServices.Caddies;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Web.Pages.AppCaddieDashboard;

public class IndexModel : MultiTenancyPageModel
{
    public int TotalCaddies { get; set; }
    public int ActiveCaddies { get; set; }
    public int TotalBookingsThisMonth { get; set; }
    public int TotalRatingsThisMonth { get; set; }
    public int PendingRatings { get; set; }
    public decimal AvgRating { get; set; }

    // Chart data
    public string BookingsByDayLabels { get; set; } = "[]";
    public string BookingsByDayData { get; set; } = "[]";
    public string RatingDistLabels { get; set; } = "[]";
    public string RatingDistData { get; set; } = "[]";
    public string TopCaddiesLabels { get; set; } = "[]";
    public string TopCaddiesData { get; set; } = "[]";

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
        var monthStart = new DateTime(today.Year, today.Month, 1);

        // KPIs
        var allCaddies = await _caddieService.GetListAsync(new GetCaddieListInput { MaxResultCount = 1000 });
        TotalCaddies = (int)allCaddies.TotalCount;
        ActiveCaddies = allCaddies.Items.Count(x => x.Status == 1);

        var monthBookings = await _bookingService.GetListAsync(new GetCaddieBookingListInput
        {
            FromDate = monthStart,
            ToDate = today,
            MaxResultCount = 1000
        });
        TotalBookingsThisMonth = (int)monthBookings.TotalCount;

        var allRatings = await _ratingService.GetListAsync(new GetCaddieRatingListInput
        {
            FromDate = monthStart,
            ToDate = today,
            MaxResultCount = 1000
        });
        TotalRatingsThisMonth = (int)allRatings.TotalCount;
        PendingRatings = allRatings.Items.Count(x => x.ApprovalStatus == 1);
        AvgRating = allRatings.Items.Any() ? Math.Round((decimal)allRatings.Items.Average(x => x.OverallRating), 1) : 0;

        // Chart: Bookings per day (last 14 days)
        var last14Days = Enumerable.Range(0, 14).Select(i => today.AddDays(-13 + i)).ToList();
        var bookingCounts = last14Days.Select(d =>
            monthBookings.Items.Count(b => b.BookingDate.Date == d.Date)).ToList();
        BookingsByDayLabels = System.Text.Json.JsonSerializer.Serialize(
            last14Days.Select(d => d.ToString("dd/MM")));
        BookingsByDayData = System.Text.Json.JsonSerializer.Serialize(bookingCounts);

        // Chart: Rating distribution (1-5 stars)
        var ratingDist = Enumerable.Range(1, 5).Select(star =>
            allRatings.Items.Count(r => (int)Math.Round((double)r.OverallRating) == star)).ToList();
        RatingDistLabels = System.Text.Json.JsonSerializer.Serialize(
            new[] { "1 sao", "2 sao", "3 sao", "4 sao", "5 sao" });
        RatingDistData = System.Text.Json.JsonSerializer.Serialize(ratingDist);

        // Chart: Top 5 caddies by booking count
        var topCaddies = allCaddies.Items
            .OrderByDescending(x => x.TotalBooking)
            .Take(5)
            .ToList();
        TopCaddiesLabels = System.Text.Json.JsonSerializer.Serialize(
            topCaddies.Select(x => x.CaddieName));
        TopCaddiesData = System.Text.Json.JsonSerializer.Serialize(
            topCaddies.Select(x => x.TotalBooking));
    }
}
