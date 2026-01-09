using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppPromotionTypes
{
    public class PromotionType : FullAuditedAggregateRoot<Guid>, IMultiTenant
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
