using System.Linq;
using Genora.MultiTenancy.DomainModels.AppHlGiftReceipts;
using Genora.MultiTenancy.Migrations;
using Genora.MultiTenancy.HoaLinh;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Genora.MultiTenancy.EntityFrameworkCore;

public class HlGiftReceiptModelTests
{
    [Fact]
    public void Abp_Convention_Registers_The_Custom_Repository_Interface()
    {
        var services = new ServiceCollection();
        services.AddAssemblyOf<EfCoreHlGiftReceiptRepository>();
        // ABP may forward exposed interfaces with an ImplementationFactory.
        services.ShouldContain(x => x.ServiceType == typeof(IHlGiftReceiptRepository));
    }

    [Fact]
    public void SqlServer_Model_Prevents_Duplicate_Branch_Gift_Per_Period_Independently_Of_Phone()
    {
        // Model inspection only. Never reads the configured staging/production connection.
        using var db = new MultiTenancyDbContext(new DbContextOptionsBuilder<MultiTenancyDbContext>()
            .UseSqlServer("Server=(local);Database=GiftReceiptModelOnly;Integrated Security=True").Options);
        var entity = db.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(HlGiftReceipt))!;
        entity.GetSchema().ShouldBe("HL");
        entity.GetTableName().ShouldBe("AppHlGiftReceipts");
        var key = entity.GetIndexes().Single(x => x.GetDatabaseName() == "UX_HlGiftReceipts_Entitlement");
        key.Properties.Select(x => x.Name).ToArray().ShouldBe(new[]
            { "TenantId", "CustCode", "CampaignCode", "CampaignPeriod", "VoucherCode" });
        key.GetFilter().ShouldBe("[TenantId] IS NOT NULL");
        key.IsUnique.ShouldBeTrue();
        var hostKey = entity.GetIndexes().Single(x => x.GetDatabaseName() == "UX_HlGiftReceipts_HostEntitlement");
        hostKey.IsUnique.ShouldBeTrue(); hostKey.GetFilter().ShouldBe("[TenantId] IS NULL");
        hostKey.Properties.Select(x => x.Name).ToArray().ShouldBe(new[]
            { "CustCode", "CampaignCode", "CampaignPeriod", "VoucherCode" });
        entity.FindProperty("CampaignPeriod")!.IsNullable.ShouldBeFalse();
        entity.FindProperty("ConcurrencyStamp")!.IsConcurrencyToken.ShouldBeTrue();
        entity.FindProperty("VoucherValue")!.GetPrecision().ShouldBe(18);
        entity.FindProperty("VoucherValue")!.GetScale().ShouldBe(2);
        entity.GetIndexes().ShouldContain(x => string.Join(",", x.Properties.Select(p => p.Name))
            == "TenantId,PhoneNumber,CustCode,ConfirmedAt");
    }

    [Fact]
    public void Host_Support_Migration_Adds_Only_A_Unique_Index_For_Null_Tenant()
    {
        var migration = new AddHlGiftReceiptHostUniqueness();
        var index = migration.UpOperations.Single().ShouldBeOfType<CreateIndexOperation>();
        index.Schema.ShouldBe("HL"); index.Table.ShouldBe("AppHlGiftReceipts");
        index.Name.ShouldBe("UX_HlGiftReceipts_HostEntitlement");
        index.IsUnique.ShouldBeTrue(); index.Filter.ShouldBe("[TenantId] IS NULL");
        index.Columns.ShouldBe(new[] { "CustCode", "CampaignCode", "CampaignPeriod", "VoucherCode" });
        migration.DownOperations.Single().ShouldBeOfType<DropIndexOperation>().Name.ShouldBe(index.Name);
    }

    [Fact]
    public void Migration_Only_Creates_New_History_Table_And_Indexes()
    {
        var migration = new AddHlGiftReceipts();
        migration.UpOperations.Count.ShouldBe(4);
        var table = migration.UpOperations.OfType<CreateTableOperation>().Single();
        table.Schema.ShouldBe("HL"); table.Name.ShouldBe("AppHlGiftReceipts");
        table.Columns.ShouldContain(x => x.Name == "Address" && x.MaxLength == 1000);
        table.Columns.ShouldContain(x => x.Name == "ConfirmedAt" && !x.IsNullable);
        foreach (var index in migration.UpOperations.OfType<CreateIndexOperation>())
        { index.Table.ShouldBe(table.Name); index.Schema.ShouldBe(table.Schema); }
        migration.UpOperations.OfType<CreateIndexOperation>().Count().ShouldBe(3);
        migration.DownOperations.Single().ShouldBeOfType<DropTableOperation>().Name.ShouldBe(table.Name);
    }
}
