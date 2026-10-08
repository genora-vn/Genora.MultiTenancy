using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Hlg;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace Genora.MultiTenancy.AppServices.Hlg;

public partial class HlgProfileAppService
{
    /// <summary>Read-only preflight. Upsert rechecks local state under a transaction lock.</summary>
    public async Task<HlgCustomerCheckDto> CheckCustomerAsync(string phone, string? pharmaPhone = null, CancellationToken ct = default)
    {
        phone = RequireRegistrationPhone(phone);
        pharmaPhone = RequireRegistrationPhone(pharmaPhone ?? phone);
        var branches = await GetDmsBranchesAsync(pharmaPhone, ct);
        var customer = await FindRegistrationCustomerAsync(phone, ct);
        var profile = customer == null ? null : await FindRegistrationProfileAsync(customer.Id, ct);
        var count = await ValidatePharmacyMembershipAsync(phone, pharmaPhone, profile, ct);
        return new HlgCustomerCheckDto
        {
            Phone = phone, PharmaPhone = pharmaPhone, IsOwner = phone == pharmaPhone,
            CanRegister = true, LinkedAccountCount = count,
            MaxLinkedAccounts = HlgRegistrationRules.MaxLinkedAccounts, Branches = branches
        };
    }

    public async Task<GamificationUserDto> UpsertCustomerAsync(HlgCustomerUpsertPayloadDto payload, CancellationToken ct = default)
    {
        if (payload == null) throw RegistrationError("InvalidInput", "Thiếu thông tin đăng ký.");
        ValidatePharmacyCode(payload.PharmacyCode ?? payload.VgaCode);
        ValidateCustomerType(payload.CustomerType);
        var phone = RequireRegistrationPhone(payload.Phone);
        var pharmaPhone = RequireRegistrationPhone(payload.PharmaPhone ?? phone);
        var selectedCode = NullIfBlank(payload.CustomerCode);
        if (selectedCode == null || selectedCode.Length > 50)
            throw RegistrationError("BranchRequired", "Vui lòng chọn chi nhánh nhà thuốc trước khi đăng ký.");
        if (payload.FullName?.Length > 150 || payload.Address?.Length > 500
            || payload.AvatarUrl?.Length > 500 || payload.ZaloUserId?.Length > 100)
            throw RegistrationError("InvalidInput", "Thông tin đăng ký vượt quá độ dài cho phép.");

        // Never hold the registration lock while waiting for the partner HTTP service.
        var branches = await GetDmsBranchesAsync(pharmaPhone, ct);
        var branch = branches.FirstOrDefault(x => string.Equals(x.CustCode?.Trim(), selectedCode, StringComparison.OrdinalIgnoreCase))
            ?? throw RegistrationError("InvalidBranch", "Chi nhánh đã chọn không thuộc số điện thoại nhà thuốc.");

        using var uow = _registrationUow.Begin(requiresNew: true, isTransactional: true);
        await _registrationLock.AcquireAsync(ct);
        var customer = await FindRegistrationCustomerAsync(phone, ct);
        var profile = customer == null ? null : await FindRegistrationProfileAsync(customer.Id, ct);
        await ValidatePharmacyMembershipAsync(phone, pharmaPhone, profile, ct);

        var isOwner = phone == pharmaPhone;
        var name = NullIfBlank(payload.FullName) ?? customer?.FullName ?? "Zalo User";
        var branchCode = branch.CustCode!.Trim();
        var isNew = customer == null;
        if (customer == null)
        {
            var code = isOwner ? branchCode : await GenerateCustomerCodeAsync();
            await EnsureCustomerCodeAvailableAsync(code, null, ct);
            customer = new Customer(GuidGenerator.Create(), phone, name)
            {
                TenantId = _currentTenant.Id,
                CustomerCode = code,
                CustomerSource = CustomerSource.ZaloMiniApp,
                IsActive = true
            };
        }
        else if (isOwner && string.IsNullOrWhiteSpace(customer.CustomerCode))
        {
            // Preserve established Sales identities; store HLG branch choice on the profile.
            await EnsureCustomerCodeAvailableAsync(branchCode, customer.Id, ct);
            customer.CustomerCode = branchCode;
        }

        customer.FullName = name;
        customer.AvatarUrl = NullIfBlank(payload.AvatarUrl) ?? customer.AvatarUrl;
        customer.ZaloUserId = NullIfBlank(payload.ZaloUserId) ?? customer.ZaloUserId;
        if (payload.IsFollower.HasValue) customer.IsFollower = payload.IsFollower.Value;
        customer.Address = NullIfBlank(payload.Address) ?? customer.Address;
        customer.Gender = HlgEnumMapper.GenderStringToByte(payload.Gender) ?? customer.Gender;
        customer.DateOfBirth = ParseDate(payload.Birthday) ?? customer.DateOfBirth;
        if (isNew) await _customerRepo.InsertAsync(customer, autoSave: true, cancellationToken: ct);
        else await _customerRepo.UpdateAsync(customer, autoSave: true, cancellationToken: ct);

        var isNewProfile = profile == null;
        profile ??= new HlgUserProfile(GuidGenerator.Create(), customer.Id, _currentTenant.Id);
        profile.PharmaPhone = pharmaPhone;
        profile.DmsCustomerCode = branchCode;
        profile.ZaloId = customer.ZaloUserId ?? profile.ZaloId;
        profile.PharmacyCode = NullIfBlank(payload.PharmacyCode ?? payload.VgaCode) ?? profile.PharmacyCode;
        var customerType = HlgEnumMapper.CustomerTypeFromString(payload.CustomerType);
        if (customerType.HasValue) profile.CustomerType = customerType;
        if (profile.CustomerType.HasValue) profile.IsRegistered = true;
        if (isNewProfile) await _profileRepo.InsertAsync(profile, autoSave: true, cancellationToken: ct);
        else await _profileRepo.UpdateAsync(profile, autoSave: true, cancellationToken: ct);

        await uow.CompleteAsync(ct);
        return MapToDto(customer, profile);
    }

