using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppMembershipTiers;

public class AppMembershipTierDto : AuditedEntityDto<Guid>
{
    public string Code { get; set; }              // Đồng / Bạc / Vàng / Platinum...
    public string Name { get; set; }              // Tên hiển thị
    public string Description { get; set; }       // Mô tả quyền lợi tổng quan

    public decimal? MinTotalSpending { get; set; } // Tổng chi tiêu tối thiểu (12 tháng)
    public int? MinRounds { get; set; }            // Số rounds tối thiểu

    public byte EvaluationPeriod { get; set; }     // 0 = None, 1 = Rolling 12 months, 2 = Calendar year...
    public string Benefits { get; set; }           // Quyền lợi chi tiết (text)

    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }         // Thứ tự hiển thị
}