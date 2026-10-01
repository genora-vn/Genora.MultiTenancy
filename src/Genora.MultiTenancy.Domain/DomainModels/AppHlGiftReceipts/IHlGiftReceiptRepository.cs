using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.DomainModels.AppHlGiftReceipts;

public interface IHlGiftReceiptRepository : IRepository<HlGiftReceipt, Guid>
{
    // Caller must keep a transactional UOW open until insert + commit complete.
    Task<HlGiftReceipt?> FindForConfirmationAsync(Guid? tenantId, string custCode, string campaignCode,
        int campaignPeriod, string voucherCode, CancellationToken ct = default);
}
