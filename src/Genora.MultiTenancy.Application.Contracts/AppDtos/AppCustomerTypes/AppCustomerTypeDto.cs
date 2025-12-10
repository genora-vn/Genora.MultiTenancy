using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppCustomerTypes;

public class AppCustomerTypeDto : AuditedEntityDto<Guid>
{
    public string Code { get; set; }          // Mã loại KH (Member, Guest...)
    public string Name { get; set; }          // Tên hiển thị
    public string Description { get; set; }   // Mô tả
    public string ColorCode { get; set; }     // Màu nhãn hex (#FF9800)
    public bool IsActive { get; set; }
}