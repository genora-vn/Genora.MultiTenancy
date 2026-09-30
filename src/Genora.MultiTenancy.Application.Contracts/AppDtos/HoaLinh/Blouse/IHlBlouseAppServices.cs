using System;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.HoaLinh.Blouse;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

/// <summary>
/// AppService phục vụ Zalo Mini App "Đăng ký nhận áo Blouse" (public/anonymous).
/// Nhận payload động danh sách áo tặng + áo đổi điểm, validate tồn kho & điểm tích lũy.
/// </summary>
public interface IMiniAppHlBlouseService : IApplicationService
{
    /// <summary>Lấy cấu hình chương trình + danh mục size cho Mini App render form.</summary>
    Task<HlBlouseConfigDto> GetConfigAsync();

    /// <summary>Ghi nhận đơn đăng ký nhận áo (payload động).</summary>
    Task<HlBlouseRegistrationDto> RegisterAsync(HlBlouseRegisterRequest request, CancellationToken ct = default);

    /// <summary>Lấy danh sách đơn đã đăng ký của khách (theo SĐT).</summary>
    Task<ListResultDto<HlBlouseRegistrationDto>> GetMyRegistrationsAsync(string phone, CancellationToken ct = default);
}

/// <summary>
/// AppService quản trị (Admin) đơn đăng ký nhận áo Blouse + cấu hình chương trình + danh mục size.
/// Dual permission Tenant/Host theo chuẩn ABP.
/// </summary>
public interface IHlBlouseAdminAppService : IApplicationService
{
    // ===== Đơn đăng ký =====
    Task<PagedResultDto<HlBlouseRegistrationDto>> GetListAsync(HlBlouseRegistrationFilterDto input);
    Task<HlBlouseRegistrationDto> GetAsync(Guid id);
    Task<HlBlouseRegistrationDto> UpdateStatusAsync(HlBlouseUpdateStatusDto input);

    // ===== Cấu hình chương trình =====
    Task<HlBlouseCampaignDto?> GetCampaignAsync();
    Task<HlBlouseCampaignDto> SaveCampaignAsync(HlBlouseCampaignSaveDto input);

    // ===== Danh mục size =====
    Task<ListResultDto<HlBlouseSizeDto>> GetSizesAsync();
    Task<HlBlouseSizeDto> CreateSizeAsync(HlBlouseSizeSaveDto input);
    Task<HlBlouseSizeDto> UpdateSizeAsync(Guid id, HlBlouseSizeSaveDto input);
    Task DeleteSizeAsync(Guid id);
}
