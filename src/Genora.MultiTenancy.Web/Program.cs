

using Genora.MultiTenancy.Web.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.SqlServer.Destructurers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        // Bootstrap logger sớm để bắt lỗi khởi động
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.Console())
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting web host...");
            var builder = WebApplication.CreateBuilder(args);

            // Serilog cấu hình đầy đủ từ appsettings + enrichers + Seq
            builder.Host
                .AddAppSettingsSecretsJson()
                .UseAutofac()
                .UseSerilog((context, services, lc) =>
                {
                    var cfg = context.Configuration;

                    lc.MinimumLevel.Is(Enum.TryParse(cfg["Serilog:MinimumLevel"], out LogEventLevel lvl) ? lvl : LogEventLevel.Information)
                      .MinimumLevel.Override("Microsoft", Enum.TryParse(cfg["Serilog:Override:Microsoft"], out LogEventLevel ms) ? ms : LogEventLevel.Warning)
                      .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Enum.TryParse(cfg["Serilog:Override:Microsoft.EntityFrameworkCore"], out LogEventLevel ef) ? ef : LogEventLevel.Warning)
                      .MinimumLevel.Override("Volo.Abp", Enum.TryParse(cfg["Serilog:Override:Volo.Abp"], out LogEventLevel abp) ? abp : LogEventLevel.Information)
                      .Enrich.FromLogContext()
                      .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                            .WithDefaultDestructurers()
                            .WithDestructurers(new[] { new SqlExceptionDestructurer() }) // cần Serilog.Exceptions.SqlServer
                            .WithRootName("ExceptionDetail") // đổi tên root nếu muốn
                       )
                      .Enrich.WithProperty("Application", "Genora.MultiTenancy")
                      .Enrich.WithProperty("Service", "Web")
                      .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                      .Enrich.WithEnvironmentName()
                      .Enrich.WithMachineName()
                      .Enrich.WithThreadId()
                      // File (ndjson) – dự phòng/điều tra cục bộ
                      .WriteTo.Async(c => c.File(
                          path: cfg["Serilog:File:Path"] ?? "Logs/log-.ndjson",
                          rollingInterval: RollingInterval.Day,
                          retainedFileCountLimit: int.TryParse(cfg["Serilog:File:RetainedFileCountLimit"], out var keep) ? keep : 7,
                          restrictedToMinimumLevel: LogEventLevel.Information
                      ))
                      // Console
                      .WriteTo.Async(c => c.Console())
                      // Seq
                      .WriteTo.Async(c => c.Seq(
                          serverUrl: cfg["Serilog:Seq:Url"],
                          apiKey: string.IsNullOrWhiteSpace(cfg["Serilog:Seq:ApiKey"]) ? null : cfg["Serilog:Seq:ApiKey"],
                          restrictedToMinimumLevel: LogEventLevel.Information
                      ));
                });

            await builder.AddApplicationAsync<MultiTenancyWebModule>();

            var app = builder.Build();

            app.MapGet("/version", () => new {
                env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                commit = File.Exists(".git_commit") ? File.ReadAllText(".git_commit") : "missing"
            });

            // Ghi log request (Method, Path, StatusCode, Elapsed…)
            app.UseSerilogRequestLogging(opts =>
            {
                opts.GetLevel = (ctx, elapsed, ex) =>
                    ex != null || ctx.Response.StatusCode >= 500
                        ? LogEventLevel.Error
                        : elapsed > 1000 ? LogEventLevel.Warning : LogEventLevel.Information;

                // đưa các field hay dùng vào event
                opts.EnrichDiagnosticContext = (diag, http) =>
                {
                    diag.Set("Path", http.Request.Path.Value);
                    diag.Set("Method", http.Request.Method);
                    diag.Set("StatusCode", http.Response.StatusCode);
                    diag.Set("ClientIp", http.Connection.RemoteIpAddress?.ToString());
                    diag.Set("UserAgent", http.Request.Headers["User-Agent"].ToString());
                    diag.Set("RequestId", http.TraceIdentifier);
                };
            });

            // ABP pipeline + middleware enrich
            app.UseRouting();
            app.UseMiddleware<LogEnrichmentMiddleware>(); // đính TenantId, TenantName, UserId, UserName, CorrelationId
            app.UseStaticFiles();
            // ... (Auth, Abp, Endpoints)
            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}