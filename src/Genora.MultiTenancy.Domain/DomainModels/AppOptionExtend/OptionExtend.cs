using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppOptionExtend
{
    public class OptionExtend : FullAuditedAggregateRoot<Guid>, IMultiTenant
    {
        public int OptionId { get; set; }
        public string OptionName { get; set; }
        public int Type { get; set; }
        public string? Description { get; set; }
        public Guid? TenantId { get; set; }
        
        protected OptionExtend() { }
        public OptionExtend(int optionId, string optionName, int type, string? description)
        {
            OptionId = optionId;
            OptionName = optionName;
            Type = type;
            Description = description;
        }
    }
}
