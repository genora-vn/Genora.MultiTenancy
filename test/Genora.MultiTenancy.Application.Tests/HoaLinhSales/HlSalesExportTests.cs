using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.AppServices.HoaLinh;
using Genora.MultiTenancy.DomainModels.AppHlPoints;
using Genora.MultiTenancy.DomainModels.AppHlGiftExchanges;
using Genora.MultiTenancy.DomainModels.AppHlOrders;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Genora.MultiTenancy.HoaLinhSales;

public class HlSalesExportTests : IDisposable
{
    private readonly DateTime _day = new(2026, 9, 17);
    private readonly IRepository<HlPointTransaction, Guid> _transactions = Substitute.For<IRepository<HlPointTransaction, Guid>>();
    private readonly IRepository<HlPointBatch, Guid> _batches = Substitute.For<IRepository<HlPointBatch, Guid>>();
    private readonly IRepository<HlGiftExchange, Guid> _gifts = Substitute.For<IRepository<HlGiftExchange, Guid>>();
    private readonly IRepository<HlOrder, Guid> _orders = Substitute.For<IRepository<HlOrder, Guid>>();
    private readonly IHlAdminAppService _admin = Substitute.For<IHlAdminAppService>();
    private readonly ICurrentTenant _tenant = Substitute.For<ICurrentTenant>();
    private readonly IAbpAuthorizationService _auth = Substitute.For<IAbpAuthorizationService>();
    private readonly ServiceProvider _provider;
    private readonly HlSalesExportAppService _service;

    public HlSalesExportTests()
    {
        _transactions.GetQueryableAsync().Returns(Task.FromResult(Array.Empty<HlPointTransaction>().AsQueryable()));
        _batches.GetQueryableAsync().Returns(Task.FromResult(Array.Empty<HlPointBatch>().AsQueryable()));
        _gifts.GetQueryableAsync().Returns(Task.FromResult(Array.Empty<HlGiftExchange>().AsQueryable()));
        _orders.GetQueryableAsync().Returns(Task.FromResult(Array.Empty<HlOrder>().AsQueryable()));
        _auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Any<string>()).Returns(AuthorizationResult.Success());
        _provider = new ServiceCollection()
            .AddSingleton<IAsyncQueryableExecuter>(new AsyncQueryableExecuter(Array.Empty<IAsyncQueryableProvider>()))
            .BuildServiceProvider();
        _service = new HlSalesExportAppService(_transactions, _batches, _gifts, _orders, _admin, _tenant, _auth)
        { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
    }

    private HlPointBatch Batch(DateTime time) => new(Guid.NewGuid(), "PB2609170001")
    { CustomerCode = "C79N1005283", CustomerName = "Good Pharma", CustomerPhone = "0903008200", MembershipTier = "Vàng",
        CampaignName = "Thăng hạng tri ân", CampaignCode = "CD1", VoucherCode = "600K", VoucherName = "Sáu trăm nghìn đồng",
        ConvertedValue = 600000, ExchangedAt = time, CreationTime = time };
    private HlPointTransaction Transaction(DateTime time, HlPointBatch? batch = null) => new(Guid.NewGuid())
    { CustomerCode = "C79N1005283", CustomerName = "Good Pharma", CustomerPhone = "0903008200", BatchId = batch?.Id,
        RefCode = batch?.BatchCode ?? "UB-1", Type = batch == null ? HlPointTransactionType.Spend : HlPointTransactionType.Earn,
        Value = batch == null ? -500000 : 600000, CreationTime = time };

    [Fact]
    public void SameDay_Includes_Late_Transactions_And_Excludes_Next_Midnight()
    {
        var items = new[] { Transaction(_day.AddTicks(-1)), Transaction(_day), Transaction(_day.AddDays(1).AddTicks(-1)), Transaction(_day.AddDays(1)) };
        HlSalesQuery.Transactions(items.AsQueryable(), new HlPointHistoryFilter { DateFrom = _day, DateTo = _day }).Count().ShouldBe(2);
        HlSalesQuery.Transactions(items.AsQueryable(), new HlPointHistoryFilter { DateFrom = _day, DateTo = _day, Type = 1 }).Count().ShouldBe(0);
    }

    [Fact]
    public void BatchDates_Use_Exchange_Time_And_Search_Includes_Phone()
    {
        var batch = Batch(_day.AddHours(23)); batch.CreationTime = _day.AddDays(-1);
        var rows = new[] { batch, Batch(_day.AddDays(1)) }.AsQueryable();
        HlSalesQuery.Batches(rows, new HlPointHistoryFilter { Search = "0903", DateFrom = _day, DateTo = _day }).Single().ShouldBe(batch);
    }

