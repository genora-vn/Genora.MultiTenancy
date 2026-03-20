namespace Genora.MultiTenancy.Enums;
public enum FnbCancelReason : byte
{
    CustomerRequest = 1, // Khách yêu cầu
    OutOfStock = 2, // Hết món
    KitchenDelay = 3, // Bếp chậm
    WrongOrder = 4, // Đặt sai
    SystemError = 5, // Lỗi hệ thống
    Other = 6 // Lý do khác
}
