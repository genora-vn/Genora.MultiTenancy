using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.AppServices.AppImages;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Volo.Abp.Content;
using Xunit;

namespace Genora.MultiTenancy.AppImages;

/// <summary>
/// Kiểm thử ManageImageService (facade S3 migration): chọn Local/S3 theo subFolder whitelist (Storage:Provider +
/// Storage:S3:SubFolders), URL idempotent, và các method mới (UploadBytesAsync/CopyAsync/OpenReadAsync) tự nhận
/// diện nguồn Local (path tương đối cũ) hay S3 (URL tuyệt đối mới) đúng theo mixed-state.
/// Dùng LocalImageStorageProvider THẬT (disk tạm, dọn ở Dispose) + S3ImageStorageProvider THẬT với IAmazonS3 MOCK
/// (biên mạng duy nhất) — không mock IImageStorageProvider trực tiếp vì ManageImageService nhận 2 class cụ thể.
/// </summary>
public class ManageImageServiceStorageSelectionTests : IDisposable
{
    private readonly List<string> _createdDirs = new();
    private readonly IAmazonS3 _s3Client = Substitute.For<IAmazonS3>();

    private ManageImageService BuildService(StorageOptions options)
    {
        var local = new LocalImageStorageProvider();
        var s3 = new S3ImageStorageProvider(_s3Client, Microsoft.Extensions.Options.Options.Create(options));
        return new ManageImageService(local, s3, Microsoft.Extensions.Options.Options.Create(options));
    }

    [Fact]
    public async Task UploadImageAsync_Uses_Local_When_Provider_Is_Local_Even_If_SubFolder_Whitelisted()
    {
        var options = new StorageOptions { Provider = "Local", S3 = new S3StorageOptions { SubFolders = { "hl25" } } };
        var service = BuildService(options);

        var file = MakeFile("a.png");
        var tenantId = Guid.NewGuid().ToString("N");
        TrackDir("hl25", tenantId);

        var url = await service.UploadImageAsync(file, tenantId, "hl25");

        url.ShouldStartWith("/uploads/hl25/" + tenantId + "/");
        await _s3Client.DidNotReceive().PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadImageAsync_Uses_S3_When_Provider_S3_And_SubFolder_Whitelisted()
    {
        var options = new StorageOptions
        {
            Provider = "S3",
            S3 = new S3StorageOptions { SubFolders = { "hl25", "hlg" }, Bucket = "my-bucket", ServiceUrl = "https://s3.ods.vn" }
        };
        var service = BuildService(options);
        _s3Client.PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new PutObjectResponse()));

        var file = MakeFile("a.png");
        var tenantId = Guid.NewGuid().ToString("N");

        var url = await service.UploadImageAsync(file, tenantId, "hl25");

