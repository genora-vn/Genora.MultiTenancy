using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.HoaLinh;

/// <summary>
/// Service điểm thưởng Hoa Linh: đổi điểm/tiền từ chiến dịch, tiêu điểm (FIFO),
/// lịch sử giao dịch, số dư. Internal — controller/service khác gọi.
/// </summary>
public interface IHlPointAppService
{
    /// <summary>
    /// Đổi điểm/tiền từ chiến dịch → tạo lô (hạn +1 năm) + cộng quỹ AppCustomers + ghi sổ cái.
    /// Mỗi (khách + chiến dịch) chỉ đổi 1 lần.
    /// </summary>
    Task<HlPointBatchDto> RedeemFromCampaignAsync(HlRedeemPointInput input, CancellationToken ct = default);

    /// <summary>
    /// Tiêu điểm/tiền theo FIFO (lô cũ nhất trước) khi khách đổi quà.
    /// Trừ quỹ AppCustomers + ghi sổ cái. Throw nếu không đủ số dư.
    /// </summary>
    Task SpendAsync(string customerCode, int unit, decimal value, string? refCode = null, string? description = null, CancellationToken ct = default);

    /// <summary>Số dư điểm/tiền + danh sách lô còn hiệu lực (cho Mini App).</summary>
    Task<HlPointBalanceDto> GetBalanceAsync(string customerCode, CancellationToken ct = default);

    /// <summary>Lịch sử giao dịch điểm của khách (Mini App).</summary>
    Task<List<HlPointTransactionDto>> GetCustomerHistoryAsync(string customerCode, int skip = 0, int take = 20, CancellationToken ct = default);
}
