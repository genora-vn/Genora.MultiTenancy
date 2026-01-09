using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppPromotionTypes
{
    public class AppPromotionTypeDto : FullAuditedEntityDto<Guid>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public string ColorCode { get; set; }
        public bool Status { get; set; }
        public Guid? TenantId { get; set; }
    }
}
