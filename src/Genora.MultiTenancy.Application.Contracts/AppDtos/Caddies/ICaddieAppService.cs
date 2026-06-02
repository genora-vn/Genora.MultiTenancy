using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.Caddies;

public interface ICaddieAppService : IApplicationService
{
    Task<PagedResultDto<CaddieDto>> GetListAsync(GetCaddieListInput input);
    Task<CaddieDto> GetAsync(Guid id);
    Task<CaddieDto> CreateAsync(CreateUpdateCaddieDto input);
    Task<CaddieDto> UpdateAsync(Guid id, CreateUpdateCaddieDto input);
    Task DeleteAsync(Guid id);
    Task UpdateStatusAsync(Guid id, byte status);
    Task<string> GenerateCaddieCodeAsync();
}
