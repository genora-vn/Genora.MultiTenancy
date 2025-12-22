using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.AppCustomers;
public interface IMiniAppCustomerAppService
{
    Task<MiniAppCustomerDto> UpsertFromMiniAppAsync(MiniAppUpsertCustomerRequest input);
    Task<MiniAppCustomerDto?> GetByPhoneAsync(string phoneNumber);
}