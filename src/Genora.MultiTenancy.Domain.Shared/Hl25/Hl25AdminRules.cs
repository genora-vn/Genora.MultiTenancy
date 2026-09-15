using System;
using Volo.Abp;

namespace Genora.MultiTenancy.Hl25;

public static class Hl25AdminRules
{
    public static void ValidateDateRange(DateTime? start, DateTime? end)
    {
        if (start.HasValue && end.HasValue && end.Value < start.Value)
            throw new BusinessException("Hl25:InvalidDateRange");
    }

    public static void ValidateStock(int total, int remaining)
    {
        if (total < 0 || remaining < 0 || remaining > total)
            throw new BusinessException("Hl25:InvalidStockQuantity");
    }
}
