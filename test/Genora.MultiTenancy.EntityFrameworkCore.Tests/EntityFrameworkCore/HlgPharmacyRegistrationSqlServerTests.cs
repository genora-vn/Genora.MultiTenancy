using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.AppServices.HoaLinh;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.Migrations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Auditing;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;
using Xunit;

namespace Genora.MultiTenancy.EntityFrameworkCore;

public class HlgPharmacyRegistrationModelTests
{
    [Fact]
    public void Migration_Only_Adds_Two_Nullable_Hlg_Columns_And_A_Nonunique_Index()
    {
        var operations = new AddHlgPharmacyRegistration().UpOperations;
        operations.Count.ShouldBe(3);
        var columns = operations.OfType<AddColumnOperation>().ToList();
        columns.Count.ShouldBe(2);
        columns.All(x => x.IsNullable && x.Schema == "HLG" && x.Table == "AppHlgUserProfiles").ShouldBeTrue();
        columns.Select(x => x.Name).OrderBy(x => x).ShouldBe(new[] { "DmsCustomerCode", "PharmaPhone" });
        operations.OfType<CreateIndexOperation>().Single().IsUnique.ShouldBeFalse();
    }

    [Fact]
    public void SqlServer_Model_Preserves_Customer_Code_Uniqueness_And_Legacy_PharmacyCode()
    {
        using var db = new MultiTenancyDbContext(new DbContextOptionsBuilder<MultiTenancyDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=HlgModelOnly;Integrated Security=True").Options);
        var model = db.GetService<IDesignTimeModel>().Model;
        var profile = model.FindEntityType(typeof(HlgUserProfile))!;
        profile.FindProperty("PharmaPhone")!.IsNullable.ShouldBeTrue();
        profile.FindProperty("DmsCustomerCode")!.IsNullable.ShouldBeTrue();
        profile.FindProperty("PharmacyCode")!.GetMaxLength().ShouldBe(100);
        var index = model.FindEntityType(typeof(Customer))!.GetIndexes()
            .Single(x => string.Join(",", x.Properties.Select(p => p.Name)) == "TenantId,CustomerCode");
        index.IsUnique.ShouldBeTrue();
        index.GetFilter().ShouldBe("[IsActive] = 1 AND [CustomerCode] IS NOT NULL");
    }
}

// Opt-in: creates and removes ONLY a unique disposable LocalDB database. No appsettings is loaded.
public sealed class HlgLocalDbFactAttribute : FactAttribute
{
    public HlgLocalDbFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("HLG_REGISTRATION_LOCALDB_TESTS") != "1")
            Skip = "Set HLG_REGISTRATION_LOCALDB_TESTS=1 to test transactions on disposable SQL Server LocalDB.";
    }
}

[DependsOn(typeof(MultiTenancyApplicationModule), typeof(MultiTenancyEntityFrameworkCoreModule), typeof(AbpAutofacModule))]
public class HlgRegistrationSqlServerTestModule : AbpModule
{
    internal static readonly string DatabaseName = "HlgRegistrationTest_" + Guid.NewGuid().ToString("N");
    internal static readonly string Connection = $"Server=(localdb)\\MSSQLLocalDB;Database={DatabaseName};Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=False";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundJobOptions>(o => o.IsJobExecutionEnabled = false);
        Configure<AbpAuditingOptions>(o => o.IsEnabled = false);
        Configure<AbpBackgroundWorkerOptions>(o => o.IsEnabled = false);
        Configure<FeatureManagementOptions>(o => { o.SaveStaticFeaturesToDatabase = false; o.IsDynamicFeatureStoreEnabled = false; });
        Configure<PermissionManagementOptions>(o => { o.SaveStaticPermissionsToDatabase = false; o.IsDynamicPermissionStoreEnabled = false; });
        Configure<AbpDbContextOptions>(o => o.Configure(c => c.DbContextOptions.UseSqlServer(Connection)));
        var resolver = Substitute.For<IConnectionStringResolver>();
        resolver.ResolveAsync(Arg.Any<string>()).Returns(Connection);
        context.Services.Replace(ServiceDescriptor.Singleton(resolver));
        var dms = Substitute.For<IHlApiClientService>();
        dms.GetCustomerByPhoneAsync(Arg.Any<string>()).Returns(c => HlApiResult<List<HlCustomerDto>>.Ok(new()
        {
            new HlCustomerDto { CustCode = "DMS-" + c.Arg<string>(), CustPhone = c.Arg<string>(), Phone = c.Arg<string>(), IsCustomer = true }
        }));
        context.Services.Replace(ServiceDescriptor.Singleton(dms));
    }
}

public class HlgPharmacyRegistrationSqlServerTests : IAsyncLifetime
{
    private IAbpApplicationWithInternalServiceProvider? _app;
    private const string Owner = "0900000001";

