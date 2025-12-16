using Genora.MultiTenancy.DomainModels.AppBookingPlayers;
using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppCustomerMemberships;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppCustomers;

[Table("AppCustomers")]
public class Customer : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid? CustomerTypeId { get; set; }
    public virtual CustomerType? CustomerType { get; set; }

    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    [StringLength(150)]
    public string FullName { get; set; } = null!;

    /// <summary>
    /// 0: Unknown, 1: Male, 2: Female,...
    /// </summary>
    public byte? Gender { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime? DateOfBirth { get; set; } // Date only

    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    [StringLength(50)]
    public string? CustomerCode { get; set; }

    [StringLength(100)]
    public string? ZaloUserId { get; set; }

    [StringLength(100)]
    public string? ZaloFollowerId { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<BookingPlayer> BookingPlayers { get; set; } = new List<BookingPlayer>();
    public virtual ICollection<CustomerMembership> Memberships { get; set; } = new List<CustomerMembership>();

    protected Customer() { }

    public Customer(Guid id, string phoneNumber, string fullName) : base(id)
    {
        PhoneNumber = phoneNumber;
        FullName = fullName;
    }
}