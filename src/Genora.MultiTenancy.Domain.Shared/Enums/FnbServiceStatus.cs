namespace Genora.MultiTenancy.Enums;
public enum FnbServiceStatus : byte
{
    Created = 1, // Mới tạo
    Preparing = 2, // Đang chuẩn bị
    Delivering = 3, // Đang giao
    Served = 4, // Đã phục vụ
    Cancelled = 5 // Đã hủy
}