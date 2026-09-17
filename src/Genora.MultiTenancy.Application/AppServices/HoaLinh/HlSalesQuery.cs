using System;
using System.Linq;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.DomainModels.AppHlPoints;
using Genora.MultiTenancy.DomainModels.AppHlGiftExchanges;
using Volo.Abp;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

// Shared by list and Excel endpoints. End dates include the whole day.
public static class HlSalesQuery
{
    public static void ValidateDates(DateTime? from, DateTime? to)
    {
        if (from?.Date > to?.Date)
            throw new UserFriendlyException("Từ ngày không được lớn hơn đến ngày.");
        if (to?.Date == DateTime.MaxValue.Date)
            throw new UserFriendlyException("Đến ngày nằm ngoài phạm vi hỗ trợ.");
    }

    public static IQueryable<HlPointTransaction> Transactions(IQueryable<HlPointTransaction> query, HlPointHistoryFilter input)
    {
        ValidateDates(input.DateFrom, input.DateTo);
        if (!string.IsNullOrWhiteSpace(input.Search))
            query = query.Where(x => (x.CustomerCode ?? "").Contains(input.Search) || (x.CustomerName ?? "").Contains(input.Search)
                || (x.CustomerPhone ?? "").Contains(input.Search) || (x.RefCode ?? "").Contains(input.Search));
        if (input.Type.HasValue) query = query.Where(x => (int)x.Type == input.Type.Value);
        if (input.DateFrom.HasValue) { var from = input.DateFrom.Value.Date; query = query.Where(x => x.CreationTime >= from); }
        if (input.DateTo.HasValue) { var until = input.DateTo.Value.Date.AddDays(1); query = query.Where(x => x.CreationTime < until); }
        return query;
    }

    public static IQueryable<HlPointBatch> Batches(IQueryable<HlPointBatch> query, HlPointHistoryFilter input)
    {
        ValidateDates(input.DateFrom, input.DateTo);
        if (!string.IsNullOrWhiteSpace(input.Search))
            query = query.Where(x => (x.CustomerCode ?? "").Contains(input.Search) || (x.CustomerName ?? "").Contains(input.Search)
                || (x.CustomerPhone ?? "").Contains(input.Search) || x.BatchCode.Contains(input.Search) || (x.CampaignCode ?? "").Contains(input.Search));
        if (input.DateFrom.HasValue) { var from = input.DateFrom.Value.Date; query = query.Where(x => x.ExchangedAt >= from); }
        if (input.DateTo.HasValue) { var until = input.DateTo.Value.Date.AddDays(1); query = query.Where(x => x.ExchangedAt < until); }
        return query;
    }

    public static IQueryable<HlGiftExchange> Gifts(IQueryable<HlGiftExchange> query, HlGiftExchangeFilterDto input)
    {
        ValidateDates(input.DateFrom, input.DateTo);
        if (!string.IsNullOrWhiteSpace(input.Filter))
            query = query.Where(x => x.ExchangeCode.Contains(input.Filter) || (x.CustomerName ?? "").Contains(input.Filter)
                || (x.CustomerCode ?? "").Contains(input.Filter) || (x.CustomerPhone ?? "").Contains(input.Filter) || x.GiftName.Contains(input.Filter));
        if (input.Status.HasValue) query = query.Where(x => x.Status == input.Status.Value);
        if (input.DateFrom.HasValue) { var from = input.DateFrom.Value.Date; query = query.Where(x => x.CreationTime >= from); }
        if (input.DateTo.HasValue) { var until = input.DateTo.Value.Date.AddDays(1); query = query.Where(x => x.CreationTime < until); }
        return query;
    }
}
