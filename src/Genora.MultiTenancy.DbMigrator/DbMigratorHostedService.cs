using Genora.MultiTenancy.Data;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.DbMigrator;

// Chỉ chạy migrate một lần rồi thoát
public class DbMigratorHostedService : ITransientDependency
{
    private readonly ILogger<DbMigratorHostedService> _logger;
    private readonly MultiTenancyDbMigrationService _migrationService;

    public DbMigratorHostedService(
        ILogger<DbMigratorHostedService> logger,
        MultiTenancyDbMigrationService migrationService)
    {
        _logger = logger;
        _migrationService = migrationService;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Started database migrations...");
        await _migrationService.MigrateAsync();  // Host + all Tenants
        _logger.LogInformation("Completed database migrations.");
    }
}
