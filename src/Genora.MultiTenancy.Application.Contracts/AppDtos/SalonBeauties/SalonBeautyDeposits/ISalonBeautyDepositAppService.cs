using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyDeposits;

public interface ISalonBeautyDepositAppService :
    ICrudAppService<
        SalonBeautyDepositDto,
        Guid,
        GetSalonBeautyDepositListInput,
        CreateSalonBeautyDepositDto,
        UpdateSalonBeautyDepositDto>
{
    /// <summary>Preview tính điểm trước khi tạo (không persist).</summary>
    Task<DepositPreviewResultDto> PreviewAsync(decimal amount);

    /// <summary>Duyệt — Pending → Success, cộng điểm vào ví khách (ACID).</summary>
    Task<SalonBeautyDepositDto> ApproveAsync(Guid id);

    /// <summary>Hủy — Pending → Cancelled (không cộng điểm).</summary>
    Task<SalonBeautyDepositDto> CancelAsync(Guid id, CancelDepositDto input);
}
