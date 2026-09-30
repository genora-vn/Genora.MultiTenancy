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

            // 1) Lấy connection hiện tại (host/tenant tuỳ CurrentTenant) + chuẩn hoá để luôn kết nối được:
            //    - MultipleActiveResultSets + TrustServerCertificate luôn bật
            //    - Encrypt=False nếu người dùng không tự chỉ định (SqlClient 5.x mặc định Encrypt=True gây lỗi TLS trên SQL nội bộ)
            var rawCs = ctx.Database.GetDbConnection().ConnectionString;
            var cs = new SqlConnectionStringBuilder(rawCs)
            {
                MultipleActiveResultSets = true,
                TrustServerCertificate = true
            };
            if (rawCs.IndexOf("encrypt", StringComparison.OrdinalIgnoreCase) < 0)
            {
                cs.Encrypt = false;
            }
            _logger.LogInformation("Migrating DB: {Db}", cs.InitialCatalog);

            // 2) DB tenant đã tồn tại thì KHÔNG cần quyền master (giữ cho kịch bản đồng bộ nhiều tenant chạy tốt).
            //    Chỉ khi DB thực sự chưa có (SQL 4060/911) mới tạo qua master.
            var target = new SqlConnectionStringBuilder(cs.ConnectionString) { ConnectTimeout = 15 };
            var reachable = false;
            try
            {
                using var ping = new SqlConnection(target.ConnectionString);
                await ping.OpenAsync();
                reachable = true;
            }
            catch (SqlException ex) when (IsMissingDatabase(ex))
            {
                // DB chưa tồn tại → sẽ tạo qua master bên dưới.
            }
            catch (Exception ex)
            {
                // Lỗi kết nối KHÁC (login/TLS/tên server sai...) → báo rõ, không đoán mò.
                _logger.LogError("Cannot connect to tenant DB {Db}: {Reason}", cs.InitialCatalog, ex.Message);
                throw new BusinessException("TenantDatabaseUnreachable")
                    .WithData("Database", cs.InitialCatalog)
                    .WithData("Reason", ex.Message);
            }

            if (!reachable)
            {
                try
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
                }
                catch (Exception ex)
                {
                    _logger.LogError("Cannot create tenant DB {Db} via master: {Reason}", cs.InitialCatalog, ex.Message);
                    throw new BusinessException("TenantDatabaseCreateFailed")
                        .WithData("Database", cs.InitialCatalog)
                        .WithData("Reason", ex.Message);
                }
            }

            // 3) Áp chuỗi kết nối đã chuẩn hoá cho chính DbContext trước khi migrate (nếu connection đang đóng),
            //    để MigrateAsync cũng dùng Encrypt/TrustServerCertificate/MARS đúng — tránh lỗi TLS khi migrate.
            var dbConn = ctx.Database.GetDbConnection();
            if (dbConn.State == System.Data.ConnectionState.Closed &&
                !string.Equals(dbConn.ConnectionString, cs.ConnectionString, StringComparison.Ordinal))
            {
                dbConn.ConnectionString = cs.ConnectionString;
            }

            // 4) Migrate với timeout lớn – KHÔNG mở transaction thủ công.
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
