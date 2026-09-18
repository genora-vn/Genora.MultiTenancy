using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.DomainModels.AppHlPoints;
using Genora.MultiTenancy.DomainModels.AppHlGiftExchanges;
using Genora.MultiTenancy.DomainModels.AppHlOrders;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

// Hoa Linh Sales (schema HL), separate from HL25 and HLG.
public class HlSalesExportAppService : ApplicationService, IHlSalesExportAppService
{
    private readonly IRepository<HlPointTransaction, Guid> _transactions;
    private readonly IRepository<HlPointBatch, Guid> _batches;
    private readonly IRepository<HlGiftExchange, Guid> _gifts;
    private readonly IRepository<HlOrder, Guid> _orders;
    private readonly IHlAdminAppService _admin;
    private readonly ICurrentTenant _tenant;
    private readonly IAuthorizationService _authorization;

    public HlSalesExportAppService(IRepository<HlPointTransaction, Guid> transactions,
        IRepository<HlPointBatch, Guid> batches, IRepository<HlGiftExchange, Guid> gifts,
        IRepository<HlOrder, Guid> orders, IHlAdminAppService admin, ICurrentTenant tenant,
        IAuthorizationService authorization)
    {
        _transactions = transactions; _batches = batches; _gifts = gifts; _orders = orders;
        _admin = admin; _tenant = tenant; _authorization = authorization;
    }

    private async Task CheckPermissionAsync(string tenant, string host)
    {
        var permission = _tenant.Id.HasValue ? tenant : host;
        if (!(await _authorization.AuthorizeAsync(permission)).Succeeded)
            throw new Volo.Abp.Authorization.AbpAuthorizationException($"Permission denied: {permission}");
    }

