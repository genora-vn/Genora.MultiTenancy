using System;

namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots
{
    public class GetMiniAppCalendarSlotDetailInput
    {
        public Guid Id { get; set; }
        public Guid? CustomerId { get; set; }
        public short? NumberHoles { get; set; } = 18;
        public int PlayerNumber { get; set; } = 1;
    }
}
