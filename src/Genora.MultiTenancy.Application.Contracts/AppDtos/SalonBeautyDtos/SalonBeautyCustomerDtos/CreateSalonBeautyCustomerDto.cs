using System;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.SalonBeautyDtos.SalonBeautyCustomerDtos;

public class CreateSalonBeautyCustomerDto
{
    [Required]
    [StringLength(50)]
    public string CustomerCode { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(15)]
    public string? Phone { get; set; }

    [StringLength(255)]
    [EmailAddress]
    public string? Email { get; set; }

    public byte? Gender { get; set; }
    public DateTime? Birthday { get; set; }

    [StringLength(500)]
    public string? Avatar { get; set; }

    [StringLength(100)]
    public string? ZaloUserId { get; set; }

    public bool IsFollowOa { get; set; }
    public byte? Source { get; set; }
    public byte Status { get; set; } = 1;

    [StringLength(500)]
    public string? Note { get; set; }
}
