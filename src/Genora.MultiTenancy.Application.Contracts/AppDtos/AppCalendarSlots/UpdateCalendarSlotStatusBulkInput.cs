using System;
using System.Collections.Generic;
namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots;
public class UpdateCalendarSlotStatusBulkInput
{
    public List<Guid> Ids { get; set; } = new();
    public bool IsActive { get; set; }
}