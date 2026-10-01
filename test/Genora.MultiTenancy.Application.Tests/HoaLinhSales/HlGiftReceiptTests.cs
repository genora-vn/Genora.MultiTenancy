using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.AppServices.HoaLinh;
using Genora.MultiTenancy.DomainModels.AppHlGiftReceipts;
using Genora.MultiTenancy.Features.AppHoaLinhFeatures;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Features;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Xunit;

namespace Genora.MultiTenancy.HoaLinhSales;

public class HlGiftReceiptTests : IDisposable
{
    private readonly IHlGiftReceiptRepository _repo = Substitute.For<IHlGiftReceiptRepository>();
    private readonly IHlApiClientService _dms = Substitute.For<IHlApiClientService>();
    private readonly IFeatureChecker _features = Substitute.For<IFeatureChecker>();
    private readonly ICurrentTenant _tenant = Substitute.For<ICurrentTenant>();
    private readonly IAbpAuthorizationService _auth = Substitute.For<IAbpAuthorizationService>();
    private readonly IUnitOfWorkManager _uow = Substitute.For<IUnitOfWorkManager>();
    private readonly IUnitOfWork _transaction = Substitute.For<IUnitOfWork>();
    private readonly List<HlGiftReceipt> _rows = new();
    private readonly List<HlCampaignDto> _campaigns = new();
    private readonly ServiceProvider _provider;
    private readonly DateTime _now = new(2026, 10, 1, 9, 15, 0);
    private readonly MiniAppHlGiftReceiptService _mini;
    private readonly HlGiftReceiptAdminAppService _admin;
    private readonly HlCustomerDto _branch = new()
    {
        CustCode = "CUST01", CustName = "Chi nhánh 01", Address = "Địa chỉ từ DMS",
        DsrCode = "NV01", DsrName = "Nhân viên", DistributorCode = "NPP01", DistributorName = "Nhà phân phối"
    };

