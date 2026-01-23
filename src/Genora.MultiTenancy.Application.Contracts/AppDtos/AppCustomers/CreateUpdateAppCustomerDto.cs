using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppCustomers;

public class CreateUpdateAppCustomerDto
{
    [Required(ErrorMessage = "Validation:Required")]
    [StringLength(20, ErrorMessage = "Validation:StringMax")]
    [RegularExpression(@"^(?:\+84|0)\d{9,10}$", ErrorMessage = "Validation:PhoneInvalid")]
    public string PhoneNumber { get; set; }

    [Required]
    [StringLength(150)]
    public string FullName { get; set; }

    [StringLength(500)]
    public string? AvatarUrl { get; set; }
    public IRemoteStreamContent? AvatarFile { get; set; }

    public byte? Gender { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime? DateOfBirth { get; set; }

    public Guid? CustomerTypeId { get; set; }

    [StringLength(50)]
    public string CustomerCode { get; set; }

    [StringLength(100)]
    public string? ZaloUserId { get; set; }

    public bool IsActive { get; set; } = true;
    public string? VgaCode { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public bool IsFollower { get; set; }

    public decimal? BonusPoint { get; set; }
    public Guid? MembershipTierId { get; set; }
    public string? MembershipTierName { get; set; }
}