using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots;

public class AppCalendarSlotPriceDto : EntityDto<Guid>
{
    public Guid CalendarSlotId { get; set; }

    public Guid CustomerTypeId { get; set; }
    public string CustomerTypeCode { get; set; }
    public string CustomerTypeName { get; set; }

    public decimal? Price9 { get; set; }
    public decimal Price18 { get; set; }
    public decimal? Price27 { get; set; }
    public decimal? Price36 { get; set; }
}