    public async Task InitializeAsync()
    {
        if (Environment.GetEnvironmentVariable("HLG_REGISTRATION_LOCALDB_TESTS") != "1") return;
        using var db = NewDb();
        // Build only Customer/Profile and their FK dependencies from the production SQL model.
        // Unrelated Salon cascade paths make full-model EnsureCreated unsuitable on SQL Server.
        await db.GetService<IRelationalDatabaseCreator>().CreateAsync();
        var model = db.GetService<IDesignTimeModel>().Model;
        var operations = db.GetService<IMigrationsModelDiffer>().GetDifferences(null, model.GetRelationalModel());
        var tables = new HashSet<string> { "AppCustomers", "AppHlgUserProfiles" };
        bool changed;
        do
        {
            changed = false;
            foreach (var table in operations.OfType<CreateTableOperation>().Where(x => tables.Contains(x.Name)))
                foreach (var fk in table.ForeignKeys) changed |= tables.Add(fk.PrincipalTable);
        } while (changed);
        var selected = operations.Where(x => x is EnsureSchemaOperation
            || x is CreateTableOperation table && tables.Contains(table.Name)
            || x is CreateIndexOperation index && tables.Contains(index.Table)
                && index.Name != "IX_AppHlgUserProfiles_TenantId_PharmaPhone").ToList();
        var profileTable = selected.OfType<CreateTableOperation>().Single(x => x.Name == "AppHlgUserProfiles");
        profileTable.Columns.RemoveAll(x => x.Name is "PharmaPhone" or "DmsCustomerCode");
        var generator = db.GetService<IMigrationsSqlGenerator>();
        foreach (var command in generator.Generate(selected, model))
            await db.Database.ExecuteSqlRawAsync(command.CommandText);
        // Apply the actual new migration to the pre-change table, without running older seeders.
        foreach (var command in generator.Generate(new AddHlgPharmacyRegistration().UpOperations, model))
            await db.Database.ExecuteSqlRawAsync(command.CommandText);
        _app = await AbpApplicationFactory.CreateAsync<HlgRegistrationSqlServerTestModule>(o => o.UseAutofac());
        await _app.InitializeAsync();
    }

    private static MultiTenancyDbContext NewDb() => new(new DbContextOptionsBuilder<MultiTenancyDbContext>()
        .UseSqlServer(HlgRegistrationSqlServerTestModule.Connection).Options);

    private async Task<GamificationUserDto> Register(Guid? tenant, string phone, string owner = Owner)
    {
        using var scope = _app!.ServiceProvider.CreateScope();
        using var change = scope.ServiceProvider.GetRequiredService<ICurrentTenant>().Change(tenant);
        return await scope.ServiceProvider.GetRequiredService<IHlgProfileAppService>().UpsertCustomerAsync(new()
        { Phone = phone, PharmaPhone = owner, CustomerCode = "DMS-" + owner, FullName = "SQL test", CustomerType = "pharmacy" });
    }

    [HlgLocalDbFact]
    public async Task Concurrent_Registrations_Commit_Only_Five_Accounts_Per_Pharmacy_For_Host_And_Tenant()
    {
        foreach (var tenant in new Guid?[] { null, Guid.NewGuid() })
        {
            await Register(tenant, Owner);
            var results = await Task.WhenAll(Enumerable.Range(2, 12).Select(async n =>
            {
                try { await Register(tenant, "090000" + n.ToString("D4")); return "ok"; }
                catch (UserFriendlyException ex) { return ex.Code; }
            }));
            results.Count(x => x == "ok").ShouldBe(4);
            results.Count(x => x == "HlgRegistration:AccountLimitReached").ShouldBe(8);
            using var db = NewDb();
            var count = await Scalar(db, "SELECT COUNT(*) FROM HLG.AppHlgUserProfiles WHERE PharmaPhone=@phone AND (TenantId=@tenant OR (TenantId IS NULL AND @tenant IS NULL))", tenant, Owner);
            count.ShouldBe(5);
        }
    }

