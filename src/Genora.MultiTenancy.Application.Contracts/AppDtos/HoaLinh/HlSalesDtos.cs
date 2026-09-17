using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

public class HlSalesOrderFilter
{
    public string? Search { get; set; }
    public string? Source { get; set; }
    public int? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class HlSalesOrderRow
{
    public string Source { get; set; } = "";
    public string? Id { get; set; }
    public string? OrderCode { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public decimal? TotalAmount { get; set; }
    public int? StatusCode { get; set; }
    public string? StatusText { get; set; }
    public DateTime? OrderDate { get; set; }
    public string? SalesName { get; set; }
    public int? PaymentStatus { get; set; }
}

public interface IHlSalesExportAppService : IApplicationService
{
    Task<IRemoteStreamContent> ExportPointHistoryAsync(Genora.MultiTenancy.AppDtos.HoaLinh.HlPointHistoryFilter input, bool batches = false);
    Task<IRemoteStreamContent> ExportGiftExchangesAsync(Genora.MultiTenancy.AppDtos.HoaLinh.HlGiftExchangeFilterDto input);
    Task<List<HlSalesOrderRow>> GetOrdersAsync(HlSalesOrderFilter input);
    Task<IRemoteStreamContent> ExportOrdersAsync(HlSalesOrderFilter input);
}
