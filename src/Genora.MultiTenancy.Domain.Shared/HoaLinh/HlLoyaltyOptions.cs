namespace Genora.MultiTenancy.HoaLinh;

/// <summary>
/// Cấu hình tỉ lệ quy đổi điểm thưởng Hoa Linh.
/// Bind từ section "HlLoyalty" trong appsettings.json (tùy chọn).
/// ConvertedValue = SourceValue * rate. Mặc định 1 (giữ nguyên giá trị gốc).
/// </summary>
public sealed class HlLoyaltyOptions
{
    public const string SectionName = "HlLoyalty";

    /// <summary>Tỉ lệ quy đổi khi đổi bằng ĐIỂM tích lũy (accumulatedPoints → BonusPoint). VD: 1 = giữ nguyên.</summary>
    public decimal PointRate { get; set; } = 1m;

    /// <summary>Tỉ lệ quy đổi khi đổi bằng TIỀN tích lũy (accumulatedSales → BonusAmount). VD: 1 = giữ nguyên, 0.001 = 1000đ→1.</summary>
    public decimal AmountRate { get; set; } = 1m;
}
