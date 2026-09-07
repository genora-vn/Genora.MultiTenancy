namespace Genora.MultiTenancy.Hl25;

/// <summary>
/// Hằng số dùng chung cho module "Dược Phẩm Hoa Linh 25 Năm" (schema DB "hl25").
/// </summary>
public static class Hl25Consts
{
    /// <summary>Giới hạn dung lượng ảnh thiệp upload: 5MB. (ManageImageService KHÔNG tự chặn size → phải tự validate.)</summary>
    public const long MaxCardImageSizeBytes = 5 * 1024 * 1024;

    /// <summary>Độ dài tối đa của lời chúc (ký tự). Delta 2026-09: counter "x/250" trên màn Tạo thiệp.</summary>
    public const int MaxWishLength = 250;

    /// <summary>Thư mục con lưu ảnh của module (dùng cho IManageImageService.UploadImageAsync).</summary>
    public const string DefaultImageSubFolder = "hl25";

    /// <summary>
    /// Trần số lượt quay tối đa mỗi người dùng.
    /// Quy tắc (đã chốt): mỗi chu kỳ "Tạo thiệp → Chia sẻ thành công" = +1 lượt, tối đa 2 chu kỳ = 2 lượt.
    /// </summary>
    public const int MaxSpinTurnsPerUser = 2;

    /// <summary>Regex số điện thoại VN: bắt đầu bằng 0 (10-11 số) hoặc 84 (11-12 số). Đồng bộ DTO + cshtml + JS + server.</summary>
    public const string PhoneRegex = @"^(0\d{9,10}|84\d{9,10})$";

    /// <summary>Độ dài tối đa cột số điện thoại.</summary>
    public const int MaxPhoneLength = 13;
}
