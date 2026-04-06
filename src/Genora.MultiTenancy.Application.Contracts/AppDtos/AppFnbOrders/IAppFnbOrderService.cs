using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppFnbOrders;
public interface IAppFnbOrderService : IApplicationService
{
    Task<PagedResultDto<FnbOrderDto>> GetListAsync(GetFnbOrderListInput input);
    Task<FnbOrderDetailDto> GetAsync(Guid id);
    Task<FnbOrderDetailDto> CreateAsync(CreateFnbOrderDto input);
    Task<FnbOrderDto> UpdateServiceStatusAsync(Guid id, UpdateFnbOrderServiceStatusDto input);
    Task<FnbOrderDto> UpdatePaymentStatusAsync(Guid id, UpdateFnbOrderPaymentStatusDto input);
    Task<FnbOrderDto> CancelAsync(Guid id, CancelFnbOrderDto input);
    Task<FnbOrderHistoryPageDto> GetHistoryPageAsync(Guid id);
    Task<FnbOrderHistoryPageDto> GetHistoryPageAsync(GetFnbOrderHistoryInput input);
    Task<List<FnbKitchenBoardItemDto>> GetKitchenBoardAsync(GetFnbKitchenBoardInput input);
    Task<IRemoteStreamContent> ExportExcelAsync(GetFnbOrderListInput input);
}