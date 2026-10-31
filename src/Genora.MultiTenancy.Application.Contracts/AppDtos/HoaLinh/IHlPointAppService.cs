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
    /// Tiêu điểm/tiền theo FIFO (lô hết hạn sớm nhất trước) khi khách đổi quà.
    /// KHÓA THEO SĐT — quỹ thưởng thuộc về số điện thoại, không thuộc từng mã KH DMS
    /// (1 SĐT có nhiều mã KH, mọi mã đều cộng dồn vào cùng 1 quỹ).
    /// Throw UserFriendlyException nếu không đủ số dư khả dụng.
    /// Phải gọi TRƯỚC khi phát hành quà — không bao giờ gọi sau.
    /// </summary>
    Task SpendAsync(string customerPhone, int unit, decimal value, string? refCode = null, string? description = null, CancellationToken ct = default);

    /// <summary>
    /// Hoàn lại giá trị đã tiêu (giao dịch bù) khi phát hành quà thất bại sau khi đã trừ.
    /// Tạo lô hoàn mới giữ nguyên hạn của lô gốc gần nhất; ghi sổ cái Type=Adjust.
    /// </summary>
    Task RefundAsync(string customerPhone, int unit, decimal value, string? refCode = null, string? description = null, CancellationToken ct = default);

    /// <summary>Số dư điểm/tiền + danh sách lô còn hiệu lực (cho Mini App). Khóa theo SĐT.</summary>
    Task<HlPointBalanceDto> GetBalanceAsync(string customerPhone, CancellationToken ct = default);

    /// <summary>Lịch sử giao dịch điểm của khách (Mini App). Khóa theo SĐT.</summary>
    Task<List<HlPointTransactionDto>> GetCustomerHistoryAsync(string customerPhone, int skip = 0, int take = 20, CancellationToken ct = default);

    /// <summary>
    /// Tìm SĐT (khóa quỹ) từ mã KH DMS hoặc chính SĐT. Dùng cho các endpoint Mini App cũ
    /// vẫn truyền mã KH trên URL.
    /// Thứ tự tra: đã là SĐT → dbo.AppCustomers.CustomerCode → AppHlPointBatches.CustomerCode
    /// (bước cuối cần thiết vì có mã KH DMS không khớp CustomerCode đang lưu trong AppCustomers).
    /// Trả null khi không tra được.
    /// </summary>
    Task<string?> ResolvePhoneAsync(string customerCodeOrPhone, CancellationToken ct = default);
}
