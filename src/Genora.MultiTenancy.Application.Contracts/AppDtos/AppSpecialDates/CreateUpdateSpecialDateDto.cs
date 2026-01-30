using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppSpecialDates;
public class CreateUpdateSpecialDateDto
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = default!;

    [StringLength(500)]
    public string? Description { get; set; }

    // chỉ dùng khi Name = "Ngày lễ"
    public List<DateTime>? Dates { get; set; }

    // NEW: chỉ dùng khi Name != "Ngày lễ"
    // Quy ước: 0..6 = T2..CN (Mon..Sun). Nếu null/empty => mặc định "Tất cả".
    public List<int>? Weekdays { get; set; }

    public Guid? GolfCourseId { get; set; }
    public bool IsActive { get; set; } = true;
}