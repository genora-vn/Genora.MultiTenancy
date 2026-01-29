using Genora.MultiTenancy.AppDtos.AppCustomers;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.AppCustomers;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
    private readonly IMiniAppCustomerTypeService _customerTypeService;
    private readonly IManageImageService _manageImageService;
    private readonly AppCustomerExcelTemplateGenerator _customerExcelTemplateGenerator;
    private readonly AppCustomerExcelImporter _customerExcelImporter;
    private readonly IObjectValidator _objectValidator;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    private const int AVATAR_MAX_MB = 15;
    private const long AVATAR_MAX_BYTES = AVATAR_MAX_MB * 1024L * 1024L;

    public AppCustomerService(
        IRepository<Customer, Guid> repository,
        IRepository<CustomerType, Guid> customerTypeRepository,
        IManageImageService manageImageService,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        AppCustomerExcelTemplateGenerator customerExcelTemplateGenerator,
        AppCustomerExcelImporter customerExcelImporter,
        IObjectValidator objectValidator,
        IMiniAppCustomerTypeService customerTypeService,
        IStringLocalizer<MultiTenancyResource> l)
        : base(repository, currentTenant, featureChecker)
    {
        GetPolicyName = MultiTenancyPermissions.AppCustomers.Default;
        GetListPolicyName = MultiTenancyPermissions.AppCustomers.Default;
        CreatePolicyName = MultiTenancyPermissions.AppCustomers.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppCustomers.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppCustomers.Delete;

        _customerTypeRepository = customerTypeRepository;
        _manageImageService = manageImageService;
        _customerExcelTemplateGenerator = customerExcelTemplateGenerator;
        _customerExcelImporter = customerExcelImporter;
        _objectValidator = objectValidator;
        _customerTypeService = customerTypeService;
        _l = l;
    }

    public async Task<string> GenerateCustomerCodeAsync()
    {
        await CheckCreatePolicyAsync();

        const string prefix = "KH";
        var tenantId = CurrentTenant.Id;

        var q = await Repository.GetQueryableAsync();

        // ✅ chỉ lấy tập ACTIVE để tính max
        var activeCodes = await AsyncExecuter.ToListAsync(
            q.Where(c =>
                c.TenantId == tenantId &&
                c.CustomerCode != null &&
                c.CustomerCode.StartsWith(prefix)
            )
            .Select(c => c.CustomerCode!)
        );

        var maxNumber = 0;
        foreach (var code in activeCodes)
        {
            var numberPart = code.Substring(prefix.Length);
            if (int.TryParse(numberPart, NumberStyles.None, CultureInfo.InvariantCulture, out var n))
                if (n > maxNumber) maxNumber = n;
        }

        var nextNumber = maxNumber + 1;
        return $"{prefix}{nextNumber.ToString("D6", CultureInfo.InvariantCulture)}";
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

            ProvinceCode = x.c.ProvinceCode,

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

        var tenantId = CurrentTenant.Id;

        var normalizedPhone = NormalizePhoneTo84(input.PhoneNumber);
        if (normalizedPhone.IsNullOrWhiteSpace())
            throw ErrorHelper.BusinessError(_l, "Customer:PhoneRequired");

        // Kiểm tra Customer code tồn tại hay không
        // Auto CustomerCode KHxxxxxx khi tạo từ UI/import mà không có
        var normalizedCustomerCode = (input.CustomerCode ?? "").Trim();
        if (normalizedCustomerCode.IsNullOrWhiteSpace())
        {
            normalizedCustomerCode = await GenerateCustomerCodeAsync();
            input.CustomerCode = normalizedCustomerCode;
        }

        if (!normalizedCustomerCode.IsNullOrWhiteSpace())
        {
            var existingCustomerCode = await Repository.FirstOrDefaultAsync(c =>
                c.TenantId == tenantId && c.CustomerCode == normalizedCustomerCode
            );

            if (existingCustomerCode != null)
            {
                if (existingCustomerCode.IsActive)
                {
                    throw ErrorHelper.BusinessError(
                            _l,
                            "Customer:CustomerCodeAlreadyExists",
                            detailCode: "Customer:CustomerCodeAlreadyExists_Data",
                            detailArgs: new { CustomerCode = normalizedCustomerCode }
                        )
                        .WithData("CustomerCode", normalizedCustomerCode);
                }

                // CustomerCode tồn tại nhưng inactive -> reActive + update lại
                var customerCodeExist = await Repository.AnyAsync(c =>
                    c.TenantId == tenantId &&
                    c.CustomerCode != existingCustomerCode.CustomerCode &&
                    c.PhoneNumber == normalizedPhone
                );

                if (customerCodeExist)
                {
                    throw ErrorHelper.BusinessError(
                            _l,
                            "Customer:PhoneAlreadyExists",
                            detailCode: "Customer:PhoneAlreadyExists_Data",
                            detailArgs: new { PhoneNumber = normalizedPhone }
                        )
                        .WithData("PhoneNumber", normalizedPhone);
                }

                ObjectMapper.Map(input, existingCustomerCode);

                existingCustomerCode.TenantId = tenantId;
                existingCustomerCode.PhoneNumber = normalizedPhone;
                existingCustomerCode.CustomerCode = normalizedCustomerCode;
                existingCustomerCode.IsActive = true;

                var updated = await Repository.UpdateAsync(existingCustomerCode, autoSave: true);
                return ObjectMapper.Map<Customer, AppCustomerDto>(updated);
            }
        }

       // Kiểm tra theo phone tồn tại hay không?
        var existingByPhone = await Repository.FirstOrDefaultAsync(c =>
            c.TenantId == tenantId && c.PhoneNumber == normalizedPhone
        );

        if (existingByPhone != null)
        {
            if (existingByPhone.IsActive)
            {
                throw ErrorHelper.BusinessError(
                        _l,
                        "Customer:PhoneAlreadyExists",
                        detailCode: "Customer:PhoneAlreadyExists_Data",
                        detailArgs: new { PhoneNumber = normalizedPhone }
                    )
                    .WithData("PhoneNumber", normalizedPhone);
            }

            ObjectMapper.Map(input, existingByPhone);

            existingByPhone.TenantId = tenantId;
            existingByPhone.PhoneNumber = normalizedPhone;
            existingByPhone.CustomerCode = normalizedCustomerCode;
            existingByPhone.IsActive = true;

            var updated = await Repository.UpdateAsync(existingByPhone, autoSave: true);
            return ObjectMapper.Map<Customer, AppCustomerDto>(updated);
        }

        // Thêm mới
        var entity = ObjectMapper.Map<CreateUpdateAppCustomerDto, Customer>(input);
        entity.TenantId = tenantId;
        entity.PhoneNumber = normalizedPhone;
        entity.CustomerCode = normalizedCustomerCode;
        entity.IsActive = true;

        entity = await Repository.InsertAsync(entity, autoSave: true);
        return ObjectMapper.Map<Customer, AppCustomerDto>(entity);
    }

    public override async Task<AppCustomerDto> UpdateAsync(Guid id, CreateUpdateAppCustomerDto input)
    {
        await CheckUpdatePolicyAsync();
        await EnsurePhoneUniqueAsync(input.PhoneNumber, id);

        var entity = await Repository.GetAsync(id);

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
            input.AvatarUrl = entity.AvatarUrl;
        }

        input.PhoneNumber = ToAlt0Format(NormalizePhoneTo84(input.PhoneNumber)) ?? input.PhoneNumber;

        ObjectMapper.Map(input, entity);

        // Cấu hình để luôn lưu DB dạng 84...
        entity.PhoneNumber = NormalizePhoneTo84(entity.PhoneNumber);

        entity = await Repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<Customer, AppCustomerDto>(entity);
    }

    public async Task<IRemoteStreamContent> DownloadImportTemplateAsync()
    {
        await CheckGetPolicyAsync();
        return _customerExcelTemplateGenerator.GenerateTemplate();
    }

    [DisableValidation]
    public async Task<int> ImportExcelAsync(ImportCustomerExcelInput input)
    {
        await CheckCreatePolicyAsync();
        await CheckUpdatePolicyAsync();

        if (input.File == null)
            throw ErrorHelper.BusinessError(_l, "Customer:ImportFileRequired");

        using var stream = input.File.GetStream();
        var rows = _customerExcelImporter.Read(stream);

        // Chuẩn hóa theo đầu số 84 để tránh trùng giữa 0/+84/84
        var seenPhone84 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var success = 0;

        foreach (var r in rows)
        {
            var rowNumber = r.Row;

            var fullName = (r.FullName ?? "").Trim();
            var vgaCode = r.VgaCode?.Trim();
            var phoneRaw = (r.PhoneNumber ?? "").Trim();
            var email = r.Email?.Trim();

            if (string.IsNullOrWhiteSpace(fullName))
                throw ErrorHelper.ImportError(_l, "Customer:FullNameRequired", rowNumber, field: "FullName");

            if (string.IsNullOrWhiteSpace(phoneRaw))
                throw ErrorHelper.ImportError(_l, "Customer:PhoneRequired", rowNumber, field: "PhoneNumber");

            if (fullName.Length > 150)
                throw ErrorHelper.ImportError(
                    _l,
                    "Customer:FullNameTooLong",
                    rowNumber,
                    field: "FullName",
                    value: fullName,
                    detailCode: "Customer:FullNameTooLong_Data",
                    detailArgs: new { Max = 150 }
                );

            if (phoneRaw.Length > 20)
                throw ErrorHelper.ImportError(
                    _l,
                    "Customer:PhoneTooLong",
                    rowNumber,
                    field: "PhoneNumber",
                    value: phoneRaw,
                    detailCode: "Customer:PhoneTooLong_Data",
                    detailArgs: new { Max = 20 }
                );

            if (vgaCode != null && vgaCode.Length > 20)
                throw ErrorHelper.ImportError(
                    _l,
                    "Customer:VgaCodeTooLong",
                    rowNumber,
                    field: "VgaCode",
                    value: vgaCode,
                    detailCode: "Customer:VgaCodeTooLong_Data",
                    detailArgs: new { Max = 20 }
                );

            if (email != null && email.Length > 100)
                throw ErrorHelper.ImportError(
                    _l,
                    "Customer:EmailTooLong",
                    rowNumber,
                    field: "Email",
                    value: email,
                    detailCode: "Customer:EmailTooLong_Data",
                    detailArgs: new { Max = 100 }
                );

            var phone84 = NormalizePhoneTo84(phoneRaw);

            if (!seenPhone84.Add(phone84))
                continue;

            // Trong model DTO regex hiện tại chỉ accept 0... hoặc +84...
            // Convert 84 về 0 để validator pass
            var phoneForDto = ToAlt0Format(phone84) ?? phoneRaw;
            var customerType = await _customerTypeService.GetCustomerTypeByCode("MB");

            var dto = new CreateUpdateAppCustomerDto
            {
                FullName = fullName,
                PhoneNumber = phoneForDto,
                VgaCode = vgaCode,
                DateOfBirth = r.DateOfBirth,
                Email = email,
                CustomerTypeId = customerType?.Id,
                CustomerSource = CustomerSource.Other,
                IsActive = true
            };

            try
            {
                await _objectValidator.ValidateAsync(dto);
            }
            catch (AbpValidationException ex)
            {
                var details = string.Join(" | ",
                    ex.ValidationErrors.Select(e =>
                        $"{string.Join(",", e.MemberNames)}: {e.ErrorMessage}")
                );

                throw ErrorHelper.ImportError(
                    _l,
                    "Customer:ImportRowValidationFailed",
                    rowNumber,
                    detailCode: "Customer:ImportRowValidationFailed_Data",
                    detailArgs: new { Details = details }
                );
            }

            await CreateAsync(dto);
            success++;
        }

        return success;
    }

    private async Task ValidateAvatarAsync(IRemoteStreamContent file)
    {
        var len = file.ContentLength;

        if (len.HasValue)
        {
            if (len.Value > AVATAR_MAX_BYTES)
            {
                var mb = (len.Value / (1024d * 1024d)).ToString("0.00", CultureInfo.InvariantCulture);
                throw ErrorHelper.BusinessError(
                        _l,
                        "Customer:AvatarTooLarge",
                        messageArgs: new { MaxMB = AVATAR_MAX_MB, SizeMB = mb }
                    )
                    .WithData("MaxMB", AVATAR_MAX_MB)
                    .WithData("SizeMB", mb);
            }
            return;
        }

        using var s = file.GetStream();
        if (s.CanSeek)
        {
            if (s.Length > AVATAR_MAX_BYTES)
            {
                var mb = (s.Length / (1024d * 1024d)).ToString("0.00", CultureInfo.InvariantCulture);
                throw ErrorHelper.BusinessError(
                        _l,
                        "Customer:AvatarTooLarge",
                        messageArgs: new { MaxMB = AVATAR_MAX_MB, SizeMB = mb }
                    )
                    .WithData("MaxMB", AVATAR_MAX_MB)
                    .WithData("SizeMB", mb);
            }
            return;
        }

        await Task.CompletedTask;
    }

    private async Task EnsurePhoneUniqueAsync(string phoneNumber, Guid? currentId)
    {
        if (phoneNumber.IsNullOrWhiteSpace())
            throw ErrorHelper.BusinessError(_l, "Customer:PhoneRequired");

        var normalized = NormalizePhoneTo84(phoneNumber);

        var exists = await Repository.AnyAsync(c =>
            c.PhoneNumber == normalized &&
            (!currentId.HasValue || c.Id != currentId.Value)
        );

        if (exists)
        {
            throw ErrorHelper.BusinessError(
                    _l,
                    "Customer:PhoneAlreadyExists",
                    detailCode: "Customer:PhoneAlreadyExists_Data",
                    detailArgs: new { PhoneNumber = normalized }
                )
                .WithData("PhoneNumber", normalized);
        }
    }

    public async Task<AppCustomerDto> GetByPhoneAsync(string phoneNumber)
    {
        await CheckGetPolicyAsync();

        if (phoneNumber.IsNullOrWhiteSpace())
            throw ErrorHelper.BusinessError(_l, "Customer:PhoneRequired");

        var normalized = NormalizePhoneTo84(phoneNumber);
        var customer = await Repository.FirstOrDefaultAsync(c => c.PhoneNumber == normalized);

        return ObjectMapper.Map<Customer, AppCustomerDto>(customer);
    }

    private static string? ToAlt0Format(string phone84)
    {
        var p = (phone84 ?? "").Trim();
        if (p.StartsWith("+84")) p = "84" + p.Substring(3);
        if (p.StartsWith("84") && p.Length > 2)
            return "0" + p.Substring(2);
        return null;
    }

    private string NormalizePhoneTo84(string input)
    {
        var s = (input ?? "").Trim();
        s = Regex.Replace(s, @"[\s\.\-\(\)]", "");

        if (string.IsNullOrWhiteSpace(s))
            throw ErrorHelper.BusinessError(_l, "Customer:PhoneRequired");

        if (s.StartsWith("+")) s = s.Substring(1);

        // Format đầu 0 sang 84
        if (s.StartsWith("0"))
        {
            var rest = s.Substring(1);
            return "84" + rest;
        }

        // Format để chuẩn 84 loại bỏ kiểu 840
        if (s.StartsWith("84"))
        {
            var rest = s.Substring(2);
            if (rest.StartsWith("0")) rest = rest.Substring(1);
            return "84" + rest;
        }

        // Xử lý khi file excel hoặc tạo mới mất số 0 (vd 974456114)
        if (Regex.IsMatch(s, @"^\d{9,10}$"))
        {
            return "84" + s;
        }

        throw ErrorHelper.BusinessError(
                _l,
                "Customer:PhoneInvalid",
                detailCode: "Customer:PhoneInvalid_Data",
                detailArgs: new { PhoneNumber = input }
            )
            .WithData("PhoneNumber", input);
    }
}