    [Fact]
    public void Reversed_Dates_Are_Rejected_ServerSide()
    {
        Should.Throw<UserFriendlyException>(() => HlSalesQuery.Transactions(Array.Empty<HlPointTransaction>().AsQueryable(),
            new HlPointHistoryFilter { DateFrom = _day.AddDays(1), DateTo = _day }));
        Should.Throw<UserFriendlyException>(() => HlSalesQuery.Gifts(Array.Empty<HlGiftExchange>().AsQueryable(),
            new HlGiftExchangeFilterDto { DateFrom = _day.AddDays(1), DateTo = _day }));
        Should.Throw<UserFriendlyException>(() => HlSalesExportAppService.FilterOrders(Array.Empty<HlSalesOrderRow>(),
            new HlSalesOrderFilter { DateFrom = _day.AddDays(1), DateTo = _day }));
    }

    [Fact]
    public async Task PointExcel_Preserves_Codes_Phone_And_BatchMetadata_Without_Paging()
    {
        var batch = Batch(_day.AddHours(13));
        _batches.GetQueryableAsync().Returns(Task.FromResult(new[] { batch }.AsQueryable()));
        var txn = Transaction(_day.AddHours(13), batch);
        _transactions.GetQueryableAsync().Returns(Task.FromResult(new[] { txn, Transaction(_day.AddHours(14)), Transaction(_day.AddDays(1)) }.AsQueryable()));
        using var content = await _service.ExportPointHistoryAsync(new HlPointHistoryFilter { Search = "Good", DateFrom = _day, DateTo = _day, Limit = 1, Page = 9 });
        using var book = new XLWorkbook(content.GetStream()); var sheet = book.Worksheet(1);
        sheet.LastRowUsed()!.RowNumber().ShouldBe(3);
        sheet.Cell(1, 11).GetString().ShouldBe("Ngày giờ đổi thưởng");
        sheet.Cell(3, 2).GetString().ShouldBe("PB2609170001");
        sheet.Cell(3, 5).GetString().ShouldBe("0903008200");
        sheet.Cell(3, 5).DataType.ShouldBe(XLDataType.Text);
        sheet.Cell(3, 6).GetString().ShouldBe("Vàng");
        sheet.Cell(3, 8).GetString().ShouldBe("600K");
        sheet.Cell(3, 10).GetValue<decimal>().ShouldBe(600000m);
        sheet.Cell(3, 10).GetFormattedString(System.Globalization.CultureInfo.InvariantCulture).ShouldBe("600,000");
        sheet.Cell(2, 10).GetValue<decimal>().ShouldBe(-500000m);
        sheet.Cell(3, 11).GetDateTime().ShouldBe(txn.CreationTime);
    }

    [Fact]
    public async Task BatchExcel_Exports_Only_Filtered_Batches()
    {
        _batches.GetQueryableAsync().Returns(Task.FromResult(new[] { Batch(_day.AddHours(23)), Batch(_day.AddDays(1)) }.AsQueryable()));
        using var content = await _service.ExportPointHistoryAsync(new HlPointHistoryFilter { DateFrom = _day, DateTo = _day }, batches: true);
        using var book = new XLWorkbook(content.GetStream());
        book.Worksheet(1).LastRowUsed()!.RowNumber().ShouldBe(2);
        book.Worksheet(1).Cell(2, 9).GetString().ShouldBe("Sáu trăm nghìn đồng");
    }

