using Genora.MultiTenancy.AppDtos.AppProOrders;
using Genora.MultiTenancy.AppServices.AppProOrders;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Genora.MultiTenancy.Web.Pages.AppProOrders.Board;

[Authorize]
public class DetailModel : MultiTenancyPageModel
{
    private readonly IAppProOrderService _proOrderService;
    private readonly IRepository<GolfCourse, Guid> _golfCourseRepository;
    private readonly ICurrentUser _currentUser;

    public string ShopName    { get; private set; } = "LAGUNA GOLF LĂNG CÔ";
    public string ShopAddress { get; private set; } = "Xã Lộc Vĩnh, Phú Lộc, Thừa Thiên Huế";
    public string ShopPhone   { get; private set; } = "0234.3695.888";
    public string CashierName { get; private set; } = "Admin";
    public string KioskLabel  { get; private set; } = "--- PROSHOP KIOSK #01 ---";

    public string? PaymentQrText        { get; private set; }
    public string? PaymentQrBankCode    { get; private set; }
    public string? PaymentQrBankAccount { get; private set; }
    public string? PaymentQrBankDisplay { get; private set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public ProOrderDetailDto Order { get; private set; } = default!;

    public decimal SubTotal     => Order?.Items?.Sum(x => x.Price * x.Quantity) ?? 0m;
    public int     TotalQuantity => Order?.Items?.Sum(x => x.Quantity) ?? 0;

    public DetailModel(
        IAppProOrderService proOrderService,
        IRepository<GolfCourse, Guid> golfCourseRepository,
        ICurrentUser currentUser)
    {
        _proOrderService      = proOrderService;
        _golfCourseRepository = golfCourseRepository;
        _currentUser          = currentUser;
    }

    public async Task OnGetAsync()
    {
        if (Id == Guid.Empty)
            throw new UserFriendlyException("Thiếu mã đơn hàng.");

        var golfCourse = await _golfCourseRepository.FirstOrDefaultAsync();
        if (golfCourse != null)
        {
            ShopName    = golfCourse.Name    ?? ShopName;
            ShopAddress = golfCourse.Address ?? ShopAddress;
            ShopPhone   = golfCourse.Phone   ?? ShopPhone;
        }

        PaymentQrText        = golfCourse?.PaymentQrText;
        PaymentQrBankCode    = golfCourse?.PaymentQrBankCode;
        PaymentQrBankAccount = golfCourse?.PaymentQrBankAccount;
        PaymentQrBankDisplay = golfCourse?.PaymentQrBankDisplay;

        CashierName = !string.IsNullOrWhiteSpace(_currentUser.Name)
            ? _currentUser.Name!
            : (!string.IsNullOrWhiteSpace(_currentUser.UserName) ? _currentUser.UserName! : "Admin");

        Order = await _proOrderService.GetAsync(Id);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    public int GetCurrentStep() => Order.ServiceStatus switch
    {
        ProServiceStatus.Created    => 1,
        ProServiceStatus.Processing => 2,
        ProServiceStatus.Ready      => 3,
        ProServiceStatus.Delivered  => 4,
        ProServiceStatus.Cancelled  => 0,
        _                           => 0
    };

    public bool IsCancelled() => Order.ServiceStatus == ProServiceStatus.Cancelled;

    public bool CanCancel() =>
        Order.ServiceStatus == ProServiceStatus.Created ||
        Order.ServiceStatus == ProServiceStatus.Processing;

    public string GetServiceStatusText() => Order.ServiceStatus switch
    {
        ProServiceStatus.Created    => "Đơn mới",
        ProServiceStatus.Processing => "Đang xử lý",
        ProServiceStatus.Ready      => "Sẵn sàng giao",
        ProServiceStatus.Delivered  => "Đã giao",
        ProServiceStatus.Cancelled  => "Đã hủy",
        _                           => "Không xác định"
    };

    public string GetPaymentStatusText() => Order.PaymentStatus switch
    {
        ProPaymentStatus.Unpaid => "Chưa thanh toán",
        ProPaymentStatus.Paid   => "Đã thanh toán",
        ProPaymentStatus.Failed => "Thanh toán lỗi",
        _                       => "Không xác định"
    };

    public string GetPaymentMethodText() => Order.PaymentMethod?.ToString() ?? "—";

    public string GetCustomerInitials()
    {
        if (string.IsNullOrWhiteSpace(Order.CustomerName)) return "KH";
        var parts = Order.CustomerName.Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .TakeLast(2)
            .Select(x => char.ToUpperInvariant(x[0]))
            .ToArray();
        return parts.Length == 0 ? "KH" : new string(parts);
    }
}
