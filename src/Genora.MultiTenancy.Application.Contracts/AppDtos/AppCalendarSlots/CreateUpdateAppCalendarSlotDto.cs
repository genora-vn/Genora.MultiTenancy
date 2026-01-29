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

        [Range(0, double.MaxValue)]
        public decimal? Price9 { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price18 { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price27 { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price36 { get; set; }
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
        public Guid PromotionTypeId { get; set; }

        [Range(1, 100)]
        public int MaxSlots { get; set; }

        [StringLength(500)]
        public string? InternalNote { get; set; }

        public bool IsActive { get; set; } = true;

        public List<CreateUpdateCalendarSlotPriceDto> Prices { get; set; } = new();
    }
}
