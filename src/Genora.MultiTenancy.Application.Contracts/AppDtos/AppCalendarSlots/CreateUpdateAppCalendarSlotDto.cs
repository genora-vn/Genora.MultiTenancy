using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots
{
    public class CreateUpdateCalendarSlotPriceDto
    {
        [Required]
        public Guid CustomerTypeId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
    }

    public class CreateUpdateAppCalendarSlotDto
    {
        [Required]
        public Guid GolfCourseId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ApplyDate { get; set; }

        [Required]
        public TimeSpan TimeFrom { get; set; }

        [Required]
        public TimeSpan TimeTo { get; set; }

        [Required]
        public PromotionType PromotionType { get; set; }

        [Range(1, int.MaxValue)]
        public int MaxSlots { get; set; }

        [StringLength(500)]
        public string InternalNote { get; set; }

        public bool IsActive { get; set; } = true;

        public List<CreateUpdateCalendarSlotPriceDto> Prices { get; set; } = new();
    }
}
