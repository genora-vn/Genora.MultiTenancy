using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>
/// AppService lịch sử tạo ảnh thiệp (read-only) — chỉ liệt kê để đối soát & chăm sóc.
/// </summary>
public interface IHl25FrameCreationAppService : IApplicationService
{
    Task<PagedResultDto<Hl25FrameCreationDto>> GetListAsync(GetHl25FrameCreationListInput input);

    /// <summary>Xuất Excel lịch sử tạo ảnh thiệp (toàn bộ, không phân trang).</summary>
    Task<IRemoteStreamContent> ExportExcelAsync(GetHl25FrameCreationListInput input);

    /// <summary>ZIP toàn bộ ảnh thiệp đã tạo và trả về stream download.</summary>
    Task<IRemoteStreamContent> DownloadAllImagesAsync();
}
