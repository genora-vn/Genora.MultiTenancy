using Genora.MultiTenancy.AppDtos.AppSettings;
using Genora.MultiTenancy.Apps.AppSettings;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppSettings
{
    public class MiniAppSettingService : ApplicationService, IMiniAppSettingService
    {
        private readonly IRepository<AppSetting, Guid> _settingRepo;
        private readonly IHostEnvironment _env;
        public MiniAppSettingService(IRepository<AppSetting, Guid> settingRepo, IHostEnvironment env)
        {
            _settingRepo = settingRepo;
            _env = env;
        }

        public async Task<MiniAppAppSettingListDto> GetListAsync(GetMiniAppSettingListInput input)
        {
            try
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
                foreach ( var item in itemDtos )
                {
                    if (item.SettingValue != null && item.SettingValue.StartsWith("/uploads"))
                    {
                        item.SettingValue = _env.ContentRootPath +"/wwwroot" + item.SettingValue;
                    }
                }
                var dto = new PagedResultDto<AppSettingDto>(total, itemDtos);
                return new MiniAppAppSettingListDto { Data = dto, Error = 0, Message = "Success" };
            }
            catch (Exception ex)
            {
                return new MiniAppAppSettingListDto { Error = (int)HttpStatusCode.BadRequest, Message = ex.Message };
            }
        }
        public async Task<MiniAppAppSettingDetailDto> GetAsync(Guid id)
        {
            var record = await _settingRepo.GetAsync(id);
            return new MiniAppAppSettingDetailDto { Data = ObjectMapper.Map<AppSetting, AppSettingDto>(record), Error = 0, Message = "Success" };
        }
    }
}
