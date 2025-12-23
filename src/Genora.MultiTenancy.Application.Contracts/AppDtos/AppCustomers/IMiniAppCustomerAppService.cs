using System.Threading;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.AppCustomers;
public interface IMiniAppCustomerAppService
{
    Task<MiniAppCustomerDto> UpsertFromMiniAppAsync(MiniAppUpsertCustomerRequest input, CancellationToken ct);
    Task<MiniAppCustomerDto?> GetByPhoneAsync(string phoneNumber, CancellationToken ct);
}