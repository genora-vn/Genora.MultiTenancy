using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyCustomers;

public class DetailModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public SalonBeautyCustomerDto Customer { get; set; } = new();
    public List<SalonBeautyCustomerBookingHistoryDto> BookingHistory { get; set; } = new();
    public List<SalonBeautyCustomerLoyaltyTransactionDto> LoyaltyTransactions { get; set; } = new();
    public List<SalonBeautyCustomerPurchaseHistoryDto> PurchaseHistory { get; set; } = new();
    public List<SalonBeautyCustomerLedgerDto> DepositLedger { get; set; } = new();
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
        PurchaseHistory = await _customerAppService.GetPurchaseHistoryAsync(Id, 50);
        DepositLedger = await _customerAppService.GetDepositLedgerAsync(Id, 100);
        NextBooking = BookingHistory
            .Where(x => x.BookingDate.Date >= DateTime.Today)
            .OrderBy(x => x.BookingDate)
            .ThenBy(x => x.StartTime)
            .FirstOrDefault();
    }

    public string Money(decimal value) => string.Format("{0:N0}", value);
    public string MoneyWithSuffix(decimal value) => string.Format("{0:N0}đ", value);
    public string DateText(DateTime? value) => value.HasValue ? value.Value.ToString("dd/MM/yyyy") : "--";
    public string TimeText(TimeSpan value) => value.ToString(@"hh\:mm");

    public string BookingStatusKey(string status)
    {
        if (string.IsNullOrWhiteSpace(status)) return "pending";
        return status.Trim().ToLowerInvariant();
    }

    public string BookingStatusText(string status)
    {
        var key = BookingStatusKey(status);
        return key switch
        {
            "pending" => "Chờ xác nhận",
            "confirmed" => "Đã xác nhận",
            "inservice" => "Đang thực hiện",
            "in_service" => "Đang thực hiện",
            "in-service" => "Đang thực hiện",
            "completed" => "Hoàn thành",
            "cancelled" => "Đã hủy",
            "canceled" => "Đã hủy",
            "noshow" => "Không đến",
            "no_show" => "Không đến",
            _ => status
        };
    }

    public string BookingStatusBadgeClass(string status) => BookingStatusKey(status) switch
    {
        "pending" => "salon-badge-warning",
        "confirmed" => "salon-badge-info",
        "inservice" or "in_service" or "in-service" => "salon-badge-primary",
        "completed" => "salon-badge-success",
        "cancelled" or "canceled" or "noshow" or "no_show" => "salon-badge-danger",
        _ => "salon-badge-muted"
    };

    public string LedgerStatusBadgeClass(string statusKey) => statusKey switch
    {
        "SUCCESS" or "DONE" => "salon-badge-success",
        "PENDING" => "salon-badge-warning",
        "CANCELLED" => "salon-badge-danger",
        _ => "salon-badge-muted"
    };

    public string LedgerEntryBadgeClass(string entryType) => entryType switch
    {
        "DEPOSIT" => "salon-ledger-deposit",
        "EARN" => "salon-ledger-earn",
        "REDEEM" => "salon-ledger-redeem",
        "ADJUST" => "salon-ledger-adjust",
        "REFUND" => "salon-ledger-refund",
        _ => "salon-ledger-muted"
    };

    public string ChangePercentText(decimal percent)
    {
        var sign = percent >= 0 ? "+" : "";
        return $"{sign}{percent:0.#}%";
    }

    public string TierIcon(string level) => level switch
    {
        "DIAMOND" => "fa-gem",
        "VIP" => "fa-crown",
        "REGULAR" => "fa-user-check",
        _ => "fa-user"
    };

    public string TierColorClass(string level) => level switch
    {
        "DIAMOND" => "tier-diamond",
        "VIP" => "tier-gold",
        "REGULAR" => "tier-regular",
        _ => "tier-new"
    };

    public string ProServiceStatusBadgeClass(string status) => (status ?? string.Empty).ToLowerInvariant() switch
    {
        "delivered" or "completed" => "salon-badge-success",
        "cancelled" or "canceled" => "salon-badge-danger",
        "ready" => "salon-badge-info",
        "processing" => "salon-badge-primary",
        _ => "salon-badge-muted"
    };

    public string ProPaymentStatusBadgeClass(string status) => (status ?? string.Empty).ToLowerInvariant() switch
    {
        "paid" => "salon-badge-success",
        "failed" => "salon-badge-danger",
        _ => "salon-badge-warning"
    };
}
