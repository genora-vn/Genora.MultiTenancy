using Genora.MultiTenancy.AppDtos.AppSettings;
using Genora.MultiTenancy.Apps.AppSettings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.UI.Navigation.Urls;

namespace Genora.MultiTenancy.AppServices.AppSettings
{
    public class MiniAppSettingService : ApplicationService, IMiniAppSettingService, ITransientDependency
    {
        private readonly IRepository<AppSetting, Guid> _settingRepo;
        private readonly IConfiguration _configuration;
        public MiniAppSettingService(IRepository<AppSetting, Guid> settingRepo, IConfiguration configuration)
        {
            _settingRepo = settingRepo;
            _configuration = configuration;
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
                        item.SettingValue = _configuration["App:AppUrl"] + item.SettingValue;
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
            if (record.SettingValue.StartsWith("/uploads"))
            {
                record.SettingValue = _configuration["App:AppUrl"] + record.SettingValue;
            }
            return new MiniAppAppSettingDetailDto { Data = ObjectMapper.Map<AppSetting, AppSettingDto>(record), Error = 0, Message = "Success" };
        }
    }
}
