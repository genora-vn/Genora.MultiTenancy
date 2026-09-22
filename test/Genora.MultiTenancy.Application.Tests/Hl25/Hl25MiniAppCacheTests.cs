using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hl25.MiniApp;
using Genora.MultiTenancy.AppServices.Hl25;
using NSubstitute;
using Shouldly;
using Volo.Abp.Uow;
using Xunit;

namespace Genora.MultiTenancy.Hl25;

public class Hl25MiniAppCacheTests
{
    [Theory]
    [InlineData(Hl25CacheArea.Config)]
    [InlineData(Hl25CacheArea.FrameCampaigns)]
    [InlineData(Hl25CacheArea.FrameTemplates)]
    [InlineData(Hl25CacheArea.Gifts)]
    public async Task Seven_Thousand_Concurrent_Misses_Load_Once_Also_After_Expiry(Hl25CacheArea area)
    {
        var clock = new TestClock();
        using var cache = new Hl25MiniAppCache(clock);
        var tenant = Guid.NewGuid();
        var loads = 0;
        for (var wave = 1; wave <= 2; wave++)
        {
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            async Task<string> Load() { Interlocked.Increment(ref loads); await release.Task; return "catalog"; }
            var readers = Enumerable.Range(0, 7000).Select(_ => cache.GetAsync(area, tenant, Load)).ToArray();
            loads.ShouldBe(wave);
            release.SetResult();
            (await Task.WhenAll(readers)).ShouldAllBe(x => x == "catalog");
            loads.ShouldBe(wave);
            clock.Advance(TimeSpan.FromMinutes(21));
        }
    }

    [Fact]
    public async Task Host_And_Tenants_Are_Isolated_Including_Invalidation()
    {
        using var cache = new Hl25MiniAppCache();
        var a = Guid.NewGuid(); var b = Guid.NewGuid();
        await cache.GetAsync(Hl25CacheArea.Config, null, () => Task.FromResult("host"));
        await cache.GetAsync(Hl25CacheArea.Config, a, () => Task.FromResult("a"));
        await cache.GetAsync(Hl25CacheArea.Config, b, () => Task.FromResult("b"));
        await cache.InvalidateAsync(a, Hl25CacheArea.Config);
        (await cache.GetAsync(Hl25CacheArea.Config, a, () => Task.FromResult("new-a"))).ShouldBe("new-a");
        (await cache.GetAsync(Hl25CacheArea.Config, b, () => Task.FromResult("wrong"))).ShouldBe("b");
        (await cache.GetAsync(Hl25CacheArea.Config, null, () => Task.FromResult("wrong"))).ShouldBe("host");
    }

    [Fact]
    public async Task Template_Invalidation_Refreshes_All_And_Every_Campaign_Variant()
    {
        using var cache = new Hl25MiniAppCache();
        var tenant = Guid.NewGuid();
        var variants = new Guid?[] { null, Guid.NewGuid(), Guid.NewGuid() };
        foreach (var id in variants)
            await cache.GetAsync(Hl25CacheArea.FrameTemplates, tenant, () => Task.FromResult(id?.ToString() ?? "all"), id);
        foreach (var id in variants)
            (await cache.GetAsync(Hl25CacheArea.FrameTemplates, tenant, () => Task.FromResult("wrong"), id))
                .ShouldBe(id?.ToString() ?? "all");
        await cache.InvalidateAsync(tenant, Hl25CacheArea.FrameTemplates);
        foreach (var id in variants)
            (await cache.GetAsync(Hl25CacheArea.FrameTemplates, tenant, () => Task.FromResult("fresh"), id)).ShouldBe("fresh");
    }

    [Fact]
    public async Task Invalidation_Waits_For_An_Old_Fill_And_Does_Not_Leave_Stale_Data()
    {
        using var cache = new Hl25MiniAppCache();
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var oldRead = cache.GetAsync(Hl25CacheArea.Config, null, async () => { await release.Task; return "old"; });
        var invalidation = cache.InvalidateAsync(null, Hl25CacheArea.Config);
        invalidation.IsCompleted.ShouldBeFalse();
        release.SetResult();
        await Task.WhenAll(oldRead, invalidation);
        (await cache.GetAsync(Hl25CacheArea.Config, null, () => Task.FromResult("new"))).ShouldBe("new");
    }

    [Fact]
    public async Task Failed_Factory_Does_Not_Poison_Cache_Or_Keep_Lock()
    {
        using var cache = new Hl25MiniAppCache();
        await Should.ThrowAsync<InvalidOperationException>(() => cache.GetAsync<string>(Hl25CacheArea.Config, null,
            () => throw new InvalidOperationException("DB unavailable")));
        (await cache.GetAsync(Hl25CacheArea.Config, null, () => Task.FromResult("recovered"))).ShouldBe("recovered");
    }

    [Fact]
    public async Task Each_Caller_Receives_An_Independent_Dto()
    {
        using var cache = new Hl25MiniAppCache();
        var first = await cache.GetAsync(Hl25CacheArea.Config, null,
            () => Task.FromResult(new Hl25MiniAppConfigDto { ProgramName = "original" }));
        first.ProgramName = "mutated";
        var second = await cache.GetAsync<Hl25MiniAppConfigDto>(Hl25CacheArea.Config, null,
            () => throw new InvalidOperationException("Unexpected reload"));
        second.ProgramName.ShouldBe("original");
    }

    [Fact]
    public async Task Invalidation_Is_Deferred_Until_Commit_And_Captures_Tenant()
    {
        using var cache = new Hl25MiniAppCache();
        var tenant = Guid.NewGuid();
        var manager = Substitute.For<IUnitOfWorkManager>();
        var uow = Substitute.For<IUnitOfWork>();
        manager.Current.Returns(uow);
        var callbacks = new List<Func<Task>>();
        uow.When(x => x.OnCompleted(Arg.Any<Func<Task>>())).Do(x => callbacks.Add(x.Arg<Func<Task>>()));
        await cache.GetAsync(Hl25CacheArea.Config, tenant, () => Task.FromResult("old"));
        await new Hl25MiniAppCacheInvalidator(cache, manager).AfterCommitAsync(tenant, Hl25CacheArea.Config);
        // Rollback never invokes OnCompleted: the committed catalog remains valid.
        (await cache.GetAsync(Hl25CacheArea.Config, tenant, () => Task.FromResult("uncommitted"))).ShouldBe("old");
        callbacks.Count.ShouldBe(1);
        await callbacks.Single()();
        (await cache.GetAsync(Hl25CacheArea.Config, tenant, () => Task.FromResult("committed"))).ShouldBe("committed");
    }

    private sealed class TestClock : TimeProvider
    {
        private DateTimeOffset _now = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan elapsed) => _now += elapsed;
    }
}
