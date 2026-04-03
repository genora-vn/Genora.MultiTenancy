using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppBookings;
public interface IMiniAppBookingAppService : IApplicationService
{
    Task<MiniAppBookingDetailDto> CreateFromMiniAppAsync(MiniAppCreateBookingDto input);

    Task<MiniAppBookingDetailDto> UpdateFromMiniAppAsync(Guid id, MiniAppUpdateBookingDto input);

    Task<MiniAppBookingListDto> GetListMiniAppAsync(GetMiniAppBookingListInput input);

    Task<MiniAppBookingDetailDto> GetMiniAppAsync(Guid id, Guid customerId);

    /// <summary>
    /// Huỷ booking từ Mini App — chỉ chủ booking mới được huỷ.
    /// Tự động gửi ZBS "BookingCancelled" + Email cancel.
    /// Status cập nhật: BookingStatus.CancelledRefund
    /// </summary>
    Task<MiniAppBookingDetailDto> CancelFromMiniAppAsync(Guid id, MiniAppCancelBookingDto input);
}