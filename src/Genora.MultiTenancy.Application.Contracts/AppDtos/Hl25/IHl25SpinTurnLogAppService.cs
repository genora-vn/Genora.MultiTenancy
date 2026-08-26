using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>
/// AppService lịch sử nhận lượt quay (read-only) — chỉ liệt kê, không CRUD.
/// </summary>
public interface IHl25SpinTurnLogAppService : IApplicationService
{
    Task<PagedResultDto<Hl25SpinTurnLogDto>> GetListAsync(GetHl25SpinTurnLogListInput input);
}
