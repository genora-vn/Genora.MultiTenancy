using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.DomainModels.AppHlGiftReceipts;
using Genora.MultiTenancy.Features.AppHoaLinhFeatures;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Features;
using Volo.Abp.Uow;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

[AllowAnonymous]
[RemoteService(false)]
public class MiniAppHlGiftReceiptService : ApplicationService, IMiniAppHlGiftReceiptService
{
    private readonly IHlGiftReceiptRepository _receipts;
    private readonly IHlApiClientService _dms;
    private readonly IFeatureChecker _features;
    private readonly IUnitOfWorkManager _uow;

    public MiniAppHlGiftReceiptService(IHlGiftReceiptRepository receipts, IHlApiClientService dms,
        IFeatureChecker features, IUnitOfWorkManager uow)
    { _receipts = receipts; _dms = dms; _features = features; _uow = uow; }

    private static UserFriendlyException Error(string code, string message) => new(message, "HlGiftReceipt:" + code);

    private async Task<Guid?> CheckScopeAsync()
    {
        // Host uses the default database with TenantId = null, just like other Sales Mini App flows.
        // Tenant feature switches do not apply to Host; admin pages retain their separate permissions.
        var tenantId = CurrentTenant.Id;
        if (tenantId.HasValue && (!await _features.IsEnabledAsync(AppHoaLinhFeatures.Management)
            || !await _features.IsEnabledAsync(AppHoaLinhFeatures.GiftReceipts)))
            throw Error("Disabled", "Tính năng nhận quà chưa được bật.");
        return tenantId;
    }

    public static string NormalizePhone(string phone)
    {
        var value = (phone ?? "").Trim();
        if (value.StartsWith("+84")) value = "0" + value[3..];
        else if (value.StartsWith("84")) value = "0" + value[2..];
        if (!Regex.IsMatch(value, @"^0\d{9,10}$"))
            throw Error("InvalidPhone", "Số điện thoại không hợp lệ.");
        return value;
    }

