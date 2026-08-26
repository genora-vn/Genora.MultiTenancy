using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>
/// AppService quản lý người tham gia chương trình (Hl25Participant).
/// Admin: xem/sửa thông tin, cộng lượt quay thủ công, xuất Excel. KHÔNG cho tạo/xóa thủ công
/// (người tham gia tạo qua Mini App).
/// </summary>
public interface IHl25ParticipantAppService : IApplicationService
{
    Task<Hl25ParticipantDto> GetAsync(Guid id);

    Task<PagedResultDto<Hl25ParticipantDto>> GetListAsync(GetHl25ParticipantListInput input);

    /// <summary>Admin sửa thông tin người tham gia (họ tên/SĐT/địa chỉ/consent/follow).</summary>
    Task<Hl25ParticipantDto> UpdateAsync(Guid id, CreateUpdateHl25ParticipantDto input);

    /// <summary>
    /// Cộng lượt quay thủ công (source = AdminGrant). Ghi Hl25SpinTurnLog + tăng RemainingSpinTurns/TotalSpinTurns.
    /// KHÔNG áp trần MaxSpinTurnsPerUser vì đây là cấp thủ công bởi Admin.
    /// </summary>
    Task<Hl25ParticipantDto> GrantSpinTurnAsync(Guid id, int turns, string? note);

    /// <summary>Xuất Excel danh sách người tham gia theo filter.</summary>
    Task<IRemoteStreamContent> ExportExcelAsync(GetHl25ParticipantListInput input);
}
