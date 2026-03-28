using Genora.MultiTenancy.AppDtos.AppFnbOrders;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Genora.MultiTenancy.Web.Pages.AppFnbOrders.Kitchen;

public class DetailModel : MultiTenancyPageModel
{
    private readonly IAppFnbOrderService _appFnbOrderService;

    private readonly IRepository<GolfCourse, Guid> _golfCourseRepository;
    private readonly ICurrentUser _currentUser;

    public string ShopName { get; private set; } = "LAGUNA GOLF LĂNG CÔ";
    public string ShopAddress { get; private set; } = "Xã Lộc Vĩnh, Phú Lộc, Thừa Thiên Huế";
    public string ShopPhone { get; private set; } = "0234.3695.888";
    public string CashierName { get; private set; } = "Admin";
    public string KioskLabel { get; private set; } = "--- F&B KIOSK #09 ---";

    public string? PaymentQrText { get; private set; }
    public string? PaymentQrBankCode { get; private set; }
    public string? PaymentQrBankAccount { get; private set; }
    public string? PaymentQrBankDisplay { get; private set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public FnbOrderDetailDto Order { get; private set; } = default!;

    public decimal SubTotal => Order?.Items?.Sum(x => x.Price * x.Quantity) ?? 0m;

    public int TotalQuantity => Order?.Items?.Sum(x => x.Quantity) ?? 0;

    public IReadOnlyList<ActivityTimelineItem> Activities { get; private set; } = Array.Empty<ActivityTimelineItem>();

    public DetailModel(IAppFnbOrderService appFnbOrderService, IRepository<GolfCourse, Guid> golfCourseRepository, ICurrentUser currentUser)
    {
        _appFnbOrderService = appFnbOrderService;
        _golfCourseRepository = golfCourseRepository;
        _currentUser = currentUser;
    }

    public async Task OnGetAsync()
    {
        if (Id == Guid.Empty)
        {
            throw new UserFriendlyException("Thiếu mã đơn hàng.");
        }

        var golfCourse = await _golfCourseRepository.FirstOrDefaultAsync();

        if (golfCourse != null)
        {
            ShopName = golfCourse.Name ?? ShopName;
            ShopAddress = golfCourse.Address ?? ShopAddress;      // nếu entity dùng tên khác thì đổi tại đây
            ShopPhone = golfCourse.Phone ?? ShopPhone;      // nếu entity dùng tên khác thì đổi tại đây
        }

        PaymentQrText = golfCourse?.PaymentQrText;
        PaymentQrBankCode = golfCourse?.PaymentQrBankCode;
        PaymentQrBankAccount = golfCourse?.PaymentQrBankAccount;
        PaymentQrBankDisplay = golfCourse?.PaymentQrBankDisplay;

        CashierName = !string.IsNullOrWhiteSpace(_currentUser.Name)
            ? _currentUser.Name!
            : (!string.IsNullOrWhiteSpace(_currentUser.UserName) ? _currentUser.UserName! : "Admin");

        Order = await _appFnbOrderService.GetAsync(Id);
        Activities = BuildActivities(Order);
    }

    public int GetCurrentStep()
    {
        return Order.ServiceStatus switch
        {
            FnbServiceStatus.Created => 1,
            FnbServiceStatus.Preparing => 2,
            FnbServiceStatus.Delivering => 3,
            FnbServiceStatus.Served => 4,
            FnbServiceStatus.Cancelled => 0,
            _ => 0
        };
    }

    public bool IsCancelled()
    {
        return Order.ServiceStatus == FnbServiceStatus.Cancelled;
    }

    public bool CanCancel()
    {
        return Order.ServiceStatus == FnbServiceStatus.Created
            || Order.ServiceStatus == FnbServiceStatus.Preparing;
    }

    public string GetCustomerInitials()
    {
        if (string.IsNullOrWhiteSpace(Order.CustomerName))
        {
            return "KH";
        }

        var parts = Order.CustomerName
            .Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .TakeLast(2)
            .Select(x => char.ToUpperInvariant(x[0]))
            .ToArray();

        return parts.Length == 0 ? "KH" : new string(parts);
    }

    public string GetServiceStatusText()
    {
        return Order.ServiceStatus switch
        {
            FnbServiceStatus.Created => "Mới tạo",
            FnbServiceStatus.Preparing => "Đang xử lý",
            FnbServiceStatus.Delivering => "Đang giao",
            FnbServiceStatus.Served => "Đã phục vụ",
            FnbServiceStatus.Cancelled => "Đã hủy",
            _ => "Không xác định"
        };
    }

    public string GetCustomerTypeText()
    {
        return !string.IsNullOrWhiteSpace(Order.CustomerTypeName)
            ? Order.CustomerTypeName!
            : "Khách lẻ";
    }

    public string GetPaymentStatusText()
    {
        return Order.PaymentStatus switch
        {
            FnbPaymentStatus.Unpaid => "Chưa thanh toán",
            FnbPaymentStatus.Paid => "Đã thanh toán",
            FnbPaymentStatus.Failed => "Thanh toán lỗi",
            _ => "Không xác định"
        };
    }

    public string GetPaymentMethodText()
    {
        return Order.PaymentMethod?.ToString() ?? "—";
    }

    private static IReadOnlyList<ActivityTimelineItem> BuildActivities(FnbOrderDetailDto order)
    {
        var items = new List<ActivityTimelineItem>
        {
            new()
            {
                Title = "Đơn hàng được tạo",
                Description = $"Mã đơn {order.OrderCode}",
                Time = order.CreationTime
            }
        };

        if (order.LastModificationTime.HasValue && order.LastModificationTime.Value > order.CreationTime)
        {
            items.Insert(0, new ActivityTimelineItem
            {
                Title = $"Cập nhật trạng thái: {MapStatus(order.ServiceStatus)}",
                Description = "Cập nhật gần nhất của đơn hàng",
                Time = order.LastModificationTime.Value
            });
        }

        return items;
    }

    private static string MapStatus(FnbServiceStatus status)
    {
        return status switch
        {
            FnbServiceStatus.Created => "Mới tạo",
            FnbServiceStatus.Preparing => "Đang xử lý",
            FnbServiceStatus.Delivering => "Đang giao",
            FnbServiceStatus.Served => "Đã phục vụ",
            FnbServiceStatus.Cancelled => "Đã hủy",
            _ => "Không xác định"
        };
    }

    public class ActivityTimelineItem
    {
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public DateTime Time { get; set; }
    }
}