    private static void Validate(object input)
    {
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), results, true))
            throw Error("InvalidData", "Dữ liệu nhận quà thiếu hoặc vượt quá giới hạn cho phép.");
    }

    public async Task<HlGiftReceiptDto> ConfirmAsync(HlConfirmGiftInput input)
    {
        var tenantId = await CheckScopeAsync();
        Validate(input);
        var phone = NormalizePhone(input.PhoneNumber);
        var custCode = input.CustCode.Trim().ToUpperInvariant();
        var campaignCode = input.CampaignCode.Trim().ToUpperInvariant();
        var voucherCode = input.VoucherCode.Trim().ToUpperInvariant();

        // Verify branch membership using the DMS phone lookup; never trust client-supplied snapshots.
        var customers = await _dms.GetCustomerDetailAsync(phone);
        if (!customers.Success) throw Error("DmsUnavailable", "Không thể kiểm tra khách hàng. Vui lòng thử lại.");
        var customer = customers.Data?.FirstOrDefault(x =>
            string.Equals(x.CustCode?.Trim(), custCode, StringComparison.OrdinalIgnoreCase) && x.IsCustomer != false);
        if (customer == null) throw Error("BranchNotFound", "Số điện thoại không thuộc mã khách hàng/chi nhánh đã chọn.");

        // A retry after confirmation returns the same receipt even if the campaign subsequently expires.
        var query = await _receipts.GetQueryableAsync();
        var previous = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.TenantId == tenantId
            && x.CustCode == custCode && x.CampaignCode == campaignCode
            && x.CampaignPeriod == input.CampaignPeriod && x.VoucherCode == voucherCode));
        if (previous != null) return HlGiftReceiptQuery.Map(previous);

        var result = await _dms.GetCampaignDetailAsync(custCode);
        if (!result.Success || result.Data == null)
            throw Error("DmsUnavailable", "Không thể kiểm tra chiến dịch. Vui lòng thử lại.");
        var matches = result.Data.Where(x => string.Equals(x.CustCode?.Trim(), custCode, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.CampaignCode?.Trim(), campaignCode, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.VoucherCode?.Trim(), voucherCode, StringComparison.OrdinalIgnoreCase)
            && (x.CampaignPeriod ?? 0) == input.CampaignPeriod).ToList();
        if (matches.Count != 1) throw Error("GiftNotFound", "Không xác định được duy nhất quà của khách hàng trong kỳ chiến dịch này.");
        var campaign = matches[0];
        if (campaign.VoucherType != 2) throw Error("InvalidVoucherType", "API nhận quà chỉ áp dụng voucherType = 2.");
        var value = campaign.VoucherValue ?? 0;
        if (value <= 0 || value > int.MaxValue || value != decimal.Truncate(value))
            throw Error("InvalidQuantity", "Số lượng quà của chiến dịch không hợp lệ.");
        var start = ParseDate(campaign.StartDate);
        var end = ParseDate(campaign.EndDate);
        var now = Clock.Now;
        // These are campaign/reporting dates, not a separate claim deadline. Existing Sales redemption
        // also uses DMS entitlement rather than rejecting claims after the accumulation period ends.
        if (start > end) throw Error("InvalidCampaignDate", "Khoảng ngày chiến dịch từ DMS không hợp lệ.");

        var receipt = new HlGiftReceipt(GuidGenerator.Create(), tenantId)
        {
            CustCode = custCode, CustName = customer.CustName!, PhoneNumber = phone, Address = customer.Address,
            CampaignCode = campaignCode, CampaignName = campaign.CampaignName, CampaignPeriod = input.CampaignPeriod,
            CampaignStartDate = start, CampaignEndDate = end, VoucherCode = voucherCode, VoucherName = campaign.VoucherName!,
            VoucherValue = value, Quantity = (int)value, ConfirmedAt = now,
            MembershipTier = campaign.MembershipTier ?? customer.MembershipTier,
            AccumulatedSales = campaign.AccumulatedSales, AccumulatedPoints = campaign.AccumulatedPoints,
            DsrCode = customer.DsrCode, DsrName = customer.DsrName,
            DistributorCode = customer.DistributorCode, DistributorName = customer.DistributorName, Note = input.Note?.Trim()
        };
        Validate(receipt);

        using var uow = _uow.Begin(requiresNew: true, isTransactional: true);
        var existing = await _receipts.FindForConfirmationAsync(tenantId, custCode, campaignCode, input.CampaignPeriod, voucherCode);
        if (existing == null) await _receipts.InsertAsync(receipt, autoSave: true);
        await uow.CompleteAsync();
        return HlGiftReceiptQuery.Map(existing ?? receipt);
    }

    public async Task<PagedResultDto<HlGiftReceiptDto>> GetHistoryAsync(HlGiftReceiptHistoryInput input)
    {
        var tenantId = await CheckScopeAsync();
        Validate(input);
        var phone = NormalizePhone(input.PhoneNumber);
        var code = input.CustCode.Trim().ToUpperInvariant();
        var query = (await _receipts.GetQueryableAsync()).Where(x => x.TenantId == tenantId && x.PhoneNumber == phone && x.CustCode == code);
        if (!string.IsNullOrWhiteSpace(input.CampaignCode))
        { var campaign = input.CampaignCode.Trim().ToUpperInvariant(); query = query.Where(x => x.CampaignCode == campaign); }
        if (input.CampaignPeriod.HasValue) query = query.Where(x => x.CampaignPeriod == input.CampaignPeriod.Value);
        var total = await AsyncExecuter.CountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.ConfirmedAt).ThenBy(x => x.Id)
            .Skip(input.SkipCount).Take(Math.Clamp(input.MaxResultCount, 1, 100)));
        return new PagedResultDto<HlGiftReceiptDto>(total, rows.Select(HlGiftReceiptQuery.Map).ToList());
    }

    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (DateTime.TryParseExact(value.Trim(), new[] { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.FFFFFFF" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)) return date.Date;
        throw Error("InvalidCampaignDate", "Ngày hiệu lực chiến dịch từ DMS không hợp lệ.");
    }
}
