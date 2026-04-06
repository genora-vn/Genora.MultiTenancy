using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Realtime;

public interface IProOrderRealtimeNotifier
{
    Task OrderCreatedAsync(Guid orderId);
    Task OrderUpdatedAsync(Guid orderId);
}
