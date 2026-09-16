using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>
/// AppService báo cáo thống kê chương trình Hoa Linh 25 Năm (không CRUD, chỉ query tổng hợp).
/// </summary>
public interface IHl25ReportAppService : IApplicationService
{
    /// <summary>Thống kê lượt tạo/chia sẻ thiệp (Frame) theo thời gian/chiến dịch.</summary>
    Task<Hl25FrameStatsDto> GetFrameStatsAsync(Hl25ReportInput input);

    /// <summary>Thống kê lượt tham gia Vòng quay (số người + tổng lượt quay/cấp).</summary>
    Task<Hl25WheelParticipationStatsDto> GetWheelParticipationStatsAsync(Hl25ReportInput input);

    /// <summary>Thống kê Vòng quay theo Quà tặng (cơ cấu giải đã trao, tỷ lệ, tồn kho).</summary>
    Task<Hl25WheelGiftStatsDto> GetWheelGiftStatsAsync(Hl25ReportInput input);

    /// <summary>Thống kê phân bổ người tham gia theo nhóm tuổi (Delta 2026-09).</summary>
    Task<Hl25AgeGroupStatsDto> GetAgeGroupStatsAsync(Hl25ReportInput input);

    /// <summary>Thống kê phân bổ người tham gia theo giới tính.</summary>
    Task<Hl25GenderStatsDto> GetGenderStatsAsync(Hl25ReportInput input);
}