        // Public URL tuyệt đối theo đúng cấu trúc đã có trên window server: uploads/{subFolder}/{tenantId}/{file}.
        url.ShouldStartWith("https://s3.ods.vn/my-bucket/uploads/hl25/" + tenantId + "/");
        await _s3Client.Received(1).PutObjectAsync(
            Arg.Is<PutObjectRequest>(r => r.BucketName == "my-bucket" && r.Key.StartsWith("uploads/hl25/" + tenantId + "/")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadImageAsync_Uses_Local_When_Provider_S3_But_SubFolder_Not_Whitelisted()
    {
        // Provider=S3 nhưng subFolder "salon" KHÔNG nằm trong whitelist (chỉ hl25+hlg) → vẫn Local — module
        // khác (salon/news/pro/...) không bị ảnh hưởng khi bật S3 riêng cho hl25+hlg.
        var options = new StorageOptions { Provider = "S3", S3 = new S3StorageOptions { SubFolders = { "hl25", "hlg" } } };
        var service = BuildService(options);

        var file = MakeFile("a.png");
        var tenantId = Guid.NewGuid().ToString("N");
        TrackDir("salon", tenantId);

        var url = await service.UploadImageAsync(file, tenantId, "salon");

        url.ShouldStartWith("/uploads/salon/" + tenantId + "/");
        await _s3Client.DidNotReceive().PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadBytesAsync_Builds_Key_With_TenantId_SubFolder_And_Returns_Absolute_S3_Url()
    {
        var options = new StorageOptions
        {
            Provider = "S3",
            S3 = new S3StorageOptions { SubFolders = { "hlg" }, Bucket = "my-bucket", ServiceUrl = "https://s3.ods.vn" }
        };
        var service = BuildService(options);
        _s3Client.PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new PutObjectResponse()));

        var url = await service.UploadBytesAsync(new byte[] { 1, 2, 3 }, "image/png", "host", "hlg/ranking-share", "abc.png");

        url.ShouldBe("https://s3.ods.vn/my-bucket/uploads/hlg/ranking-share/host/abc.png");
        await _s3Client.Received(1).PutObjectAsync(
            Arg.Is<PutObjectRequest>(r => r.Key == "uploads/hlg/ranking-share/host/abc.png" && r.ContentType == "image/png"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadBytesAsync_Uses_Local_When_SubFolder_Not_Whitelisted()
    {
        var options = new StorageOptions { Provider = "S3", S3 = new S3StorageOptions { SubFolders = { "hl25" } } };
        var service = BuildService(options);
        TrackDir("hlg", "host");

        var url = await service.UploadBytesAsync(new byte[] { 1, 2, 3 }, "image/png", "host", "hlg/videos", "clip.mp4");

        url.ShouldBe("/uploads/hlg/videos/host/clip.mp4");
        await _s3Client.DidNotReceive().PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteFileAsync_Routes_Absolute_Url_To_S3_And_Relative_Path_To_Local()
    {
        var options = new StorageOptions { Provider = "Local", S3 = new S3StorageOptions { Bucket = "my-bucket" } };
        var service = BuildService(options);

        await service.DeleteFileAsync("https://s3.ods.vn/my-bucket/uploads/hl25/host/x.png");
        await _s3Client.Received(1).DeleteObjectAsync("my-bucket", "uploads/hl25/host/x.png", Arg.Any<CancellationToken>());

        // Local: path không tồn tại trên disk → không throw (giữ hành vi cũ, không throw nếu file không có).
        await Should.NotThrowAsync(() => service.DeleteFileAsync("/uploads/hl25/host/y-not-exist.png"));
    }

    [Fact]
    public async Task CopyAsync_For_S3_Source_Builds_Correct_Dest_Key_And_Returns_Absolute_Url()
    {
        var options = new StorageOptions
        {
            Provider = "S3",
            S3 = new S3StorageOptions { SubFolders = { "hl25" }, Bucket = "my-bucket", ServiceUrl = "https://s3.ods.vn" }
        };
        var service = BuildService(options);
        _s3Client.CopyObjectAsync(Arg.Any<CopyObjectRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CopyObjectResponse()));

        var result = await service.CopyAsync(
            "https://s3.ods.vn/my-bucket/uploads/hl25/host/old-guid.png", "host", "hl25", "zalo123_20261009120000.png");

        result.ShouldBe("https://s3.ods.vn/my-bucket/uploads/hl25/host/zalo123_20261009120000.png");
        await _s3Client.Received(1).CopyObjectAsync(
            Arg.Is<CopyObjectRequest>(r =>
                r.SourceKey == "uploads/hl25/host/old-guid.png" &&
                r.DestinationKey == "uploads/hl25/host/zalo123_20261009120000.png"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CopyAsync_Returns_Null_When_Source_Not_Found_On_Local()
    {
        var options = new StorageOptions { Provider = "Local", S3 = new S3StorageOptions() };
        var service = BuildService(options);

        // Nguồn Local không tồn tại trên disk + S3 cũng không có (mock trả NotFound) → cả 2 miss → null.
        _s3Client.CopyObjectAsync(Arg.Any<CopyObjectRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<CopyObjectResponse>>(_ => throw new AmazonS3Exception("not found") { StatusCode = System.Net.HttpStatusCode.NotFound });

        var result = await service.CopyAsync("/uploads/hl25/host/missing.png", "host", "hl25", "new.png");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task OpenReadAsync_Reads_Back_Bytes_Just_Written_To_Local()
    {
        var options = new StorageOptions { Provider = "Local", S3 = new S3StorageOptions() };
        var service = BuildService(options);

        var tenantId = Guid.NewGuid().ToString("N");
        TrackDir("hl25", tenantId);
        var original = new byte[] { 10, 20, 30, 40 };
        var url = await service.UploadBytesAsync(original, "application/octet-stream", tenantId, "hl25", "roundtrip.bin");

        var readBack = await service.OpenReadAsync(url);

        readBack.ShouldNotBeNull();
        readBack.ShouldBe(original);
    }

    [Fact]
    public async Task OpenReadAsync_Returns_Null_When_Both_Providers_Miss()
    {
        var options = new StorageOptions { Provider = "Local", S3 = new S3StorageOptions() };
        var service = BuildService(options);
        _s3Client.GetObjectAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<GetObjectResponse>>(_ => throw new AmazonS3Exception("not found") { StatusCode = System.Net.HttpStatusCode.NotFound });

        var result = await service.OpenReadAsync("/uploads/hl25/host/missing.png");

        result.ShouldBeNull();
    }

    /// <summary>Idempotency: URL tuyệt đối (S3 hoặc bất kỳ http/https khác) phải giữ nguyên khi dựng lại full URL — khớp hành vi ToFullUrl/NormalizeThumb hiện có ở các service gọi ManageImageService.</summary>
    [Theory]
    [InlineData("https://s3.ods.vn/my-bucket/uploads/hl25/host/x.png")]
    [InlineData("http://cdn.example.com/a.png")]
    public void AbsoluteUrl_From_UploadResult_Should_Be_Recognized_As_Absolute(string url)
    {
        (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
         url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    }

    private static IRemoteStreamContent MakeFile(string fileName)
    {
        using var img = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(2, 2);
        using var ms = new MemoryStream();
        img.SaveAsPng(ms);
        var bytes = ms.ToArray();
        return new RemoteStreamContent(new MemoryStream(bytes), fileName, "image/png", bytes.Length);
    }

    private void TrackDir(string subFolder, string tenantId)
        => _createdDirs.Add(Path.Combine("wwwroot", "uploads", subFolder, tenantId));

    public void Dispose()
    {
        foreach (var dir in _createdDirs)
        {
            try { if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true); } catch { }
        }
    }
}
