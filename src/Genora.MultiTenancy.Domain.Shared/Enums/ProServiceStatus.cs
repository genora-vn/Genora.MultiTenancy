namespace Genora.MultiTenancy.Enums;
public enum ProServiceStatus : byte
{
    Created     = 1, // Mới tạo
    Processing  = 2, // Đang xử lý
    Ready       = 3, // Sẵn sàng giao
    Delivered   = 4, // Đã giao
    Cancelled   = 5  // Đã hủy
}
