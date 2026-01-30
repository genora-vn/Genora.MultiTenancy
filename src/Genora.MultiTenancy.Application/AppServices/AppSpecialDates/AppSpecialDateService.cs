using Genora.MultiTenancy.AppDtos.AppSpecialDates;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppMembershipTiers;
using Genora.MultiTenancy.DomainModels.AppSpecialDates;
using Genora.MultiTenancy.Enums.ErrorCodes;
using Genora.MultiTenancy.Features.AppSpecialDates;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
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

        // mapping 0..6 = T2..CN  => All = 127
        private const int AllWeekdaysMask = (1 << 7) - 1;

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

        // =========================
        // ERROR HELPERS
        // =========================
        private static BusinessException Err(string code, string field, object? value = null)
        {
            var ex = new BusinessException(code)
                .WithData("Field", field);

            if (value != null) ex.WithData("Value", value);

            return ex;
        }

        private static bool IsHolidayName(string? name)
        {
            var x = (name ?? "").Trim();
            return x.Equals(SpecialDateNames.Holiday, StringComparison.OrdinalIgnoreCase)
                || x.Equals("Ngay le", StringComparison.OrdinalIgnoreCase)
                || x.Equals("Holiday", StringComparison.OrdinalIgnoreCase);
        }

        [DisableValidation]
        public override async Task<SpecialDateDto> GetAsync(Guid id)
        {
            await CheckGetPolicyAsync();

            var entity = await Repository.GetAsync(id);
            return MapToDto(entity);
        }

        /// <summary>
        /// Lấy danh sách cấu hình loại ngày
        /// </summary>
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

            // ✅ sorting ổn định theo Name (đỡ dính MembershipTier)
            var sorting = string.IsNullOrWhiteSpace(input.Sorting)
                ? nameof(SpecialDate.Name) + " asc"
                : input.Sorting;

            var items = await AsyncExecuter.ToListAsync(
                queryable
                    .OrderBy(sorting)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );

            var dtoItems = items.Select(MapToDto).ToList();
            return new PagedResultDto<SpecialDateDto>(totalCount, dtoItems);
        }

        /// <summary>
        /// Tạo mới cấu hình loại ngày (Cho phép thêm loại mới thoải mái)
        /// - Holiday => bắt buộc Dates
        /// - Non-holiday => dùng Weekdays (0..6); empty => All
        /// </summary>
        public override async Task<SpecialDateDto> CreateAsync(CreateUpdateSpecialDateDto input)
        {
            await CheckCreatePolicyAsync();

            NormalizeInput(input);

            // Upsert theo (GolfCourseId + Name)
            var existing = await Repository.FirstOrDefaultAsync(x =>
                x.GolfCourseId == input.GolfCourseId &&
                x.Name == input.Name);

            if (existing != null)
            {
                existing.Description = input.Description?.Trim();
                existing.IsActive = input.IsActive;

                existing.DatesJson = BuildDatesJsonForHoliday(input);
                existing.WeekdaysMask = BuildWeekdaysMask(input);

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
                IsActive = input.IsActive,
                WeekdaysMask = BuildWeekdaysMask(input)
            };

            entity = await Repository.InsertAsync(entity, autoSave: true);
            return MapToDto(entity);
        }

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
            entity.WeekdaysMask = BuildWeekdaysMask(input);

            entity = await Repository.UpdateAsync(entity, autoSave: true);
            return MapToDto(entity);
        }

        /// <summary>
        /// Normalize + Validate nghiệp vụ nhưng KHÔNG khóa theo whitelist
        /// </summary>
        private static void NormalizeInput(CreateUpdateSpecialDateDto input)
        {
            if (input == null)
                throw Err(SpecialDateErrorCodes.InvalidInput, "Input");

            // Name required + trim + canonical known names
            input.Name = CanonicalizeName(input.Name);

            if (string.IsNullOrWhiteSpace(input.Name))
                throw Err(SpecialDateErrorCodes.NameInvalid, "Name", input.Name);

            if (input.Name.Length > 50)
                throw Err(SpecialDateErrorCodes.NameInvalid, "Name", input.Name)
                    .WithData("MaxLength", 50);

            var isHoliday = IsHolidayName(input.Name);

            if (isHoliday)
            {
                input.Weekdays = null;

                var dates = (input.Dates ?? new List<DateTime>())
                    .Select(d => d.Date)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();

                if (dates.Count == 0)
                {
                    throw Err(SpecialDateErrorCodes.HolidayDatesRequired, "Dates")
                        .WithData("Name", input.Name);
                }

                input.Dates = dates;
            }
            else
            {
                input.Dates = null;

                // Weekdays: accept 0..6 (T2..CN). Empty => All
                var wd = (input.Weekdays ?? new List<int>())
                    .Distinct()
                    .Where(x => x >= 0 && x <= 6)
                    .OrderBy(x => x)
                    .ToList();

                input.Weekdays = wd.Count == 0 ? null : wd;
            }
        }

        private static string CanonicalizeName(string? name)
        {
            var x = (name ?? "").Trim();

            // canonical 3 loại core + Member day
            if (x.Equals(SpecialDateNames.Weekday, StringComparison.OrdinalIgnoreCase)) return SpecialDateNames.Weekday;
            if (x.Equals(SpecialDateNames.Weekend, StringComparison.OrdinalIgnoreCase)) return SpecialDateNames.Weekend;
            if (x.Equals(SpecialDateNames.Holiday, StringComparison.OrdinalIgnoreCase) || x.Equals("Ngay le", StringComparison.OrdinalIgnoreCase)) return SpecialDateNames.Holiday;

            // Member day: cho phép user gõ nhiều kiểu
            if (x.Equals(SpecialDateNames.MemberDay, StringComparison.OrdinalIgnoreCase)
                || x.Equals("Memberday", StringComparison.OrdinalIgnoreCase)
                || x.Equals("Member Day", StringComparison.OrdinalIgnoreCase))
                return SpecialDateNames.MemberDay;

            // ✅ cho phép mọi loại khác
            return x;
        }

        private static string? BuildDatesJsonForHoliday(CreateUpdateSpecialDateDto input)
        {
            if (!IsHolidayName(input.Name))
                return null;

            var dates = (input.Dates ?? new List<DateTime>())
                .Select(d => d.Date.ToString("yyyy-MM-dd"))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return JsonSerializer.Serialize(dates);
        }

        private static int? BuildWeekdaysMask(CreateUpdateSpecialDateDto input)
        {
            if (IsHolidayName(input.Name))
                return null;

            var list = (input.Weekdays ?? new List<int>())
                .Distinct()
                .Where(x => x >= 0 && x <= 6)
                .ToList();

            if (list.Count == 0) return AllWeekdaysMask;

            var mask = 0;
            foreach (var d in list) mask |= (1 << d);

            return mask == 0 ? AllWeekdaysMask : mask;
        }

        private static List<int> ParseWeekdays(int? mask)
        {
            if (!mask.HasValue) return new List<int>();
            var m = mask.Value;

            var res = new List<int>();
            for (var i = 0; i <= 6; i++)
            {
                if (((m >> i) & 1) == 1) res.Add(i);
            }
            return res;
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
                Weekdays = IsHolidayName(e.Name) ? new List<int>() : ParseWeekdays(e.WeekdaysMask),
                CreationTime = e.CreationTime,
                CreatorId = e.CreatorId,
                LastModificationTime = e.LastModificationTime,
                LastModifierId = e.LastModifierId
            };
        }
    }

    public static class SpecialDateNames
    {
        public const string Weekday = "Ngày trong tuần";
        public const string Weekend = "Ngày cuối tuần";
        public const string Holiday = "Ngày lễ";
        public const string MemberDay = "Member day";
    }
}
