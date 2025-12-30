using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppOptionExtend
{
    public class OptionExtend : FullAuditedAggregateRoot<Guid>, IMultiTenant
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public int Type { get; set; }
        public string? Description { get; set; }
        public Guid? TenantId { get; set; }
    }
}
