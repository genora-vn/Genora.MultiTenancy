using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.AppPaymentConfigurations;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppServices.AppPaymentConfigurations;

public interface IAppPaymentConfigurationService : IApplicationService
{
    Task<List<PaymentConfigurationDto>> GetListAsync();
    Task<PaymentConfigurationDto> GetAsync(Guid id);
    Task<PaymentConfigurationDto> CreateAsync(CreateUpdatePaymentConfigurationDto input);
    Task<PaymentConfigurationDto> UpdateAsync(Guid id, CreateUpdatePaymentConfigurationDto input);
    Task DeleteAsync(Guid id);

    /// <summary>Lấy config đang active đầu tiên theo DisplayOrder, dùng để xuất bill.</summary>
    Task<PaymentConfigurationDto?> GetActiveAsync();
}
