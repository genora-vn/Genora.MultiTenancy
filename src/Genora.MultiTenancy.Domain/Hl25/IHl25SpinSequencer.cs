using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Hl25;

/// <summary>Serializes wheel spins within the ambient transaction and counts eligible turns when pacing is enabled.</summary>
public interface IHl25SpinSequencer
{
    Task AcquireAsync(Guid wheelConfigId);
    Task<long> GetNextEligibleOrdinalAsync(Guid? tenantId);
}
