namespace Genora.MultiTenancy.AppServices.SalonBeauty;

/// <summary>
/// Setting keys cho cấu hình quy đổi điểm thưởng Salon Beauty (lưu per-tenant).
/// </summary>
public static class SalonBeautyLoyaltySettingNames
{
    /// <summary>
    /// Tỷ lệ quy đổi: 1 điểm = bao nhiêu VND.
    /// VD: 1000 → 1.000đ = 1 P.
    /// Default = 1000.
    /// </summary>
    public const string ExchangeRate = "Genora.SalonBeauty.Loyalty.ExchangeRate";
}
