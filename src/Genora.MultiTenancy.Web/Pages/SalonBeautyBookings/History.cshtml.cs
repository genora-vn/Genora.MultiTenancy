using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyBookings;

[Authorize]
public class HistoryModel : MultiTenancyPageModel
{
    private readonly ISalonBeautyBookingAppService _bookingService;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ActionType { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public SalonBeautyBookingHistoryPageDto Data { get; private set; } = default!;

    public int TotalPages =>
        Data == null || Data.PagedActivities == null || PageSize <= 0
            ? 1
            : (int)Math.Ceiling((double)Data.PagedActivities.TotalCount / PageSize);

    public HistoryModel(ISalonBeautyBookingAppService bookingService)
    {
        _bookingService = bookingService;
    }

    public async Task OnGetAsync()
    {
        if (Id == Guid.Empty)
            throw new UserFriendlyException("Thiếu mã đặt lịch.");

        if (CurrentPage <= 0) CurrentPage = 1;
        if (PageSize <= 0) PageSize = 10;

        Data = await _bookingService.GetHistoryPageAsync(new GetSalonBeautyBookingHistoryInput
        {
            BookingId      = Id,
            ActionType     = string.IsNullOrWhiteSpace(ActionType) ? null : ActionType,
            SkipCount      = (CurrentPage - 1) * PageSize,
            MaxResultCount = PageSize
        });
    }

    public string GetStatusText() => Data?.Status switch
    {
        SalonBeautyBookingStatus.New        => "Mới",
        SalonBeautyBookingStatus.Confirmed  => "Đã xác nhận",
        SalonBeautyBookingStatus.Processing => "Đang phục vụ",
        SalonBeautyBookingStatus.Completed  => "Hoàn tất",
        SalonBeautyBookingStatus.Cancelled  => "Đã hủy",
        _                                   => "Không xác định"
    };

    public string GetStatusClass() => Data?.Status switch
    {
        SalonBeautyBookingStatus.New        => "is-info",
        SalonBeautyBookingStatus.Confirmed  => "is-primary",
        SalonBeautyBookingStatus.Processing => "is-warning",
        SalonBeautyBookingStatus.Completed  => "is-success",
        SalonBeautyBookingStatus.Cancelled  => "is-danger",
        _                                   => "is-muted"
    };
}
