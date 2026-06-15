namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots;

/// <summary>
/// Kết quả validate VGA Code và trả về giá theo loại khách hàng tương ứng.
/// Dùng để front-end cập nhật lại giá khi người chơi cùng nhập Mã hội viên.
/// </summary>
public class ValidateVgaCodeResultDto
{
    /// <summary>
    /// VGA Code có tồn tại trong hệ thống hay không
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Mã loại khách hàng (VIS, MB, MBG, ...)
    /// </summary>
    public string? CustomerTypeCode { get; set; }

    /// <summary>
    /// Tên loại khách hàng (Visitor, Member, Member Guest, ...)
    /// </summary>
    public string? CustomerTypeName { get; set; }

    /// <summary>
    /// Giá / golfer theo loại khách hàng (lấy từ AppCalendarSlotPrices)
    /// </summary>
    public decimal? PricePerGolfer { get; set; }

    /// <summary>
    /// Giá gốc theo loại khách hàng (lấy từ AppCustomerTypes.OriginalPrice*)
    /// </summary>
    public decimal? OriginalPrice { get; set; }
}
