using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppImages;

/// <summary>
/// Cấu hình storage ảnh/video, section "Storage" trong appsettings.
/// Provider = "Local" (mặc định, hành vi hiện tại — wwwroot) hoặc "S3" (ods.vn/MinIO/S3-compatible).
/// Chỉ các subFolder nằm trong S3.SubFolders mới dùng S3; subFolder khác (salon/news/pro/...) luôn ở Local,
/// cho phép bật S3 cho hl25+hlg mà không ảnh hưởng module khác.
/// </summary>
public class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>"Local" hoặc "S3". Mặc định "Local" để không đổi hành vi khi chưa cấu hình.</summary>
    public string Provider { get; set; } = "Local";

    public S3StorageOptions S3 { get; set; } = new();
}

public class S3StorageOptions
{
    /// <summary>Endpoint S3-compatible, VD "https://s3.ods.vn".</summary>
    public string ServiceUrl { get; set; } = "";
    public string Bucket { get; set; } = "";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    /// <summary>Region bắt buộc với AWSSDK dù S3-compatible không dùng; để giá trị bất kỳ hợp lệ.</summary>
    public string Region { get; set; } = "us-east-1";
    /// <summary>Bắt buộc true cho hầu hết S3-compatible (ods.vn/MinIO) — path-style URL thay virtual-hosted-style.</summary>
    public bool ForcePathStyle { get; set; } = true;
    /// <summary>Base URL công khai để dựng URL trả FE, VD "https://s3.ods.vn/c01-hoalinh-prod-01". Nếu rỗng, tự dựng từ ServiceUrl+Bucket.</summary>
    public string? PublicBaseUrl { get; set; }
    /// <summary>
    /// Canned ACL set khi PutObject, VD "public-read". Để RỖNG nếu bucket đã có policy public-read sẵn
    /// (một số S3-compatible từ chối header ACL không hỗ trợ → upload lỗi). Mặc định rỗng = không gửi ACL.
    /// </summary>
    public string? CannedAcl { get; set; }
    /// <summary>
    /// Danh sách subFolder (hl25, hlg, ...) sẽ dùng S3 khi Provider=S3. SubFolder không có trong danh sách
    /// này luôn dùng Local, dù Provider=S3 — cho phép migrate dần theo module.
    /// </summary>
    public List<string> SubFolders { get; set; } = new();
}
