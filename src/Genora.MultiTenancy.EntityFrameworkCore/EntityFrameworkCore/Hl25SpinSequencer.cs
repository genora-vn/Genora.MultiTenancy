using Genora.MultiTenancy.Hl25;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace Genora.MultiTenancy.EntityFrameworkCore;

/// <summary>
/// SQL Server transaction lock keeps the eligible-spin ordinal, stock change and SpinLog
/// in the same commit, including requests served by different Web processes.
/// </summary>
public class Hl25SpinSequencer : IHl25SpinSequencer, ITransientDependency
{
    private readonly IDbContextProvider<MultiTenancyDbContext> _db;

    public Hl25SpinSequencer(IDbContextProvider<MultiTenancyDbContext> db) => _db = db;

    public async Task AcquireAsync(Guid wheelConfigId)
    {
        var context = await _db.GetDbContextAsync();
        if (context.Database.CurrentTransaction == null)
            throw new BusinessException("Hl25:SpinTransactionRequired");

        // Transaction-owned lock releases automatically on commit or rollback.
        try
        {
            await context.Database.ExecuteSqlRawAsync(@"
DECLARE @result int;
EXEC @result = sys.sp_getapplock
    @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 15000;
IF @result < 0 THROW 51025, 'Unable to acquire HL25 spin lock.', 1;",
                new SqlParameter("@resource", "HL25:Spin:" + wheelConfigId.ToString("N")));
        }
        catch (SqlException ex) when (ex.Number == 51025)
        {
            throw new BusinessException("Hl25:WheelBusy");
        }
    }

    public async Task<long> GetNextEligibleOrdinalAsync(Guid? tenantId)
    {
        var context = await _db.GetDbContextAsync();
        // A historical spin is eligible when no earlier spin for that participant
        // issued a gift. This reconstructs eligibility without changing old logs.
        return await context.Database.SqlQueryRaw<long>(@"
SELECT COUNT_BIG(*) + CONVERT(bigint, 1) AS [Value]
FROM [hl25].[AppHl25SpinLogs] AS s
WHERE s.[IsDeleted] = 0
  AND ((@tenant IS NULL AND s.[TenantId] IS NULL) OR s.[TenantId] = @tenant)
  AND NOT EXISTS (
      SELECT 1 FROM [hl25].[AppHl25SpinLogs] AS w
      WHERE w.[IsDeleted] = 0 AND w.[ParticipantId] = s.[ParticipantId]
        AND w.[GiftId] IS NOT NULL
        AND (w.[SpinTime] < s.[SpinTime]
          OR (w.[SpinTime] = s.[SpinTime] AND CONVERT(binary(16), w.[Id]) < CONVERT(binary(16), s.[Id]))))",
            new SqlParameter("@tenant", SqlDbType.UniqueIdentifier) { Value = (object?)tenantId ?? DBNull.Value }).SingleAsync();
    }

}
