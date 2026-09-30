using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.HoaLinh.Blouse;
using Genora.MultiTenancy.AppServices.HoaLinh;
using Genora.MultiTenancy.DomainModels.AppHlBlouse;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Genora.MultiTenancy.HoaLinhSales;

public class HlBlouseCampaignTests : IDisposable
{
    private readonly List<HlBlouseCampaign> _rows = new();
    private readonly IRepository<HlBlouseCampaign, Guid> _campaigns = Substitute.For<IRepository<HlBlouseCampaign, Guid>>();
    private readonly ICurrentTenant _tenant = Substitute.For<ICurrentTenant>();
    private readonly IAbpAuthorizationService _auth = Substitute.For<IAbpAuthorizationService>();
    private readonly ServiceProvider _provider;
    private readonly HlBlouseAdminAppService _service;

    public HlBlouseCampaignTests()
    {
        _tenant.Id.Returns(Guid.NewGuid());
        _auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Any<string>()).Returns(AuthorizationResult.Success());
        // Mirror repository tenant filtering without a live database.
        _campaigns.GetQueryableAsync().Returns(_ => _rows.Where(x => x.TenantId == _tenant.Id).AsQueryable());
        _campaigns.InsertAsync(Arg.Any<HlBlouseCampaign>(), true, Arg.Any<CancellationToken>())
            .Returns(c => { var row = c.Arg<HlBlouseCampaign>(); _rows.Add(row); return row; });
        _campaigns.UpdateAsync(Arg.Any<HlBlouseCampaign>(), true, Arg.Any<CancellationToken>()).Returns(c => c.Arg<HlBlouseCampaign>());
        var guids = Substitute.For<IGuidGenerator>();
        guids.Create().Returns(_ => Guid.NewGuid());
        _provider = new ServiceCollection().AddSingleton(guids)
            .AddSingleton<IAsyncQueryableExecuter>(new AsyncQueryableExecuter(Array.Empty<IAsyncQueryableProvider>()))
            .BuildServiceProvider();
        _service = new HlBlouseAdminAppService(
            Substitute.For<IRepository<HlBlouseRegistration, Guid>>(), _campaigns,
            Substitute.For<IRepository<HlBlouseSize, Guid>>(), _tenant, _auth)
        { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
    }

    private static HlBlouseCampaignSaveDto Input() => new()
    {
        ProgramName = " Chương trình áo Blouse ", IntroductionHtml = "<p>Thông tin mới</p>",
        FreeShirtLimit = 3, PointsPerShirt = 175, MaxExchangeShirt = 4,
        SizeChartImageUrl = "/uploads/hl-blouse/banner.png",
        StartTime = new DateTime(2026, 9, 30, 8, 37, 0), EndTime = new DateTime(2026, 10, 30, 23, 59, 0), IsActive = true
    };

