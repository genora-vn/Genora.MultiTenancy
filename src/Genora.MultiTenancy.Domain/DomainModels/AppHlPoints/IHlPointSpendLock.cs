using System.Threading;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.DomainModels.AppHlPoints;

/// <summary>
/// Serialize việc tiêu tiền thưởng Hoa Linh của MỘT số điện thoại trong tenant hiện tại.
/// Quỹ thưởng khóa theo SĐT nên lock cũng theo SĐT — hai request đổi quà song song của
/// cùng khách không thể cùng đọc một số dư rồi cùng trừ (double-spend).
/// Lock do SQL giữ theo transaction, tự nhả khi commit/rollback.
/// </summary>
public interface IHlPointSpendLock
{
    Task AcquireAsync(string customerPhone, CancellationToken cancellationToken = default);
}
