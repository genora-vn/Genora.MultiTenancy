using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

/// <summary>
/// AppService quản lý đơn hàng Genora (tạo từ Mini App, lưu DB)
/// </summary>
public interface IHlOrderAppService : IApplicationService
{
    Task<PagedResultDto<HlOrderDto>> GetListAsync(HlOrderFilterDto input);
    Task<HlOrderDto> GetAsync(Guid id);
    Task<HlOrderDto> UpdateStatusAsync(HlOrderUpdateStatusDto input);
    Task<HlOrderDto> CancelAsync(HlOrderCancelDto input);
}

/// <summary>
/// AppService quản lý đổi quà Genora
/// </summary>
public interface IHlGiftExchangeAppService : IApplicationService
{
    Task<PagedResultDto<HlGiftExchangeDto>> GetListAsync(HlGiftExchangeFilterDto input);
    Task<HlGiftExchangeDto> GetAsync(Guid id);
    Task<HlGiftExchangeDto> ApproveOrRejectAsync(HlGiftExchangeApproveDto input);
}
