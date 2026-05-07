using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos.SalonBeautyCustomerDtos;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Permissions;
using Genora.MultiTenancy.SalonBeauty;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.Application.SalonBeauty;

[Authorize]
public class SalonBeautyCustomerAppService : ApplicationService, ISalonBeautyCustomerAppService
{
    private readonly IRepository<SalonBeautyCustomer, Guid> _customerRepository;

    public SalonBeautyCustomerAppService(IRepository<SalonBeautyCustomer, Guid> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<PagedResultDto<SalonBeautyCustomerDto>> GetListAsync(GetSalonBeautyListInput input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyCustomers.Default);

        var query = await _customerRepository.GetQueryableAsync();
        query = query.WhereIf(!input.FilterText.IsNullOrWhiteSpace(),
            x => x.Name.Contains(input.FilterText) || (x.CustomerCode != null && x.CustomerCode.Contains(input.FilterText)));

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<SalonBeautyCustomerDto>
        {
            TotalCount = totalCount,
            Items = ObjectMapper.Map<List<SalonBeautyCustomer>, List<SalonBeautyCustomerDto>>(items)
        };
    }

    public async Task<SalonBeautyCustomerDto> GetAsync(Guid id)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyCustomers.Default);
        var customer = await _customerRepository.GetAsync(id);
        return ObjectMapper.Map<SalonBeautyCustomer, SalonBeautyCustomerDto>(customer);
    }

    public async Task<SalonBeautyCustomerDto> CreateAsync(CreateSalonBeautyCustomerDto input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyCustomers.Create);

        var customer = new SalonBeautyCustomer
        {
            CustomerCode = input.CustomerCode,
            Name = input.Name,
            Phone = input.Phone,
            Email = input.Email,
            Gender = input.Gender,
            Birthday = input.Birthday,
            Avatar = input.Avatar,
            ZaloUserId = input.ZaloUserId,
            IsFollowOa = input.IsFollowOa,
            Source = input.Source,
            Status = input.Status,
            Note = input.Note
        };

        var created = await _customerRepository.InsertAsync(customer);
        return ObjectMapper.Map<SalonBeautyCustomer, SalonBeautyCustomerDto>(created);
    }

    public async Task<SalonBeautyCustomerDto> UpdateAsync(Guid id, UpdateSalonBeautyCustomerDto input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyCustomers.Edit);

        var customer = await _customerRepository.GetAsync(id);
        customer.Name = input.Name;
        customer.Phone = input.Phone;
        customer.Email = input.Email;
        customer.Gender = input.Gender;
        customer.Birthday = input.Birthday;
        customer.Avatar = input.Avatar;
        customer.ZaloUserId = input.ZaloUserId;
        customer.IsFollowOa = input.IsFollowOa;
        customer.Source = input.Source;
        customer.Status = input.Status;
        customer.Note = input.Note;

        var updated = await _customerRepository.UpdateAsync(customer);
        return ObjectMapper.Map<SalonBeautyCustomer, SalonBeautyCustomerDto>(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyCustomers.Delete);
        await _customerRepository.DeleteAsync(id);
    }

    private async Task CheckPolicyAsync(string permission)
        => await AuthorizationService.CheckAsync(permission);
}
