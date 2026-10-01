using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.DomainModels.AppHlGiftReceipts;
using Genora.MultiTenancy.Features.AppHoaLinhFeatures;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Content;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Features;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

[Authorize]
public class HlGiftReceiptAdminAppService : ApplicationService, IHlGiftReceiptAdminAppService
{
    private readonly IHlGiftReceiptRepository _receipts;
    private readonly IFeatureChecker _features;
    private readonly IAuthorizationService _auth;

    public HlGiftReceiptAdminAppService(IHlGiftReceiptRepository receipts, IFeatureChecker features, IAuthorizationService auth)
    { _receipts = receipts; _features = features; _auth = auth; }

    private async Task CheckAsync(bool export = false)
    {
        if (CurrentTenant.Id.HasValue && (!await _features.IsEnabledAsync(AppHoaLinhFeatures.Management)
            || !await _features.IsEnabledAsync(AppHoaLinhFeatures.GiftReceipts)))
            throw new AbpAuthorizationException("Gift receipts feature is disabled.");
        var root = CurrentTenant.Id.HasValue ? MultiTenancyPermissions.AppHlGiftReceipts.Default : MultiTenancyPermissions.HostAppHlGiftReceipts.Default;
        if (!(await _auth.AuthorizeAsync(root)).Succeeded || (export && !(await _auth.AuthorizeAsync(root + ".Export")).Succeeded))
            throw new AbpAuthorizationException("Permission denied: " + root);
    }

    private async Task<IQueryable<HlGiftReceipt>> QueryAsync(HlGiftReceiptFilter input)
    {
        var tenantId = CurrentTenant.Id;
        return HlGiftReceiptQuery.Filter((await _receipts.GetQueryableAsync()).Where(x => x.TenantId == tenantId), input);
    }

    public async Task<PagedResultDto<HlGiftReceiptDto>> GetListAsync(HlGiftReceiptFilter input)
    {
        await CheckAsync();
        var query = await QueryAsync(input);
        var count = await AsyncExecuter.CountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.ConfirmedAt).ThenBy(x => x.Id)
            .Skip(Math.Max(0, input.SkipCount)).Take(Math.Clamp(input.MaxResultCount, 1, 100)));
        return new PagedResultDto<HlGiftReceiptDto>(count, rows.Select(HlGiftReceiptQuery.Map).ToList());
    }

    public async Task<HlGiftReceiptDto> GetAsync(Guid id)
    {
        await CheckAsync();
        var row = await AsyncExecuter.FirstOrDefaultAsync((await QueryAsync(new HlGiftReceiptFilter())).Where(x => x.Id == id));
        return row == null ? throw new EntityNotFoundException(typeof(HlGiftReceipt), id) : HlGiftReceiptQuery.Map(row);
    }

    public async Task<IRemoteStreamContent> ExportAsync(HlGiftReceiptFilter input)
    {
        await CheckAsync(export: true);
        var rows = await AsyncExecuter.ToListAsync((await QueryAsync(input)).OrderByDescending(x => x.ConfirmedAt).ThenBy(x => x.Id));
        using var book = new XLWorkbook();
        var sheet = book.AddWorksheet("Lịch sử nhận quà");
        var headers = new[] { "STT", "Mã xác nhận", "Mã KH / chi nhánh", "Tên khách hàng", "Số điện thoại", "Địa chỉ chi nhánh",
            "Mã chiến dịch", "Tên chiến dịch", "Kỳ chiến dịch", "Ngày bắt đầu", "Ngày kết thúc", "Mã quà", "Tên quà", "Loại voucher",
            "Giá trị voucher", "Số lượng", "Thời gian xác nhận nhận quà", "Trạng thái", "Hạng thành viên", "Doanh số tích lũy", "Điểm tích lũy",
            "Mã nhân viên", "Tên nhân viên", "Mã nhà phân phối", "Tên nhà phân phối", "Nguồn", "Ghi chú", "Thời gian tạo bản ghi" };
        for (var c = 0; c < headers.Length; c++) sheet.Cell(1, c + 1).Value = headers[c];
        var row = 2;
        foreach (var x in rows)
        {
            sheet.Cell(row, 1).Value = row - 1;
            Text(2, x.ReceiptCode); Text(3, x.CustCode); Text(4, x.CustName); Text(5, x.PhoneNumber); Text(6, x.Address);
            Text(7, x.CampaignCode); Text(8, x.CampaignName); sheet.Cell(row, 9).Value = x.CampaignPeriod;
            if (x.CampaignStartDate.HasValue) sheet.Cell(row, 10).Value = x.CampaignStartDate.Value;
            if (x.CampaignEndDate.HasValue) sheet.Cell(row, 11).Value = x.CampaignEndDate.Value;
            Text(12, x.VoucherCode); Text(13, x.VoucherName); sheet.Cell(row, 14).Value = x.VoucherType;
            sheet.Cell(row, 15).Value = x.VoucherValue; sheet.Cell(row, 16).Value = x.Quantity;
            sheet.Cell(row, 17).Value = x.ConfirmedAt; Text(18, "Đã xác nhận"); Text(19, x.MembershipTier);
            if (x.AccumulatedSales.HasValue) sheet.Cell(row, 20).Value = x.AccumulatedSales.Value;
            if (x.AccumulatedPoints.HasValue) sheet.Cell(row, 21).Value = x.AccumulatedPoints.Value;
            Text(22, x.DsrCode); Text(23, x.DsrName); Text(24, x.DistributorCode); Text(25, x.DistributorName);
            Text(26, x.Source); Text(27, x.Note); sheet.Cell(row, 28).Value = x.CreationTime;
            row++;
            // Explicit text avoids losing leading zeroes or interpreting upstream text as formulas.
            void Text(int column, string? value) => sheet.Cell(row, column).SetValue(value ?? "");
        }
        sheet.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;
        sheet.Range(1, 1, Math.Max(1, row - 1), headers.Length).SetAutoFilter();
        sheet.SheetView.FreezeRows(1);
        sheet.Columns().AdjustToContents(1, Math.Min(row, 200));
        foreach (var column in sheet.ColumnsUsed()) if (column.Width > 55) column.Width = 55;
        foreach (var column in new[] { 10, 11 }) sheet.Column(column).Style.DateFormat.Format = "dd/MM/yyyy";
        foreach (var column in new[] { 17, 28 }) sheet.Column(column).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
        foreach (var column in new[] { 15, 16, 20, 21 }) sheet.Column(column).Style.NumberFormat.Format = "#,##0";
        var stream = new MemoryStream(); book.SaveAs(stream); stream.Position = 0;
        return new RemoteStreamContent(stream, $"HoaLinh_GiftReceipts_{Clock.Now:yyyyMMdd_HHmmss}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}