    private async Task<List<HlCustomerDto>> GetDmsBranchesAsync(string pharmaPhone, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var result = await _dms.GetCustomerByPhoneAsync(pharmaPhone);
        ct.ThrowIfCancellationRequested();
        if (result == null || !result.Success)
            throw RegistrationError("DmsUnavailable", "Không thể kiểm tra thông tin nhà thuốc lúc này. Vui lòng thử lại sau.");
        var branches = (result.Data ?? new List<HlCustomerDto>())
            .Where(x => x != null && x.IsCustomer != false && !string.IsNullOrWhiteSpace(x.CustCode)
                && x.CustCode.Trim().Length <= 50
                && (NormalizePhone(x.CustPhone) == pharmaPhone || NormalizePhone(x.Phone) == pharmaPhone))
            .GroupBy(x => x.CustCode!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First()).ToList();
        if (branches.Count == 0)
            throw RegistrationError("DmsCustomerNotFound", HlgRegistrationRules.DmsCustomerNotFound);
        return branches;
    }

    private async Task<int> ValidatePharmacyMembershipAsync(string phone, string pharmaPhone, HlgUserProfile? profile, CancellationToken ct)
    {
        if (profile?.PharmaPhone != null && profile.PharmaPhone != pharmaPhone)
            throw RegistrationError("PharmacyAlreadyLinked", "Tài khoản đã liên kết với nhà thuốc khác. Vui lòng liên hệ hỗ trợ.");
        if (phone != pharmaPhone)
        {
            var owner = await FindRegistrationCustomerAsync(pharmaPhone, ct);
            if (owner == null) throw RegistrationError("OwnerRequired", HlgRegistrationRules.OwnerRequired);
            var ownerProfile = await FindRegistrationProfileAsync(owner.Id, ct);
            if (ownerProfile?.PharmaPhone != null && ownerProfile.PharmaPhone != pharmaPhone)
                throw RegistrationError("OwnerRequired", HlgRegistrationRules.OwnerRequired);
        }

        var customers = await _customerRepo.GetQueryableAsync();
        var profiles = await _profileRepo.GetQueryableAsync();
        var linkedPhones = await AsyncExecuter.ToListAsync(
            from p in profiles
            join c in customers on p.CustomerId equals c.Id
            where p.TenantId == _currentTenant.Id && c.TenantId == _currentTenant.Id
                && !p.IsDeleted && !c.IsDeleted && p.PharmaPhone == pharmaPhone
            select c.PhoneNumber, ct);
        // Reserve exactly one owner place, including owners registered through Sales only.
        var count = linkedPhones.Select(NormalizePhone).Where(x => x != null)
            .Append(pharmaPhone).Distinct(StringComparer.Ordinal).Count();
        if (phone != pharmaPhone && profile?.PharmaPhone != pharmaPhone && count >= HlgRegistrationRules.MaxLinkedAccounts)
            throw RegistrationError("AccountLimitReached", HlgRegistrationRules.AccountLimitReached);
        return count;
    }

