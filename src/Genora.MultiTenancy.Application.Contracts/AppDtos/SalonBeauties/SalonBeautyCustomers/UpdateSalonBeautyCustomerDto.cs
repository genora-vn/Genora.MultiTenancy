using System;
using System.ComponentModel.DataAnnotations;
using Genora.MultiTenancy.Enums;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;

public class UpdateSalonBeautyCustomerDto
{
    [Required(ErrorMessage = "SalonBeautyCustomer:NameRequired")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "SalonBeautyCustomer:PhoneRequired")]
    [StringLength(15)]
    [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "SalonBeautyCustomer:PhoneInvalid")]
    public string Phone { get; set; } = null!;

    [StringLength(255)]
    [EmailAddress(ErrorMessage = "SalonBeautyCustomer:EmailInvalid")]
    public string? Email { get; set; }

    public SalonBeautyGender? Gender { get; set; }
    public DateTime? Birthday { get; set; }

    [StringLength(500)]
    public string? Avatar { get; set; }

    [StringLength(100)]
    public string? ZaloUserId { get; set; }

    public bool IsFollowOa { get; set; }
    public SalonBeautyCustomerSource? Source { get; set; }
    public byte Status { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }
}
