using Genora.MultiTenancy.Web.Pages.Hl25;
using System;
using System.IO;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hl25;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc.Validation;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Genora.MultiTenancy.Hl25;

public class Hl25GiftModalTests
{
    private static void SetUpValidation(Hl25GiftModalModelBase model)
    {
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<IModelStateValidator>().Returns(Substitute.For<IModelStateValidator>());
        model.LazyServiceProvider = lazy;
    }
    [Theory]
    [InlineData("1.000.000", "1000000")]
    [InlineData("1000000", "1000000")]
    [InlineData("1.000.000,50", "1000000.50")]
    [InlineData("0", "0")]
    [InlineData("9.999.999.999.999.999,99", "9999999999999999.99")]
    public void Vnd_Input_Is_Parsed_Without_Depending_On_Request_Culture(string input, string expected)
        => Hl25GiftModalModelBase.ParseVndValue(input).ShouldBe(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture));

    [Theory]
    [InlineData("1.2.3")]
    [InlineData("-1000")]
    [InlineData("1000000.00")]
    [InlineData("NaN")]
    [InlineData("10000000000000000")]
    public void Invalid_Or_Out_Of_Range_Value_Is_Rejected(string input)
        => Should.Throw<BusinessException>(() => Hl25GiftModalModelBase.ParseVndValue(input)).Code.ShouldBe("Hl25:InvalidGiftValue");

    [Fact]
    public void Empty_Value_Remains_Null() => Hl25GiftModalModelBase.ParseVndValue("").ShouldBeNull();

    [Fact]
    public async Task Edit_Loads_Both_Image_Urls_And_Vnd_Display()
    {
        var service = Substitute.For<IHl25GiftAppService>();
        var id = Guid.NewGuid();
        service.GetAsync(id).Returns(new Hl25GiftDto { Name = "Gift", ImageUrl = "/prize.png", WheelImageUrl = "/wheel.png", Value = 1000000m });
        var model = new GiftEditModalModel(service) { Id = id };
        await model.OnGetAsync();
        model.Gift.ImageUrl.ShouldBe("/prize.png");
        model.Gift.WheelImageUrl.ShouldBe("/wheel.png");
        model.GiftValue.ShouldBe("1.000.000");
    }

    [Fact]
    public async Task Create_With_Two_Files_Uploads_Each_To_Its_Own_Field()
    {
        var service = Substitute.For<IHl25GiftAppService>();
        service.UploadGiftImageAsync(Arg.Any<IRemoteStreamContent>())
            .Returns(call => "/uploads/" + call.Arg<IRemoteStreamContent>().FileName);
        using var first = new MemoryStream(new byte[] { 1 });
        using var second = new MemoryStream(new byte[] { 2 });
        var model = new GiftCreateModalModel(service)
        {
            Gift = new CreateUpdateHl25GiftDto { Name = "Gift" }, GiftValue = "1.000.000",
            GiftImageFile = new FormFile(first, 0, 1, "GiftImageFile", "prize.png") { Headers = new HeaderDictionary(), ContentType = "image/png" },
            WheelImageFile = new FormFile(second, 0, 1, "WheelImageFile", "wheel.png") { Headers = new HeaderDictionary(), ContentType = "image/png" }
        };
        SetUpValidation(model);
        await model.OnPostAsync();
        await service.Received().CreateAsync(Arg.Is<CreateUpdateHl25GiftDto>(x =>
            x.ImageUrl == "/uploads/prize.png" && x.WheelImageUrl == "/uploads/wheel.png" && x.Value == 1000000m));
    }

    [Fact]
    public async Task Pasted_Urls_Are_Saved_Without_Uploading_Files()
    {
        var service = Substitute.For<IHl25GiftAppService>();
        var model = new GiftCreateModalModel(service)
        {
            Gift = new CreateUpdateHl25GiftDto { Name = "Gift", ImageUrl = "https://example.test/prize.png", WheelImageUrl = "https://example.test/wheel.png" },
            GiftValue = "1.000.000"
        };
        SetUpValidation(model);
        await model.OnPostAsync();
        await service.DidNotReceive().UploadGiftImageAsync(Arg.Any<IRemoteStreamContent>());
        await service.Received().CreateAsync(Arg.Is<CreateUpdateHl25GiftDto>(x =>
            x.WheelImageUrl == "https://example.test/wheel.png" && x.ImageUrl == "https://example.test/prize.png"));
    }
}
