using Genora.MultiTenancy.AppDtos.AppBookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.Controllers;

[IgnoreAntiforgeryToken]
[RemoteService(false)]
[Area("MultiTenancy")]
[Route("api/mini-app")]
public class MiniAppController : MultiTenancyController
{
    private readonly IMiniAppBookingAppService _miniBooking;

    public MiniAppController(IMiniAppBookingAppService miniBooking)
    {
        _miniBooking = miniBooking;
    }

    [HttpPost("create-booking")]
    [AllowAnonymous] // tạm: sẽ thay bằng custom auth sau
    public Task<AppBookingDto> CreateBookingAsync([FromBody] MiniAppCreateBookingDto input)
        => _miniBooking.CreateFromMiniAppAsync(input);

    [HttpGet("get-bookings")]
    [AllowAnonymous]
    public Task<PagedResultDto<AppBookingDto>> GetBookingsAsync([FromQuery] GetMiniAppBookingListInput input)
        => _miniBooking.GetListMiniAppAsync(input);

    [HttpGet("get-bookings/{id}")]
    [AllowAnonymous]
    public Task<AppBookingDto> GetBookingAsync(Guid id, [FromQuery] Guid customerId)
        => _miniBooking.GetMiniAppAsync(id, customerId);
}