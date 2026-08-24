namespace Genora.MultiTenancy.Enums.Hlg;

/// <summary>
/// Loại khách hàng Gamification — quyết định luồng nhận quà sau game.
/// Map contract frontend: "pharmacy" | "consumer".
/// </summary>
public enum HlgCustomerType : byte
{
    /// <summary>Nhà thuốc — nhận quà theo luồng pharmacy (không cần địa chỉ ship).</summary>
    Pharmacy = 1,

    /// <summary>Người tiêu dùng — nhận quà vật lý cần địa chỉ giao hàng.</summary>
    Consumer = 2
}
