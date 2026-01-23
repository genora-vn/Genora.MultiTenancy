using Genora.MultiTenancy.AppDtos.AppBookings;
using Genora.MultiTenancy.DomainModels.AppOptionExtend;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.Web.Pages.AppBookings;

public class ViewModalModel : PageModel
{
    private readonly IAppBookingService _appBookingService;
    private readonly IStringLocalizer<MultiTenancyResource> _l;
    private readonly IRepository<OptionExtend, Guid> _optionExtendRepo;

    public ViewModalModel(
        IAppBookingService appBookingService,
        IStringLocalizer<MultiTenancyResource> l,
        IRepository<OptionExtend, Guid> optionExtendRepo)
    {
        _appBookingService = appBookingService;
        _l = l;
        _optionExtendRepo = optionExtendRepo;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public AppBookingDto Booking { get; set; } = default!;

    public List<string> UtilityNames { get; set; } = new();

    public string StatusText { get; set; } = "";
    public string PaymentMethodText { get; set; } = "";
    public string TotalAmountText { get; set; } = "";
    public string SourceText { get; set; } = "";

    public async Task OnGetAsync()
    {
        Booking = await _appBookingService.GetAsync(Id);

        StatusText = _l[$"BookingStatus:{Booking.Status}"];
        PaymentMethodText = Booking.PaymentMethod.HasValue ? _l[$"PaymentMethod:{Booking.PaymentMethod.Value}"] : "N/A";
        SourceText = _l[$"BookingSource:{Booking.Source}"];
        TotalAmountText = $"{Booking.TotalAmount:N0}";

        var ids = ParseUtilityIds(Booking.Utilities);

        if (ids.Count > 0)
        {
            var q = await _optionExtendRepo.GetQueryableAsync();
            var dict = q
                .Where(x => x.Type == OptionExtendTypeEnum.GolfCourseUlitity.Value && ids.Contains(x.OptionId))
                .ToDictionary(x => x.OptionId, x => x.OptionName);

            UtilityNames = ids.Where(i => dict.ContainsKey(i)).Select(i => dict[i]).ToList();
        }
    }

    private static List<int> ParseUtilityIds(string? utility)
    {
        if (string.IsNullOrWhiteSpace(utility)) return new List<int>();

        return utility
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => int.TryParse(x, out var n) ? n : (int?)null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();
    }
}
