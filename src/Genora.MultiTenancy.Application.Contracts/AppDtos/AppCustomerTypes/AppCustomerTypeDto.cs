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
    public decimal? OriginalPrice { get; set; }              // Giá gốc Ngày trong tuần (Weekday)
    public decimal? OriginalPriceWeekend { get; set; }       // Giá gốc Ngày cuối tuần
    public decimal? OriginalPriceHoliday { get; set; }       // Giá gốc Ngày lễ
    public decimal? OriginalPriceMemberDay { get; set; }     // Giá gốc Member day
}