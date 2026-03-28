using Genora.MultiTenancy.AppDtos.AppFnbOrders;
using Genora.MultiTenancy.AppServices.AppFnbOrders;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Genora.MultiTenancy.Web.Pages.AppFnbOrders.Kitchen;

public class HistoryModel : MultiTenancyPageModel
{
    private readonly IAppFnbOrderService _appFnbOrderService;
    private readonly IRepository<GolfCourse, Guid> _golfCourseRepository;
    private readonly ICurrentUser _currentUser;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ActionType { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public FnbOrderHistoryPageDto Data { get; private set; } = default!;
    public FnbOrderDetailDto OrderDetail { get; private set; } = default!;

    public string ShopName { get; private set; } = "LAGUNA GOLF LĂNG CÔ";
    public string ShopAddress { get; private set; } = "Xã Lộc Vĩnh, Phú Lộc, Thừa Thiên Huế";
    public string ShopPhone { get; private set; } = "0234.3695.888";
    public string CashierName { get; private set; } = "Admin";
    public string KioskLabel { get; private set; } = "--- F&B KIOSK #09 ---";

    public string? PaymentQrText { get; private set; }
    public string? PaymentQrBankCode { get; private set; }
    public string? PaymentQrBankAccount { get; private set; }
    public string? PaymentQrBankDisplay { get; private set; }

    public int TotalPages =>
        Data == null || Data.PagedActivities == null || PageSize <= 0
            ? 1
            : (int)Math.Ceiling((double)Data.PagedActivities.TotalCount / PageSize);

    public HistoryModel(
        IAppFnbOrderService appFnbOrderService,
        IRepository<GolfCourse, Guid> golfCourseRepository,
        ICurrentUser currentUser)
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

        if (CurrentPage <= 0) CurrentPage = 1;
        if (PageSize <= 0) PageSize = 10;

        var golfCourse = await _golfCourseRepository.FirstOrDefaultAsync();

        if (golfCourse != null)
        {
            ShopName = golfCourse.Name ?? ShopName;
            ShopAddress = golfCourse.Address ?? ShopAddress;
            ShopPhone = golfCourse.Phone ?? ShopPhone;
        }

        CashierName = !string.IsNullOrWhiteSpace(_currentUser.Name)
            ? _currentUser.Name!
            : (!string.IsNullOrWhiteSpace(_currentUser.UserName) ? _currentUser.UserName! : "Admin");

        PaymentQrText = golfCourse?.PaymentQrText;
        PaymentQrBankCode = golfCourse?.PaymentQrBankCode;
        PaymentQrBankAccount = golfCourse?.PaymentQrBankAccount;
        PaymentQrBankDisplay = golfCourse?.PaymentQrBankDisplay;

        Data = await _appFnbOrderService.GetHistoryPageAsync(new GetFnbOrderHistoryInput
        {
            OrderId = Id,
            ActionType = string.IsNullOrWhiteSpace(ActionType) ? null : ActionType,
            SkipCount = (CurrentPage - 1) * PageSize,
            MaxResultCount = PageSize
        });

        OrderDetail = await _appFnbOrderService.GetAsync(Id);
    }

    public string GetServiceStatusText()
    {
        return Data.ServiceStatus switch
        {
            Enums.FnbServiceStatus.Created => "Mới tạo",
            Enums.FnbServiceStatus.Preparing => "Đang xử lý",
            Enums.FnbServiceStatus.Delivering => "Đang giao",
            Enums.FnbServiceStatus.Served => "Đã phục vụ",
            Enums.FnbServiceStatus.Cancelled => "Đã hủy",
            _ => "Không xác định"
        };
    }
}