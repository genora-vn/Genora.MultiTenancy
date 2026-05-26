using System;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;

/// <summary>
/// Một dòng trong tab "Lịch sử nạp & tiêu điểm" — gộp từ deposit transactions + loyalty transactions.
/// </summary>
public class SalonBeautyCustomerLedgerDto
{
    public Guid Id { get; set; }

    /// <summary>"DEPOSIT" | "EARN" | "REDEEM" | "ADJUST".</summary>
    public string EntryType { get; set; } = string.Empty;
    public string EntryTypeText { get; set; } = string.Empty;

    public DateTime EntryDate { get; set; }

    /// <summary>Mã giao dịch (DEP... cho deposit, transactionCode cho loyalty).</summary>
    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>Số tiền (VND). Null nếu là loyalty thuần (chỉ điểm).</summary>
    public decimal? Amount { get; set; }

    /// <summary>Số điểm (+/-).</summary>
    public int Point { get; set; }

    /// <summary>Trạng thái: Pending / Success / Cancelled (cho deposit) hoặc Done (cho loyalty).</summary>
    public string Status { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
}