    [Fact]
    public async Task GiftExcel_Uses_ResponseMoney_Not_Points_And_Applies_All_Filters()
    {
        var gift = new HlGiftExchange(Guid.NewGuid(), "UB-260911F044", "Bách Hóa Xanh", 1)
        { CustomerCode = "C7998026868", CustomerPhone = "0971082552", GiftCode = "13060", Quantity = 2, TotalPointsUsed = 7,
            CreationTime = _day.AddHours(23), Status = HlGiftExchangeStatus.Success,
            InternalNote = "UrBox transaction_id=c4c8692004f4400f841b9301e6bc455e",
            UrBoxResponse = "{\"data\":{\"cart\":{\"money_total\":500000}}}" };
        var next = new HlGiftExchange(Guid.NewGuid(), "UB-next", "Bách Hóa Xanh", 1)
        { CreationTime = _day.AddDays(1), Status = HlGiftExchangeStatus.Success };
        _gifts.GetQueryableAsync().Returns(Task.FromResult(new[] { gift, next }.AsQueryable()));
        using var content = await _service.ExportGiftExchangesAsync(new HlGiftExchangeFilterDto
        { Filter = "0971", Status = HlGiftExchangeStatus.Success, DateFrom = _day, DateTo = _day, SkipCount = 50, MaxResultCount = 1 });
        using var book = new XLWorkbook(content.GetStream()); var sheet = book.Worksheet(1);
        sheet.LastRowUsed()!.RowNumber().ShouldBe(2);
        sheet.Cell(2, 5).GetString().ShouldBe("0971082552");
        sheet.Cell(2, 8).GetValue<decimal>().ShouldBe(2m);
        sheet.Cell(2, 9).GetValue<decimal>().ShouldBe(500000m);
        sheet.Cell(2, 9).GetFormattedString(System.Globalization.CultureInfo.InvariantCulture).ShouldBe("500,000");
        sheet.Cell(2, 10).GetString().ShouldBe("Thành công");
        sheet.Cell(2, 11).GetString().ShouldBe(gift.InternalNote);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("bad JSON", null)]
    [InlineData("{\"data\":null}", null)]
    [InlineData("{\"data\":{\"cart\":{\"money_total\":\"500000\"}}}", 500000)]
    public void MoneyParser_Tolerates_Legacy_Responses(string? json, int? amount)
        => HlSalesExportAppService.GetUrBoxAmount(json).ShouldBe(amount.HasValue ? (decimal?)amount.Value : null);

    [Fact]
    public async Task Orders_Read_All_Upstream_Pages_And_Excel_Uses_Same_Filter()
    {
        _admin.GetOrderHeadersAsync(1, 500).Returns(Task.FromResult(HlApiResult<HlPagedResponse<HlOrderHeaderDto>>.Ok(
            new HlPagedResponse<HlOrderHeaderDto> { TotalPages = 2, TotalRecords = 2, Data = new()
                { new() { OrderNumber = "DMS-1", CustomerName = "Ngọc", OrderStatusCode = 1, OrderDate = "16/09/2026" } } })));
        _admin.GetOrderHeadersAsync(2, 500).Returns(Task.FromResult(HlApiResult<HlPagedResponse<HlOrderHeaderDto>>.Ok(
            new HlPagedResponse<HlOrderHeaderDto> { TotalPages = 2, TotalRecords = 2, Data = new()
                { new() { OrderNumber = "DMS-2", CustomerName = "Ngọc", OrderStatusCode = 1, OrderDate = "17/09/2026", TotalAmount = 900000 } } })));
        var filter = new HlSalesOrderFilter { Search = "ngọc", Source = "hoalinh", Status = 1, DateFrom = _day, DateTo = _day };
        var rows = await _service.GetOrdersAsync(filter);
        rows.Single().OrderCode.ShouldBe("DMS-2");
        using var content = await _service.ExportOrdersAsync(filter);
        using var book = new XLWorkbook(content.GetStream());
        book.Worksheet(1).LastRowUsed()!.RowNumber().ShouldBe(2);
        book.Worksheet(1).Cell(2, 3).GetString().ShouldBe(rows.Single().OrderCode);
        book.Worksheet(1).Cell(2, 5).GetValue<decimal>().ShouldBe(900000m);
        book.Worksheet(1).Cell(2, 5).GetFormattedString(System.Globalization.CultureInfo.InvariantCulture).ShouldBe("900,000");
    }

    [Fact]
    public async Task Orders_Fail_Instead_Of_Exporting_Incomplete_Dms_Data()
    {
        _admin.GetOrderHeadersAsync(1, 500).Returns(Task.FromResult(HlApiResult<HlPagedResponse<HlOrderHeaderDto>>.Fail("offline")));
        await Should.ThrowAsync<UserFriendlyException>(() => _service.ExportOrdersAsync(new HlSalesOrderFilter()));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Export_Checks_Tenant_Or_Host_Permission(bool tenant)
    {
        _tenant.Id.Returns(tenant ? Guid.NewGuid() : (Guid?)null);
        var permission = tenant ? MultiTenancyPermissions.AppHlLoyalty.Default : MultiTenancyPermissions.HostAppHlLoyalty.Default;
        _auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), permission).Returns(AuthorizationResult.Failed());
        await Should.ThrowAsync<AbpAuthorizationException>(() => _service.ExportPointHistoryAsync(new HlPointHistoryFilter()));
    }

    public void Dispose() => _provider.Dispose();
}
