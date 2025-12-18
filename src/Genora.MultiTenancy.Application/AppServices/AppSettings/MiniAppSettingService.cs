using Genora.MultiTenancy.AppDtos.AppSettings;
using Genora.MultiTenancy.Apps.AppSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppSettings
{
    public class MiniAppSettingService : ApplicationService, IMiniAppSettingService
    {
        private readonly IRepository<AppSetting, Guid> _settingRepo;

        public MiniAppSettingService(IRepository<AppSetting, Guid> settingRepo)
        {
            _settingRepo = settingRepo;
        }

        public async Task<PagedResultDto<AppSettingDto>> GetListAsync(GetMiniAppSettingListInput input)
        {
            var query = await _settingRepo.GetQueryableAsync();
            if (!string.IsNullOrWhiteSpace(input.SettingKey))
            {
                query = query.Where(s => s.SettingKey == input.SettingKey);
            }
            var sorting = string.IsNullOrWhiteSpace(input.Sorting)
                ? nameof(AppSetting.CreationTime) + " desc"
                : input.Sorting;
            //query = query.OrderBy(sorting);
            var total = await AsyncExecuter.CountAsync(query);
            var items = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));
            var itemDtos = ObjectMapper.Map<List<AppSetting>, List<AppSettingDto>>(items);
            return new PagedResultDto<AppSettingDto>(total, itemDtos);
        }
        public async Task<AppSettingDto> GetAsync(Guid id)
        {
            var record = await _settingRepo.GetAsync(id);
            return ObjectMapper.Map<AppSetting, AppSettingDto>(record);
        }
    }
}
