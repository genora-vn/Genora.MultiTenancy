using Genora.MultiTenancy.AppDtos.AppProOrders;
using Genora.MultiTenancy.AppServices.AppProOrders;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;

namespace Genora.MultiTenancy.Web.Pages.AppProOrders.Board;

[Authorize]
public class HistoryModel : MultiTenancyPageModel
{
    private readonly IAppProOrderService _proOrderService;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ActionType { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public ProOrderHistoryPageDto Data { get; private set; } = default!;

    public int TotalPages =>
        Data == null || Data.PagedActivities == null || PageSize <= 0
            ? 1
            : (int)Math.Ceiling((double)Data.PagedActivities.TotalCount / PageSize);

    public HistoryModel(IAppProOrderService proOrderService)
    {
        _proOrderService = proOrderService;
    }

    public async Task OnGetAsync()
    {
        if (Id == Guid.Empty)
            throw new UserFriendlyException("Thiếu mã đơn hàng.");

        if (CurrentPage <= 0) CurrentPage = 1;
        if (PageSize <= 0)    PageSize    = 10;

        Data = await _proOrderService.GetHistoryPageAsync(new GetProOrderHistoryInput
        {
            OrderId        = Id,
            ActionType     = string.IsNullOrWhiteSpace(ActionType) ? null : ActionType,
            SkipCount      = (CurrentPage - 1) * PageSize,
            MaxResultCount = PageSize
        });
    }

    public string GetServiceStatusText() => Data?.ServiceStatus switch
    {
        ProServiceStatus.Created    => "Đơn mới",
        ProServiceStatus.Processing => "Đang xử lý",
        ProServiceStatus.Ready      => "Sẵn sàng giao",
        ProServiceStatus.Delivered  => "Đã giao",
        ProServiceStatus.Cancelled  => "Đã hủy",
        _                           => "Không xác định"
    };
}
