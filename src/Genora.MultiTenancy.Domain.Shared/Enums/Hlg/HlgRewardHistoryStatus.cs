namespace Genora.MultiTenancy.Enums.Hlg;

/// <summary>Trạng thái đổi quà. Map contract: "pending" | "shipping" | "delivered" | "done".</summary>
public enum HlgRewardHistoryStatus : byte
{
    /// <summary>Chờ xử lý.</summary>
    Pending = 1,

    /// <summary>Đang giao (quà vật lý).</summary>
    Shipping = 2,

    /// <summary>Đã giao.</summary>
    Delivered = 3,

    /// <summary>Hoàn tất (voucher đã cấp / quà đã nhận).</summary>
    Done = 4
}
