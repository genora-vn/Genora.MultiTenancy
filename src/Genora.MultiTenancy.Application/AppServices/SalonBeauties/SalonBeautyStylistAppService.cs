using System;
using System.Linq;
using System.Text.RegularExpressions;
using Volo.Abp.Content;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyStylists;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Genora.MultiTenancy.AppServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.SalonBeauties;

[Authorize]
public class SalonBeautyStylistAppService :
    FeatureProtectedCrudAppService<
        SalonBeautyStylist,
        SalonBeautyStylistDto,
        Guid,
        GetSalonBeautyListInput,
        CreateSalonBeautyStylistDto,
        UpdateSalonBeautyStylistDto>,
    ISalonBeautyStylistAppService
{
    protected override string FeatureName => string.Empty;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.SalonBeautyStylists.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostSalonBeautyStylists.Default;
    private const long MaxAvatarBytes = 2 * 1024 * 1024;
    private static readonly string[] AvatarAllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly IRepository<SalonBeautyStylist, Guid> _repository;
    private readonly IStringLocalizer<MultiTenancyResource> _l;
    private readonly IManageImageService _manageImageService;

    public SalonBeautyStylistAppService(
        IRepository<SalonBeautyStylist, Guid> repository,
        IStringLocalizer<MultiTenancyResource> l,
        IManageImageService manageImageService,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(repository, currentTenant, featureChecker)
    {
        _repository = repository;
        _l = l;
        _manageImageService = manageImageService;
        LocalizationResource = typeof(MultiTenancyResource);
    }

    public override async Task<PagedResultDto<SalonBeautyStylistDto>> GetListAsync(GetSalonBeautyListInput input)
    {
        await CheckStylistPolicyAsync(
            MultiTenancyPermissions.SalonBeautyStylists.Default,
            MultiTenancyPermissions.HostSalonBeautyStylists.Default);

        input.MaxResultCount = input.MaxResultCount <= 0 ? 10 : Math.Min(input.MaxResultCount, 100);

        var query = await _repository.GetQueryableAsync();

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var keyword = input.FilterText.Trim();
            query = query.Where(x =>
                x.DisplayName.Contains(keyword) ||
                (x.Phone != null && x.Phone.Contains(keyword)) ||
                (x.Note != null && x.Note.Contains(keyword)));
        }

        if (input.Gender.HasValue)
            query = query.Where(x => x.Gender == input.Gender.Value);

        if (input.Role.HasValue)
            query = query.Where(x => x.Role == input.Role.Value);

        if (input.Level.HasValue)
            query = query.Where(x => x.Level == input.Level.Value);

        if (input.Status.HasValue)
            query = query.Where(x => x.Status == input.Status.Value);

        if (input.IsShowOnApp.HasValue)
            query = query.Where(x => x.IsShowOnApp == input.IsShowOnApp.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var items = await AsyncExecuter.ToListAsync(
            query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.DisplayName)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<SalonBeautyStylistDto>(
            totalCount,
            items.Select(MapToDto).ToList());
    }

    public override async Task<SalonBeautyStylistDto> GetAsync(Guid id)
    {
        await CheckStylistPolicyAsync(
            MultiTenancyPermissions.SalonBeautyStylists.Default,
            MultiTenancyPermissions.HostSalonBeautyStylists.Default);

        var entity = await _repository.GetAsync(id);
        return MapToDto(entity);
    }

    public override async Task<SalonBeautyStylistDto> CreateAsync(CreateSalonBeautyStylistDto input)
    {
        await CheckStylistPolicyAsync(
            MultiTenancyPermissions.SalonBeautyStylists.Create,
            MultiTenancyPermissions.HostSalonBeautyStylists.Create);

        var avatarUrl = await ResolveAvatarAsync(input.Avatar, input.Images, input.IsUploadImage);

        NormalizeAndValidate(input.DisplayName, input.Phone, input.ExperienceYear, input.Role, input.Level, input.Status, input.IsShowOnApp, avatarUrl, input.SortOrder);

        var entity = new SalonBeautyStylist
        {
            DisplayName = input.DisplayName.Trim(),
            Avatar = NullIfWhiteSpace(avatarUrl),
            Phone = NormalizePhone(input.Phone),
            Gender = input.Gender,
            Role = input.Role,
            Level = input.Level,
            ExperienceYear = input.ExperienceYear,
            Status = input.Status,
            IsShowOnApp = input.IsShowOnApp,
            Note = NullIfWhiteSpace(input.Note),
            SortOrder = input.SortOrder
        };

        var created = await _repository.InsertAsync(entity, autoSave: true);
        return MapToDto(created);
    }

    public override async Task<SalonBeautyStylistDto> UpdateAsync(Guid id, UpdateSalonBeautyStylistDto input)
    {
        await CheckStylistPolicyAsync(
            MultiTenancyPermissions.SalonBeautyStylists.Edit,
            MultiTenancyPermissions.HostSalonBeautyStylists.Edit);

        var entity = await _repository.GetAsync(id);
        var avatarUrl = await ResolveAvatarAsync(input.Avatar, input.Images, input.IsUploadImage, entity.Avatar);

        NormalizeAndValidate(input.DisplayName, input.Phone, input.ExperienceYear, input.Role, input.Level, input.Status, input.IsShowOnApp, avatarUrl, input.SortOrder);

        if (input.IsUploadImage && input.Images != null && (input.Images.ContentLength ?? 0) > 0)
        {
            await DeleteOldAvatarIfLocalAsync(entity.Avatar);
        }

        entity.DisplayName = input.DisplayName.Trim();
        entity.Avatar = NullIfWhiteSpace(avatarUrl);
        entity.Phone = NormalizePhone(input.Phone);
        entity.Gender = input.Gender;
        entity.Role = input.Role;
        entity.Level = input.Level;
        entity.ExperienceYear = input.ExperienceYear;
        entity.Status = input.Status;
        entity.IsShowOnApp = input.IsShowOnApp;
        entity.Note = NullIfWhiteSpace(input.Note);
        entity.SortOrder = input.SortOrder;

        var updated = await _repository.UpdateAsync(entity, autoSave: true);
        return MapToDto(updated);
    }

    public async Task UpdateShowOnAppAsync(Guid id, bool isShowOnApp)
    {
        await CheckStylistPolicyAsync(
            MultiTenancyPermissions.SalonBeautyStylists.Edit,
            MultiTenancyPermissions.HostSalonBeautyStylists.Edit);

        var entity = await _repository.GetAsync(id);

        if (isShowOnApp && entity.Status != 1)
            throw new UserFriendlyException(L("SalonBeautyStylists:ShowOnAppRequiresActive"));

        if (isShowOnApp && entity.Avatar.IsNullOrWhiteSpace())
            throw new UserFriendlyException(L("SalonBeautyStylists:ShowOnAppRequiresAvatar"));

        entity.IsShowOnApp = isShowOnApp;
        await _repository.UpdateAsync(entity, autoSave: true);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckStylistPolicyAsync(
            MultiTenancyPermissions.SalonBeautyStylists.Delete,
            MultiTenancyPermissions.HostSalonBeautyStylists.Delete);

        await _repository.DeleteAsync(id, autoSave: true);
    }

    private async Task<string?> ResolveAvatarAsync(
        string? avatarUrl,
        IRemoteStreamContent? imageFile,
        bool isUploadImage,
        string? currentAvatar = null)
    {
        if (!isUploadImage)
            return NullIfWhiteSpace(avatarUrl);

        if (imageFile == null || (imageFile.ContentLength ?? 0) <= 0)
        {
            // Edit mode: nếu bật upload nhưng chưa chọn ảnh mới, giữ ảnh hiện tại/URL hiện tại.
            return NullIfWhiteSpace(avatarUrl) ?? NullIfWhiteSpace(currentAvatar);
        }

        ValidateAvatarFile(imageFile);

        var tenantId = CurrentTenant.Id?.ToString() ?? "host";
        return await _manageImageService.UploadImageAsync(
            imageFile,
            tenantId,
            subFolder: "salon-stylists",
            allowedExtensions: AvatarAllowedExtensions);
    }

    private void ValidateAvatarFile(IRemoteStreamContent file)
    {
        if (file == null || (file.ContentLength ?? 0) <= 0)
            throw new UserFriendlyException(L("SalonBeautyStylists:AvatarFileRequired"));

        if ((file.ContentLength ?? 0) > MaxAvatarBytes)
            throw new UserFriendlyException(L("SalonBeautyStylists:AvatarMaxSize"));

        var fileName = file.FileName ?? string.Empty;
        var extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !AvatarAllowedExtensions.Contains(extension))
            throw new UserFriendlyException(L("SalonBeautyStylists:AvatarInvalidType"));

        var contentType = file.ContentType ?? string.Empty;
        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new UserFriendlyException(L("SalonBeautyStylists:AvatarInvalidType"));
    }

    private async Task DeleteOldAvatarIfLocalAsync(string? oldAvatar)
    {
        if (!oldAvatar.IsNullOrWhiteSpace() && oldAvatar!.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                await _manageImageService.DeleteFileAsync(oldAvatar);
            }
            catch
            {
                // Không chặn luồng cập nhật nếu file cũ không xóa được.
            }
        }
    }

    private void NormalizeAndValidate(
        string? displayName,
        string? phone,
        int experienceYear,
        byte? role,
        byte? level,
        byte status,
        bool isShowOnApp,
        string? avatar,
        int sortOrder)
    {
        if (displayName.IsNullOrWhiteSpace())
            throw new UserFriendlyException(L("SalonBeautyStylists:DisplayNameRequired"));

        if (displayName!.Trim().Length > 255)
            throw new UserFriendlyException(L("SalonBeautyStylists:DisplayNameMaxLength"));

        if (!phone.IsNullOrWhiteSpace() && !Regex.IsMatch(phone.Trim(), @"^0\d{9,10}$"))
            throw new UserFriendlyException(L("SalonBeautyStylists:PhoneInvalid"));

        if (!role.HasValue || !Enum.IsDefined(typeof(SalonBeautyStylistRole), role.Value))
            throw new UserFriendlyException(L("SalonBeautyStylists:RoleRequired"));

        if (!level.HasValue || !Enum.IsDefined(typeof(SalonBeautyStylistLevel), level.Value))
            throw new UserFriendlyException(L("SalonBeautyStylists:LevelRequired"));

        if (experienceYear < 0 || experienceYear > 50)
            throw new UserFriendlyException(L("SalonBeautyStylists:ExperienceInvalid"));

        if (sortOrder < 0)
            throw new UserFriendlyException(L("SalonBeautyStylists:SortOrderInvalid"));

        if (status != 0 && status != 1)
            throw new UserFriendlyException(L("SalonBeautyStylists:StatusInvalid"));

        if (isShowOnApp && status != 1)
            throw new UserFriendlyException(L("SalonBeautyStylists:ShowOnAppRequiresActive"));

        if (isShowOnApp && avatar.IsNullOrWhiteSpace())
            throw new UserFriendlyException(L("SalonBeautyStylists:ShowOnAppRequiresAvatar"));
    }

    private string L(string key)
    {
        var text = _l[key].Value;
        return text.IsNullOrWhiteSpace() || text == key ? key : text;
    }

    private string? LocalizeEnum<TEnum>(byte? value) where TEnum : struct, Enum
    {
        if (!value.HasValue || !Enum.IsDefined(typeof(TEnum), value.Value))
            return null;

        var enumValue = (TEnum)Enum.ToObject(typeof(TEnum), value.Value);
        var key = $"Enum:{typeof(TEnum).Name}.{enumValue}";
        var text = _l[key].Value;
        return text.IsNullOrWhiteSpace() || text == key ? enumValue.ToString() : text;
    }

    private static string? NormalizePhone(string? phone)
        => phone.IsNullOrWhiteSpace() ? null : Regex.Replace(phone.Trim(), @"\D", "");

    private static string? NullIfWhiteSpace(string? value)
        => value.IsNullOrWhiteSpace() ? null : value.Trim();

    private async Task CheckStylistPolicyAsync(string tenantPermission, string hostPermission)
    {
        var permission = CurrentTenant.IsAvailable ? tenantPermission : hostPermission;
        if (permission.IsNullOrWhiteSpace())
            throw new AbpAuthorizationException("Missing Salon Beauty stylist permission.");

        await AuthorizationService.CheckAsync(permission);
    }

    private SalonBeautyStylistDto MapToDto(SalonBeautyStylist entity)
    {
        var active = entity.Status == 1;
        return new SalonBeautyStylistDto
        {
            Id = entity.Id,
            DisplayName = entity.DisplayName,
            Avatar = entity.Avatar,
            Phone = entity.Phone,
            PhoneMasked = PhoneHelper.MaskPhone(entity.Phone),
            Gender = entity.Gender,
            GenderText = LocalizeEnum<SalonBeautyGender>(entity.Gender),
            Role = entity.Role,
            RoleText = LocalizeEnum<SalonBeautyStylistRole>(entity.Role),
            Level = entity.Level,
            LevelText = LocalizeEnum<SalonBeautyStylistLevel>(entity.Level),
            ExperienceYear = entity.ExperienceYear,
            RatingAvg = entity.RatingAvg,
            TotalBooking = entity.TotalBooking,
            Status = entity.Status,
            StatusText = active ? L("SalonBeautyCustomer:StatusActive") : L("SalonBeautyCustomer:StatusInactive"),
            IsShowOnApp = entity.IsShowOnApp,
            IsShowOnAppText = entity.IsShowOnApp ? L("Yes") : L("No"),
            Note = entity.Note,
            SortOrder = entity.SortOrder
        };
    }
}