    public async Task<IRemoteStreamContent> ExportPointHistoryAsync(HlPointHistoryFilter input, bool batches = false)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlLoyalty.Default, MultiTenancyPermissions.HostAppHlLoyalty.Default);
        using var workbook = new XLWorkbook();
        var sheet = CreateSheet(workbook, "Lịch sử điểm thưởng", new[] { "STT", "Mã giao dịch", "Mã khách hàng", "Tên khách hàng",
            "Số điện thoại", "Hạng thành viên", "Tên chiến dịch", "Mã voucher", "Tên voucher", "Giá trị", "Ngày giờ đổi thưởng" });
        var batchQuery = await _batches.GetQueryableAsync();
        var row = 2;
        if (batches)
        {
            var items = await AsyncExecuter.ToListAsync(HlSalesQuery.Batches(batchQuery, input)
                .OrderByDescending(x => x.CreationTime).ThenBy(x => x.Id));
            foreach (var b in items)
                WritePointRow(sheet, row++, b.BatchCode, b.CustomerCode, b.CustomerName, b.CustomerPhone,
                    b, b.ConvertedValue, b.ExchangedAt);
        }
        else
        {
            var query = HlSalesQuery.Transactions(await _transactions.GetQueryableAsync(), input);
            // Left join preserves Spend/Expire/Adjust transactions without a source batch.
            var items = await AsyncExecuter.ToListAsync(
                from t in query
                join b in batchQuery on t.BatchId equals (Guid?)b.Id into batchGroup
                from b in batchGroup.DefaultIfEmpty()
                orderby t.CreationTime descending, t.Id
                select new { Transaction = t, Batch = b });
            foreach (var item in items)
            {
                var t = item.Transaction;
                WritePointRow(sheet, row++, t.Type == HlPointTransactionType.Earn && item.Batch != null
                    ? item.Batch.BatchCode : t.RefCode ?? t.Id.ToString(), t.CustomerCode, t.CustomerName, t.CustomerPhone,
                    item.Batch, t.Value, t.CreationTime);
            }
        }
        sheet.Column(10).Style.NumberFormat.Format = "#,##0";
        sheet.Column(11).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
        return Finish(workbook, sheet, row, "HoaLinhSales_PointHistory");
    }

    private static void WritePointRow(IXLWorksheet sheet, int row, string? code, string? customerCode,
        string? name, string? phone, HlPointBatch? batch, decimal value, DateTime date)
    {
        sheet.Cell(row, 1).Value = row - 1;
        Text(sheet, row, 2, code); Text(sheet, row, 3, customerCode); Text(sheet, row, 4, name);
        Text(sheet, row, 5, phone); Text(sheet, row, 6, batch?.MembershipTier);
        Text(sheet, row, 7, batch?.CampaignName); Text(sheet, row, 8, batch?.VoucherCode);
        Text(sheet, row, 9, batch?.VoucherName); sheet.Cell(row, 10).Value = value;
        sheet.Cell(row, 11).Value = date;
    }

    public async Task<IRemoteStreamContent> ExportGiftExchangesAsync(HlGiftExchangeFilterDto input)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlGiftExchange.Default, MultiTenancyPermissions.HostAppHlGiftExchange.Default);
        var items = await AsyncExecuter.ToListAsync(HlSalesQuery.Gifts(await _gifts.GetQueryableAsync(), input)
            .OrderByDescending(x => x.CreationTime).ThenBy(x => x.Id));
        using var workbook = new XLWorkbook();
        var sheet = CreateSheet(workbook, "Lịch sử đổi quà", new[] { "STT", "Mã giao dịch", "Mã khách hàng", "Tên khách hàng",
            "Số điện thoại", "Mã quà tặng", "Tên quà tặng", "Số lượng", "Số tiền", "Trạng thái", "Mã giao dịch Urbox" });
        var row = 2;
        foreach (var item in items)
        {
            sheet.Cell(row, 1).Value = row - 1;
            Text(sheet, row, 2, item.ExchangeCode); Text(sheet, row, 3, item.CustomerCode);
            Text(sheet, row, 4, item.CustomerName); Text(sheet, row, 5, item.CustomerPhone);
            Text(sheet, row, 6, item.GiftCode); Text(sheet, row, 7, item.GiftName);
            sheet.Cell(row, 8).Value = item.Quantity;
            var amount = GetUrBoxAmount(item.UrBoxResponse);
            if (amount.HasValue) sheet.Cell(row, 9).Value = amount.Value;
            Text(sheet, row, 10, item.Status switch { HlGiftExchangeStatus.Success => "Thành công",
                HlGiftExchangeStatus.Failed => "Thất bại", HlGiftExchangeStatus.Processing => "Đang xử lý",
                HlGiftExchangeStatus.Used => "Đã sử dụng", _ => "Không xác định" });
            var match = Regex.Match(item.InternalNote ?? "", @"UrBox transaction_id=[^\s;]+", RegexOptions.IgnoreCase);
            Text(sheet, row, 11, match.Success ? match.Value : null);
            row++;
        }
        sheet.Column(9).Style.NumberFormat.Format = "#,##0";
        return Finish(workbook, sheet, row, "HoaLinhSales_GiftExchanges");
    }

    // Money comes from the saved UrBox response, never from the points balance.
    public static decimal? GetUrBoxAmount(string? response)
    {
        if (string.IsNullOrWhiteSpace(response)) return null;
        try
        {
            using var json = JsonDocument.Parse(response);
            if (json.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object
                && data.TryGetProperty("cart", out var cart) && cart.ValueKind == JsonValueKind.Object
                && cart.TryGetProperty("money_total", out var amount))
            {
                if (amount.ValueKind == JsonValueKind.Number && amount.TryGetDecimal(out var number)) return number;
                if (amount.ValueKind == JsonValueKind.String && decimal.TryParse(amount.GetString(),
                    NumberStyles.Number, CultureInfo.InvariantCulture, out number)) return number;
            }
        }
        catch (JsonException) { }
        return null;
    }

    public async Task<List<HlSalesOrderRow>> GetOrdersAsync(HlSalesOrderFilter input)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlOrders.Default, MultiTenancyPermissions.HostAppHlOrders.Default);
        HlSalesQuery.ValidateDates(input.DateFrom, input.DateTo);
        if (!string.IsNullOrEmpty(input.Source) && input.Source != "genora" && input.Source != "hoalinh")
            throw new UserFriendlyException("Nguồn đơn hàng không hợp lệ.");
        var rows = new List<HlSalesOrderRow>();
        if (input.Source != "genora")
        {
            // Follow upstream pagination instead of silently truncating at 500 orders.
            var page = 1;
            var loaded = 0;
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (true)
            {
                var result = await _admin.GetOrderHeadersAsync(page, 500);
                if (!result.Success || result.Data == null)
                    throw new UserFriendlyException("Không thể tải đầy đủ đơn hàng Hoa Linh DMS. Vui lòng thử lại.");
                var items = result.Data.Data;
                if (items.Count == 0 && ((result.Data.TotalRecords > loaded) || (result.Data.TotalPages > page)))
                    throw new UserFriendlyException("Hoa Linh DMS trả thiếu dữ liệu đơn hàng. Vui lòng thử lại.");
                foreach (var o in items)
                {
                    if (!string.IsNullOrWhiteSpace(o.OrderNumber) && !seen.Add(o.OrderNumber))
                        throw new UserFriendlyException("Hoa Linh DMS trả dữ liệu trùng giữa các trang. Vui lòng thử lại.");
                    rows.Add(new HlSalesOrderRow { Source = "hoalinh", Id = o.OrderNumber, OrderCode = o.OrderNumber,
                        CustomerCode = o.CustomerCode, CustomerName = o.CustomerName, TotalAmount = o.TotalAmount,
                        StatusCode = o.OrderStatusCode, StatusText = o.OrderStatus, OrderDate = ParseOrderDate(o.OrderDate), SalesName = o.DsrName });
                }
                loaded += items.Count;
                if (items.Count == 0 || (result.Data.TotalPages > 0 && page >= result.Data.TotalPages)
                    || (result.Data.TotalRecords > 0 && loaded >= result.Data.TotalRecords)
                    || (result.Data.TotalPages == 0 && result.Data.TotalRecords == 0 && items.Count < 500)) break;
                page++;
            }
        }
        if (input.Source != "hoalinh")
        {
            var orders = await AsyncExecuter.ToListAsync(await _orders.GetQueryableAsync());
            foreach (var o in orders)
                rows.Add(new HlSalesOrderRow { Source = "genora", Id = o.Id.ToString(), OrderCode = o.OrderCode,
                    CustomerCode = o.CustomerCode, CustomerName = o.CustomerName, CustomerPhone = o.CustomerPhone,
                    TotalAmount = o.TotalAmount, StatusCode = (int)o.DeliveryStatus, StatusText = DeliveryText((int)o.DeliveryStatus),
                    OrderDate = o.CreationTime, SalesName = o.ReceiverName, PaymentStatus = (int)o.PaymentStatus });
        }
        return FilterOrders(rows, input);
    }

    public static DateTime? ParseOrderDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (DateTime.TryParseExact(value, new[] { "dd/MM/yyyy", "d/M/yyyy", "dd/MM/yyyy HH:mm:ss" },
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)) return date;
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date) ? date : null;
    }

    public static List<HlSalesOrderRow> FilterOrders(IEnumerable<HlSalesOrderRow> rows, HlSalesOrderFilter input)
    {
        HlSalesQuery.ValidateDates(input.DateFrom, input.DateTo);
        if (!string.IsNullOrWhiteSpace(input.Source)) rows = rows.Where(x => x.Source == input.Source);
        if (!string.IsNullOrWhiteSpace(input.Search))
            rows = rows.Where(x => (x.OrderCode ?? "").Contains(input.Search, StringComparison.OrdinalIgnoreCase)
                || (x.CustomerName ?? "").Contains(input.Search, StringComparison.OrdinalIgnoreCase)
                || (x.SalesName ?? "").Contains(input.Search, StringComparison.OrdinalIgnoreCase));
        if (input.Status.HasValue) rows = rows.Where(x => x.StatusCode == input.Status.Value);
        if (input.DateFrom.HasValue) rows = rows.Where(x => x.OrderDate?.Date >= input.DateFrom.Value.Date);
        if (input.DateTo.HasValue) rows = rows.Where(x => x.OrderDate?.Date <= input.DateTo.Value.Date);
        return rows.OrderByDescending(x => x.OrderDate).ThenBy(x => x.OrderCode).ToList();
    }

    private static string DeliveryText(int status) => status switch
    { 1 => "Đơn mới", 2 => "Đang xử lý", 3 => "Đang giao", 4 => "Hoàn thành", 5 => "Đã hủy", _ => "Không xác định" };

    public async Task<IRemoteStreamContent> ExportOrdersAsync(HlSalesOrderFilter input)
    {
        var items = await GetOrdersAsync(input);
        using var workbook = new XLWorkbook();
        var sheet = CreateSheet(workbook, "Lịch sử đơn hàng", new[] { "STT", "Nguồn", "Mã đơn hàng", "Khách hàng",
            "Thành tiền", "Trạng thái", "Ngày đặt", "Nhân viên Sales" });
        var row = 2;
        foreach (var item in items)
        {
            sheet.Cell(row, 1).Value = row - 1;
            Text(sheet, row, 2, item.Source == "genora" ? "Genora" : "Hoa Linh");
            Text(sheet, row, 3, item.OrderCode); Text(sheet, row, 4, item.CustomerName);
            if (item.TotalAmount.HasValue) sheet.Cell(row, 5).Value = item.TotalAmount.Value;
            Text(sheet, row, 6, item.StatusText);
            if (item.OrderDate.HasValue) sheet.Cell(row, 7).Value = item.OrderDate.Value;
            Text(sheet, row, 8, item.SalesName); row++;
        }
        sheet.Column(5).Style.NumberFormat.Format = "#,##0";
        sheet.Column(7).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
        return Finish(workbook, sheet, row, "HoaLinhSales_Orders");
    }

    private static IXLWorksheet CreateSheet(XLWorkbook workbook, string name, string[] headers)
    {
        var sheet = workbook.Worksheets.Add(name);
        for (var i = 0; i < headers.Length; i++) sheet.Cell(1, i + 1).Value = headers[i];
        var header = sheet.Range(1, 1, 1, headers.Length);
        header.Style.Font.Bold = true; header.Style.Fill.BackgroundColor = XLColor.FromHtml("#DCE6F1");
        sheet.SheetView.FreezeRows(1);
        return sheet;
    }

    private static void Text(IXLWorksheet sheet, int row, int column, string? value)
    {
        var cell = sheet.Cell(row, column);
        cell.Style.NumberFormat.Format = "@";
        cell.Value = value ?? ""; // Preserve codes, leading zeroes and literal text.
    }

    private static IRemoteStreamContent Finish(XLWorkbook workbook, IXLWorksheet sheet, int nextRow, string name)
    {
        sheet.Range(1, 1, Math.Max(1, nextRow - 1), sheet.LastColumnUsed()!.ColumnNumber()).SetAutoFilter();
        sheet.Columns().AdjustToContents(10d, 60d);
        var stream = new MemoryStream();
        workbook.SaveAs(stream); stream.Position = 0;
        return new RemoteStreamContent(stream, $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}
