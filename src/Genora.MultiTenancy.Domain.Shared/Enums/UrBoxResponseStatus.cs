using System.Collections.Generic;

namespace Genora.MultiTenancy.Enums;

/// <summary>
/// Bảng mã trạng thái trả về từ hệ thống UrBox (field "status" trong response).
/// Dùng để map mã lỗi UrBox sang thông báo tiếng Việt cho Mini App.
/// Tham khảo: tài liệu tích hợp kho quà eVoucher UrBox.
/// </summary>
public static class UrBoxResponseStatus
{
    /// <summary>Thành công</summary>
    public const int Success = 200;

    private static readonly Dictionary<int, string> Messages = new()
    {
        { 200, "Thành công" },

        // 21x — Xác thực app
        { 210, "Không tìm thấy app_id hoặc app_secret" },
        { 211, "Access UrBox không đúng" },
        { 212, "Thông tin xác thực không chính xác" },
        { 213, "Xảy ra lỗi xác thực" },

        // 22x — Kho quà / sản phẩm
        { 220, "Hiện kho quà đang hết, vui lòng quay lại sau" },
        { 221, "Không tìm thấy sản phẩm mua" },
        { 222, "Không tìm thấy sản phẩm" },
        { 223, "1 trong số quà tặng bạn đặt mua đã hết hạn, bạn hãy chọn lại quà khác" },
        { 224, "Không tìm thấy quà tặng" },
        { 225, "Sản phẩm đang hết, vui lòng bỏ sản phẩm ra khỏi giỏ hàng" },
        { 226, "Quà tặng không nằm trong chương trình" },

        // 30x — Dữ liệu / điểm / chương trình
        { 304, "Email không đúng định dạng" },
        { 306, "Hệ thống khách hàng không đủ tiền" },
        { 307, "Request bị thiếu campaign_code" },
        { 308, "Mã chương trình không thuộc app_id" },
        { 309, "Hệ thống khách hàng không đủ tiền" },

        // 40x — Giao dịch / đơn hàng
        { 403, "Không tìm thấy Mã Giao Dịch (transaction_id)" },
        { 404, "Số lượng phải lớn hơn 0" },
        { 405, "Bạn vui lòng chọn quà muốn tặng trước" },
        { 406, "Mã khuyến mại không đúng" },
        { 407, "Số lượng sản phẩm không đủ" },
        { 408, "Hệ thống hiện tại không tạo được đơn hàng" },
        { 409, "Không tìm thấy dữ liệu" },

        // 60x — Địa chỉ giao hàng (quà vật lý)
        { 601, "Không tìm thấy địa chỉ" },
        { 602, "Không tìm thấy mã tỉnh thành phố" },
        { 603, "Tỉnh thành bạn nhập không tồn tại" },
        { 604, "Không tìm thấy mã quận huyện" },
        { 605, "Quận huyện bạn nhập không tồn tại" },
        { 606, "Quận huyện không thuộc thành phố bạn chọn" },
        { 607, "Không tìm thấy mã phường xã" },
        { 608, "Phường xã bạn nhập không tồn tại" },
        { 609, "Xã phường không thuộc tỉnh thành bạn chọn" },
        { 610, "Không tìm thấy số điện thoại" }
    };

    /// <summary>
    /// Lấy thông báo tiếng Việt theo mã status. Nếu không có trong bảng → trả về thông báo mặc định kèm mã.
    /// </summary>
    public static string GetMessage(int status)
    {
        return Messages.TryGetValue(status, out var msg)
            ? msg
            : $"Lỗi không xác định từ UrBox (mã {status})";
    }
}
