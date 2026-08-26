using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>
/// AppService lịch sử tạo ảnh thiệp (read-only) — chỉ liệt kê để đối soát & chăm sóc.
/// </summary>
public interface IHl25FrameCreationAppService : IApplicationService
{
    Task<PagedResultDto<Hl25FrameCreationDto>> GetListAsync(GetHl25FrameCreationListInput input);
}
