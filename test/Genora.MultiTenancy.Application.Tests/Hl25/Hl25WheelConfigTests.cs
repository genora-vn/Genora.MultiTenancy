using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.AppServices.Hl25;
using Genora.MultiTenancy.DomainModels.AppHl25;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace Genora.MultiTenancy.Hl25;

public class Hl25WheelConfigTests : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly Hl25WheelConfigAppService _service;
    private readonly IRepository<Hl25WheelSlot, Guid> _slots = Substitute.For<IRepository<Hl25WheelSlot, Guid>>();
    private readonly Hl25WheelSlot _slot;
    private readonly Hl25WheelConfig _config = new(Guid.NewGuid());

    public Hl25WheelConfigTests()
    {
        _slot = new Hl25WheelSlot(Guid.NewGuid(), _config.Id) { WinRate = 100 };
        var configs = Substitute.For<IRepository<Hl25WheelConfig, Guid>>();
        configs.GetQueryableAsync().Returns(Task.FromResult(new[] { _config }.AsQueryable()));
        _slots.GetQueryableAsync().Returns(Task.FromResult(new[] { _slot }.AsQueryable()));
        var auth = Substitute.For<IAbpAuthorizationService>();
        auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Any<string>()).Returns(AuthorizationResult.Success());
        var mapper = Substitute.For<IObjectMapper>();
        mapper.Map<Hl25WheelConfig, Hl25WheelConfigDto>(Arg.Any<Hl25WheelConfig>()).Returns(new Hl25WheelConfigDto());
        mapper.Map<List<Hl25WheelSlot>, List<Hl25WheelSlotDto>>(Arg.Any<List<Hl25WheelSlot>>()).Returns(new List<Hl25WheelSlotDto>());
        _provider = new ServiceCollection()
            .AddSingleton<IAsyncQueryableExecuter>(new AsyncQueryableExecuter(Array.Empty<IAsyncQueryableProvider>()))
            .AddSingleton(Substitute.For<ICurrentTenant>())
            .AddSingleton<IAuthorizationService>(auth)
            .AddSingleton(mapper)
            .BuildServiceProvider();
        _service = new Hl25WheelConfigAppService(configs, _slots, Substitute.For<IManageImageService>(), Substitute.For<IFeatureChecker>())
        { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
    }

    [Fact]
    public async Task Saving_Existing_Slot_Preserves_Identity_And_Assets()
    {
        var id = _slot.Id;
        await _service.UpdateAsync(new CreateUpdateHl25WheelConfigDto
        {
            BackgroundImageUrl = "/background.png", PointerImageUrl = "/pointer.png",
            Slots = new() { new() { Id = id, Label = "Updated", SlotImageUrl = "/gift.png", ColorHex = "#ffffff", WinRate = 100 } }
        });
        _slot.Id.ShouldBe(id);
        _slot.Label.ShouldBe("Updated");
        _slot.SlotImageUrl.ShouldBe("/gift.png");
        _slot.ColorHex.ShouldBe("#ffffff");
        _config.BackgroundImageUrl.ShouldBe("/background.png");
        _config.PointerImageUrl.ShouldBe("/pointer.png");
        await _slots.Received(1).UpdateAsync(_slot, true, Arg.Any<CancellationToken>());
        await _slots.DidNotReceive().DeleteManyAsync(Arg.Any<IEnumerable<Hl25WheelSlot>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Slot_From_Another_Wheel_Cannot_Be_Updated()
    {
        var error = await Should.ThrowAsync<BusinessException>(() => _service.UpdateAsync(new CreateUpdateHl25WheelConfigDto
        { Slots = new() { new() { Id = Guid.NewGuid(), WinRate = 100 } } }));
        error.Code.ShouldBe("Hl25:InvalidWheelSlot");
    }

    [Fact]
    public async Task Duplicate_Slot_Ids_Cannot_Be_Saved()
    {
        var error = await Should.ThrowAsync<BusinessException>(() => _service.UpdateAsync(new CreateUpdateHl25WheelConfigDto
        { Slots = new() { new() { Id = _slot.Id, WinRate = 50 }, new() { Id = _slot.Id, WinRate = 50 } } }));
        error.Code.ShouldBe("Hl25:InvalidWheelSlot");
    }

    [Fact]
    public async Task Negative_Rate_Cannot_Be_Hidden_By_A_Total_Of_100()
    {
        var error = await Should.ThrowAsync<BusinessException>(() => _service.UpdateAsync(new CreateUpdateHl25WheelConfigDto
        { Slots = new() { new() { WinRate = -5 }, new() { WinRate = 105 } } }));
        error.Code.ShouldBe("Hl25:WheelWinRateInvalid");
    }

    public void Dispose() => _provider.Dispose();
}
