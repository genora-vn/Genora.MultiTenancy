using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppOptionExtend
{
    public class AppOptionExtendDto : FullAuditedEntityDto<Guid>
    {
        public int OptionId { get; set; }
        public string OptionName { get; set; }
        public int Type { get; set; }
        public string? Description { get; set; }
        public Guid? TenantId { get; set; }
    }
}
