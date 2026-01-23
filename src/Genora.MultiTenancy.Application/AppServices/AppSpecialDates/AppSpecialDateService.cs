using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.AppSpecialDates;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppMembershipTiers;
using Genora.MultiTenancy.DomainModels.AppSpecialDates;
using Genora.MultiTenancy.Features.AppSpecialDates;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.AppSpecialDates
{
    [Authorize]
    public class AppSpecialDateService : FeatureProtectedCrudAppService<
        SpecialDate,
        SpecialDateDto,
        Guid,
        GetSpecialDateListInput,
        CreateUpdateSpecialDateDto>, IAppSpecialDateService
    {
        protected override string FeatureName => AppSpecialDateFeatures.Management;
        protected override string TenantDefaultPermission => MultiTenancyPermissions.AppSpecialDates.Default;
        protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppSpecialDates.Default;

        private readonly IRepository<GolfCourse, Guid> _golfCourseRepository;
        private readonly ICurrentTenant _currentTenant;

        private static readonly string[] AllowedNames = new[]
        {
            SpecialDateNames.Weekday,
            SpecialDateNames.Weekend,
            SpecialDateNames.Holiday
        };

        public AppSpecialDateService(
            IRepository<SpecialDate, Guid> repository,
            IRepository<GolfCourse, Guid> golfCourseRepository,
            ICurrentTenant currentTenant,
            IFeatureChecker featureChecker)
            : base(repository, currentTenant, featureChecker)
        {
            _golfCourseRepository = golfCourseRepository;
            _currentTenant = currentTenant;

            GetPolicyName = MultiTenancyPermissions.AppSpecialDates.Default;
            GetListPolicyName = MultiTenancyPermissions.AppSpecialDates.Default;
            CreatePolicyName = MultiTenancyPermissions.AppSpecialDates.Create;
            UpdatePolicyName = MultiTenancyPermissions.AppSpecialDates.Edit;
            DeletePolicyName = MultiTenancyPermissions.AppSpecialDates.Delete;
        }

        /// <summary>
        /// Lấy danh sách cấu hình loại ngày (Mặc đinhk: Ngày trong tuần, ngày cuối tuần, ngày lễ,...)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [DisableValidation]
        public override async Task<PagedResultDto<SpecialDateDto>> GetListAsync(GetSpecialDateListInput input)
        {
            await CheckGetListPolicyAsync();

            var queryable = await Repository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                var f = input.Filter.Trim();
                queryable = queryable.Where(x =>
                    x.Name.Contains(f) ||
                    (x.Description != null && x.Description.Contains(f))
                );
            }

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            var sorting = string.IsNullOrWhiteSpace(input.Sorting)
                ? nameof(MembershipTier.DisplayOrder) + "," + nameof(MembershipTier.Code)
                : input.Sorting;

            var items = await AsyncExecuter.ToListAsync(
                queryable
                    .OrderBy(sorting)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );

            return new PagedResultDto<SpecialDateDto>(
                totalCount,
                ObjectMapper.Map<List<SpecialDate>, List<SpecialDateDto>>(items)
            );
        }

        /// <summary>
        /// Tạo mới cấu hình loại ngày
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override async Task<SpecialDateDto> CreateAsync(CreateUpdateSpecialDateDto input)
        {
            await CheckCreatePolicyAsync();

            NormalizeInput(input);

            var existing = await Repository.FirstOrDefaultAsync(x =>
                x.GolfCourseId == input.GolfCourseId &&
                x.Name == input.Name);

            if (existing != null)
            {
                existing.Description = input.Description?.Trim();
                existing.IsActive = input.IsActive;
                existing.DatesJson = BuildDatesJsonForHoliday(input);

                await Repository.UpdateAsync(existing, autoSave: true);
                return MapToDto(existing);
            }

            var entity = new SpecialDate(
                GuidGenerator.Create(),
                input.Name,
                input.Description?.Trim(),
                input.GolfCourseId,
                BuildDatesJsonForHoliday(input))
            {
                TenantId = _currentTenant.Id,
                IsActive = input.IsActive
            };

            entity = await Repository.InsertAsync(entity, autoSave: true);
            return MapToDto(entity);
        }

       /// <summary>
       /// Cập nhật cấu hình loại ngày
       /// </summary>
       /// <param name="id"></param>
       /// <param name="input"></param>
       /// <returns></returns>
        public override async Task<SpecialDateDto> UpdateAsync(Guid id, CreateUpdateSpecialDateDto input)
        {
            await CheckUpdatePolicyAsync();

            NormalizeInput(input);

            var entity = await Repository.GetAsync(id);

            entity.Name = input.Name;
            entity.Description = input.Description?.Trim();
            entity.GolfCourseId = input.GolfCourseId;
            entity.IsActive = input.IsActive;
            entity.DatesJson = BuildDatesJsonForHoliday(input);

            entity = await Repository.UpdateAsync(entity, autoSave: true);
            return MapToDto(entity);
        }

        private static void NormalizeInput(CreateUpdateSpecialDateDto input)
        {
            if (input == null) throw new UserFriendlyException("Invalid input");

            input.Name = NormalizeName(input.Name);

            static string NormalizeName(string? name)
            {
                var x = (name ?? "").Trim();
                if (x.Equals("Ngày trong tuần", StringComparison.OrdinalIgnoreCase)) return "Ngày trong tuần";
                if (x.Equals("Ngày cuối tuần", StringComparison.OrdinalIgnoreCase)) return "Ngày cuối tuần";
                if (x.Equals("Ngày lễ", StringComparison.OrdinalIgnoreCase) || x.Equals("Ngay le", StringComparison.OrdinalIgnoreCase)) return "Ngày lễ";
                return "Ngày trong tuần";
            }

            input.Name = (input.Name ?? "").Trim();

            if (!AllowedNames.Contains(input.Name))
                throw new UserFriendlyException("Tên cấu hình không hợp lệ. Chỉ nhận: Ngày trong tuần / Ngày cuối tuần / Ngày lễ");

            // rule: chỉ "Ngày lễ" mới có Dates
            if (!string.Equals(input.Name, SpecialDateNames.Holiday, StringComparison.OrdinalIgnoreCase))
            {
                input.Dates = null;
            }
            else
            {
                var dates = (input.Dates ?? new List<DateTime>())
                    .Select(d => d.Date)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();

                if (dates.Count == 0)
                    throw new UserFriendlyException("Ngày lễ phải có ít nhất 1 ngày trong danh sách Dates");

                input.Dates = dates;
            }
        }

        private static string? BuildDatesJsonForHoliday(CreateUpdateSpecialDateDto input)
        {
            if (!string.Equals(input.Name, SpecialDateNames.Holiday, StringComparison.OrdinalIgnoreCase))
                return null;

            var dates = (input.Dates ?? new List<DateTime>())
                .Select(d => d.Date.ToString("yyyy-MM-dd"))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return JsonSerializer.Serialize(dates);
        }

        private static List<DateTime> ParseDates(string? datesJson)
        {
            if (string.IsNullOrWhiteSpace(datesJson)) return new List<DateTime>();

            try
            {
                var arr = JsonSerializer.Deserialize<List<string>>(datesJson) ?? new List<string>();
                var res = new List<DateTime>();
                foreach (var s in arr)
                {
                    if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                        res.Add(d.Date);
                    else if (DateTime.TryParse(s, out d))
                        res.Add(d.Date);
                }
                return res.Distinct().OrderBy(x => x).ToList();
            }
            catch
            {
                return new List<DateTime>();
            }
        }

        private static SpecialDateDto MapToDto(SpecialDate e)
        {
            return new SpecialDateDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                GolfCourseId = e.GolfCourseId,
                IsActive = e.IsActive,
                Dates = ParseDates(e.DatesJson),
                CreationTime = e.CreationTime,
                CreatorId = e.CreatorId,
                LastModificationTime = e.LastModificationTime,
                LastModifierId = e.LastModifierId
            };
        }
    }

    // ====== Constants ======
    public static class SpecialDateNames
    {
        public const string Weekday = "Ngày trong tuần";
        public const string Weekend = "Ngày cuối tuần";
        public const string Holiday = "Ngày lễ";
    }
}