    private async Task<Customer?> FindRegistrationCustomerAsync(string phone, CancellationToken ct)
    {
        var aliases = HlgRegistrationRules.PhoneAliases(phone);
        var rows = await AsyncExecuter.ToListAsync((await _customerRepo.GetQueryableAsync())
            .Where(x => x.TenantId == _currentTenant.Id && !x.IsDeleted && aliases.Contains(x.PhoneNumber))
            .Take(2), ct);
        if (rows.Count > 1)
            throw RegistrationError("AmbiguousCustomer", "Số điện thoại có nhiều hồ sơ khách hàng. Vui lòng liên hệ hỗ trợ.");
        if (rows.Count == 0)
        {
            // Deleted customer phones can still be reserved by SQL uniqueness constraints.
            using var includeDeleted = DataFilter.Disable<ISoftDelete>();
            if (await _customerRepo.AnyAsync(x => x.TenantId == _currentTenant.Id && x.IsDeleted && aliases.Contains(x.PhoneNumber), ct))
                throw RegistrationError("CustomerUnavailable", "Hồ sơ số điện thoại này đã bị xóa. Vui lòng liên hệ hỗ trợ để khôi phục.");
        }
        return rows.FirstOrDefault();
    }

    private async Task<HlgUserProfile?> FindRegistrationProfileAsync(Guid customerId, CancellationToken ct)
    {
        var rows = await AsyncExecuter.ToListAsync((await _profileRepo.GetQueryableAsync())
            .Where(x => x.TenantId == _currentTenant.Id && !x.IsDeleted && x.CustomerId == customerId).Take(2), ct);
        if (rows.Count > 1)
            throw RegistrationError("AmbiguousCustomer", "Khách hàng có nhiều hồ sơ HLG. Vui lòng liên hệ hỗ trợ.");
        if (rows.Count == 0)
        {
            using var includeDeleted = DataFilter.Disable<ISoftDelete>();
            if (await _profileRepo.AnyAsync(x => x.TenantId == _currentTenant.Id && x.CustomerId == customerId && x.IsDeleted, ct))
                throw RegistrationError("CustomerUnavailable", "Hồ sơ HLG đã bị xóa. Vui lòng liên hệ hỗ trợ để khôi phục.");
        }
        return rows.FirstOrDefault();
    }

    private async Task EnsureCustomerCodeAvailableAsync(string code, Guid? currentCustomerId, CancellationToken ct)
    {
        using var includeDeleted = DataFilter.Disable<ISoftDelete>();
        if (await _customerRepo.AnyAsync(x => x.TenantId == _currentTenant.Id && x.CustomerCode == code && x.Id != currentCustomerId, ct))
            throw RegistrationError("CustomerCodeInUse", "Mã chi nhánh đã gắn với khách hàng khác. Vui lòng liên hệ hỗ trợ.");
    }

    private static string RequireRegistrationPhone(string? value)
    {
        var phone = NormalizePhone(value);
        if (!HlgRegistrationRules.IsValidPhone(phone))
            throw RegistrationError("InvalidPhone", "Số điện thoại không hợp lệ.");
        return phone!;
    }

    private static UserFriendlyException RegistrationError(string code, string message)
        => new(message, "HlgRegistration:" + code);
}