    [HlgLocalDbFact]
    public async Task Duplicate_Requests_And_Concurrent_Pharmacies_Do_Not_Duplicate_Profiles_Or_Local_Codes()
    {
        var tenant = Guid.NewGuid();
        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Register(tenant, Owner)));
        await Register(tenant, "0900000100", "0900000100");
        await Task.WhenAll(Enumerable.Range(2, 4).Select(n => Register(tenant, "090000000" + n))
            .Concat(Enumerable.Range(2, 4).Select(n => Register(tenant, "090000010" + n, "0900000100"))));
        using var db = NewDb();
        (await Scalar(db, "SELECT COUNT(*) FROM HLG.AppHlgUserProfiles WHERE TenantId=@tenant", tenant)).ShouldBe(10);
        (await Scalar(db, "SELECT COUNT(DISTINCT CustomerCode) FROM dbo.AppCustomers WHERE TenantId=@tenant", tenant)).ShouldBe(10);
    }

    [HlgLocalDbFact]
    public async Task Deleted_Customer_Code_Remains_Reserved_And_Phone_Returns_Friendly_Error()
    {
        var tenant = Guid.NewGuid();
        await Register(tenant, Owner);
        await Register(tenant, "0900000002");
        using var db = NewDb();
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE dbo.AppCustomers SET IsDeleted=1 WHERE TenantId={tenant} AND PhoneNumber='0900000002'");
        (await Register(tenant, "0900000003")).CustomerCode.ShouldBe("HLGKH000002");
        (await Should.ThrowAsync<UserFriendlyException>(() => Register(tenant, "0900000002")))
            .Code.ShouldBe("HlgRegistration:CustomerUnavailable");
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE HLG.AppHlgUserProfiles SET IsDeleted=1 WHERE TenantId={tenant}");
        (await Should.ThrowAsync<UserFriendlyException>(() => Register(tenant, Owner)))
            .Code.ShouldBe("HlgRegistration:CustomerUnavailable");
    }

    [HlgLocalDbFact]
    public async Task Legacy_Profile_Read_And_Registration_Cannot_Create_Duplicate_Profiles()
    {
        var tenant = Guid.NewGuid();
        using (var scope = _app!.ServiceProvider.CreateScope())
        using (scope.ServiceProvider.GetRequiredService<ICurrentTenant>().Change(tenant))
        using (var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: true))
        {
            await scope.ServiceProvider.GetRequiredService<IRepository<Customer, Guid>>().InsertAsync(
                new Customer(Guid.NewGuid(), Owner, "Sales owner") { TenantId = tenant, CustomerCode = "SALES01", BonusPoint = 123, BonusAmount = 456 }, true);
            await uow.CompleteAsync();
        }
        await Task.WhenAll(Enumerable.Range(0, 8).Select(async n =>
        {
            if (n % 2 == 0) { await Register(tenant, Owner); return; }
            using var scope = _app!.ServiceProvider.CreateScope();
            using var change = scope.ServiceProvider.GetRequiredService<ICurrentTenant>().Change(tenant);
            await scope.ServiceProvider.GetRequiredService<IHlgProfileAppService>().GetByPhoneAsync(Owner);
        }));
        var result = await Register(tenant, Owner);
        result.CustomerCode.ShouldBe("SALES01"); result.Points.ShouldBe(123);
        using var db = NewDb();
        (await Scalar(db, "SELECT COUNT(*) FROM HLG.AppHlgUserProfiles WHERE TenantId=@tenant", tenant)).ShouldBe(1);
        (await Scalar(db, "SELECT CAST(BonusAmount AS INT) FROM dbo.AppCustomers WHERE TenantId=@tenant", tenant)).ShouldBe(456);
    }

    [HlgLocalDbFact]
    public async Task Profile_Insert_Failure_Rolls_Back_Customer_And_Releases_Lock()
    {
        var tenant = Guid.NewGuid();
        using var db = NewDb();
        // Controlled failure AFTER the customer INSERT, to verify real ABP transaction rollback.
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE HLG.AppHlgUserProfiles ADD CONSTRAINT CK_HlgRegistrationTest_Failure CHECK (PharmaPhone <> '0900000999')");
        try
        {
            await Should.ThrowAsync<Exception>(() => Register(tenant, "0900000999", "0900000999"));
            (await Scalar(db, "SELECT COUNT(*) FROM dbo.AppCustomers WHERE TenantId=@tenant", tenant)).ShouldBe(0);
        }
        finally
        {
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE HLG.AppHlgUserProfiles DROP CONSTRAINT CK_HlgRegistrationTest_Failure");
        }
        await Register(tenant, "0900000999", "0900000999");
        (await Scalar(db, "SELECT COUNT(*) FROM HLG.AppHlgUserProfiles WHERE TenantId=@tenant", tenant)).ShouldBe(1);
    }

    private static async Task<int> Scalar(MultiTenancyDbContext db, string sql, Guid? tenant, string phone = Owner)
    {
        await db.Database.OpenConnectionAsync();
        using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(new SqlParameter("@tenant", System.Data.SqlDbType.UniqueIdentifier) { Value = (object?)tenant ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@phone", phone));
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task DisposeAsync()
    {
        if (_app != null) { await _app.ShutdownAsync(); _app.Dispose(); }
        if (Environment.GetEnvironmentVariable("HLG_REGISTRATION_LOCALDB_TESTS") != "1") return;
        // The name is generated internally, never supplied by configuration/environment.
        using var db = NewDb();
        await db.Database.EnsureDeletedAsync();
    }
}
