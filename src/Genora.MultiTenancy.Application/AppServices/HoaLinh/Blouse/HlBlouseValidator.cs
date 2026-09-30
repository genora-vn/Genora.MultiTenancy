using System.Collections.Generic;
using System.Linq;
using Genora.MultiTenancy.Enums;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

/// <summary>
/// Logic thuần (pure) tính toán & validate payload động của đơn đăng ký áo Blouse.
/// Tách khỏi AppService để unit-test độc lập (giống HlSalesQuery).
/// Ném <see cref="BlouseValidationError"/> với mã lỗi + tham số để tầng service map sang localization.
/// </summary>
public static class HlBlouseValidator
{
    /// <summary>Một dòng áo đã được resolve size (dùng cho tính toán).</summary>
    public sealed class Line
    {
        public HlBlouseItemType ItemType { get; init; }
        public HlBlouseStyle Style { get; init; }
        public string SizeKey { get; init; } = null!; // định danh size (VD size Id)
        public int Quantity { get; init; }
        public int Stock { get; init; }
        public string SizeLabel { get; init; } = null!; // để hiển thị lỗi
    }

    public sealed class Summary
    {
        public int FreeQuantity { get; set; }
        public int ExchangeQuantity { get; set; }
        public int TotalQuantity => FreeQuantity + ExchangeQuantity;
        public int TotalPointsUsed { get; set; }
        /// <summary>Tổng số lượng cần trừ kho theo từng SizeKey.</summary>
        public Dictionary<string, int> StockNeededBySize { get; set; } = new();
    }

    /// <summary>
    /// Tính tổng hợp + validate quy tắc:
    /// - Bỏ qua dòng quantity &lt;= 0.
    /// - Tồn kho: tổng số lượng mỗi size (gộp free+exchange) không vượt Stock.
    /// - Số áo tặng không vượt freeShirtLimit.
    /// - Số áo đổi không vượt maxExchangeShirt (nếu &gt; 0).
    /// - Tổng điểm = tổng số áo đổi * pointsPerShirt.
    /// </summary>
    public static Summary BuildSummary(
        IEnumerable<Line> lines,
        int freeShirtLimit,
        int pointsPerShirt,
        int maxExchangeShirt)
    {
        var valid = lines.Where(l => l.Quantity > 0).ToList();
        if (valid.Count == 0)
            throw new BlouseValidationError("HlBlouse:NoItems");

        // Validate tồn kho theo tổng mỗi size
        var neededBySize = valid
            .GroupBy(l => l.SizeKey)
            .Select(g => new { Key = g.Key, First = g.First(), Needed = g.Sum(x => x.Quantity) })
            .ToList();
        foreach (var s in neededBySize)
        {
            if (s.Needed > s.First.Stock)
                throw new BlouseValidationError("HlBlouse:OutOfStock", s.First.SizeLabel, s.First.Stock);
        }

        var freeQty = valid.Where(l => l.ItemType == HlBlouseItemType.Free).Sum(l => l.Quantity);
        if (freeQty > freeShirtLimit)
            throw new BlouseValidationError("HlBlouse:FreeLimitExceeded", freeShirtLimit);

        var exchangeQty = valid.Where(l => l.ItemType == HlBlouseItemType.Exchange).Sum(l => l.Quantity);
        if (maxExchangeShirt > 0 && exchangeQty > maxExchangeShirt)
            throw new BlouseValidationError("HlBlouse:ExchangeLimitExceeded", maxExchangeShirt);

        return new Summary
        {
            FreeQuantity = freeQty,
            ExchangeQuantity = exchangeQty,
            TotalPointsUsed = exchangeQty * pointsPerShirt,
            StockNeededBySize = neededBySize.ToDictionary(x => x.Key, x => x.Needed)
        };
    }
}

/// <summary>Lỗi validate nghiệp vụ (mã + tham số) để service map sang UserFriendlyException + localization.</summary>
public sealed class BlouseValidationError : System.Exception
{
    public string Code { get; }
    public object[] Args { get; }

    public BlouseValidationError(string code, params object[] args) : base(code)
    {
        Code = code;
        Args = args;
    }
}
