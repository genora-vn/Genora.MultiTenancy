using System.Collections.Generic;
using Genora.MultiTenancy.AppServices.HoaLinh;
using Genora.MultiTenancy.Enums;
using Shouldly;
using Xunit;

namespace Genora.MultiTenancy.HoaLinhSales;

/// <summary>
/// Unit test cho logic payload động của đơn đăng ký áo Blouse:
/// tồn kho, giới hạn áo tặng/đổi, tính tổng điểm, gộp dòng cùng size.
/// </summary>
public class HlBlouseValidatorTests
{
    private static HlBlouseValidator.Line Line(
        HlBlouseItemType type, HlBlouseStyle style, string sizeKey, int qty, int stock)
        => new()
        {
            ItemType = type,
            Style = style,
            SizeKey = sizeKey,
            Quantity = qty,
            Stock = stock,
            SizeLabel = $"{style} {sizeKey}"
        };

    [Fact]
    public void Computes_Free_Exchange_Totals_And_Points()
    {
        var lines = new[]
        {
            Line(HlBlouseItemType.Free, HlBlouseStyle.Male, "M", 1, 100),
            Line(HlBlouseItemType.Free, HlBlouseStyle.Female, "M", 1, 100),
            Line(HlBlouseItemType.Exchange, HlBlouseStyle.Male, "L", 2, 100),
        };

        var s = HlBlouseValidator.BuildSummary(lines, freeShirtLimit: 2, pointsPerShirt: 150, maxExchangeShirt: 0);

        s.FreeQuantity.ShouldBe(2);
        s.ExchangeQuantity.ShouldBe(2);
        s.TotalQuantity.ShouldBe(4);
        s.TotalPointsUsed.ShouldBe(300); // 2 áo đổi * 150
    }

    [Fact]
    public void Ignores_Zero_Quantity_Lines()
    {
        var lines = new[]
        {
            Line(HlBlouseItemType.Free, HlBlouseStyle.Male, "M", 1, 10),
            Line(HlBlouseItemType.Free, HlBlouseStyle.Female, "M", 0, 10), // bỏ qua
        };

        var s = HlBlouseValidator.BuildSummary(lines, 2, 150, 0);
        s.FreeQuantity.ShouldBe(1);
        s.TotalQuantity.ShouldBe(1);
    }

    [Fact]
    public void Sums_Stock_Across_Free_And_Exchange_For_Same_Size()
    {
        // Cùng size "M" (nam): 1 free + 2 exchange = cần 3, kho chỉ có 2 → hết hàng
        var lines = new[]
        {
            Line(HlBlouseItemType.Free, HlBlouseStyle.Male, "M", 1, 2),
            Line(HlBlouseItemType.Exchange, HlBlouseStyle.Male, "M", 2, 2),
        };

        var ex = Should.Throw<BlouseValidationError>(() =>
            HlBlouseValidator.BuildSummary(lines, 2, 150, 0));
        ex.Code.ShouldBe("HlBlouse:OutOfStock");
    }

    [Fact]
    public void Allows_When_Total_Within_Stock()
    {
        var lines = new[]
        {
            Line(HlBlouseItemType.Free, HlBlouseStyle.Male, "M", 1, 3),
            Line(HlBlouseItemType.Exchange, HlBlouseStyle.Male, "M", 2, 3),
        };

        var s = HlBlouseValidator.BuildSummary(lines, 2, 150, 0);
        s.StockNeededBySize["M"].ShouldBe(3);
    }

    [Fact]
    public void Throws_When_Free_Limit_Exceeded()
    {
        var lines = new[]
        {
            Line(HlBlouseItemType.Free, HlBlouseStyle.Male, "M", 2, 100),
            Line(HlBlouseItemType.Free, HlBlouseStyle.Female, "L", 1, 100),
        };

        var ex = Should.Throw<BlouseValidationError>(() =>
            HlBlouseValidator.BuildSummary(lines, freeShirtLimit: 2, pointsPerShirt: 150, maxExchangeShirt: 0));
        ex.Code.ShouldBe("HlBlouse:FreeLimitExceeded");
    }

    [Fact]
    public void Throws_When_Exchange_Limit_Exceeded()
    {
        var lines = new[]
        {
            Line(HlBlouseItemType.Exchange, HlBlouseStyle.Male, "M", 3, 100),
        };

        var ex = Should.Throw<BlouseValidationError>(() =>
            HlBlouseValidator.BuildSummary(lines, freeShirtLimit: 2, pointsPerShirt: 150, maxExchangeShirt: 2));
        ex.Code.ShouldBe("HlBlouse:ExchangeLimitExceeded");
    }

    [Fact]
    public void Throws_When_No_Items()
    {
        var lines = new List<HlBlouseValidator.Line>();
        var ex = Should.Throw<BlouseValidationError>(() =>
            HlBlouseValidator.BuildSummary(lines, 2, 150, 0));
        ex.Code.ShouldBe("HlBlouse:NoItems");
    }

    [Fact]
    public void No_Exchange_Means_Zero_Points()
    {
        var lines = new[]
        {
            Line(HlBlouseItemType.Free, HlBlouseStyle.Male, "M", 2, 100),
        };

        var s = HlBlouseValidator.BuildSummary(lines, 2, 150, 0);
        s.TotalPointsUsed.ShouldBe(0);
        s.ExchangeQuantity.ShouldBe(0);
    }
}
