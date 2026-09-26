using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppServices.Hlg;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Genora.MultiTenancy.Hlg;

/// <summary>
/// Kiểm thử POST /api/mini-app/hlg/ranking/share-image (HlgRankingAppService.SaveShareImageAsync):
/// validate nội dung ảnh thật, kích thước, khách hàng, và mã HTTP status (Code) đúng yêu cầu.
/// </summary>
public class HlgRankingShareImageTests : IDisposable
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly ICurrentTenant _tenant = Substitute.For<ICurrentTenant>();
    private readonly ServiceCollection _services = new();
    private readonly List<ServiceProvider> _providers = new();
    private readonly List<string> _createdFiles = new();

    // Ảnh hợp lệ sinh từ ImageSharp (CRC đúng) để test decoder thật.
    private static readonly byte[] Png1x1 = MakePng();
    private static readonly byte[] Gif1x1 = MakeGif();

    private static byte[] MakePng(int w = 4, int h = 4)
    {
        using var img = new Image<Rgba32>(w, h);
        using var ms = new MemoryStream();
        img.SaveAsPng(ms);
        return ms.ToArray();
    }

    private static byte[] MakeGif()
    {
        using var img = new Image<Rgba32>(2, 2);
        using var ms = new MemoryStream();
        img.SaveAsGif(ms);
        return ms.ToArray();
    }

    public HlgRankingShareImageTests()
    {
        _tenant.Id.Returns(_tenantId);
        _tenant.IsAvailable.Returns(true);
        _services.AddSingleton<ICurrentTenant>(_tenant);
        _services.AddSingleton<Volo.Abp.Linq.IAsyncQueryableExecuter>(
            new Volo.Abp.Linq.AsyncQueryableExecuter(Array.Empty<Volo.Abp.Linq.IAsyncQueryableProvider>()));
    }

    private IRepository<T, Guid> Repo<T>(params T[] rows) where T : class, IEntity<Guid>
    {
        var repo = Substitute.For<IRepository<T, Guid>>();
        repo.GetQueryableAsync().Returns(Task.FromResult(rows.AsQueryable()));
        repo.FirstOrDefaultAsync(Arg.Any<Expression<Func<T, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(c => Task.FromResult(rows.FirstOrDefault(c.Arg<Expression<Func<T, bool>>>().Compile())));
        _services.AddSingleton(repo);
        return repo;
    }

    private HlgRankingAppService Build(params Customer[] customers)
    {
        var events = Repo<HlgRankingEvent>();
        var sessions = Repo<HlgGameSession>();
        var custRepo = Repo(customers);
        var service = new HlgRankingAppService(events, sessions, custRepo, NullLogger<HlgRankingAppService>.Instance);
        var provider = _services.BuildServiceProvider();
        _providers.Add(provider);
        service.LazyServiceProvider = new AbpLazyServiceProvider(provider);
        return service;
    }

    private static Customer Player(string phone = "0976687984")
        => new(Guid.NewGuid(), phone, "Player") { TenantId = Guid.NewGuid() };

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Rejects_Missing_Or_Blank_Phone_With_400(string? phone)
    {
        var ex = await Should.ThrowAsync<UserFriendlyException>(() => Build().SaveShareImageAsync(phone!, Png1x1));
        ex.Code.ShouldBe("400");
    }

    [Fact]
    public async Task Rejects_Missing_File_With_400()
    {
        var ex = await Should.ThrowAsync<UserFriendlyException>(() => Build(Player()).SaveShareImageAsync("0976687984", null));
        ex.Code.ShouldBe("400");
        var ex2 = await Should.ThrowAsync<UserFriendlyException>(() => Build(Player()).SaveShareImageAsync("0976687984", Array.Empty<byte>()));
        ex2.Code.ShouldBe("400");
    }

    [Fact]
    public async Task Rejects_Oversize_File_With_413()
    {
        var big = new byte[5 * 1024 * 1024 + 1];
        var ex = await Should.ThrowAsync<UserFriendlyException>(() => Build(Player()).SaveShareImageAsync("0976687984", big));
        ex.Code.ShouldBe("413");
    }

    [Fact]
    public async Task Rejects_Unknown_Customer_With_404()
    {
        // repo khách hàng rỗng → không tìm thấy.
        var ex = await Should.ThrowAsync<UserFriendlyException>(() => Build().SaveShareImageAsync("0976687984", Png1x1));
        ex.Code.ShouldBe("404");
    }

    [Fact]
    public async Task Rejects_Non_Image_Bytes_With_400()
    {
        var garbage = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var ex = await Should.ThrowAsync<UserFriendlyException>(() => Build(Player()).SaveShareImageAsync("0976687984", garbage));
        ex.Code.ShouldBe("400");
    }

    [Fact]
    public async Task Rejects_Valid_Image_But_Not_Png_Or_Jpeg_With_400()
    {
        // GIF hợp lệ nhưng không thuộc PNG/JPEG.
        var ex = await Should.ThrowAsync<UserFriendlyException>(() => Build(Player()).SaveShareImageAsync("0976687984", Gif1x1));
        ex.Code.ShouldBe("400");
    }

    [Fact]
    public async Task Accepts_Valid_Png_And_Returns_Public_Uploads_Url()
    {
        var result = await Build(Player()).SaveShareImageAsync("0976687984", Png1x1);

        result.Url.ShouldNotBeNullOrWhiteSpace();
        result.Url.ShouldContain("/uploads/hlg/ranking-share/");
        result.Url.ShouldEndWith(".png");

        // File thực sự được ghi (không HTTP context nên url là path tương đối).
        var fsPath = Path.Combine("wwwroot", result.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        File.Exists(fsPath).ShouldBeTrue();
        _createdFiles.Add(fsPath);
    }

    public void Dispose()
    {
        foreach (var f in _createdFiles)
        {
            try { if (File.Exists(f)) File.Delete(f); } catch { /* best-effort cleanup */ }
        }
        foreach (var provider in _providers) provider.Dispose();
    }
}
