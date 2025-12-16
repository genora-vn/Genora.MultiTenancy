using System;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppCustomers;

public class CreateUpdateAppCustomerDto
{
    [Required]
    [Phone]
    [StringLength(20)]
    public string PhoneNumber { get; set; }

    [Required]
    [StringLength(150)]
    public string FullName { get; set; }

    [StringLength(500)]
    public string AvatarUrl { get; set; }

    public byte? Gender { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime? DateOfBirth { get; set; }

    public Guid? CustomerTypeId { get; set; }

    [StringLength(50)]
    public string CustomerCode { get; set; }

    [StringLength(100)]
    public string ZaloUserId { get; set; }

    public bool IsActive { get; set; } = true;
}