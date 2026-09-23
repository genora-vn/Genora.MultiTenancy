using Genora.MultiTenancy.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Uow;

namespace Genora.MultiTenancy.EntityFrameworkCore;

public class EntityFrameworkCoreMultiTenancyDbSchemaMigrator
    : IMultiTenancyDbSchemaMigrator, ITransientDependency
{
    private readonly IDbContextProvider<MultiTenancyDbContext> _db;
    private readonly IUnitOfWorkManager _uow;
    private readonly ILogger<EntityFrameworkCoreMultiTenancyDbSchemaMigrator> _logger;

    public EntityFrameworkCoreMultiTenancyDbSchemaMigrator(
        IDbContextProvider<MultiTenancyDbContext> db,
        IUnitOfWorkManager uow,
        ILogger<EntityFrameworkCoreMultiTenancyDbSchemaMigrator> logger)
    {
        _db = db;
        _uow = uow;
        _logger = logger;
    }

    public async Task MigrateAsync()
    {
        using (var uow = _uow.Begin(requiresNew: true, isTransactional: false))
        {
            var ctx = await _db.GetDbContextAsync();

            // 1) lấy connection hiện tại (host/tenant tuỳ CurrentTenant)
            var rawCs = ctx.Database.GetDbConnection().ConnectionString;
            var cs = new SqlConnectionStringBuilder(rawCs) { MultipleActiveResultSets = true };
            _logger.LogInformation("Migrating DB: {Db}", cs.InitialCatalog);

            // Existing tenant databases should not depend on access to master.
            // Only use master when SQL Server explicitly reports a missing database.
            var quick = new SqlConnectionStringBuilder(cs.ConnectionString) { ConnectTimeout = 15 };
            try
            {
                try
                {
                    using var ping = new SqlConnection(quick.ConnectionString);
                    await ping.OpenAsync();
                }
                catch (SqlException ex) when (IsMissingDatabase(ex))
                {
                    var master = new SqlConnectionStringBuilder(cs.ConnectionString)
                    { InitialCatalog = "master", ConnectTimeout = 15 };
                    using var conn = new SqlConnection(master.ConnectionString);
                    await conn.OpenAsync();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
IF DB_ID(@db) IS NULL
BEGIN
    DECLARE @sql nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(@db);
    EXEC (@sql);
END";
                    cmd.Parameters.AddWithValue("@db", cs.InitialCatalog);
                    cmd.CommandTimeout = 30;
                    await cmd.ExecuteNonQueryAsync();

                    using var ping = new SqlConnection(quick.ConnectionString);
                    await ping.OpenAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Database preflight failed for {Db}: {Reason}", cs.InitialCatalog, ex.Message);
                throw new BusinessException("TenantDatabaseUnreachable")
                    .WithData("Database", cs.InitialCatalog)
                    .WithData("Reason", ex.Message);
            }

            // Migrate with a longer command timeout; do not open a manual transaction.
            ctx.Database.SetCommandTimeout(180);
            await ctx.Database.MigrateAsync();

            await uow.CompleteAsync();
            _logger.LogInformation("Migrated DB OK: {Db}", cs.InitialCatalog);
        }
    }

    private static bool IsMissingDatabase(SqlException exception)
    {
        foreach (SqlError error in exception.Errors)
        {
            if (error.Number is 4060 or 911) return true;
        }

        return false;
    }
}