    [Fact]
    public async Task Create_Update_And_Read_Preserve_All_Fields_And_Local_DateTimes()
    {
        var input = Input();
        var created = await _service.SaveCampaignAsync(input);
        _rows.Single().TenantId.ShouldBe(_tenant.Id);
        input.ProgramName = "Cấu hình đã sửa";
        input.StartTime = new DateTime(2026, 10, 1, 0, 1, 0);
        input.EndTime = new DateTime(2026, 10, 31, 23, 58, 0);
        input.FreeShirtLimit = 0; input.PointsPerShirt = 0; input.MaxExchangeShirt = 0;
        input.IsActive = false;
        await _service.SaveCampaignAsync(input);
        var read = (await _service.GetCampaignAsync())!;
        read.Id.ShouldBe(created.Id);
        read.ProgramName.ShouldBe(input.ProgramName);
        read.IntroductionHtml.ShouldBe(input.IntroductionHtml);
        read.FreeShirtLimit.ShouldBe(0); read.PointsPerShirt.ShouldBe(0); read.MaxExchangeShirt.ShouldBe(0);
        read.SizeChartImageUrl.ShouldBe(input.SizeChartImageUrl);
        read.StartTime.ShouldBe(input.StartTime); read.EndTime.ShouldBe(input.EndTime);
        read.StartTime!.Value.Kind.ShouldBe(DateTimeKind.Unspecified);
        read.IsActive.ShouldBeFalse();
        _rows.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Read_And_Save_Select_The_Same_Active_Campaign_When_Newer_Inactive_Row_Exists()
    {
        var active = new HlBlouseCampaign(Guid.NewGuid(), "Active", _tenant.Id) { CreationTime = new DateTime(2026, 9, 29) };
        var inactive = new HlBlouseCampaign(Guid.NewGuid(), "Inactive", _tenant.Id) { IsActive = false, CreationTime = new DateTime(2026, 9, 30) };
        var otherTenant = new HlBlouseCampaign(Guid.NewGuid(), "Other tenant", Guid.NewGuid()) { CreationTime = new DateTime(2026, 10, 1) };
        _rows.AddRange(new[] { active, inactive, otherTenant });
        (await _service.GetCampaignAsync())!.Id.ShouldBe(active.Id);
        var saved = await _service.SaveCampaignAsync(Input());
        saved.Id.ShouldBe(active.Id);
        (await _service.GetCampaignAsync())!.StartTime.ShouldBe(Input().StartTime);
        inactive.ProgramName.ShouldBe("Inactive"); otherTenant.ProgramName.ShouldBe("Other tenant");
    }

    [Fact]
    public async Task Dates_And_Optional_Content_Can_Be_Cleared()
    {
        await _service.SaveCampaignAsync(Input());
        var input = Input();
        input.StartTime = null; input.EndTime = null;
        input.SizeChartImageUrl = null; input.IntroductionHtml = null;
        await _service.SaveCampaignAsync(input);
        var read = (await _service.GetCampaignAsync())!;
        read.StartTime.ShouldBeNull(); read.EndTime.ShouldBeNull();
        read.SizeChartImageUrl.ShouldBeNull(); read.IntroductionHtml.ShouldBeNull();
    }

    [Fact]
    public async Task Reversed_Range_Is_Rejected_Before_Changing_Stored_Configuration()
    {
        await _service.SaveCampaignAsync(Input());
        var invalid = Input(); invalid.EndTime = invalid.StartTime!.Value.AddMinutes(-1);
        await Should.ThrowAsync<UserFriendlyException>(() => _service.SaveCampaignAsync(invalid));
        (await _service.GetCampaignAsync())!.EndTime.ShouldBe(Input().EndTime);
    }

    [Fact]
    public async Task Equal_And_One_Sided_Date_Limits_Are_Allowed()
    {
        var input = Input(); input.EndTime = input.StartTime;
        await _service.SaveCampaignAsync(input);
        input.StartTime = null;
        await _service.SaveCampaignAsync(input);
        input.StartTime = Input().StartTime; input.EndTime = null;
        await _service.SaveCampaignAsync(input);
        (await _service.GetCampaignAsync())!.StartTime.ShouldBe(input.StartTime);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Save_Requires_The_Correct_Host_Or_Tenant_Permission(bool host)
    {
        if (host) _tenant.Id.Returns((Guid?)null);
        _auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Any<string>()).Returns(AuthorizationResult.Failed());
        await Should.ThrowAsync<AbpAuthorizationException>(() => _service.SaveCampaignAsync(Input()));
        await _auth.Received().AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(),
            host ? MultiTenancyPermissions.HostAppHlBlouse.Edit : MultiTenancyPermissions.AppHlBlouse.Edit);
        _rows.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("ProgramName", "")]
    [InlineData("ProgramName", "too-long")]
    [InlineData("SizeChartImageUrl", "too-long")]
    [InlineData("FreeShirtLimit", "-1")]
    [InlineData("PointsPerShirt", "-1")]
    [InlineData("MaxExchangeShirt", "-1")]
    public void Dto_Rejects_Invalid_Fields_Before_Database_Save(string property, string value)
    {
        var input = Input();
        var prop = typeof(HlBlouseCampaignSaveDto).GetProperty(property)!;
        prop.SetValue(input, prop.PropertyType == typeof(int) ? int.Parse(value)
            : value == "too-long" ? new string('a', property == "ProgramName" ? 251 : 501) : value);
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), errors, true).ShouldBeFalse();
        errors.ShouldContain(e => e.MemberNames.Contains(property));
    }

    public void Dispose() => _provider.Dispose();
}
