using Genora.MultiTenancy.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;

namespace Genora.MultiTenancy.Web.Middlewares;

public class TenantAutoMigrateMiddleware : IMiddleware, ITransientDependency
{
    private readonly ICurrentTenant _current;
    private readonly ITenantRepository _tenantRepo;
    private readonly IDbContextProvider<MultiTenancyDbContext> _db;
    private readonly IMemoryCache _cache;
    private readonly IUnitOfWorkManager _uow;

    public TenantAutoMigrateMiddleware(
        ICurrentTenant current,
        ITenantRepository tenantRepo,
        IDbContextProvider<MultiTenancyDbContext> db,
        IMemoryCache cache,
        IUnitOfWorkManager uow)
    {
        _current = current;
        _tenantRepo = tenantRepo;
        _db = db;
        _cache = cache;
        _uow = uow;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var tenantId = _current.Id; // lấy trước khi Change(null)
        if (tenantId != null)
        {
            // 1) GUARD: chặn nếu tenant bị disable (đọc HOST DB)
            var activeKey = $"tenant-active:{tenantId}";
            var isActive = await _cache.GetOrCreateAsync(activeKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

                using (_current.Change(null)) // về host
                using (var uow = _uow.Begin(requiresNew: true, isTransactional: false)) // ⬅️ KHÔNG transactional
                {
                    var t = await _tenantRepo.GetAsync(tenantId.Value, includeDetails: false);
                    await uow.CompleteAsync();
                    return t.GetProperty<bool?>("IsActive") ?? true;
                }
            });

            if (!isActive)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "text/plain; charset=utf-8";
                await context.Response.WriteAsync("Xin lỗi. Hệ thống đang không hoạt động do chưa đăng ký hoặc chưa được kích hoạt.");
                return;
            }

            // 2) MIGRATE-ONCE: migrate schema cho DB tenant
            var migrateKey = $"tenant-migrated:{tenantId}";
            if (!_cache.TryGetValue(migrateKey, out _))
            {
                // UoW không transactional để tránh xung đột với retry strategy
                using (var uow = _uow.Begin(requiresNew: true, isTransactional: false))
                {
                    var ctx = await _db.GetDbContextAsync(); // ngữ cảnh tenant đã có
                    // Dùng execution strategy để bao trọn migrate (đúng chuẩn EF khi có retry)
                    var strategy = ctx.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () =>
                    {
                        // KHÔNG mở transaction thủ công ở đây
                        ctx.Database.SetCommandTimeout(180);
                        await ctx.Database.MigrateAsync();
                    });

                    await uow.CompleteAsync();
                }

                _cache.Set(migrateKey, true, TimeSpan.FromMinutes(30));
            }
        }

        await next(context);
    }
}