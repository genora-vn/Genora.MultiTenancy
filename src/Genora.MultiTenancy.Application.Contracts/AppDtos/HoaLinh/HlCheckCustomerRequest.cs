namespace Genora.MultiTenancy.AppDtos.HoaLinh;

/// <summary>
/// Dữ liệu Mini App truyền vào khi check + đăng ký khách hàng Hoa Linh.
/// Dùng để lưu/đăng ký vào dbo.AppCustomers khi khách hàng chưa tồn tại bên HL DMS.
/// </summary>
public class HlCheckCustomerRequest
{
    /// <summary>SĐT khách hàng (bắt buộc) — dùng để check bên HL DMS và làm key upsert.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Tên hiển thị từ Zalo (me.name).</summary>
    public string? FullName { get; set; }

    /// <summary>Ảnh đại diện Zalo (me.avatar).</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Zalo user id (me.id).</summary>
    public string? ZaloUserId { get; set; }

    /// <summary>Đã follow OA hay chưa (sFinal.followedOA).</summary>
    public bool? IsFollower { get; set; }

    /// <summary>Ghi chú (mặc định "Khách tạo từ Zalo Mini App").</summary>
    public string? Note { get; set; }
}
