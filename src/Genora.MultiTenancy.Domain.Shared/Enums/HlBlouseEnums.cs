namespace Genora.MultiTenancy.Enums;

/// <summary>
/// Dáng áo Blouse: Nam / Nữ
/// </summary>
public enum HlBlouseStyle : byte
{
    /// <summary>Dáng nam</summary>
    Male = 1,

    /// <summary>Dáng nữ</summary>
    Female = 2
}

/// <summary>
/// Loại dòng đăng ký áo Blouse: Tặng miễn phí / Đổi bằng điểm tích lũy
/// </summary>
public enum HlBlouseItemType : byte
{
    /// <summary>Áo tặng miễn phí (theo suất GKHL)</summary>
    Free = 1,

    /// <summary>Áo đổi bằng điểm tích lũy (thêu tên)</summary>
    Exchange = 2
}

/// <summary>
/// Trạng thái xử lý đơn đăng ký nhận áo Blouse
/// </summary>
public enum HlBlouseRegistrationStatus : byte
{
    /// <summary>Chờ xác nhận (mới tạo từ Mini App)</summary>
    Pending = 0,

    /// <summary>Đã xác nhận (admin duyệt)</summary>
    Confirmed = 1,

    /// <summary>Đang xử lý / sản xuất</summary>
    Processing = 2,

    /// <summary>Đã giao</summary>
    Delivered = 3,

    /// <summary>Đã hủy</summary>
    Cancelled = 4,

    /// <summary>Bị từ chối</summary>
    Rejected = 5
}

/// <summary>
/// Loại hình kinh doanh hiển thị/in trên áo
/// </summary>
public enum HlBlouseBusinessType : byte
{
    /// <summary>Nhà thuốc</summary>
    Pharmacy = 1,

    /// <summary>Quầy thuốc</summary>
    Drugstore = 2,

    /// <summary>Khác (nhập tên loại hình ở BusinessTypeName)</summary>
    Other = 3
}
