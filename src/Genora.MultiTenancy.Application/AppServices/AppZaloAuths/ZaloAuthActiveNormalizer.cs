using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;

public static class ZaloAuthActiveNormalizer
{
    private static bool IsExpired(ZaloAuth a)
        => a.ExpireTokenTime.HasValue && a.ExpireTokenTime.Value <= DateTime.UtcNow;

    /// <summary>
    /// Rule:
    /// - Active mà hết hạn => auto IsActive=false
    /// - Sau đó đảm bảo chỉ còn 1 active (ưu tiên active mới nhất và chưa hết hạn)
    /// </summary>
    public static async Task<ZaloAuth?> EnsureSingleActiveNonExpiredAsync(IRepository<ZaloAuth, Guid> repo)
    {
        var q = await repo.GetQueryableAsync();

        // 1) Vô hiệu hóa tất cả token đã hết hạn
        var expiredActives = q.Where(x => x.IsActive && x.ExpireTokenTime.HasValue && x.ExpireTokenTime.Value <= DateTime.UtcNow)
                              .ToList();

        foreach (var a in expiredActives) a.IsActive = false;
        foreach (var a in expiredActives) await repo.UpdateAsync(a, autoSave: true);

        // 2) Lấy ra số lương bản ghi đang active
        var actives = q.Where(x => x.IsActive)
                       .OrderByDescending(x => x.CreationTime)
                       .ToList();

        if (actives.Count == 0) return null;

        var keep = actives[0];

        // 3) Chỉ để lại 1 token đang hiệu lực active
        if (actives.Count > 1)
        {
            foreach (var a in actives.Skip(1)) a.IsActive = false;
            foreach (var a in actives.Skip(1)) await repo.UpdateAsync(a, autoSave: true);
        }

        return keep;
    }

    /// <summary>
    /// Set 1 record active, nhưng nếu record đó expired => tự set inactive và kết thúc.
    /// </summary>
    public static async Task SetActiveOnlyAsync(IRepository<ZaloAuth, Guid> repo, Guid keepId)
    {
        var keep = await repo.FindAsync(keepId);
        if (keep == null) return;

        // Nếu token quá hạn thì không được active
        if (IsExpired(keep))
        {
            if (keep.IsActive)
            {
                keep.IsActive = false;
                await repo.UpdateAsync(keep, autoSave: true);
            }

            // Tắt luôn các active khác (để chắc chắn hệ thống không có active expired)
            var q0 = await repo.GetQueryableAsync();
            var actives0 = q0.Where(x => x.IsActive).ToList();
            foreach (var a in actives0) a.IsActive = false;
            foreach (var a in actives0) await repo.UpdateAsync(a, autoSave: true);

            return;
        }

        // Vô hiệu hóa các token quá hạn khác
        var q = await repo.GetQueryableAsync();
        var others = q.Where(x => x.IsActive && x.Id != keepId).ToList();
        if (others.Count == 0) return;

        foreach (var o in others) o.IsActive = false;
        foreach (var o in others) await repo.UpdateAsync(o, autoSave: true);
    }
}