using Amazon.S3;
using Amazon.S3.Model;
using Genora.MultiTenancy.AppDtos.AppImages;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppServices.AppImages;

/// <summary>
/// Lưu trữ S3/compatible (ods.vn). Object key = path tương đối không dấu "/" đầu (VD "uploads/hl25/{tenantId}/{guid}.jpg"),
/// khớp đúng cấu trúc thư mục hiện có trên window server để tái sử dụng được URL/structure đã có trên store.
/// </summary>
public class S3ImageStorageProvider : IImageStorageProvider
{
    private readonly IAmazonS3 _client;
    private readonly S3StorageOptions _options;

    public S3ImageStorageProvider(IAmazonS3 client, IOptions<StorageOptions> options)
    {
        _client = client;
        _options = options.Value.S3;
    }

    public async Task<string> SaveAsync(string key, byte[] content, string contentType)
    {
        var normalizedKey = key.TrimStart('/');
        using var stream = new MemoryStream(content);
        var request = new PutObjectRequest
        {
            BucketName = _options.Bucket,
            Key = normalizedKey,
            InputStream = stream,
            ContentType = contentType,
            AutoCloseStream = true,
            DisablePayloadSigning = true // một số S3-compatible (ods.vn) không hỗ trợ chunked/streaming signature
        };
        if (!string.IsNullOrWhiteSpace(_options.CannedAcl))
            request.CannedACL = new S3CannedACL(_options.CannedAcl);

        await _client.PutObjectAsync(request);
        return BuildPublicUrl(normalizedKey);
    }

    public async Task<string?> CopyAsync(string sourceUrlOrKey, string destKey)
    {
        var sourceKey = ToKey(sourceUrlOrKey);
        var destNormalized = destKey.TrimStart('/');
        try
        {
            // MetadataDirective = REPLACE: ghi lại metadata cho object copy thay vì sao chép nguyên từ nguồn.
            // Tránh kế thừa "Content-Encoding: aws-chunked" nếu object nguồn cũ còn dính (upload bằng SDK v4
            // trước khi tắt flexible-checksum). Set ContentType đúng theo đuôi file, KHÔNG set ContentEncoding.
            await _client.CopyObjectAsync(new CopyObjectRequest
            {
                SourceBucket = _options.Bucket,
                SourceKey = sourceKey,
                DestinationBucket = _options.Bucket,
                DestinationKey = destNormalized,
                MetadataDirective = S3MetadataDirective.REPLACE,
                ContentType = GuessContentType(destNormalized),
                CannedACL = !string.IsNullOrWhiteSpace(_options.CannedAcl) ? new S3CannedACL(_options.CannedAcl) : null
            });
            return BuildPublicUrl(destNormalized);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<byte[]?> ReadAsync(string urlOrKey)
    {
        var key = ToKey(urlOrKey);
        try
        {
            using var response = await _client.GetObjectAsync(_options.Bucket, key);
            using var ms = new MemoryStream();
            await response.ResponseStream.CopyToAsync(ms);
            return ms.ToArray();
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task DeleteAsync(string urlOrKey)
    {
        var key = ToKey(urlOrKey);
        try
        {
            await _client.DeleteObjectAsync(_options.Bucket, key);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // giữ hành vi hiện tại: không throw nếu object không tồn tại
        }
    }

    private string BuildPublicUrl(string key)
    {
        var baseUrl = !string.IsNullOrWhiteSpace(_options.PublicBaseUrl)
            ? _options.PublicBaseUrl!.TrimEnd('/')
            : $"{_options.ServiceUrl.TrimEnd('/')}/{_options.Bucket}";
        return $"{baseUrl}/{key}";
    }

    /// <summary>Chấp nhận URL S3 tuyệt đối (bóc key sau bucket) hoặc key/path tương đối thuần.</summary>
    private string ToKey(string urlOrKey)
    {
        if (urlOrKey.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            urlOrKey.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            var path = new Uri(urlOrKey).AbsolutePath.TrimStart('/'); // "{bucket}/{key...}" (path-style) hoặc "{key...}" (virtual-hosted)
            var bucketPrefix = _options.Bucket + "/";
            return path.StartsWith(bucketPrefix, StringComparison.OrdinalIgnoreCase)
                ? path[bucketPrefix.Length..]
                : path;
        }
        return urlOrKey.TrimStart('/');
    }

    /// <summary>Suy content-type từ đuôi file của key (dùng khi copy với MetadataDirective=REPLACE).</summary>
    private static string GuessContentType(string key)
    {
        var ext = Path.GetExtension(key)?.ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".mp4" => "video/mp4",
            _ => "application/octet-stream"
        };
    }
}
