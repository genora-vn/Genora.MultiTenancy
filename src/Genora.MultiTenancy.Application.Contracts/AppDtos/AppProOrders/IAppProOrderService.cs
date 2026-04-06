using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppProOrders;

public interface IAppProOrderService : IApplicationService
{
    Task<PagedResultDto<ProOrderDto>> GetListAsync(GetProOrderListInput input);
    Task<ProOrderDetailDto> GetAsync(Guid id);
    Task<ProOrderDetailDto> CreateAsync(CreateProOrderDto input);
    Task<ProOrderDto> UpdateServiceStatusAsync(Guid id, UpdateProOrderServiceStatusDto input);
    Task<ProOrderDto> UpdatePaymentStatusAsync(Guid id, UpdateProOrderPaymentStatusDto input);
    Task<ProOrderDto> CancelAsync(Guid id, CancelProOrderDto input);
    Task<IRemoteStreamContent> ExportExcelAsync(GetProOrderListInput input);
    Task<List<ProBoardItemDto>> GetBoardAsync(GetProBoardInput input);
    Task<ProOrderHistoryPageDto> GetHistoryPageAsync(GetProOrderHistoryInput input);
}
