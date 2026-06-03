using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.DomainModels.AppCaddie;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.Caddie;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.Caddies;

[Authorize]
public class CaddieAppService : FeatureProtectedCrudAppService<
    AppCaddie, CaddieDto, Guid, GetCaddieListInput, CreateUpdateCaddieDto>, ICaddieAppService
{
    private const int AVATAR_MAX_MB = 15;
    private const long AVATAR_MAX_BYTES = AVATAR_MAX_MB * 1024L * 1024L;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    protected override string FeatureName => CaddieFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppCaddies.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppCaddies.Default;

    private readonly IRepository<AppCaddie, Guid> _caddieRepo;
    private readonly IRepository<AppCaddieLanguage, Guid> _caddieLanguageRepo;
    private readonly IRepository<AppCaddieVoiceRegion, Guid> _caddieVoiceRegionRepo;
    private readonly IRepository<AppLanguage, Guid> _languageRepo;
    private readonly IRepository<GolfCourse, Guid> _golfCourseRepo;
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IManageImageService _manageImageService;

    public CaddieAppService(
        IRepository<AppCaddie, Guid> caddieRepo,
        IRepository<AppCaddieLanguage, Guid> caddieLanguageRepo,
        IRepository<AppCaddieVoiceRegion, Guid> caddieVoiceRegionRepo,
        IRepository<AppLanguage, Guid> languageRepo,
        IRepository<GolfCourse, Guid> golfCourseRepo,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        IGuidGenerator guidGenerator,
        IManageImageService manageImageService)
        : base(caddieRepo, currentTenant, featureChecker)
    {
        _caddieRepo = caddieRepo;
        _caddieLanguageRepo = caddieLanguageRepo;
        _caddieVoiceRegionRepo = caddieVoiceRegionRepo;
        _languageRepo = languageRepo;
        _golfCourseRepo = golfCourseRepo;
        _currentTenant = currentTenant;
        _guidGenerator = guidGenerator;
        _manageImageService = manageImageService;
        LocalizationResource = typeof(MultiTenancyResource);

        GetPolicyName = MultiTenancyPermissions.AppCaddies.Default;
        GetListPolicyName = MultiTenancyPermissions.AppCaddies.Default;
        CreatePolicyName = MultiTenancyPermissions.AppCaddies.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppCaddies.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppCaddies.Delete;
    }

    private string P(string tenantPerm, string hostPerm)
        => _currentTenant.IsAvailable ? tenantPerm : hostPerm;

    public override async Task<PagedResultDto<CaddieDto>> GetListAsync(GetCaddieListInput input)
    {
        await CheckGetListPolicyAsync();

        var query = await _caddieRepo.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var keyword = input.Filter.Trim().ToLower();
            query = query.Where(x =>
                x.CaddieName.ToLower().Contains(keyword) ||
                x.CaddieCode.ToLower().Contains(keyword) ||
                (x.Phone != null && x.Phone.Contains(keyword)));
        }

        if (input.GolfCourseId.HasValue)
            query = query.Where(x => x.GolfCourseId == input.GolfCourseId.Value);

        if (input.Status.HasValue)
            query = query.Where(x => x.Status == input.Status.Value);

        if (input.IsShowOnApp.HasValue)
            query = query.Where(x => x.IsShowOnApp == input.IsShowOnApp.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "CreationTime DESC" : input.Sorting;
        query = query.OrderBy(sorting);

        var items = await AsyncExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount));

        var caddieIds = items.Select(x => x.Id).ToList();

        // Load languages
        var langQuery = (await _caddieLanguageRepo.GetQueryableAsync())
            .Where(x => caddieIds.Contains(x.CaddieId));
        var allLangQuery = await _languageRepo.GetQueryableAsync();
        var joinedLangQuery = langQuery.Join(allLangQuery,
            cl => cl.LanguageId, l => l.Id,
            (cl, l) => new { cl.CaddieId, l.LanguageName });
        var caddieLanguages = await AsyncExecuter.ToListAsync(joinedLangQuery);

        // Load voice regions
        var vrQuery = (await _caddieVoiceRegionRepo.GetQueryableAsync())
            .Where(x => caddieIds.Contains(x.CaddieId));
        var caddieVoiceRegions = await AsyncExecuter.ToListAsync(vrQuery);

        var dtos = items.Select(x =>
        {
            var dto = MapToDto(x);
            dto.Languages = caddieLanguages
                .Where(cl => cl.CaddieId == x.Id)
                .Select(cl => cl.LanguageName)
                .ToList();
            dto.VoiceRegions = caddieVoiceRegions
                .Where(vr => vr.CaddieId == x.Id)
                .Select(vr => GetVoiceRegionText(vr.VoiceRegion))
                .ToList();
            return dto;
        }).ToList();

        return new PagedResultDto<CaddieDto>(totalCount, dtos);
    }

    public override async Task<CaddieDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();

        var caddie = await _caddieRepo.GetAsync(id);
        var dto = MapToDto(caddie);

        // Load languages
        var langQuery = (await _caddieLanguageRepo.GetQueryableAsync())
            .Where(x => x.CaddieId == id);
        var allLangQuery = await _languageRepo.GetQueryableAsync();
        var joinedQuery = langQuery.Join(allLangQuery,
            cl => cl.LanguageId, l => l.Id,
            (cl, l) => new { l.Id, l.LanguageName });
        var languages = await AsyncExecuter.ToListAsync(joinedQuery);

        dto.Languages = languages.Select(x => x.LanguageName).ToList();
        dto.LanguageIds = languages.Select(x => x.Id).ToList();

        // Load voice regions
        var vrQuery = (await _caddieVoiceRegionRepo.GetQueryableAsync())
            .Where(x => x.CaddieId == id);
        var voiceRegions = await AsyncExecuter.ToListAsync(vrQuery);
        dto.VoiceRegions = voiceRegions.Select(x => GetVoiceRegionText(x.VoiceRegion)).ToList();
        dto.VoiceRegionValues = voiceRegions.Select(x => x.VoiceRegion).ToList();

        return dto;
    }

    public override async Task<CaddieDto> CreateAsync(CreateUpdateCaddieDto input)
    {
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddies.Create, MultiTenancyPermissions.HostAppCaddies.Create));

        var code = await GenerateCaddieCodeAsync();

        // Handle avatar upload
        var avatarUrl = await ResolveAvatarAsync(input.AvatarFile, null);

        // Resolve GolfCourseId: use input or fallback to the single configured course
        var golfCourseId = await ResolveGolfCourseIdAsync(input.GolfCourseId);

        var caddie = new AppCaddie(_guidGenerator.Create())
        {
            CaddieCode = code,
            CaddieName = input.CaddieName,
            Avatar = avatarUrl,
            Gender = input.Gender,
            Phone = input.Phone,
            GolfCourseId = golfCourseId,
            JoinDate = input.JoinDate,
            HeightCm = input.HeightCm,
            Status = input.Status,
            IsShowOnApp = input.IsShowOnApp,
            Note = input.Note
        };

        await _caddieRepo.InsertAsync(caddie, autoSave: true);

        // Save languages
        await SaveCaddieLanguagesAsync(caddie.Id, input.LanguageIds);

        // Save voice regions
        await SaveCaddieVoiceRegionsAsync(caddie.Id, input.VoiceRegions);

        return await GetAsync(caddie.Id);
    }

    public override async Task<CaddieDto> UpdateAsync(Guid id, CreateUpdateCaddieDto input)
    {
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddies.Edit, MultiTenancyPermissions.HostAppCaddies.Edit));

        var caddie = await _caddieRepo.GetAsync(id);

        // Handle avatar upload
        var avatarUrl = await ResolveAvatarAsync(input.AvatarFile, caddie.Avatar);

        // Resolve GolfCourseId: use input or fallback to the single configured course
        var golfCourseId = await ResolveGolfCourseIdAsync(input.GolfCourseId);

        caddie.CaddieName = input.CaddieName;
        caddie.Avatar = avatarUrl;
        caddie.Gender = input.Gender;
        caddie.Phone = input.Phone;
        caddie.GolfCourseId = golfCourseId;
        caddie.JoinDate = input.JoinDate;
        caddie.HeightCm = input.HeightCm;
        caddie.Status = input.Status;
        caddie.IsShowOnApp = input.IsShowOnApp;
        caddie.Note = input.Note;

        await _caddieRepo.UpdateAsync(caddie, autoSave: true);

        // Update languages
        await SaveCaddieLanguagesAsync(caddie.Id, input.LanguageIds);

        // Update voice regions
        await SaveCaddieVoiceRegionsAsync(caddie.Id, input.VoiceRegions);

        return await GetAsync(caddie.Id);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddies.Delete, MultiTenancyPermissions.HostAppCaddies.Delete));

        var caddie = await _caddieRepo.GetAsync(id);

        // Delete avatar file if local
        await DeleteOldAvatarIfLocalAsync(caddie.Avatar);

        await _caddieRepo.DeleteAsync(id);
    }

    public async Task UpdateStatusAsync(Guid id, byte status)
    {
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddies.Edit, MultiTenancyPermissions.HostAppCaddies.Edit));

        var caddie = await _caddieRepo.GetAsync(id);
        caddie.Status = status;
        await _caddieRepo.UpdateAsync(caddie, autoSave: true);
    }

    public async Task<string> GenerateCaddieCodeAsync()
    {
        var query = (await _caddieRepo.GetQueryableAsync())
            .Where(x => x.CaddieCode.StartsWith("CD-"))
            .OrderByDescending(x => x.CaddieCode)
            .Select(x => x.CaddieCode);

        var maxCode = await AsyncExecuter.FirstOrDefaultAsync(query);

        if (maxCode == null)
            return "CD-001";

        var numPart = maxCode.Replace("CD-", "");
        if (int.TryParse(numPart, out var num))
            return $"CD-{(num + 1):D3}";

        return $"CD-{DateTime.Now:yyyyMMddHHmmss}";
    }

    /// <summary>
    /// Resolve GolfCourseId: use provided value, or fallback to the single configured golf course.
    /// </summary>
    private async Task<Guid> ResolveGolfCourseIdAsync(Guid? inputGolfCourseId)
    {
        if (inputGolfCourseId.HasValue && inputGolfCourseId.Value != Guid.Empty)
            return inputGolfCourseId.Value;

        // Fallback: get the first (and typically only) golf course
        var courseQuery = (await _golfCourseRepo.GetQueryableAsync())
            .Select(x => x.Id);
        var courseId = await AsyncExecuter.FirstOrDefaultAsync(courseQuery);

        if (courseId == Guid.Empty)
            throw new UserFriendlyException("Chưa có sân golf nào được cấu hình. Vui lòng tạo sân golf trước.");

        return courseId;
    }

    #region Avatar helpers

    private async Task<string?> ResolveAvatarAsync(IRemoteStreamContent? avatarFile, string? currentAvatar)
    {
        if (avatarFile == null || avatarFile.ContentLength == null || avatarFile.ContentLength == 0)
            return currentAvatar;

        // Validate file
        ValidateAvatarFile(avatarFile);

        // Delete old avatar if local
        await DeleteOldAvatarIfLocalAsync(currentAvatar);

        // Upload new avatar
        var tenantId = _currentTenant.Id?.ToString() ?? "host";

        var uploadedUrl = await _manageImageService.UploadImageAsync(
            avatarFile, tenantId, "caddies", AllowedExtensions);

        return uploadedUrl;
    }

    private void ValidateAvatarFile(IRemoteStreamContent file)
    {
        var fileSize = file.ContentLength ?? 0;
        if (fileSize > AVATAR_MAX_BYTES)
        {
            throw new UserFriendlyException(
                $"Dung lượng ảnh không được vượt quá {AVATAR_MAX_MB} MB. File hiện tại: {fileSize / (1024.0 * 1024.0):F1} MB");
        }

        var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
        {
            throw new UserFriendlyException(
                $"Chỉ chấp nhận file ảnh: {string.Join(", ", AllowedExtensions)}");
        }

        if (file.ContentType == null || !file.ContentType.StartsWith("image/"))
        {
            throw new UserFriendlyException("File không phải là ảnh hợp lệ.");
        }
    }

    private async Task DeleteOldAvatarIfLocalAsync(string? avatarUrl)
    {
        if (!string.IsNullOrWhiteSpace(avatarUrl) && avatarUrl.StartsWith("/uploads/"))
        {
            await _manageImageService.DeleteFileAsync(avatarUrl);
        }
    }

    #endregion

    #region Private helpers

    private async Task SaveCaddieLanguagesAsync(Guid caddieId, List<Guid> languageIds)
    {
        // Delete existing
        var existingQuery = (await _caddieLanguageRepo.GetQueryableAsync())
            .Where(x => x.CaddieId == caddieId);
        var existing = await AsyncExecuter.ToListAsync(existingQuery);

        if (existing.Any())
            await _caddieLanguageRepo.DeleteManyAsync(existing, autoSave: true);

        // Insert new
        if (languageIds?.Any() == true)
        {
            var newItems = languageIds.Select(langId =>
                new AppCaddieLanguage(_guidGenerator.Create(), caddieId, langId)).ToList();

            await _caddieLanguageRepo.InsertManyAsync(newItems, autoSave: true);
        }
    }

    private async Task SaveCaddieVoiceRegionsAsync(Guid caddieId, List<byte> voiceRegions)
    {
        // Delete existing
        var existingQuery = (await _caddieVoiceRegionRepo.GetQueryableAsync())
            .Where(x => x.CaddieId == caddieId);
        var existing = await AsyncExecuter.ToListAsync(existingQuery);

        if (existing.Any())
            await _caddieVoiceRegionRepo.DeleteManyAsync(existing, autoSave: true);

        // Insert new
        if (voiceRegions?.Any() == true)
        {
            var newItems = voiceRegions.Distinct().Select(vr =>
                new AppCaddieVoiceRegion(_guidGenerator.Create(), caddieId, vr)).ToList();

            await _caddieVoiceRegionRepo.InsertManyAsync(newItems, autoSave: true);
        }
    }

    private CaddieDto MapToDto(AppCaddie entity)
    {
        var experienceYear = 0;
        if (entity.JoinDate.HasValue)
            experienceYear = (int)((DateTime.Now - entity.JoinDate.Value).TotalDays / 365.25);

        return new CaddieDto
        {
            Id = entity.Id,
            CaddieCode = entity.CaddieCode,
            CaddieName = entity.CaddieName,
            Avatar = entity.Avatar,
            Gender = entity.Gender,
            GenderText = entity.Gender switch
            {
                (byte)CaddieGender.Male => "Nam",
                (byte)CaddieGender.Female => "Nữ",
                _ => null
            },
            Phone = entity.Phone,
            PhoneMasked = MaskPhone(entity.Phone),
            GolfCourseId = entity.GolfCourseId ?? Guid.Empty,
            JoinDate = entity.JoinDate,
            HeightCm = entity.HeightCm,
            ExperienceYear = experienceYear,
            RatingAvg = entity.RatingAvg,
            TotalBooking = entity.TotalBooking,
            Status = entity.Status,
            IsShowOnApp = entity.IsShowOnApp,
            Note = entity.Note,
            CreationTime = entity.CreationTime
        };
    }

    private static string? MaskPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 7)
            return phone;
        return phone[..3] + " " + phone.Substring(3, 3) + " " + new string('x', phone.Length - 7) + phone[^1];
    }

    private static string GetVoiceRegionText(byte region) => region switch
    {
        (byte)CaddieVoiceRegion.North => "Miền Bắc",
        (byte)CaddieVoiceRegion.Central => "Miền Trung",
        (byte)CaddieVoiceRegion.South => "Miền Nam",
        _ => "Khác"
    };

    #endregion
}
