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

            // 2) preflight tới master + tạo DB nếu thiếu
            var master = new SqlConnectionStringBuilder(cs.ConnectionString)
            { InitialCatalog = "master", ConnectTimeout = 5 };

            try
            {
                using (var conn = new SqlConnection(master.ConnectionString))
                {
                    await conn.OpenAsync();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "IF DB_ID(@db) IS NULL EXEC('CREATE DATABASE [' + @db + ']')";
                    cmd.Parameters.AddWithValue("@db", cs.InitialCatalog);
                    cmd.CommandTimeout = 30;
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                throw new BusinessException("TenantDatabaseUnreachable")
                    .WithData("ConnectionString", cs.ConnectionString)
                    .WithData("Reason", ex.Message);
            }

            // 3) ping nhanh DB đích 5s (nếu unreachable → fail ngay, khỏi treo timeout dài)
            var quick = new SqlConnectionStringBuilder(cs.ConnectionString) { ConnectTimeout = 5 };
            using (var ping = new SqlConnection(quick.ConnectionString))
            { await ping.OpenAsync(); }

            // 4) migrate với timeout lớn – KHÔNG mở transaction thủ công
            ctx.Database.SetCommandTimeout(180);
            await ctx.Database.MigrateAsync();

            await uow.CompleteAsync();
            _logger.LogInformation("Migrated DB OK: {Db}", cs.InitialCatalog);
        }
    }
}