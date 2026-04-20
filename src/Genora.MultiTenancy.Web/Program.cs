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
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.Console())
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting web host...");
            var builder = WebApplication.CreateBuilder(args);

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
                            .WithDestructurers(new[] { new SqlExceptionDestructurer() })
                            .WithRootName("ExceptionDetail")
                       )
                      .Enrich.WithProperty("Application", "Genora.MultiTenancy")
                      .Enrich.WithProperty("Service", "Web")
                      .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                      .Enrich.WithEnvironmentName()
                      .Enrich.WithMachineName()
                      .Enrich.WithThreadId()
                      .WriteTo.Async(c => c.File(
                          path: cfg["Serilog:File:Path"] ?? "Logs/log-.ndjson",
                          rollingInterval: RollingInterval.Day,
                          retainedFileCountLimit: int.TryParse(cfg["Serilog:File:RetainedFileCountLimit"], out var keep) ? keep : 7,
                          restrictedToMinimumLevel: LogEventLevel.Information
                      ))
                      .WriteTo.Async(c => c.Console())
                      .WriteTo.Async(c => c.Seq(
                          serverUrl: cfg["Serilog:Seq:Url"],
                          apiKey: string.IsNullOrWhiteSpace(cfg["Serilog:Seq:ApiKey"]) ? null : cfg["Serilog:Seq:ApiKey"],
                          restrictedToMinimumLevel: LogEventLevel.Information
                      ));
                });

            await builder.AddApplicationAsync<MultiTenancyWebModule>();

            var app = builder.Build();

            // 0) MUST: apply forwarded headers BEFORE anything else (esp. antiforgery/config scripts)
            app.UseForwardedHeaders();

            // optional: request logging after forwarded headers (so scheme/host are correct)
            app.UseSerilogRequestLogging();

            // debug version
            app.MapGet("/version", () => new
            {
                env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                commit = File.Exists(".git_commit") ? File.ReadAllText(".git_commit") : "missing"
            });

            // Map hubs (endpoints are picked up when ABP later calls UseRouting/UseEndpoints)
            app.MapHub<Genora.MultiTenancy.SignalR.FnbOrderHub>("/signalr-hubs/fnb-orders");
            app.MapHub<Genora.MultiTenancy.SignalR.ProOrderHub>("/signalr-hubs/pro-orders");

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