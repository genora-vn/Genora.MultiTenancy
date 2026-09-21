
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.Hlg
{
    public interface IHlgBrandAppService : IApplicationService
    {
        Task<List<BrandKnowledgeDto>> GetBrandsAsync(CancellationToken ct = default);
        Task<BrandKnowledgeDto> GetBrandAsync(Guid id, CancellationToken ct = default);
        Task<HlgProductDto> GetProductAsync(Guid id, CancellationToken ct = default);
    }
}
