using Genora.MultiTenancy.AppDtos.AppCustomers;
using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.Features.AppCustomers;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.AppCustomers;

[Authorize]
public class AppCustomerService :
     FeatureProtectedCrudAppService<
         Customer,
         AppCustomerDto,
         Guid,
         GetCustomerListInput,
         CreateUpdateAppCustomerDto>,
     IAppCustomerService
{
    protected override string FeatureName => AppCustomerFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppCustomers.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppCustomers.Default;

    private readonly IRepository<CustomerType, Guid> _customerTypeRepository;
    private readonly IManageImageService _manageImageService;

    private const int AVATAR_MAX_MB = 15;
    private const long AVATAR_MAX_BYTES = AVATAR_MAX_MB * 1024L * 1024L;

    public AppCustomerService(
        IRepository<Customer, Guid> repository,
        IRepository<CustomerType, Guid> customerTypeRepository,
        IManageImageService manageImageService,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(repository, currentTenant, featureChecker)
    {
        GetPolicyName = MultiTenancyPermissions.AppCustomers.Default;
        GetListPolicyName = MultiTenancyPermissions.AppCustomers.Default;
        CreatePolicyName = MultiTenancyPermissions.AppCustomers.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppCustomers.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppCustomers.Delete;

        _customerTypeRepository = customerTypeRepository;
        _manageImageService = manageImageService;
    }

    public async Task<string> GenerateCustomerCodeAsync()
    {
        await CheckCreatePolicyAsync();

        const string prefix = "KH";

        var queryable = await Repository.GetQueryableAsync();

        var codes = await AsyncExecuter.ToListAsync(
            queryable.Where(c => c.CustomerCode != null && c.CustomerCode.StartsWith(prefix))
        );

        var maxNumber = 0;

        foreach (var code in codes.Select(c => c.CustomerCode))
        {
            var numberPart = code.Substring(prefix.Length);
            if (int.TryParse(numberPart, NumberStyles.None, CultureInfo.InvariantCulture, out var n))
            {
                if (n > maxNumber) maxNumber = n;
            }
        }

        var nextNumber = maxNumber + 1;
        var candidate = $"{prefix}{nextNumber.ToString("D6", CultureInfo.InvariantCulture)}";

        while (await Repository.AnyAsync(c => c.CustomerCode == candidate))
        {
            nextNumber++;
            candidate = $"{prefix}{nextNumber.ToString("D6", CultureInfo.InvariantCulture)}";
        }

        return candidate;
    }

    [DisableValidation]
    public override async Task<PagedResultDto<AppCustomerDto>> GetListAsync(GetCustomerListInput input)
    {
        await CheckGetListPolicyAsync();

        var customers = await Repository.GetQueryableAsync();
        var customerTypes = await _customerTypeRepository.GetQueryableAsync();

        var query =
            from c in customers
            join ct in customerTypes on c.CustomerTypeId equals ct.Id into ctj
            from ct in ctj.DefaultIfEmpty()
            select new { c, ct };

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            var f = input.Filter.Trim();
            query = query.Where(x =>
                (x.c.CustomerCode != null && x.c.CustomerCode.Contains(f)) ||
                (x.c.FullName != null && x.c.FullName.Contains(f)) ||
                (x.c.PhoneNumber != null && x.c.PhoneNumber.Contains(f)) ||
                (x.c.VgaCode != null && x.c.VgaCode.Contains(f)) ||
                (x.ct != null && x.ct.Name != null && x.ct.Name.Contains(f))
            );
        }

        if (input.CustomerTypeId.HasValue)
            query = query.Where(x => x.c.CustomerTypeId == input.CustomerTypeId);

        if (input.IsActive.HasValue)
            query = query.Where(x => x.c.IsActive == input.IsActive.Value);

        if (input.CreatedFrom.HasValue)
            query = query.Where(x => x.c.CreationTime >= input.CreatedFrom.Value);

        if (input.CreatedTo.HasValue)
            query = query.Where(x => x.c.CreationTime <= input.CreatedTo.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? "c." + nameof(Customer.CreationTime) + " DESC"
            : input.Sorting;

        query = query.OrderBy(
            sorting.StartsWith("c.") || sorting.StartsWith("ct.")
                ? sorting
                : "c." + sorting
        );

        var items = await AsyncExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount)
        );

        var dtos = items.Select(x => new AppCustomerDto
        {
            Id = x.c.Id,
            TenantId = x.c.TenantId,
            PhoneNumber = x.c.PhoneNumber,
            FullName = x.c.FullName,
            DateOfBirth = x.c.DateOfBirth,
            CustomerCode = x.c.CustomerCode ?? "",
            VgaCode = x.c.VgaCode,
            IsActive = x.c.IsActive,

            CustomerTypeId = x.c.CustomerTypeId,
            CustomerTypeName = x.ct != null ? x.ct.Name : "",
            CustomerTypeCode = x.ct != null ? x.ct.Code : "",

            AvatarUrl = x.c.AvatarUrl ?? "",
            Gender = x.c.Gender,
            ZaloUserId = x.c.ZaloUserId ?? "",
            Email = x.c.Email,
            Address = x.c.Address,
            IsFollower = x.c.IsFollower,
            BonusPoint = x.c.BonusPoint,
            MembershipTierId = x.c.MembershipTierId,
            MembershipTierName = x.c.MembershipTier != null ? x.c.MembershipTier.Name : null
        }).ToList();

        return new PagedResultDto<AppCustomerDto>(totalCount, dtos);
    }

    public override async Task<AppCustomerDto> CreateAsync(CreateUpdateAppCustomerDto input)
    {
        await CheckCreatePolicyAsync();
        await EnsurePhoneUniqueAsync(input.PhoneNumber, null);

        // ✅ validate + upload avatar (nếu có)
        if (input.AvatarFile != null)
        {
            await ValidateAvatarAsync(input.AvatarFile);
            var tenantId = CurrentTenant.Id?.ToString() ?? "host";
            var uploadedUrl = await _manageImageService.UploadImageAsync(input.AvatarFile, tenantId);
            input.AvatarUrl = uploadedUrl;
        }

        var entity = ObjectMapper.Map<CreateUpdateAppCustomerDto, Customer>(input);
        entity = await Repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<Customer, AppCustomerDto>(entity);
    }

    public override async Task<AppCustomerDto> UpdateAsync(Guid id, CreateUpdateAppCustomerDto input)
    {
        await CheckUpdatePolicyAsync();
        await EnsurePhoneUniqueAsync(input.PhoneNumber, id);

        var entity = await Repository.GetAsync(id);

        // ✅ Nếu có upload avatar mới -> validate + xóa file cũ + upload mới
        if (input.AvatarFile != null)
        {
            await ValidateAvatarAsync(input.AvatarFile);

            if (!string.IsNullOrWhiteSpace(entity.AvatarUrl))
            {
                await _manageImageService.DeleteFileAsync(entity.AvatarUrl);
            }

            var tenantId = CurrentTenant.Id?.ToString() ?? "host";
            var uploadedUrl = await _manageImageService.UploadImageAsync(input.AvatarFile, tenantId);
            input.AvatarUrl = uploadedUrl;
        }
        else
        {
            // ✅ tránh bị ObjectMapper ghi đè AvatarUrl thành null/"" nếu UI không gửi
            input.AvatarUrl = entity.AvatarUrl;
        }

        ObjectMapper.Map(input, entity);
        entity = await Repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<Customer, AppCustomerDto>(entity);
    }

    private async Task ValidateAvatarAsync(IRemoteStreamContent file)
    {
        // ABP: IRemoteStreamContent thường có ContentLength (nullable)
        var len = file.ContentLength;

        if (len.HasValue)
        {
            if (len.Value > AVATAR_MAX_BYTES)
            {
                var mb = (len.Value / (1024d * 1024d)).ToString("0.00", CultureInfo.InvariantCulture);
                throw new BusinessException("Customer:AvatarTooLarge")
                    .WithData("MaxMB", AVATAR_MAX_MB)
                    .WithData("SizeMB", mb);
            }
            return;
        }

        // Fallback: nếu ContentLength null (hiếm) thì đo bằng stream
        // (đọc stream có thể tốn, nhưng chỉ khi server không cung cấp ContentLength)
        using var s = file.GetStream();
        if (s.CanSeek)
        {
            if (s.Length > AVATAR_MAX_BYTES)
            {
                var mb = (s.Length / (1024d * 1024d)).ToString("0.00", CultureInfo.InvariantCulture);
                throw new BusinessException("Customer:AvatarTooLarge")
                    .WithData("MaxMB", AVATAR_MAX_MB)
                    .WithData("SizeMB", mb);
            }
            return;
        }

        // nếu stream không seek được thì vẫn cho qua (tránh đọc toàn bộ stream)
        await Task.CompletedTask;
    }

    private async Task EnsurePhoneUniqueAsync(string phoneNumber, Guid? currentId)
    {
        if (phoneNumber.IsNullOrWhiteSpace())
            throw new BusinessException("Customer:PhoneRequired");

        var normalized = phoneNumber.Trim();

        var exists = await Repository.AnyAsync(c =>
            c.PhoneNumber == normalized &&
            (!currentId.HasValue || c.Id != currentId.Value)
        );

        if (exists)
        {
            throw new BusinessException("Customer:PhoneAlreadyExists")
                .WithData("PhoneNumber", normalized);
        }
    }

    public async Task<AppCustomerDto> GetByPhoneAsync(string phoneNumber)
    {
        await CheckGetPolicyAsync();

        if (phoneNumber.IsNullOrWhiteSpace())
            throw new BusinessException("Customer:PhoneRequired");

        var normalized = phoneNumber.Trim();
        var customer = await Repository.FirstOrDefaultAsync(c => c.PhoneNumber == normalized);

        if (customer == null)
        {
            return null;
        }

        return ObjectMapper.Map<Customer, AppCustomerDto>(customer);
    }
}
