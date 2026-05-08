using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos.SalonBeautyCustomerDtos;
using Genora.MultiTenancy.SalonBeauty;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyCustomers;

public class DetailModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public SalonBeautyCustomerDto Customer { get; set; } = new();
    public List<SalonBeautyCustomerBookingHistoryDto> BookingHistory { get; set; } = new();
    public List<SalonBeautyCustomerLoyaltyTransactionDto> LoyaltyTransactions { get; set; } = new();
    public SalonBeautyCustomerBookingHistoryDto? NextBooking { get; set; }

    private readonly ISalonBeautyCustomerAppService _customerAppService;

    public DetailModel(ISalonBeautyCustomerAppService customerAppService)
    {
        _customerAppService = customerAppService;
    }

    public async Task OnGetAsync()
    {
        Customer = await _customerAppService.GetAsync(Id);
        BookingHistory = await _customerAppService.GetBookingHistoryAsync(Id, 50);
        LoyaltyTransactions = await _customerAppService.GetLoyaltyTransactionsAsync(Id, 50);
        NextBooking = BookingHistory
            .Where(x => x.BookingDate.Date >= DateTime.Today)
            .OrderBy(x => x.BookingDate)
            .ThenBy(x => x.StartTime)
            .FirstOrDefault();
    }

    public string Money(decimal value) => string.Format("{0:N0}", value);
    public string DateText(DateTime? value) => value.HasValue ? value.Value.ToString("dd/MM/yyyy") : "--";
    public string TimeText(TimeSpan value) => value.ToString(@"hh\:mm");
}
