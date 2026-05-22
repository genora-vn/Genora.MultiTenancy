using Genora.MultiTenancy.AppDtos.AppPromotionPolicies;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppPromotionPolicies;
using Genora.MultiTenancy.DomainModels.AppPromotionTypes;
using Genora.MultiTenancy.Features.AppPromotionPolicies;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppPromotionPolicies;

[Authorize]
public class AppPromotionPolicyService :
    FeatureProtectedCrudAppService<
        PromotionPolicy,
        AppPromotionPolicyDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateAppPromotionPolicyDto>,
    IAppPromotionPolicyService
{
    protected override string FeatureName => AppPromotionPolicyFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppPromotionPolicies.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppPromotionPolicies.Default;

    private readonly IRepository<GolfCourse, Guid> _golfCourseRepository;
    private readonly IRepository<PromotionType, Guid> _promotionTypeRepository;

    public AppPromotionPolicyService(
        IRepository<PromotionPolicy, Guid> repository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        IRepository<GolfCourse, Guid> golfCourseRepository,
        IRepository<PromotionType, Guid> promotionTypeRepository)
        : base(repository, currentTenant, featureChecker)
    {
        GetPolicyName = MultiTenancyPermissions.AppPromotionPolicies.Default;
        GetListPolicyName = MultiTenancyPermissions.AppPromotionPolicies.Default;
        CreatePolicyName = MultiTenancyPermissions.AppPromotionPolicies.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppPromotionPolicies.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppPromotionPolicies.Delete;

        _golfCourseRepository = golfCourseRepository;
        _promotionTypeRepository = promotionTypeRepository;
    }

    public override async Task<PagedResultDto<AppPromotionPolicyDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();
        var golfCourseQueryable = await _golfCourseRepository.GetQueryableAsync();
        var promotionTypeQueryable = await _promotionTypeRepository.GetQueryableAsync();

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(PromotionPolicy.CreationTime) + " desc"
            : input.Sorting;

        var query = from p in queryable
                    join g in golfCourseQueryable on p.GolfCourseId equals g.Id into gj
                    from g in gj.DefaultIfEmpty()
                    join pt in promotionTypeQueryable on p.PromotionTypeId equals pt.Id into ptj
                    from pt in ptj.DefaultIfEmpty()
                    select new AppPromotionPolicyDto
                    {
                        Id = p.Id,
                        TenantId = p.TenantId,
                        GolfCourseId = p.GolfCourseId,
                        PromotionTypeId = p.PromotionTypeId,
                        PolicyTitle = p.PolicyTitle,
                        CancellationPolicyHours = p.CancellationPolicyHours,
                        CancellationPolicyHoursWeekend = p.CancellationPolicyHoursWeekend,
                        CancellationPolicyContent = p.CancellationPolicyContent,
                        CreationTime = p.CreationTime,
                        CreatorId = p.CreatorId,
                        LastModificationTime = p.LastModificationTime,
                        LastModifierId = p.LastModifierId,
                        IsDeleted = p.IsDeleted,
                        DeleterId = p.DeleterId,
                        DeletionTime = p.DeletionTime,
                        GolfCourseName = g != null ? g.Name : null,
                        PromotionTypeName = pt != null ? pt.Name : null,
                        PromotionTypeColor = pt != null ? pt.ColorCode : null
                    };

        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount)
        );

        return new PagedResultDto<AppPromotionPolicyDto>(totalCount, items);
    }

    public override async Task<AppPromotionPolicyDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();

        var entity = await Repository.GetAsync(id);
        var golfCourse = await _golfCourseRepository.FindAsync(entity.GolfCourseId);
        var promotionType = await _promotionTypeRepository.FindAsync(entity.PromotionTypeId);

        var dto = ObjectMapper.Map<PromotionPolicy, AppPromotionPolicyDto>(entity);
        dto.GolfCourseName = golfCourse?.Name;
        dto.PromotionTypeName = promotionType?.Name;
        dto.PromotionTypeColor = promotionType?.ColorCode;
        return dto;
    }

    public override async Task<AppPromotionPolicyDto> CreateAsync(CreateUpdateAppPromotionPolicyDto input)
    {
        await CheckCreatePolicyAsync();
        await EnsureRefsExistAsync(input.GolfCourseId, input.PromotionTypeId);
        await EnsureUniqueAsync(input.GolfCourseId, input.PromotionTypeId, null);

        var entity = new PromotionPolicy(GuidGenerator.Create(), input.GolfCourseId, input.PromotionTypeId)
        {
            PolicyTitle = input.PolicyTitle?.Trim(),
            CancellationPolicyHours = input.CancellationPolicyHours,
            CancellationPolicyHoursWeekend = input.CancellationPolicyHoursWeekend,
            CancellationPolicyContent = input.CancellationPolicyContent
        };

        await Repository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    public override async Task<AppPromotionPolicyDto> UpdateAsync(Guid id, CreateUpdateAppPromotionPolicyDto input)
    {
        await CheckUpdatePolicyAsync();
        var entity = await Repository.GetAsync(id);

        await EnsureRefsExistAsync(input.GolfCourseId, input.PromotionTypeId);
        await EnsureUniqueAsync(input.GolfCourseId, input.PromotionTypeId, id);

        entity.GolfCourseId = input.GolfCourseId;
        entity.PromotionTypeId = input.PromotionTypeId;
        entity.PolicyTitle = input.PolicyTitle?.Trim();
        entity.CancellationPolicyHours = input.CancellationPolicyHours;
        entity.CancellationPolicyHoursWeekend = input.CancellationPolicyHoursWeekend;
        entity.CancellationPolicyContent = input.CancellationPolicyContent;

        await Repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    public async Task<CreateUpdateAppPromotionPolicyDto> GetEditDataAsync(Guid? id)
    {
        // Cho phép load options ngay khi mở modal
        await CheckGetListPolicyAsync();

        var golfCourses = await AsyncExecuter.ToListAsync(
            (await _golfCourseRepository.GetQueryableAsync())
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
        );

        var promotionTypes = await AsyncExecuter.ToListAsync(
            (await _promotionTypeRepository.GetQueryableAsync())
                .Where(x => x.Status)
                .OrderBy(x => x.Name)
        );

        var dto = new CreateUpdateAppPromotionPolicyDto
        {
            AvailableGolfCourses = golfCourses.Select(x => new PromotionPolicyGolfCourseDto
            {
                Id = x.Id,
                Name = x.Name
            }).ToList(),
            AvailablePromotionTypes = promotionTypes.Select(x => new PromotionPolicyPromotionTypeDto
            {
                Id = x.Id,
                Name = x.Name,
                ColorCode = x.ColorCode
            }).ToList()
        };

        if (id.HasValue && id.Value != Guid.Empty)
        {
            var entity = await Repository.GetAsync(id.Value);
            dto.GolfCourseId = entity.GolfCourseId;
            dto.PromotionTypeId = entity.PromotionTypeId;
            dto.PolicyTitle = entity.PolicyTitle;
            dto.CancellationPolicyHours = entity.CancellationPolicyHours;
            dto.CancellationPolicyHoursWeekend = entity.CancellationPolicyHoursWeekend;
            dto.CancellationPolicyContent = entity.CancellationPolicyContent;
        }
        else
        {
            // Default: chọn sân golf đầu tiên (đã cấu hình)
            var first = dto.AvailableGolfCourses.FirstOrDefault();
            if (first != null)
            {
                dto.GolfCourseId = first.Id;
            }
        }

        return dto;
    }

    private async Task EnsureRefsExistAsync(Guid golfCourseId, Guid promotionTypeId)
    {
        var golfExists = await _golfCourseRepository.AnyAsync(x => x.Id == golfCourseId);
        if (!golfExists) throw new UserFriendlyException("Sân golf không tồn tại.");

        var ptExists = await _promotionTypeRepository.AnyAsync(x => x.Id == promotionTypeId);
        if (!ptExists) throw new UserFriendlyException("Loại ưu đãi không tồn tại.");
    }

    private async Task EnsureUniqueAsync(Guid golfCourseId, Guid promotionTypeId, Guid? excludeId)
    {
        var dup = await Repository.AnyAsync(x =>
            x.GolfCourseId == golfCourseId &&
            x.PromotionTypeId == promotionTypeId &&
            (!excludeId.HasValue || x.Id != excludeId.Value));

        if (dup)
            throw new UserFriendlyException("Đã tồn tại chính sách hoãn hủy cho sân golf và loại ưu đãi này.");
    }
}
