using System;

namespace Genora.MultiTenancy.AppDtos.AppCustomers;
public class MiniAppCustomerDto
{
    public Guid Id { get; set; }
    public string CustomerCode { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string FullName { get; set; } = "";
    public string? AvatarUrl { get; set; }

    public string? ZaloUserId { get; set; }
    public string? ZaloFollowerId { get; set; }
    public bool IsActive { get; set; }

    // Info từ Zalo Me để FE dùng
    public bool? IsFollower { get; set; }
    public bool? IsSensitive { get; set; }
}