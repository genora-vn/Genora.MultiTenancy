using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.SqlServer.Destructurers;
using System;
using System.Threading.Tasks;
using Volo.Abp;

namespace Genora.MultiTenancy.DbMigrator;
class Program
{
    static async Task Main(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Volo.Abp", LogEventLevel.Information)
#if DEBUG
            .MinimumLevel.Override("Genora.MultiTenancy", LogEventLevel.Debug)
#else
            .MinimumLevel.Override("Genora.MultiTenancy", LogEventLevel.Information)
#endif
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                .WithDefaultDestructurers()
                .WithDestructurers(new[] { new SqlExceptionDestructurer() }) // cần Serilog.Exceptions.SqlServer
                .WithRootName("ExceptionDetail") // đổi tên root nếu muốn
            )
            .Enrich.WithProperty("Application", "Genora.MultiTenancy")
            .Enrich.WithProperty("Service", "DbMigrator")
            .Enrich.WithProperty("Environment", env)
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Async(c => c.File("Logs/log-.ndjson", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3))
            .WriteTo.Async(c => c.Seq("http://localhost:5341", apiKey: null, restrictedToMinimumLevel: LogEventLevel.Information))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        var host = Host.CreateDefaultBuilder(args)
            .AddAppSettingsSecretsJson()
            .UseAutofac()
            .UseSerilog()
            .ConfigureLogging(lb => lb.ClearProviders())
            .ConfigureServices(services =>
            {
                services.AddApplication<MultiTenancyDbMigratorModule>();
                services.AddTransient<DbMigratorHostedService>();
            })
            .Build();

        var app = host.Services.GetRequiredService<IAbpApplicationWithExternalServiceProvider>();
        await app.InitializeAsync(host.Services);
        try
        {
            await host.Services.GetRequiredService<DbMigratorHostedService>().RunAsync();
        }
        finally
        {
            await app.ShutdownAsync();
            if (host is IAsyncDisposable ad) await ad.DisposeAsync(); else host.Dispose();
            Log.CloseAndFlush();
        }
    }
}
