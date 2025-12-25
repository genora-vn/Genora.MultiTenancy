using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppCustomers;

public class AppCustomerDto : AuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public string AvatarUrl { get; set; }

    public string PhoneNumber { get; set; }

    public string FullName { get; set; }

    /// <summary>
    /// 0 = Unknown, 1 = Male, 2 = Female
    /// </summary>
    public byte? Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public Guid? CustomerTypeId { get; set; }
    public string CustomerTypeCode { get; set; }
    public string CustomerTypeName { get; set; }

    public string CustomerCode { get; set; }

    public string ZaloUserId { get; set; }

    public bool IsActive { get; set; }
    public string? VgaCode { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public bool IsFollower { get; set; }

    public decimal BonusPoint { get; set; }
    public Guid? MembershipTierId { get; set; }
    public string? MembershipTierName { get; set; }
}