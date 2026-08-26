using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.Enums;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>
/// AppService lịch sử lượt quay. Read-only list + cập nhật trạng thái trao thưởng (2 bước: Won → Delivered).
/// </summary>
public interface IHl25SpinLogAppService : IApplicationService
{
    Task<PagedResultDto<Hl25SpinLogDto>> GetListAsync(GetHl25SpinLogListInput input);

    /// <summary>
    /// Cập nhật trạng thái trao thưởng của một lượt quay (Admin xác nhận trao → Delivered).
    /// </summary>
    Task<Hl25SpinLogDto> UpdateRewardStatusAsync(Guid id, Hl25RewardStatus status);
}
