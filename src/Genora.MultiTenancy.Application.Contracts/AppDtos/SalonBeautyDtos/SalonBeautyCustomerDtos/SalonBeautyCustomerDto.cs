using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.SalonBeautyDtos.SalonBeautyCustomerDtos;

public class SalonBeautyCustomerDto : FullAuditedEntityDto<Guid>
{
    public string CustomerCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public byte? Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Avatar { get; set; }
    public string? ZaloUserId { get; set; }
    public bool IsFollowOa { get; set; }
    public byte? Source { get; set; }
    public byte Status { get; set; }
    public string? Note { get; set; }
}
