using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.UrBox;

/// <summary>
/// Model tổng hợp chi tiết đơn đổi quà cho Mini App hiển thị.
/// Gộp dữ liệu từ: HL.AppHlGiftExchanges (Genora) + UrBox getByTransaction + UrBox gift detail.
/// </summary>
public class UrBoxGiftTransactionDetailDto
{
    // ── Thông tin bản ghi Genora (HL.AppHlGiftExchanges) ─────────────────────
    public Guid Id { get; set; }
    public string? ExchangeCode { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    /// <summary>Trạng thái: 0=Failed, 1=Success, 2=Processing, 3=Used</summary>
    public int Status { get; set; }
    public string? StatusText { get; set; }
    public int PointsRequired { get; set; }
    public int Quantity { get; set; }
    public int TotalPointsUsed { get; set; }
    public DateTime CreationTime { get; set; }

    // ── Thông tin voucher (từ UrBox getByTransaction / gift detail) ──────────
    public string? GiftName { get; set; }
    public string? GiftCode { get; set; }
    public string? GiftImageUrl { get; set; }
    /// <summary>Ảnh QR/Barcode để hiển thị (code_image) — voucher ĐẦU TIÊN (giữ để tương thích ngược).</summary>
    public string? CodeImage { get; set; }
    /// <summary>Mã voucher/eVoucher (code) — voucher ĐẦU TIÊN (giữ để tương thích ngược).</summary>
    public string? VoucherCode { get; set; }
    public string? CodeDisplay { get; set; }
    public int? CodeDisplayType { get; set; }
    /// <summary>Hạn sử dụng (expired) — voucher ĐẦU TIÊN (giữ để tương thích ngược).</summary>
    public string? Expired { get; set; }
    /// <summary>Số tiền voucher (money_total)</summary>
    public decimal? MoneyTotal { get; set; }
    /// <summary>Link nhận quà UrBox (mở trang chi tiết voucher) — voucher ĐẦU TIÊN (giữ để tương thích ngược).</summary>
    public string? LinkGift { get; set; }
    public string? DeliveryStatus { get; set; }

    /// <summary>
    /// Danh sách TẤT CẢ voucher/phần quà trong giao dịch (mỗi giao dịch có thể đổi số lượng > 1).
    /// Mini App hiển thị đầy đủ nhiều mã quà thay vì chỉ 1 phần.
    /// </summary>
    public List<UrBoxVoucherDetailDto> Vouchers { get; set; } = new();

    // ── Thông tin hiệu lực + thương hiệu + note (từ gift detail) ─────────────
    public string? BrandName { get; set; }
    public string? BrandImage { get; set; }
    public string? ExpireDuration { get; set; }
    /// <summary>Điều kiện/lưu ý sử dụng (note HTML)</summary>
    public string? Note { get; set; }
    /// <summary>Nội dung mô tả (content HTML)</summary>
    public string? Content { get; set; }
    /// <summary>Danh sách chi nhánh/điểm áp dụng (office)</summary>
    public List<UrBoxOfficeDto> Offices { get; set; } = new();

    // ── Thông tin người nhận (từ getByTransaction) ───────────────────────────
    public string? ReceiverPhone { get; set; }
    public string? ReceiverEmail { get; set; }
    public string? ReceiverAddress { get; set; }
    public string? TransactionId { get; set; }
}

/// <summary>
/// Chi tiết một voucher/phần quà trong giao dịch (map từ UrBox getByTransaction → data.detail[]).
/// Dùng cho danh sách <see cref="UrBoxGiftTransactionDetailDto.Vouchers"/>.
/// </summary>
public class UrBoxVoucherDetailDto
{
    /// <summary>Mã voucher/eVoucher (code)</summary>
    public string? Code { get; set; }
    /// <summary>Ảnh QR/Barcode (code_image)</summary>
    public string? CodeImage { get; set; }
    /// <summary>Mã hiển thị (code_display)</summary>
    public string? CodeDisplay { get; set; }
    /// <summary>Kiểu hiển thị mã (code_display_type)</summary>
    public int? CodeDisplayType { get; set; }
    /// <summary>Hạn sử dụng (expired)</summary>
    public string? Expired { get; set; }
    /// <summary>Link nhận quà UrBox (link)</summary>
    public string? Link { get; set; }
    /// <summary>Trạng thái giao (delivery)</summary>
    public string? Delivery { get; set; }
}
