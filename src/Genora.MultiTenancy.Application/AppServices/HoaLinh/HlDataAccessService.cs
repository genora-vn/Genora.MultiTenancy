using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.Apps.AppSettings;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

/// <summary>
/// Service xác định data-level access cho user hiện tại
/// Sales chỉ thấy KH được phân công (filter by dsr_code)
/// Admin/Marketing/Kế toán thấy tất cả
/// </summary>
public interface IHlDataAccessService
{
    /// <summary>
    /// Lấy DsrCode của user hiện tại (nếu là Sales).
    /// Trả null nếu user là Admin (xem tất cả).
    /// </summary>
    Task<string?> GetCurrentUserDsrCodeAsync();

    /// <summary>
    /// Check user hiện tại có phải Sales restricted hay không
    /// </summary>
    Task<bool> IsSalesRestrictedAsync();
}

public class HlDataAccessService : IHlDataAccessService
{
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<AppSetting, Guid> _settingRepo;

    public HlDataAccessService(
        ICurrentUser currentUser,
        IRepository<AppSetting, Guid> settingRepo)
    {
        _currentUser = currentUser;
        _settingRepo = settingRepo;
    }

    public async Task<string?> GetCurrentUserDsrCodeAsync()
    {
        if (_currentUser.Id == null) return null;

        // Admin thấy tất cả
        if (_currentUser.IsInRole("admin")) return null;

        // Tìm mapping user → dsr_code trong AppSettings
        var settingKey = HlSettingNames.GetUserDsrCodeKey(_currentUser.Id.Value);
        var setting = await _settingRepo.FindAsync(x => x.SettingKey == settingKey);

        return setting?.SettingValue;
    }

    public async Task<bool> IsSalesRestrictedAsync()
    {
        if (_currentUser.Id == null) return false;
        if (_currentUser.IsInRole("admin")) return false;

        var dsrCode = await GetCurrentUserDsrCodeAsync();
        return !string.IsNullOrEmpty(dsrCode);
    }
}
