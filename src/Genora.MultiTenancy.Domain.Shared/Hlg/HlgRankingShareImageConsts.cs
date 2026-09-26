namespace Genora.MultiTenancy.Hlg;

/// <summary>
/// Hằng số cho API upload ảnh chia sẻ Bảng xếp hạng HLG (POST /api/mini-app/hlg/ranking/share-image).
/// </summary>
public static class HlgRankingShareImageConsts
{
    /// <summary>Dung lượng tối đa: 5 MiB.</summary>
    public const long MaxBytes = 5L * 1024 * 1024;

    /// <summary>Giới hạn mỗi cạnh ảnh (px).</summary>
    public const int MaxSide = 8192;

    /// <summary>Diện tích tối đa (~16 megapixel).</summary>
    public const long MaxPixels = 16L * 1024 * 1024;

    /// <summary>Thư mục public dưới wwwroot lưu ảnh chia sẻ (UUID server sinh).</summary>
    public const string PublicSubPath = "uploads/hlg/ranking-share";

    /// <summary>Thời gian giữ ảnh tối thiểu (ngày) để thumbnail trong tin đã chia sẻ không mất sớm.</summary>
    public const int RetentionDays = 30;
}
