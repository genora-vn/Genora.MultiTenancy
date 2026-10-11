using System;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.HoaLinh.Blouse;
using Genora.MultiTenancy.DomainModels.AppHlBlouse;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Authorization;
using Xunit;

namespace Genora.MultiTenancy.HoaLinhSales;

public partial class HlSalesExportTests
{
    private HlBlouseRegistration Blouse(params (HlBlouseStyle Style, string Size, int Free, int Exchange)[] sizes)
    {
        var row = new HlBlouseRegistration(Guid.NewGuid(), "HLBL-TEST")
        {
            CreationTime = _day, CustomerCode = "001234", CustomerPhone = "0900000001",
            CustomerName = "Test pharmacy", Note = "Test note", InternalNote = "Internal note"
        };
        foreach (var size in sizes)
        {
            if (size.Free > 0) row.Items.Add(new HlBlouseRegistrationItem(Guid.NewGuid(), row.Id,
                HlBlouseItemType.Free, size.Style, size.Size, size.Free));
            if (size.Exchange > 0) row.Items.Add(new HlBlouseRegistrationItem(Guid.NewGuid(), row.Id,
                HlBlouseItemType.Exchange, size.Style, size.Size, size.Exchange, pointsPerItem: 150));
        }
        row.FreeQuantity = sizes.Sum(x => x.Free);
        row.ExchangeQuantity = sizes.Sum(x => x.Exchange);
        row.TotalQuantity = row.FreeQuantity + row.ExchangeQuantity;
        row.TotalPointsUsed = row.ExchangeQuantity * 150;
        return row;
    }

    private void SetBlouseRows(params HlBlouseRegistration[] rows)
        => _blouseRegistrations.WithDetailsAsync(Arg.Any<Expression<Func<HlBlouseRegistration, object>>[]>())
            .Returns(rows.AsQueryable());

    [Fact]
    public async Task Blouse_Export_Has_Reference_Headers_Filter_And_Native_Cell_Types()
    {
        SetBlouseRows(Blouse((HlBlouseStyle.Male, "M", 2, 1)));
        using var content = await _service.ExportBlouseRegistrationsAsync(new HlBlouseRegistrationFilterDto());
        content.FileName.ShouldStartWith("HoaLinhSales_BlouseRegistrations_");
        using var book = new XLWorkbook(content.GetStream());
        var sheet = book.Worksheet("Đăng ký áo Blouse");
        sheet.LastColumnUsed()!.ColumnNumber().ShouldBe(28);
        sheet.Cell("Q1").GetString().ShouldBe("Dáng Nam");
        sheet.Cell("U1").GetString().ShouldBe("Dáng Nữ");
        sheet.MergedRanges.Select(x => x.RangeAddress.ToString()).ShouldContain("Q1:T1");
        sheet.MergedRanges.Select(x => x.RangeAddress.ToString()).ShouldContain("U1:X1");
        sheet.MergedRanges.Select(x => x.RangeAddress.ToString()).ShouldContain("AB1:AB2");
        sheet.Range("Q2:X2").Cells().Select(x => x.GetString())
            .ShouldBe(new[] { "M", "L", "XL", "2XL", "M", "L", "XL", "2XL" });
        sheet.SheetView.SplitRow.ShouldBe(2);
        sheet.AutoFilter.Range.RangeAddress.ToString().ShouldBe("A2:AB4");
        sheet.Cell("A4").GetValue<int>().ShouldBe(1);
        sheet.Cell("C4").DataType.ShouldBe(XLDataType.Text);
        sheet.Cell("C4").GetString().ShouldBe("001234");
        sheet.Cell("E4").GetString().ShouldBe("0900000001");
        sheet.Cell("Q4").DataType.ShouldBe(XLDataType.Number);
        sheet.Cell("Q4").GetValue<int>().ShouldBe(3);
        sheet.Cell("R4").IsEmpty().ShouldBeTrue();
        sheet.Cell("O4").GetString().ShouldBe("Nam M x2");
        sheet.Cell("P4").GetString().ShouldBe("Nam M x1");
        sheet.Cell("Y4").GetString().ShouldBe("Chờ xác nhận");
        sheet.Cell("Z4").GetString().ShouldBe("Test note");
        sheet.Cell("AA4").GetString().ShouldBe("Internal note");
        sheet.Cell("AB4").DataType.ShouldBe(XLDataType.DateTime);
        sheet.Cell("AB4").GetDateTime().ShouldBe(_day);
    }

