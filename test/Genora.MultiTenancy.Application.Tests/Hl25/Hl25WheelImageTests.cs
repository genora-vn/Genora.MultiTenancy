using System;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.AppServices.Hl25;
using Genora.MultiTenancy.DomainModels.AppHl25;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Uow;
using Xunit;

namespace Genora.MultiTenancy.Hl25;

public class Hl25WheelImageTests
{
    [Theory]
    [InlineData("/wheel.png", null, "/wheel.png")]
    [InlineData(null, null, "/prize.png")]
    [InlineData("/wheel.png", "/slot.png", "/slot.png")]
    public async Task Wheel_Response_Separates_Prize_And_Wheel_Images_And_Preserves_Slot_Override(
        string? wheelImage, string? slotImage, string expectedSlotImage)
    {
        var config = new Hl25WheelConfig(Guid.NewGuid());
        var gift = new Hl25Gift(Guid.NewGuid(), "Gift") { ImageUrl = "/prize.png", WheelImageUrl = wheelImage };
        var slot = new Hl25WheelSlot(Guid.NewGuid(), config.Id) { GiftId = gift.Id, SlotImageUrl = slotImage };
        var configs = Substitute.For<IRepository<Hl25WheelConfig, Guid>>();
        configs.GetQueryableAsync().Returns(Task.FromResult(new[] { config }.AsQueryable()));
        var slots = Substitute.For<IRepository<Hl25WheelSlot, Guid>>();
        slots.GetQueryableAsync().Returns(Task.FromResult(new[] { slot }.AsQueryable()));
        var gifts = Substitute.For<IRepository<Hl25Gift, Guid>>();
        gifts.GetQueryableAsync().Returns(Task.FromResult(new[] { gift }.AsQueryable()));
        var participants = Substitute.For<IRepository<Hl25Participant, Guid>>();
        participants.GetQueryableAsync().Returns(Task.FromResult(Array.Empty<Hl25Participant>().AsQueryable()));
        var request = Substitute.For<HttpRequest>();
        request.Scheme.Returns("https");
        request.Host.Returns(new HostString("localhost", 44374));
        var http = Substitute.For<HttpContext>();
        http.Request.Returns(request);
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(http);
        using var provider = new ServiceCollection()
            .AddSingleton<IAsyncQueryableExecuter>(new AsyncQueryableExecuter(Array.Empty<IAsyncQueryableProvider>()))
            .BuildServiceProvider();
        var service = new MiniAppHl25Service(Substitute.For<IRepository<Hl25AppConfig, Guid>>(), participants,
            Substitute.For<IRepository<Hl25FrameCampaign, Guid>>(), Substitute.For<IRepository<Hl25FrameTemplate, Guid>>(),
            Substitute.For<IRepository<Hl25FrameCreation, Guid>>(), Substitute.For<IRepository<Hl25SpinTurnLog, Guid>>(),
            configs, slots, gifts, Substitute.For<IRepository<Hl25SpinLog, Guid>>(),
            Substitute.For<IUnitOfWorkManager>(), Substitute.For<IManageImageService>(), accessor, new ConfigurationBuilder().Build())
        { LazyServiceProvider = new AbpLazyServiceProvider(provider) };

        var response = await service.GetWheelAsync("test-user");
        var result = response.Slots.Single();
        result.WheelImageUrl.ShouldBe(wheelImage == null ? null : "https://localhost:44374" + wheelImage);
        result.GiftImageUrl.ShouldBe("https://localhost:44374/prize.png");
        result.SlotImageUrl.ShouldBe("https://localhost:44374" + expectedSlotImage);
    }
}
