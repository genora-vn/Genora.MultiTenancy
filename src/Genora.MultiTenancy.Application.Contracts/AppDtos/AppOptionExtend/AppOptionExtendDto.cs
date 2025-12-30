using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppOptionExtend
{
    public class AppOptionExtendDto : FullAuditedEntityDto<Guid>
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public int Type { get; set; }
        public string? Description { get; set; }
        public Guid? TenantId { get; set; }
    }
}