    public HlGiftReceiptTests()
    {
        _tenant.Id.Returns(Guid.NewGuid()); _tenant.IsAvailable.Returns(true);
        _features.IsEnabledAsync(Arg.Any<string>()).Returns(true);
        _auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Any<string>()).Returns(AuthorizationResult.Success());
        _repo.GetQueryableAsync().Returns(_ => Task.FromResult(_rows.AsQueryable())); // unfiltered on purpose: service must enforce tenant.
        _repo.InsertAsync(Arg.Any<HlGiftReceipt>(), true, Arg.Any<CancellationToken>()).Returns(c =>
        { var row = c.Arg<HlGiftReceipt>(); row.CreationTime = _now; _rows.Add(row); return Task.FromResult(row); });
        _repo.FindForConfirmationAsync(Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(c => Task.FromResult(_rows.SingleOrDefault(x => x.TenantId == c.ArgAt<Guid?>(0) && x.CustCode == c.ArgAt<string>(1)
                && x.CampaignCode == c.ArgAt<string>(2) && x.CampaignPeriod == c.ArgAt<int>(3) && x.VoucherCode == c.ArgAt<string>(4))));
        _dms.GetCustomerDetailAsync(Arg.Any<string>()).Returns(_ => HlApiResult<List<HlCustomerDto>>.Ok(new() { _branch }));
        _campaigns.Add(Campaign("QT34")); _campaigns.Add(Campaign("QTHOA"));
        _dms.GetCampaignDetailAsync(Arg.Any<string>()).Returns(_ => HlApiResult<List<HlCampaignDto>>.Ok(_campaigns));
        _uow.Begin(Arg.Any<AbpUnitOfWorkOptions>(), Arg.Any<bool>()).Returns(_transaction);
        var clock = Substitute.For<IClock>(); clock.Now.Returns(_now);
        _provider = new ServiceCollection().AddSingleton(_tenant).AddSingleton(clock)
            .AddSingleton<IGuidGenerator>(SimpleGuidGenerator.Instance)
            .AddSingleton<IAsyncQueryableExecuter>(new AsyncQueryableExecuter(Array.Empty<IAsyncQueryableProvider>())).BuildServiceProvider();
        _mini = new MiniAppHlGiftReceiptService(_repo, _dms, _features, _uow) { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
        _admin = new HlGiftReceiptAdminAppService(_repo, _features, _auth) { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
    }

    private static HlCampaignDto Campaign(string voucher) => new()
    {
        CustCode = "CUST01", CampaignCode = "GIFT25NAM", CampaignName = "Tặng quà 25 năm", CampaignPeriod = 1,
        StartDate = "2026-09-25", EndDate = "2026-09-30", VoucherCode = voucher, VoucherName = "Quà " + voucher,
        VoucherType = 2, VoucherValue = 1, MembershipTier = "Vàng", AccumulatedPoints = 315, AccumulatedSales = 31540863m
    };
    private static HlConfirmGiftInput Input(string gift = "QT34") => new()
    { PhoneNumber = "0900000001", CustCode = "CUST01", CampaignCode = "GIFT25NAM", CampaignPeriod = 1, VoucherCode = gift, Note = "Nhận tại chi nhánh" };

    [Fact]
    public async Task Confirms_Two_Different_Gifts_In_One_Campaign_With_Dms_Snapshots()
    {
        var first = await _mini.ConfirmAsync(Input());
        var second = await _mini.ConfirmAsync(Input("QTHOA"));
        _rows.Count.ShouldBe(2); first.Id.ShouldNotBe(second.Id);
        first.CustName.ShouldBe(_branch.CustName); first.Address.ShouldBe(_branch.Address);
        first.DsrCode.ShouldBe("NV01"); first.DistributorCode.ShouldBe("NPP01");
        first.VoucherType.ShouldBe(2); first.Quantity.ShouldBe(1); first.IsConfirmed.ShouldBeTrue();
        first.ConfirmedAt.ShouldBe(_now); first.CreationTime.ShouldBe(_now);
        first.CampaignEndDate.ShouldBe(new DateTime(2026, 9, 30)); // Reporting period is not an invented claim deadline.
        first.MembershipTier.ShouldBe("Vàng"); first.AccumulatedSales.ShouldBe(31540863m);
        await _transaction.Received(2).CompleteAsync(Arg.Any<CancellationToken>());
        _uow.Received(2).Begin(Arg.Is<AbpUnitOfWorkOptions>(o => o.IsTransactional == true), true);
    }

    [Fact]
    public async Task Retry_Returns_Original_Receipt_Without_Reapplying_Changes_Or_Refetching_Campaign()
    {
        var first = await _mini.ConfirmAsync(Input());
        _campaigns.Clear(); _branch.CustName = "New DMS name";
        var retry = Input(); retry.Note = "Different note";
        var result = await _mini.ConfirmAsync(retry);
        result.Id.ShouldBe(first.Id); result.CustName.ShouldBe(first.CustName); result.Note.ShouldBe(first.Note);
        _rows.Count.ShouldBe(1);
        await _dms.Received(1).GetCampaignDetailAsync("CUST01");
    }

    // Also used by local HTTP regression tests: real service, isolated in-memory repository and fake DMS.
    public IMiniAppHlGiftReceiptService MiniAppService => _mini;
    public void UseHost()
    {
        _tenant.Id.Returns((Guid?)null); _tenant.IsAvailable.Returns(false);
        _features.IsEnabledAsync(Arg.Any<string>()).Returns(false);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Racing_Request_Uses_Receipt_Found_Under_Transaction_Lock(bool host)
    {
        if (host) UseHost();
        var winner = new HlGiftReceipt(Guid.NewGuid(), _tenant.Id) { CustCode = "CUST01", VoucherCode = "QT34" };
        _repo.FindForConfirmationAsync(Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(winner);
        var result = await _mini.ConfirmAsync(Input());
        result.Id.ShouldBe(winner.Id);
        await _repo.DidNotReceive().InsertAsync(Arg.Any<HlGiftReceipt>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _transaction.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("+84900000001")]
    [InlineData("84900000001")]
    [InlineData("0900000001")]
    public async Task Phone_And_Code_Normalization_Prevents_Duplicate_Claims(string phone)
    {
        var first = await _mini.ConfirmAsync(Input());
        var input = Input(); input.PhoneNumber = phone; input.CustCode = " cust01 "; input.CampaignCode = "gift25nam"; input.VoucherCode = "qt34";
        (await _mini.ConfirmAsync(input)).Id.ShouldBe(first.Id);
        _rows.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Different_Phone_For_Same_Dms_Branch_Does_Not_Claim_Same_Entitlement_Twice()
    {
        var first = await _mini.ConfirmAsync(Input());
        var input = Input(); input.PhoneNumber = "0900000002";
        (await _mini.ConfirmAsync(input)).Id.ShouldBe(first.Id); _rows.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Different_Period_And_Tenant_Have_Independent_Entitlements()
    {
        await _mini.ConfirmAsync(Input());
        _campaigns[0].CampaignPeriod = 2;
        var input = Input(); input.CampaignPeriod = 2;
        await _mini.ConfirmAsync(input);
        _tenant.Id.Returns(Guid.NewGuid());
        await _mini.ConfirmAsync(input); _rows.Count.ShouldBe(3);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public async Task Rejects_Non_Gift_Voucher_Types(int type)
    {
        _campaigns[0].VoucherType = type;
        (await Should.ThrowAsync<UserFriendlyException>(() => _mini.ConfirmAsync(Input()))).Code.ShouldBe("HlGiftReceipt:InvalidVoucherType");
        _rows.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1.5)]
    public async Task Rejects_Invalid_Quantities(double quantity)
    {
        _campaigns[0].VoucherValue = (decimal)quantity;
        (await Should.ThrowAsync<UserFriendlyException>(() => _mini.ConfirmAsync(Input()))).Code.ShouldBe("HlGiftReceipt:InvalidQuantity");
        _rows.ShouldBeEmpty();
    }

    [Fact]
    public async Task Rejects_Phone_Not_Associated_With_Selected_Branch()
    {
        _branch.CustCode = "OTHER";
        (await Should.ThrowAsync<UserFriendlyException>(() => _mini.ConfirmAsync(Input()))).Code.ShouldBe("HlGiftReceipt:BranchNotFound");
        _rows.ShouldBeEmpty(); await _dms.DidNotReceive().GetCampaignDetailAsync(Arg.Any<string>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Rejects_Missing_Or_Ambiguous_Dms_Entitlement(bool duplicate)
    {
        if (duplicate) _campaigns.Add(Campaign("QT34")); else _campaigns.Clear();
        (await Should.ThrowAsync<UserFriendlyException>(() => _mini.ConfirmAsync(Input()))).Code.ShouldBe("HlGiftReceipt:GiftNotFound");
        _rows.ShouldBeEmpty();
    }

    [Fact]
    public async Task Dms_Error_Does_Not_Create_A_Receipt()
    {
        _dms.GetCampaignDetailAsync(Arg.Any<string>()).Returns(HlApiResult<List<HlCampaignDto>>.Fail("offline"));
        (await Should.ThrowAsync<UserFriendlyException>(() => _mini.ConfirmAsync(Input()))).Code.ShouldBe("HlGiftReceipt:DmsUnavailable");
        _rows.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(AppHoaLinhFeatures.Management)]
    [InlineData(AppHoaLinhFeatures.GiftReceipts)]
    public async Task Disabled_Tenant_Feature_Does_Not_Reach_Dms_Or_Storage(string feature)
    {
        _features.IsEnabledAsync(feature).Returns(false);
        (await Should.ThrowAsync<UserFriendlyException>(() => _mini.ConfirmAsync(Input()))).Code.ShouldBe("HlGiftReceipt:Disabled");
        (await Should.ThrowAsync<UserFriendlyException>(() => _mini.GetHistoryAsync(new()
            { PhoneNumber = "0900000001", CustCode = "CUST01" }))).Code.ShouldBe("HlGiftReceipt:Disabled");
        await _dms.DidNotReceive().GetCustomerDetailAsync(Arg.Any<string>()); _rows.ShouldBeEmpty();
    }

    [Fact]
    public async Task Host_Can_Confirm_Retry_View_And_Export_Its_Own_History_Without_Tenant_Features()
    {
        var tenantId = _tenant.Id;
        var tenantReceipt = await _mini.ConfirmAsync(Input());
        UseHost(); _features.ClearReceivedCalls();
        var hostReceipt = await _mini.ConfirmAsync(Input());
        _rows.Single(x => x.Id == hostReceipt.Id).TenantId.ShouldBeNull();
        hostReceipt.Id.ShouldNotBe(tenantReceipt.Id); hostReceipt.IsConfirmed.ShouldBeTrue();
        (await _mini.ConfirmAsync(Input())).Id.ShouldBe(hostReceipt.Id);
        _rows.Count.ShouldBe(2); // Same entitlement in Host and Tenant is independent; retry isn't a third row.
        var history = await _mini.GetHistoryAsync(new() { PhoneNumber = "0900000001", CustCode = "CUST01" });
        history.TotalCount.ShouldBe(1); history.Items.Single().Id.ShouldBe(hostReceipt.Id);
        (await _admin.GetListAsync(new())).Items.Single().Id.ShouldBe(hostReceipt.Id);
        (await _admin.GetAsync(hostReceipt.Id)).Id.ShouldBe(hostReceipt.Id);
        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(() => _admin.GetAsync(tenantReceipt.Id));
        using var export = await _admin.ExportAsync(new());
        using var book = new XLWorkbook(export.GetStream());
        book.Worksheet(1).LastRowUsed()!.RowNumber().ShouldBe(2);
        book.Worksheet(1).Cell(2, 2).GetString().ShouldBe(hostReceipt.ReceiptCode);
        await _auth.Received().AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), MultiTenancyPermissions.HostAppHlGiftReceipts.Default);
        await _auth.Received().AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), MultiTenancyPermissions.HostAppHlGiftReceipts.Export);
        await _features.DidNotReceive().IsEnabledAsync(Arg.Any<string>());
        await _repo.Received().FindForConfirmationAsync(null, "CUST01", "GIFT25NAM", 1, "QT34", Arg.Any<CancellationToken>());

        _tenant.Id.Returns(tenantId); _tenant.IsAvailable.Returns(true);
        _features.IsEnabledAsync(Arg.Any<string>()).Returns(true);
        (await _mini.GetHistoryAsync(new() { PhoneNumber = "0900000001", CustCode = "CUST01" })).Items.Single().Id.ShouldBe(tenantReceipt.Id);
        (await _admin.GetListAsync(new())).Items.Single().Id.ShouldBe(tenantReceipt.Id);
        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(() => _admin.GetAsync(hostReceipt.Id));
    }

    [Fact]
    public async Task History_Filters_Phone_Branch_Tenant_And_Supports_Paging()
    {
        await _mini.ConfirmAsync(Input()); await _mini.ConfirmAsync(Input("QTHOA"));
        _rows.Add(new HlGiftReceipt(Guid.NewGuid(), Guid.NewGuid()) { CustCode = "CUST01", PhoneNumber = "0900000001" });
        var history = await _mini.GetHistoryAsync(new() { PhoneNumber = "+84900000001", CustCode = "cust01", MaxResultCount = 1 });
        history.TotalCount.ShouldBe(2); history.Items.Count.ShouldBe(1);
        (await _mini.GetHistoryAsync(new() { PhoneNumber = "0900000002", CustCode = "CUST01" })).TotalCount.ShouldBe(0);
        (await _mini.GetHistoryAsync(new() { PhoneNumber = "0900000001", CustCode = "OTHER" })).TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Admin_List_And_Excel_Use_Identical_Filters_But_Export_All_Matching_Rows()
    {
        _branch.CustName = "=1+1"; // Must stay text in Excel, not a formula.
        var first = await _mini.ConfirmAsync(Input()); await _mini.ConfirmAsync(Input("QTHOA"));
        _rows[0].ConfirmedAt = _now.Date.AddHours(23).AddMinutes(59);
        _rows[1].ConfirmedAt = _now.Date.AddDays(1);
        _rows.Add(new HlGiftReceipt(Guid.NewGuid(), Guid.NewGuid()) { CustCode = "CUST01", ConfirmedAt = _now });
        var filter = new HlGiftReceiptFilter { DateFrom = _now.Date, DateTo = _now.Date, CustCode = "CUST01", CampaignCode = "GIFT25NAM", CampaignPeriod = 1, VoucherCode = "QT34", PhoneNumber = "0900000001", MaxResultCount = 1, SkipCount = 20 };
        var list = await _admin.GetListAsync(filter); list.TotalCount.ShouldBe(1); list.Items.ShouldBeEmpty();
        using var file = await _admin.ExportAsync(filter);
        using var book = new XLWorkbook(file.GetStream()); var sheet = book.Worksheet(1);
        sheet.LastRowUsed()!.RowNumber().ShouldBe(2);
        sheet.Cell(2, 2).GetString().ShouldBe(first.ReceiptCode);
        sheet.Cell(2, 4).GetString().ShouldBe("=1+1"); sheet.Cell(2, 4).HasFormula.ShouldBeFalse();
        sheet.Cell(2, 5).GetString().ShouldBe("0900000001"); sheet.Cell(2, 5).DataType.ShouldBe(XLDataType.Text);
        sheet.Cell(2, 6).GetString().ShouldBe(_branch.Address);
        sheet.Cell(2, 17).GetDateTime().ShouldBe(_rows[0].ConfirmedAt);
        sheet.Cell(2, 18).GetString().ShouldBe("Đã xác nhận");
        sheet.Cell(2, 22).GetString().ShouldBe("NV01");
        sheet.Cell(2, 20).GetValue<decimal>().ShouldBe(31540863m);
        sheet.Cell(2, 20).GetFormattedString(System.Globalization.CultureInfo.GetCultureInfo("en-US")).ShouldBe("31,540,863");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task List_Permission_Alone_Does_Not_Allow_Export(bool host)
    {
        if (host) UseHost();
        _auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), host
            ? MultiTenancyPermissions.HostAppHlGiftReceipts.Export : MultiTenancyPermissions.AppHlGiftReceipts.Export).Returns(AuthorizationResult.Failed());
        (await _admin.GetListAsync(new())).TotalCount.ShouldBe(0);
        await Should.ThrowAsync<AbpAuthorizationException>(() => _admin.ExportAsync(new()));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Admin_Requires_Specific_Host_Or_Tenant_Permission(bool host)
    {
        if (host) _tenant.Id.Returns((Guid?)null);
        _auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Any<string>()).Returns(AuthorizationResult.Failed());
        await Should.ThrowAsync<AbpAuthorizationException>(() => _admin.GetListAsync(new()));
        await _auth.Received().AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), host
            ? MultiTenancyPermissions.HostAppHlGiftReceipts.Default : MultiTenancyPermissions.AppHlGiftReceipts.Default);
    }

    [Fact]
    public async Task Admin_Rejects_Reversed_Dates_And_Cross_Tenant_Detail()
    {
        var result = await _mini.ConfirmAsync(Input());
        await Should.ThrowAsync<UserFriendlyException>(() => _admin.ExportAsync(new() { DateFrom = _now.AddDays(1), DateTo = _now }));
        _tenant.Id.Returns(Guid.NewGuid());
        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(() => _admin.GetAsync(result.Id));
    }

    public void Dispose() => _provider.Dispose();
}
