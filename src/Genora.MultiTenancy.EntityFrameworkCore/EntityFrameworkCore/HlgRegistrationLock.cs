using System;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppHlg;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.EntityFrameworkCore;

public class HlgRegistrationLock : IHlgRegistrationLock, ITransientDependency
{
    private readonly IDbContextProvider<MultiTenancyDbContext> _db;
    private readonly ICurrentTenant _tenant;

    public HlgRegistrationLock(IDbContextProvider<MultiTenancyDbContext> db, ICurrentTenant tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task AcquireAsync(CancellationToken cancellationToken = default)
    {
        var db = await _db.GetDbContextAsync();
        if (db.Database.CurrentTransaction == null)
            throw new InvalidOperationException("HLG registration requires a transactional unit of work.");

        // Tenant-wide because HLGKH sequential codes are shared by all pharmacies in the tenant.
        // DMS calls must finish BEFORE acquiring this lock; SQL releases it on commit/rollback.
        var resource = "HLG:Registration:" + (_tenant.Id?.ToString("N") ?? "host");
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                DECLARE @lockResult int;
                EXEC @lockResult = sys.sp_getapplock @Resource={resource},
                    @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=15000;
                IF @lockResult < 0
                    THROW 51066, 'HLG registration lock unavailable.', 1;
                """, cancellationToken);
        }
        catch (SqlException ex) when (ex.Number == 51066)
        {
            throw new UserFriendlyException("Hệ thống đang xử lý đăng ký. Vui lòng thử lại.", "HlgRegistration:Busy");
        }
    }
}
