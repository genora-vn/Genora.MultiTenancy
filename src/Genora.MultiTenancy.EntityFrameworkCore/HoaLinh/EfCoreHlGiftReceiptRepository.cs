using System;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.DomainModels.AppHlGiftReceipts;
using Genora.MultiTenancy.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.HoaLinh;

public class EfCoreHlGiftReceiptRepository : EfCoreRepository<MultiTenancyDbContext, HlGiftReceipt, Guid>, IHlGiftReceiptRepository, ITransientDependency
{
    public EfCoreHlGiftReceiptRepository(IDbContextProvider<MultiTenancyDbContext> provider) : base(provider) { }

    public async Task<HlGiftReceipt?> FindForConfirmationAsync(Guid? tenantId, string custCode, string campaignCode,
        int campaignPeriod, string voucherCode, CancellationToken ct = default)
    {
        var db = await GetDbContextAsync();
        if (db.Database.CurrentTransaction == null)
            throw new InvalidOperationException("Gift confirmation requires a transactional unit of work.");

        // Serializable key-range update lock: concurrent requests for an absent row wait here.
        // The unique business index is a second line of defense. All values are SQL parameters.
        var set = await GetDbSetAsync();
        // SQL equality with NULL never matches. Use an explicit Host branch, also enabling its filtered index.
        var query = tenantId.HasValue ? set.FromSqlInterpolated($@"
            SELECT * FROM [HL].[AppHlGiftReceipts] WITH (UPDLOCK, HOLDLOCK)
            WHERE [TenantId] = {tenantId.Value} AND [CustCode] = {custCode}
              AND [CampaignCode] = {campaignCode} AND [CampaignPeriod] = {campaignPeriod}
              AND [VoucherCode] = {voucherCode}")
            : set.FromSqlInterpolated($@"
            SELECT * FROM [HL].[AppHlGiftReceipts] WITH (UPDLOCK, HOLDLOCK)
            WHERE [TenantId] IS NULL AND [CustCode] = {custCode}
              AND [CampaignCode] = {campaignCode} AND [CampaignPeriod] = {campaignPeriod}
              AND [VoucherCode] = {voucherCode}");
        return await query.SingleOrDefaultAsync(ct);
    }
}