    [Fact]
    public async Task Blouse_Export_Matches_Sample_Rows_4_To_13_Using_Order_Details_For_Two_Misplaced_Examples()
    {
        var male = HlBlouseStyle.Male; var female = HlBlouseStyle.Female;
        // Only garment details from the user's example, no production customer identities.
        var rows = new[] {
            Blouse((female, "L", 2, 0)), // O4: female L => V, not manually entered U.
            Blouse((male, "M", 1, 0), (female, "M", 1, 0)),
            Blouse((male, "M", 1, 0)),
            Blouse((female, "M", 2, 0)), Blouse((female, "M", 2, 0)),
            Blouse((female, "M", 2, 0), (female, "L", 0, 1)),
            Blouse((female, "M", 2, 0)), Blouse((female, "M", 2, 0)),
            Blouse((male, "2XL", 2, 10)), // O12 + P12: male 2XL => T, not manually entered X.
            Blouse((female, "M", 2, 0))
        };
        for (var i = 0; i < rows.Length; i++) rows[i].CreationTime = _day.AddMinutes(-i);
        var expected = new[] {
            new[] { 0,0,0,0,0,2,0,0 }, new[] { 1,0,0,0,1,0,0,0 },
            new[] { 1,0,0,0,0,0,0,0 }, new[] { 0,0,0,0,2,0,0,0 },
            new[] { 0,0,0,0,2,0,0,0 }, new[] { 0,0,0,0,2,1,0,0 },
            new[] { 0,0,0,0,2,0,0,0 }, new[] { 0,0,0,0,2,0,0,0 },
            new[] { 0,0,0,12,0,0,0,0 }, new[] { 0,0,0,0,2,0,0,0 }
        };
        SetBlouseRows(rows);
        using var content = await _service.ExportBlouseRegistrationsAsync(new HlBlouseRegistrationFilterDto());
        using var book = new XLWorkbook(content.GetStream()); var sheet = book.Worksheet(1);
        for (var i = 0; i < rows.Length; i++)
        {
            var quantities = sheet.Range(i + 4, 17, i + 4, 24).Cells()
                .Select(c => c.IsEmpty() ? 0 : c.GetValue<int>()).ToArray();
            quantities.ShouldBe(expected[i]);
            quantities.Sum().ShouldBe(sheet.Cell(i + 4, 13).GetValue<int>());
        }
        sheet.AutoFilter.Range.RangeAddress.ToString().ShouldBe("A2:AB13");
    }

    [Fact]
    public async Task Blouse_Export_Separates_All_Eight_Style_Size_Buckets_And_Sums_Duplicate_Lines()
    {
        var row = Blouse((HlBlouseStyle.Male, " m ", 1, 2), (HlBlouseStyle.Male, "M", 2, 0),
            (HlBlouseStyle.Male, "L", 2, 0), (HlBlouseStyle.Male, "XL", 3, 0), (HlBlouseStyle.Male, "2XL", 0, 4),
            (HlBlouseStyle.Female, "M", 5, 0), (HlBlouseStyle.Female, "L", 0, 6),
            (HlBlouseStyle.Female, "xl", 7, 0), (HlBlouseStyle.Female, "2xl ", 0, 8));
        SetBlouseRows(row);
        using var content = await _service.ExportBlouseRegistrationsAsync(new HlBlouseRegistrationFilterDto());
        using var book = new XLWorkbook(content.GetStream()); var sheet = book.Worksheet(1);
        sheet.Range("Q4:X4").Cells().Select(c => c.GetValue<int>()).ShouldBe(new[] { 5,2,3,4,5,6,7,8 });
        sheet.Range("Q4:X4").Cells().All(c => c.DataType == XLDataType.Number).ShouldBeTrue();
    }

    [Fact]
    public async Task Blouse_Export_Uses_All_Filtered_Registrations_Ignores_Paging_And_Includes_End_Of_Day()
    {
        var first = Blouse((HlBlouseStyle.Female, "L", 1, 0));
        var last = Blouse((HlBlouseStyle.Male, "2XL", 1, 0)); last.CreationTime = _day.AddDays(1).AddTicks(-1);
        var nextDay = Blouse(); nextDay.CreationTime = _day.AddDays(1);
        var wrongStatus = Blouse(); wrongStatus.Status = HlBlouseRegistrationStatus.Cancelled;
        var wrongText = Blouse(); wrongText.CustomerName = "Unrelated";
        SetBlouseRows(first, last, nextDay, wrongStatus, wrongText);
        using var content = await _service.ExportBlouseRegistrationsAsync(new HlBlouseRegistrationFilterDto
        { Filter = "pharmacy", Status = HlBlouseRegistrationStatus.Pending, DateFrom = _day, DateTo = _day, SkipCount = 100, MaxResultCount = 1 });
        using var book = new XLWorkbook(content.GetStream()); var sheet = book.Worksheet(1);
        sheet.LastRowUsed()!.RowNumber().ShouldBe(5);
        sheet.Cell("T4").GetValue<int>().ShouldBe(1);
        sheet.Cell("V5").GetValue<int>().ShouldBe(1);
        sheet.Cell("A5").GetValue<int>().ShouldBe(2);
    }

    [Fact]
    public async Task Blouse_Export_Empty_Filter_Result_Still_Has_Both_Style_Groups_And_Filter()
    {
        SetBlouseRows();
        using var content = await _service.ExportBlouseRegistrationsAsync(new HlBlouseRegistrationFilterDto());
        using var book = new XLWorkbook(content.GetStream()); var sheet = book.Worksheet(1);
        sheet.Cell("Q1").GetString().ShouldBe("Dáng Nam");
        sheet.Cell("U1").GetString().ShouldBe("Dáng Nữ");
        sheet.AutoFilter.Range.RangeAddress.ToString().ShouldBe("A2:AB3");
        sheet.Cell("A4").IsEmpty().ShouldBeTrue();
    }

    [Fact]
    public async Task Blouse_Export_Rejects_Inverted_Date_Filter()
    {
        await Should.ThrowAsync<UserFriendlyException>(() => _service.ExportBlouseRegistrationsAsync(
            new HlBlouseRegistrationFilterDto { DateFrom = _day.AddDays(1), DateTo = _day }));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Blouse_Export_Enforces_Existing_Host_And_Tenant_Permission(bool tenant)
    {
        _tenant.Id.Returns(tenant ? Guid.NewGuid() : (Guid?)null);
        var permission = tenant ? MultiTenancyPermissions.AppHlBlouse.Default : MultiTenancyPermissions.HostAppHlBlouse.Default;
        _auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), permission).Returns(AuthorizationResult.Failed());
        await Should.ThrowAsync<AbpAuthorizationException>(() => _service.ExportBlouseRegistrationsAsync(new HlBlouseRegistrationFilterDto()));
    }
}
