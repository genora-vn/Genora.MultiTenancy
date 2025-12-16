using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppCustomers;

public class AppCustomerDto : AuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public string AvatarUrl { get; set; }

    public string PhoneNumber { get; set; }

    public string FullName { get; set; }

    /// <summary>
    /// 0 = Unknown, 1 = Male, 2 = Female, ... tuỳ enum của bạn
    /// </summary>
    public byte? Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public Guid? CustomerTypeId { get; set; }
    public string CustomerTypeCode { get; set; }
    public string CustomerTypeName { get; set; }

    public string CustomerCode { get; set; }

    public string ZaloUserId { get; set; }        // OA Follower / UserId

    public bool IsActive { get; set; }
}