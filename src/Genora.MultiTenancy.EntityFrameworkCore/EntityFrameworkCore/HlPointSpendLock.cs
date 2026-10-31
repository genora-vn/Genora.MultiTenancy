using System;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.DomainModels.AppHlPoints;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.EntityFrameworkCore;

public class HlPointSpendLock : IHlPointSpendLock, ITransientDependency
{
    private readonly IDbContextProvider<MultiTenancyDbContext> _db;
    private readonly ICurrentTenant _tenant;

    public HlPointSpendLock(IDbContextProvider<MultiTenancyDbContext> db, ICurrentTenant tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task AcquireAsync(string customerPhone, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerPhone))
            throw new ArgumentException("Thiếu số điện thoại để khóa quỹ thưởng.", nameof(customerPhone));

        var db = await _db.GetDbContextAsync();
        if (db.Database.CurrentTransaction == null)
            throw new InvalidOperationException("HL point spend requires a transactional unit of work.");

        // Khóa hẹp theo (tenant, SĐT): khách khác vẫn đổi quà song song bình thường.
        // Mọi lệnh gọi API ngoài (UrBox/DMS) phải xong TRƯỚC khi vào lock này.
        var resource = "HL:PointSpend:" + (_tenant.Id?.ToString("N") ?? "host") + ":" + customerPhone;
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                DECLARE @lockResult int;
                EXEC @lockResult = sys.sp_getapplock @Resource={resource},
                    @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000;
                IF @lockResult < 0
                    THROW 51067, 'HL point spend lock unavailable.', 1;
                """, cancellationToken);
        }
        catch (SqlException ex) when (ex.Number == 51067)
        {
            throw new UserFriendlyException(
                "Hệ thống đang xử lý một yêu cầu đổi quà khác của bạn. Vui lòng thử lại sau vài giây.",
                "HlPointSpend:Busy");
        }
    }
}
