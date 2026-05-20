using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLocations;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Genora.MultiTenancy.AppServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.SalonBeauties;

[Authorize]
public class SalonBeautyLocationAppService :
    FeatureProtectedCrudAppService<
        SalonBeautyLocation,
        SalonBeautyLocationDto,
        Guid,
        GetSalonBeautyLocationListInput,
        CreateSalonBeautyLocationDto,
        UpdateSalonBeautyLocationDto>,
    ISalonBeautyLocationAppService
{
    protected override string FeatureName => string.Empty;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.SalonBeautyLocations.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostSalonBeautyLocations.Default;
    private const long MaxImageBytes = 2 * 1024 * 1024;
    private static readonly string[] ImageAllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly IRepository<SalonBeautyLocation, Guid> _repository;
    private readonly IStringLocalizer<MultiTenancyResource> _l;
    private readonly IManageImageService _manageImageService;

    public SalonBeautyLocationAppService(
        IRepository<SalonBeautyLocation, Guid> repository,
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

    public override async Task<PagedResultDto<SalonBeautyLocationDto>> GetListAsync(GetSalonBeautyLocationListInput input)
    {
        await CheckLocationPolicyAsync(
            MultiTenancyPermissions.SalonBeautyLocations.Default,
            MultiTenancyPermissions.HostSalonBeautyLocations.Default);

        input.MaxResultCount = input.MaxResultCount <= 0 ? 10 : Math.Min(input.MaxResultCount, 100);

        var query = await _repository.GetQueryableAsync();

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var keyword = input.FilterText.Trim();
            query = query.Where(x =>
                x.Name.Contains(keyword) ||
                x.Address.Contains(keyword) ||
                (x.Phone != null && x.Phone.Contains(keyword)));
        }

        if (input.IsActive.HasValue)
            query = query.Where(x => x.IsActive == input.IsActive.Value);

        if (input.IsShowOnApp.HasValue)
            query = query.Where(x => x.IsShowOnApp == input.IsShowOnApp.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var items = await AsyncExecuter.ToListAsync(
            query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<SalonBeautyLocationDto>(
            totalCount,
            items.Select(MapToDto).ToList());
    }

    public override async Task<SalonBeautyLocationDto> GetAsync(Guid id)
    {
        await CheckLocationPolicyAsync(
            MultiTenancyPermissions.SalonBeautyLocations.Default,
            MultiTenancyPermissions.HostSalonBeautyLocations.Default);

        var entity = await _repository.GetAsync(id);
        return MapToDto(entity);
    }

    public async Task<List<SalonBeautyLocationLookupDto>> GetLookupAsync()
    {
        await CheckLocationPolicyAsync(
            MultiTenancyPermissions.SalonBeautyLocations.Default,
            MultiTenancyPermissions.HostSalonBeautyLocations.Default);

        var query = await _repository.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name));

        return items
            .Select(x => new SalonBeautyLocationLookupDto
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive
            })
            .ToList();
    }

    public override async Task<SalonBeautyLocationDto> CreateAsync(CreateSalonBeautyLocationDto input)
    {
        await CheckLocationPolicyAsync(
            MultiTenancyPermissions.SalonBeautyLocations.Create,
            MultiTenancyPermissions.HostSalonBeautyLocations.Create);

        var imageUrl = await ResolveImageAsync(input.ImageUrl, input.Images, input.IsUploadImage);

        NormalizeAndValidate(input.Name, input.Address, input.Phone, input.OpenTime, input.CloseTime, input.SortOrder);

        var entity = new SalonBeautyLocation
        {
            Name = input.Name.Trim(),
            Address = input.Address.Trim(),
            Phone = NormalizePhone(input.Phone),
            OpenTime = input.OpenTime,
            CloseTime = input.CloseTime,
            ImageUrl = NullIfWhiteSpace(imageUrl),
            IsActive = input.IsActive,
            IsShowOnApp = input.IsShowOnApp,
            Note = NullIfWhiteSpace(input.Note),
            SortOrder = input.SortOrder
        };

        var created = await _repository.InsertAsync(entity, autoSave: true);
        return MapToDto(created);
    }

    public override async Task<SalonBeautyLocationDto> UpdateAsync(Guid id, UpdateSalonBeautyLocationDto input)
    {
        await CheckLocationPolicyAsync(
            MultiTenancyPermissions.SalonBeautyLocations.Edit,
            MultiTenancyPermissions.HostSalonBeautyLocations.Edit);

        var entity = await _repository.GetAsync(id);
        var imageUrl = await ResolveImageAsync(input.ImageUrl, input.Images, input.IsUploadImage, entity.ImageUrl);

        NormalizeAndValidate(input.Name, input.Address, input.Phone, input.OpenTime, input.CloseTime, input.SortOrder);

        if (input.IsUploadImage && input.Images != null && (input.Images.ContentLength ?? 0) > 0)
        {
            await DeleteOldImageIfLocalAsync(entity.ImageUrl);
        }

        entity.Name = input.Name.Trim();
        entity.Address = input.Address.Trim();
        entity.Phone = NormalizePhone(input.Phone);
        entity.OpenTime = input.OpenTime;
        entity.CloseTime = input.CloseTime;
        entity.ImageUrl = NullIfWhiteSpace(imageUrl);
        entity.IsActive = input.IsActive;
        entity.IsShowOnApp = input.IsShowOnApp;
        entity.Note = NullIfWhiteSpace(input.Note);
        entity.SortOrder = input.SortOrder;

        var updated = await _repository.UpdateAsync(entity, autoSave: true);
        return MapToDto(updated);
    }

    public async Task UpdateActiveAsync(Guid id, bool isActive)
    {
        await CheckLocationPolicyAsync(
            MultiTenancyPermissions.SalonBeautyLocations.Edit,
            MultiTenancyPermissions.HostSalonBeautyLocations.Edit);

        var entity = await _repository.GetAsync(id);

        if (!isActive && entity.IsShowOnApp)
            entity.IsShowOnApp = false;

        entity.IsActive = isActive;
        await _repository.UpdateAsync(entity, autoSave: true);
    }

    public async Task UpdateShowOnAppAsync(Guid id, bool isShowOnApp)
    {
        await CheckLocationPolicyAsync(
            MultiTenancyPermissions.SalonBeautyLocations.Edit,
            MultiTenancyPermissions.HostSalonBeautyLocations.Edit);

        var entity = await _repository.GetAsync(id);

        if (isShowOnApp && !entity.IsActive)
            throw new UserFriendlyException(L("SalonBeautyLocations:ShowOnAppRequiresActive"));

        entity.IsShowOnApp = isShowOnApp;
        await _repository.UpdateAsync(entity, autoSave: true);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckLocationPolicyAsync(
            MultiTenancyPermissions.SalonBeautyLocations.Delete,
            MultiTenancyPermissions.HostSalonBeautyLocations.Delete);

        await _repository.DeleteAsync(id, autoSave: true);
    }

    private async Task<string?> ResolveImageAsync(
        string? imageUrl,
        IRemoteStreamContent? imageFile,
        bool isUploadImage,
        string? currentImage = null)
    {
        if (!isUploadImage)
            return NullIfWhiteSpace(imageUrl);

        if (imageFile == null || (imageFile.ContentLength ?? 0) <= 0)
        {
            return NullIfWhiteSpace(imageUrl) ?? NullIfWhiteSpace(currentImage);
        }

        ValidateImageFile(imageFile);

        var tenantId = CurrentTenant.Id?.ToString() ?? "host";
        return await _manageImageService.UploadImageAsync(
            imageFile,
            tenantId,
            subFolder: "salon-locations",
            allowedExtensions: ImageAllowedExtensions);
    }

    private void ValidateImageFile(IRemoteStreamContent file)
    {
        if (file == null || (file.ContentLength ?? 0) <= 0)
            throw new UserFriendlyException(L("SalonBeautyLocations:ImageFileRequired"));

        if ((file.ContentLength ?? 0) > MaxImageBytes)
            throw new UserFriendlyException(L("SalonBeautyLocations:ImageMaxSize"));

        var fileName = file.FileName ?? string.Empty;
        var extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !ImageAllowedExtensions.Contains(extension))
            throw new UserFriendlyException(L("SalonBeautyLocations:ImageInvalidType"));

        var contentType = file.ContentType ?? string.Empty;
        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new UserFriendlyException(L("SalonBeautyLocations:ImageInvalidType"));
    }

    private async Task DeleteOldImageIfLocalAsync(string? oldImage)
    {
        if (!oldImage.IsNullOrWhiteSpace() && oldImage!.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                await _manageImageService.DeleteFileAsync(oldImage);
            }
            catch
            {
            }
        }
    }

    private void NormalizeAndValidate(
        string? name,
        string? address,
        string? phone,
        TimeSpan openTime,
        TimeSpan closeTime,
        int sortOrder)
    {
        if (name.IsNullOrWhiteSpace())
            throw new UserFriendlyException(L("SalonBeautyLocations:NameRequired"));

        if (name!.Trim().Length > 255)
            throw new UserFriendlyException(L("SalonBeautyLocations:NameMaxLength"));

        if (address.IsNullOrWhiteSpace())
            throw new UserFriendlyException(L("SalonBeautyLocations:AddressRequired"));

        if (!phone.IsNullOrWhiteSpace() && !Regex.IsMatch(phone.Trim(), @"^0\d{9,10}$"))
            throw new UserFriendlyException(L("SalonBeautyLocations:PhoneInvalid"));

        if (openTime >= closeTime)
            throw new UserFriendlyException(L("SalonBeautyLocations:OpenCloseInvalid"));

        if (sortOrder < 0)
            throw new UserFriendlyException(L("SalonBeautyLocations:SortOrderInvalid"));
    }

    private string L(string key)
    {
        var text = _l[key].Value;
        return text.IsNullOrWhiteSpace() || text == key ? key : text;
    }

    private static string? NormalizePhone(string? phone)
        => phone.IsNullOrWhiteSpace() ? null : Regex.Replace(phone.Trim(), @"\D", "");

    private static string? NullIfWhiteSpace(string? value)
        => value.IsNullOrWhiteSpace() ? null : value.Trim();

    private async Task CheckLocationPolicyAsync(string tenantPermission, string hostPermission)
    {
        var permission = CurrentTenant.IsAvailable ? tenantPermission : hostPermission;
        if (permission.IsNullOrWhiteSpace())
            throw new AbpAuthorizationException("Missing Salon Beauty location permission.");

        await AuthorizationService.CheckAsync(permission);
    }

    private SalonBeautyLocationDto MapToDto(SalonBeautyLocation entity)
    {
        return new SalonBeautyLocationDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Address = entity.Address,
            Phone = entity.Phone,
            OpenTime = entity.OpenTime,
            CloseTime = entity.CloseTime,
            OpenTimeText = FormatTime(entity.OpenTime),
            CloseTimeText = FormatTime(entity.CloseTime),
            ImageUrl = entity.ImageUrl,
            IsActive = entity.IsActive,
            IsActiveText = entity.IsActive
                ? L("SalonBeautyLocations:StatusActive")
                : L("SalonBeautyLocations:StatusInactive"),
            IsShowOnApp = entity.IsShowOnApp,
            IsShowOnAppText = entity.IsShowOnApp ? L("Yes") : L("No"),
            Note = entity.Note,
            SortOrder = entity.SortOrder
        };
    }

    private static string FormatTime(TimeSpan ts)
        => $"{ts.Hours:D2}:{ts.Minutes:D2}";
}
