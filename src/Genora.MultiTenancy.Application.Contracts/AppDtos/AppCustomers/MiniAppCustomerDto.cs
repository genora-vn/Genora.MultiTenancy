using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using System;
using System.Text.Json.Serialization;

namespace Genora.MultiTenancy.AppDtos.AppCustomers;
public class MiniAppCustomerDto : ZaloBaseResponse
{
    [JsonPropertyName("data")]
    public CustomerData? Data { get; set; }
}
public class CustomerData {
    public Guid Id { get; set; }
    public string CustomerCode { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string FullName { get; set; } = "";
    public byte? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Email { get; set; }
    public string? VgaCode { get; set; }
    public string? AvatarUrl { get; set; }

    public string? ZaloUserId { get; set; }
    public string? ZaloFollowerId { get; set; }
    public bool IsActive { get; set; }

    // Info từ Zalo Me để FE dùng
    public bool? IsFollower { get; set; }
    public bool? IsSensitive { get; set; }
}