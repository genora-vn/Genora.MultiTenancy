using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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

            var sharedKeyPath = builder.Configuration["DataProtection:KeyPath"];
            if (string.IsNullOrWhiteSpace(sharedKeyPath))
            {
                sharedKeyPath = Path.Combine(AppContext.BaseDirectory, "App_Data", "keys");
            }

            Directory.CreateDirectory(sharedKeyPath);

            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(sharedKeyPath))
                .SetApplicationName("Genora.MultiTenancy");

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = ".Genora.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.SlidingExpiration = true;
            });

            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.Secure = CookieSecurePolicy.Always;
                options.HttpOnly = HttpOnlyPolicy.None;
            });

            await builder.AddApplicationAsync<MultiTenancyWebModule>();

            var app = builder.Build();

            if (app.Configuration.GetValue<bool>("ReverseProxy:Enabled"))
            {
                app.UseForwardedHeaders();
            }

            app.UseCookiePolicy();
            app.UseSerilogRequestLogging();

            app.MapGet("/version", () => new
            {
                env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                commit = File.Exists(".git_commit") ? File.ReadAllText(".git_commit") : "missing"
            });

            app.MapGet("/debug/scheme", (HttpContext ctx) => new
            {
                scheme = ctx.Request.Scheme,
                isHttps = ctx.Request.IsHttps,
                host = ctx.Request.Host.Value,
                remoteIp = ctx.Connection.RemoteIpAddress?.ToString(),
                xfProto = ctx.Request.Headers["X-Forwarded-Proto"].ToString(),
                xfHost = ctx.Request.Headers["X-Forwarded-Host"].ToString(),
                xfPort = ctx.Request.Headers["X-Forwarded-Port"].ToString(),
                cookies = ctx.Request.Cookies.Keys
            });

            app.MapGet("/debug/antiforgery", (
                Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Antiforgery.AntiforgeryOptions> anti,
                Microsoft.Extensions.Options.IOptions<CookiePolicyOptions> cookie) =>
            {
                var a = anti.Value;
                var c = cookie.Value;

                return new
                {
                    antiforgery = new
                    {
                        cookieName = a.Cookie.Name,
                        httpOnly = a.Cookie.HttpOnly,
                        sameSite = a.Cookie.SameSite.ToString(),
                        securePolicy = a.Cookie.SecurePolicy.ToString(),
                        path = a.Cookie.Path,
                        formFieldName = a.FormFieldName,
                        headerName = a.HeaderName
                    },
                    cookiePolicy = new
                    {
                        minimumSameSitePolicy = c.MinimumSameSitePolicy.ToString(),
                        secure = c.Secure.ToString(),
                        httpOnly = c.HttpOnly.ToString()
                    }
                };
            });

            app.MapHub<Genora.MultiTenancy.SignalR.FnbOrderHub>("/signalr-hubs/fnb-orders");
            app.MapHub<Genora.MultiTenancy.SignalR.ProOrderHub>("/signalr-hubs/pro-orders");
            app.MapHub<Genora.MultiTenancy.SignalR.HlgLiveFeedHub>("/signalr-hubs/hlg-live-feed");

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