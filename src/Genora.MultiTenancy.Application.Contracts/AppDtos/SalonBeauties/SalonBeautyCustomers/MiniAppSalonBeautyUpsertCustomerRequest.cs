using System;
using Genora.MultiTenancy.Enums;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;

/// <summary>
/// Request đăng ký/cập nhật khách hàng Salon Beauty từ Zalo Mini App.
/// Hỗ trợ cả cặp tên field theo MiniApp cũ (PhoneNumber/FullName/AvatarUrl/DateOfBirth)
/// và field theo Salon Beauty (Phone/Name/Avatar/Birthday) để dễ tích hợp frontend.
/// </summary>
public class MiniAppSalonBeautyUpsertCustomerRequest
{
    public string? PhoneNumber { get; set; }
    public string? Phone { get; set; }

    public string? FullName { get; set; }
    public string? Name { get; set; }

    public string? Email { get; set; }

    /// <summary>
    /// Có thể truyền theo byte enum SalonBeautyGender.
    /// </summary>
    public byte? Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }
    public DateTime? Birthday { get; set; }

    public string? AvatarUrl { get; set; }
    public string? Avatar { get; set; }

    public string? ZaloUserId { get; set; }
    public string? ZaloFollowerId { get; set; }

    public bool? IsFollower { get; set; }
    public bool? IsFollowOa { get; set; }

    /// <summary>
    /// Nếu không truyền, mặc định là Zalo.
    /// </summary>
    public SalonBeautyCustomerSource? Source { get; set; }

    public string? Note { get; set; }
}
