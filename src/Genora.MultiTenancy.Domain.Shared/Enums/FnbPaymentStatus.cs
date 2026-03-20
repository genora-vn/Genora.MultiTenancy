namespace Genora.MultiTenancy.Enums;
public enum FnbPaymentStatus : byte
{
    Unpaid = 1, // Chưa thanh toán
    Paid = 2, // Đã thanh toán
    Failed = 3 // Thanh toán thất bại
}
