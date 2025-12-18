using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppSettings
{
    public class GetMiniAppSettingListInput : PagedAndSortedResultRequestDto
    {
        public string? SettingKey { get; set; }
    }
}
