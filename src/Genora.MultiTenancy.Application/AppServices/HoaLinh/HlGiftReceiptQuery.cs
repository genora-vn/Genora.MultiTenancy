using System;
using System.Linq;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.DomainModels.AppHlGiftReceipts;
using Genora.MultiTenancy.Enums;
using Volo.Abp;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

public static class HlGiftReceiptQuery
{
    public static IQueryable<HlGiftReceipt> Filter(IQueryable<HlGiftReceipt> query, HlGiftReceiptFilter input)
    {
        HlSalesQuery.ValidateDates(input.DateFrom, input.DateTo);
        if (input.Status.HasValue && !Enum.IsDefined(input.Status.Value))
            throw new UserFriendlyException("Trạng thái nhận quà không hợp lệ.");
        var text = input.Filter?.Trim();
        if (!string.IsNullOrEmpty(text))
            query = query.Where(x => x.ReceiptCode.Contains(text) || x.CustCode.Contains(text)
                || x.CustName.Contains(text) || x.PhoneNumber.Contains(text) || (x.Address ?? "").Contains(text)
                || (x.CampaignName ?? "").Contains(text) || x.VoucherName.Contains(text));
        if (!string.IsNullOrWhiteSpace(input.CustCode)) { var code = input.CustCode.Trim().ToUpperInvariant(); query = query.Where(x => x.CustCode == code); }
        if (!string.IsNullOrWhiteSpace(input.PhoneNumber)) { var phone = MiniAppHlGiftReceiptService.NormalizePhone(input.PhoneNumber); query = query.Where(x => x.PhoneNumber == phone); }
        if (!string.IsNullOrWhiteSpace(input.CampaignCode)) { var code = input.CampaignCode.Trim().ToUpperInvariant(); query = query.Where(x => x.CampaignCode == code); }
        if (!string.IsNullOrWhiteSpace(input.VoucherCode)) { var code = input.VoucherCode.Trim().ToUpperInvariant(); query = query.Where(x => x.VoucherCode == code); }
        if (input.CampaignPeriod.HasValue) query = query.Where(x => x.CampaignPeriod == input.CampaignPeriod.Value);
        if (input.Status.HasValue) query = query.Where(x => x.Status == input.Status.Value);
        if (input.DateFrom.HasValue) { var from = input.DateFrom.Value.Date; query = query.Where(x => x.ConfirmedAt >= from); }
        if (input.DateTo.HasValue) { var until = input.DateTo.Value.Date.AddDays(1); query = query.Where(x => x.ConfirmedAt < until); }
        return query;
    }

    public static HlGiftReceiptDto Map(HlGiftReceipt x) => new()
    {
        Id = x.Id, ReceiptCode = x.ReceiptCode, CustCode = x.CustCode, CustName = x.CustName,
        PhoneNumber = x.PhoneNumber, Address = x.Address,
        CampaignCode = x.CampaignCode, CampaignName = x.CampaignName, CampaignPeriod = x.CampaignPeriod,
        CampaignStartDate = x.CampaignStartDate, CampaignEndDate = x.CampaignEndDate,
        VoucherCode = x.VoucherCode, VoucherName = x.VoucherName, VoucherType = x.VoucherType,
        VoucherValue = x.VoucherValue, Quantity = x.Quantity, Status = x.Status, ConfirmedAt = x.ConfirmedAt,
        MembershipTier = x.MembershipTier, AccumulatedSales = x.AccumulatedSales, AccumulatedPoints = x.AccumulatedPoints,
        DsrCode = x.DsrCode, DsrName = x.DsrName, DistributorCode = x.DistributorCode, DistributorName = x.DistributorName,
        Source = x.Source, Note = x.Note, CreationTime = x.CreationTime
    };
}
