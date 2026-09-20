using System;
using System.Linq;
using Genora.MultiTenancy.DomainModels.AppHl25;
using Genora.MultiTenancy.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Shouldly;
using Xunit;

namespace Genora.MultiTenancy.EntityFrameworkCore;

public class Hl25ReadIndexTests
{
    [Theory]
    [InlineData(typeof(Hl25Participant), "TenantId,ZaloUserId", true)]
    [InlineData(typeof(Hl25Participant), "TenantId,PhoneNumber,IsDeleted", false)]
    [InlineData(typeof(Hl25SpinLog), "TenantId,ParticipantId,SpinTime", false)]
    [InlineData(typeof(Hl25SpinLog), "TenantId,RewardStatus", false)]
    [InlineData(typeof(Hl25FrameCreation), "TenantId,ParticipantId,CreatedTime", false)]
    public void SqlServer_Model_Contains_Requested_Indexes(Type entityType, string columns, bool unique)
    {
        // Model construction only: no SQL connection or production configuration is used.
        using var context = new MultiTenancyDbContext(new DbContextOptionsBuilder<MultiTenancyDbContext>()
            .UseSqlServer("Server=(local);Database=Hl25ModelOnly;Integrated Security=True;TrustServerCertificate=True")
            .Options);
        var entity = context.GetService<IDesignTimeModel>().Model.FindEntityType(entityType)!;
        var index = entity.GetIndexes().Single(x => string.Join(",", x.Properties.Select(p => p.Name)) == columns);
        index.IsUnique.ShouldBe(unique);
        entity.GetSchema().ShouldBe("hl25");
        if (unique) index.GetFilter().ShouldBe("[TenantId] IS NOT NULL AND [ZaloUserId] IS NOT NULL");
    }

    [Fact]
    public void Migration_Only_Adds_Three_Guarded_Indexes()
    {
        var migration = new AddHl25MiniAppReadIndexes();
        migration.UpOperations.Count.ShouldBe(3);
        foreach (var operation in migration.UpOperations)
        {
            var sql = operation.ShouldBeOfType<SqlOperation>().Sql;
            sql.ShouldContain("OBJECT_ID");
            sql.ShouldContain("AND NOT EXISTS");
            sql.ShouldContain("CREATE INDEX");
            sql.ShouldNotContain("DROP ");
            sql.ShouldNotContain("ALTER TABLE");
        }
        migration.DownOperations.Count.ShouldBe(3);
        foreach (var operation in migration.DownOperations)
            operation.ShouldBeOfType<SqlOperation>().Sql.ShouldContain("DROP INDEX");
    }
}
