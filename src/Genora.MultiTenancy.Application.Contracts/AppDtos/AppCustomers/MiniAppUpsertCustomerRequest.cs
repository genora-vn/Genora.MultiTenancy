using System;

namespace Genora.MultiTenancy.AppDtos.AppCustomers;
public class MiniAppUpsertCustomerRequest
{
    public string PhoneNumber { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public byte? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Email { get; set; }
    public string? VgaCode { get; set; }
    public string? AvatarUrl { get; set; }
    public string? ZaloUserId { get; set; }
    public string? ZaloFollowerId { get; set; }
    public bool? IsFollower { get; set; }
    public bool? IsSensitive { get; set; }
}