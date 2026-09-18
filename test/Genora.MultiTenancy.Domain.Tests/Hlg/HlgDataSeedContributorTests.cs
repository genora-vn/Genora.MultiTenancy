using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppHlg;
using Genora.MultiTenancy.DomainModels.AppHlg;
using NSubstitute;
using Shouldly;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Xunit;

namespace Genora.MultiTenancy.Hlg;

public class HlgDataSeedContributorTests
{
    private readonly IRepository<HlgGame, Guid> _games = Substitute.For<IRepository<HlgGame, Guid>>();
    private readonly IFeatureChecker _features = Substitute.For<IFeatureChecker>();
    private readonly ICurrentTenant _tenant = Substitute.For<ICurrentTenant>();
    private readonly HlgDataSeedContributor _seeder;

    public HlgDataSeedContributorTests()
    {
        _seeder = new HlgDataSeedContributor(Substitute.For<IRepository<HlgKnowledgeCategory, Guid>>(),
            Substitute.For<IRepository<HlgProduct, Guid>>(), _games, Substitute.For<IRepository<HlgQuestion, Guid>>(),
            Substitute.For<IRepository<HlgAnswerOption, Guid>>(), Substitute.For<IRepository<HlgReward, Guid>>(),
            Substitute.For<IRepository<HlgRankingEvent, Guid>>(), _features, Substitute.For<IGuidGenerator>(),
            Substitute.For<IClock>(), _tenant);
    }

    [Fact]
    public async Task Host_Does_Not_Check_Features_Or_Query_Hlg_Tables()
    {
        _features.IsEnabledAsync("Hlg.Management").Returns(Task.FromResult(true));
        _games.GetQueryableAsync().Returns<System.Linq.IQueryable<HlgGame>>(_ => throw new InvalidOperationException("Missing host HLG table"));

        await _seeder.SeedAsync(new DataSeedContext(null));

        _features.ReceivedCalls().ShouldBeEmpty();
        _games.ReceivedCalls().ShouldBeEmpty();
        _tenant.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public async Task Tenant_Without_Hlg_Does_Not_Query_Hlg_Tables()
    {
        var tenantId = Guid.NewGuid();
        _features.IsEnabledAsync("Hlg.Management").Returns(Task.FromResult(false));
        _games.GetQueryableAsync().Returns<System.Linq.IQueryable<HlgGame>>(_ => throw new InvalidOperationException("Missing tenant HLG table"));

        await _seeder.SeedAsync(new DataSeedContext(tenantId));

        _games.ReceivedCalls().ShouldBeEmpty();
        _tenant.Received(1).Change(tenantId);
    }

    [Fact]
    public async Task Enabled_Tenant_Queries_In_Its_Scope_And_Does_Not_Hide_Schema_Errors()
    {
        var tenantId = Guid.NewGuid();
        var inTenantScope = false;
        _tenant.Change(tenantId).Returns(_ => {
            inTenantScope = true;
            return new Scope(() => inTenantScope = false);
        });
        _features.IsEnabledAsync("Hlg.Management").Returns(_ => {
            inTenantScope.ShouldBeTrue();
            return Task.FromResult(true);
        });
        _games.GetQueryableAsync().Returns<System.Linq.IQueryable<HlgGame>>(_ => {
            inTenantScope.ShouldBeTrue();
            throw new InvalidOperationException("Missing enabled tenant HLG table");
        });

        var error = await Should.ThrowAsync<InvalidOperationException>(() => _seeder.SeedAsync(new DataSeedContext(tenantId)));
        error.Message.ShouldBe("Missing enabled tenant HLG table");
        inTenantScope.ShouldBeFalse();
    }

    private sealed class Scope : IDisposable
    {
        private readonly Action _dispose;
        public Scope(Action dispose) => _dispose = dispose;
        public void Dispose() => _dispose();
    }
}
