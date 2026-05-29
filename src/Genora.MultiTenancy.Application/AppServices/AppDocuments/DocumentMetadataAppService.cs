using Genora.MultiTenancy.AppDtos.AppDocuments;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;

namespace Genora.MultiTenancy.AppServices.AppDocuments;

[Authorize]
public class DocumentMetadataAppService : ApplicationService, IDocumentMetadataAppService
{
    public async Task<List<DocumentLookupDto>> GetFeatureLookupAsync()
    {
        await CheckHostAsync();

        // Curated list of feature flags Host can use to gate sections/pages.
        // Strings literal to avoid coupling Application.Contracts to Application feature constants.
        var items = new List<DocumentLookupDto>
        {
            New("Cài đặt Mini App", "MiniAppSetting.Management"),
            New("Sân golf", "MiniAppGolfCourse.Management"),
            New("Loại khách hàng", "MiniAppCustomerType.Management"),
            New("Hạng thành viên", "MiniAppMembershipTier.Management"),
            New("Khách hàng", "MiniAppCustomer.Management"),
            New("Khung giờ chơi", "MiniAppCalendarSlot.Management"),
            New("Đặt chỗ", "MiniAppBooking.Management"),
            New("Tin tức", "MiniAppNews.Management"),
            New("Loại khuyến mãi", "MiniAppPromotionType.Management"),
            New("Chính sách hoãn hủy", "MiniAppPromotionPolicy.Management"),
            New("Ngày đặc biệt", "MiniAppSpecialDate.Management"),
            New("Tích hợp Zalo OA", "MiniAppZaloAuth.Management"),
            New("Nhật ký Zalo", "MiniAppZaloLog.Management"),
            New("Email", "MiniAppEmail.Management"),
            New("Cấu hình thanh toán", "MiniAppPaymentConfiguration.Management"),
            New("Cấu hình trang chủ", "MiniAppHomePage.Management"),
            New("F&B", "MiniAppFnb.Management"),
            New("Proshop", "MiniAppProshop.Management"),
            New("Salon Beauty", "SalonBeauty.Management")
        };

        return items.OrderBy(x => x.Name).ToList();
    }

    public async Task<List<DocumentLookupDto>> GetTenantPermissionLookupAsync()
    {
        await CheckHostAsync();
        return CollectPermissionConsts(tenantOnly: true)
            .OrderBy(x => x.Value)
            .ToList();
    }

    public async Task<List<DocumentLookupDto>> GetHostPermissionLookupAsync()
    {
        await CheckHostAsync();
        return CollectPermissionConsts(tenantOnly: false)
            .OrderBy(x => x.Value)
            .ToList();
    }

    private async Task CheckHostAsync()
        => await AuthorizationService.CheckAsync(MultiTenancyPermissions.HostAppDocuments.Default);

    private static IEnumerable<DocumentLookupDto> CollectPermissionConsts(bool tenantOnly)
    {
        var rootType = typeof(MultiTenancyPermissions);
        var nestedTypes = rootType.GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

        foreach (var nested in nestedTypes)
        {
            var isHostClass = nested.Name.StartsWith("Host", StringComparison.Ordinal);
            if (tenantOnly && isHostClass) continue;
            if (!tenantOnly && !isHostClass) continue;

            var defaultField = nested.GetField("Default", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (defaultField == null) continue;
            var value = defaultField.GetRawConstantValue() as string;
            if (string.IsNullOrEmpty(value)) continue;

            yield return new DocumentLookupDto
            {
                Name = $"{nested.Name}",
                Value = value
            };
        }
    }

    private static DocumentLookupDto New(string name, string value)
        => new() { Name = name, Value = value };
}
