using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots
{
    public interface IMiniAppCalendarSlotService : IApplicationService
    {
        Task<MiniAppCalendarSlotDto> GetListMiniAppAsync(GetMiniAppCalendarListInput input);
        Task<AppCalendarSlotDto> GetMiniAppAsync(Guid id);
        Task<AppCalendarSlotDto> GetMiniAppAsync(GetMiniAppCalendarSlotDetailInput input);

        /// <summary>
        /// Validate VGA Code và trả về giá theo loại khách hàng tương ứng.
        /// Dùng cho front-end cập nhật lại giá khi người chơi cùng nhập Mã hội viên.
        /// <paramref name="usedVgaCodes"/> là danh sách các mã VGA đã nhập cho những người chơi khác
        /// trong CÙNG booking — dùng để chặn 1 mã áp cho nhiều người (mỗi mã chỉ 1 người chơi).
        /// </summary>
        Task<ValidateVgaCodeResultDto> ValidateVgaCodeAsync(
            string vgaCode,
            Guid calendarSlotId,
            short numberHoles,
            List<string>? usedVgaCodes = null);
    }
}
