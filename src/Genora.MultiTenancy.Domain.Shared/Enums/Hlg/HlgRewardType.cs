namespace Genora.MultiTenancy.Enums.Hlg;

/// <summary>Loại phần thưởng. Map contract: "physical" | "voucher".</summary>
public enum HlgRewardType : byte
{
    /// <summary>Quà vật lý — cần địa chỉ giao hàng (luồng consumer).</summary>
    Physical = 1,

    /// <summary>eVoucher — phát qua UrBox, không cần ship.</summary>
    Voucher = 2
}
