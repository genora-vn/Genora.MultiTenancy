namespace Genora.MultiTenancy.Enums;
public enum ProCancelReason : byte
{
    CustomerRequest = 1, // Khách yêu cầu
    OutOfStock      = 2, // Hết hàng
    WrongOrder      = 3, // Đặt sai
    SystemError     = 4, // Lỗi hệ thống
    Other           = 5  // Lý do khác
}
