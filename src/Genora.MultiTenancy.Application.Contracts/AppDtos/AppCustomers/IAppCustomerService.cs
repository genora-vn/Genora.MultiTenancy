using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppCustomers;

public interface IAppCustomerService :
        ICrudAppService<
            AppCustomerDto,
            Guid,
            GetCustomerListInput,
            CreateUpdateAppCustomerDto>
{
    /// <summary>
    /// Lấy thông tin khách theo SĐT (định danh chính).
    /// </summary>
    Task<AppCustomerDto> GetByPhoneAsync(string phoneNumber);
    Task<string> GenerateCustomerCodeAsync();
}