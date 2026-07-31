using Genora.MultiTenancy.Enums;

namespace Genora.MultiTenancy.Helpers;
public static class EnumExtension
{
    public static string HLPaymentMethodToDisplayText(this HlPaymentMethod? paymentMethod)
    {
        switch (paymentMethod)
        {
            case HlPaymentMethod.Cash:
                return "Thanh toán tiền mặt (COD)";

            case HlPaymentMethod.BankTransfer:
                return "Chuyển khoản ngân hàng";

            default:
                return "Không xác định";
        }
    }

    public static string HLPaymentStatusToDisplayText(this HlOrderPaymentStatus? paymentStatus)
    {
        switch (paymentStatus)
        {
            case HlOrderPaymentStatus.Unpaid:
                return "Chưa thanh toán";

            case HlOrderPaymentStatus.Paid:
                return "Đã thanh toán";

            default:
                return "Công nợ";
        }
    }
    
    public static string HLOrderStatusToDisplayText(this HlOrderDeliveryStatus? orderStatus)
    {
        switch (orderStatus)
        {
            case HlOrderDeliveryStatus.PendingConfirmation:
                return "Chờ xác nhận";

            case HlOrderDeliveryStatus.Processing:
                return "Đang xử lý";
            case HlOrderDeliveryStatus.Delivering:
                return "Đang giao";
            case HlOrderDeliveryStatus.Completed:
                return "Hoàn thành";

            default:
                return "Đã hủy";
        }
    }
}