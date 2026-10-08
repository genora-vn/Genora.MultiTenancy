using System.Threading;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppHlg;

/// <summary>Serializes HLG account allocation/code generation in the current tenant and transaction.</summary>
public interface IHlgRegistrationLock
{
    Task AcquireAsync(CancellationToken cancellationToken = default);
}
