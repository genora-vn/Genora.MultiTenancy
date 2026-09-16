using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.DomainModels.AppHl25;
using Genora.MultiTenancy.Features.AppHl25Features;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using ClosedXML.Excel;

namespace Genora.MultiTenancy.AppServices.Hl25;

/// <summary>
/// AppService lịch sử tạo ảnh thiệp (read-only). Join Participant + Campaign để hiển thị tên.
/// </summary>
[Authorize]
public class Hl25FrameCreationAppService : ApplicationService, IHl25FrameCreationAppService
{
    private readonly IRepository<Hl25FrameCreation, Guid> _repository;
    private readonly IRepository<Hl25Participant, Guid> _participantRepository;
    private readonly IRepository<Hl25FrameCampaign, Guid> _campaignRepository;
    private readonly IFeatureChecker _featureChecker;
    private readonly IHostEnvironment _hostEnvironment;

    public Hl25FrameCreationAppService(
        IRepository<Hl25FrameCreation, Guid> repository,
        IRepository<Hl25Participant, Guid> participantRepository,
        IRepository<Hl25FrameCampaign, Guid> campaignRepository,
        IFeatureChecker featureChecker,
        IHostEnvironment hostEnvironment)
    {
        _repository = repository;
        _participantRepository = participantRepository;
        _campaignRepository = campaignRepository;
        _featureChecker = featureChecker;
        _hostEnvironment = hostEnvironment;
        LocalizationResource = typeof(MultiTenancyResource);
    }

    public async Task<PagedResultDto<Hl25FrameCreationDto>> GetListAsync(GetHl25FrameCreationListInput input)
    {
        await CheckViewPolicyAsync();

        var creationQueryable = await _repository.GetQueryableAsync();
        var participantQueryable = await _participantRepository.GetQueryableAsync();
        var campaignQueryable = await _campaignRepository.GetQueryableAsync();

        var query = from c in creationQueryable
                    join p in participantQueryable on c.ParticipantId equals p.Id into pg
                    from p in pg.DefaultIfEmpty()
                    join cp in campaignQueryable on c.CampaignId equals cp.Id into cpg
                    from cp in cpg.DefaultIfEmpty()
                    select new { c, p, cp };

        if (input.CampaignId.HasValue)
            query = query.Where(x => x.c.CampaignId == input.CampaignId.Value);

        if (input.ParticipantId.HasValue)
            query = query.Where(x => x.c.ParticipantId == input.ParticipantId.Value);

        if (input.SharePlatform.HasValue)
            query = query.Where(x => x.c.SharePlatform == input.SharePlatform.Value);

        if (!string.IsNullOrWhiteSpace(input.FilterText))
        {
            var f = input.FilterText.Trim();
            query = query.Where(x => x.p != null &&
                ((x.p.FullName != null && x.p.FullName.Contains(f)) ||
                 (x.p.PhoneNumber != null && x.p.PhoneNumber.Contains(f))));
        }

        if (input.CreatedFrom.HasValue)
            query = query.Where(x => x.c.CreatedTime >= input.CreatedFrom.Value);

        if (input.CreatedTo.HasValue)
            query = query.Where(x => x.c.CreatedTime <= input.CreatedTo.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? "c.CreatedTime DESC"
            : "c." + input.Sorting;

        var rows = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        var items = rows.Select(x =>
        {
            var dto = ObjectMapper.Map<Hl25FrameCreation, Hl25FrameCreationDto>(x.c);
            dto.ParticipantName = x.p?.FullName;
            dto.ParticipantPhone = x.p?.PhoneNumber;
            dto.CampaignName = x.cp?.Name;
            return dto;
        }).ToList();

        return new PagedResultDto<Hl25FrameCreationDto>(totalCount, items);
    }

    private async Task EnsureFeatureAsync()
    {
        if (!CurrentTenant.IsAvailable) return;
        if (!await _featureChecker.IsEnabledAsync(AppHl25Features.Management))
            throw new AbpAuthorizationException($"Feature '{AppHl25Features.Management}' is disabled for this tenant.");
    }

    private async Task CheckViewPolicyAsync()
    {
        var policy = CurrentTenant.IsAvailable
            ? MultiTenancyPermissions.AppHl25Frames.Default
            : MultiTenancyPermissions.HostAppHl25Frames.Default;
        await AuthorizationService.CheckAsync(policy);
        await EnsureFeatureAsync();
    }

    // ===== Xuất Excel lịch sử tạo ảnh =====
    public async Task<IRemoteStreamContent> ExportExcelAsync(GetHl25FrameCreationListInput input)
    {
        await CheckViewPolicyAsync();

        var creationQueryable = await _repository.GetQueryableAsync();
        var participantQueryable = await _participantRepository.GetQueryableAsync();
        var campaignQueryable = await _campaignRepository.GetQueryableAsync();

        var query = from c in creationQueryable
                    join p in participantQueryable on c.ParticipantId equals p.Id into pg
                    from p in pg.DefaultIfEmpty()
                    join cp in campaignQueryable on c.CampaignId equals cp.Id into cpg
                    from cp in cpg.DefaultIfEmpty()
                    select new { c, p, cp };

        if (input.CampaignId.HasValue)
            query = query.Where(x => x.c.CampaignId == input.CampaignId.Value);

        if (input.ParticipantId.HasValue)
            query = query.Where(x => x.c.ParticipantId == input.ParticipantId.Value);

        if (input.SharePlatform.HasValue)
            query = query.Where(x => x.c.SharePlatform == input.SharePlatform.Value);

        if (!string.IsNullOrWhiteSpace(input.FilterText))
        {
            var f = input.FilterText.Trim();
            query = query.Where(x => x.p != null &&
                ((x.p.FullName != null && x.p.FullName.Contains(f)) ||
                 (x.p.PhoneNumber != null && x.p.PhoneNumber.Contains(f))));
        }

        if (input.CreatedFrom.HasValue)
            query = query.Where(x => x.c.CreatedTime >= input.CreatedFrom.Value);

        if (input.CreatedTo.HasValue)
            query = query.Where(x => x.c.CreatedTime <= input.CreatedTo.Value);

        var rows = await AsyncExecuter.ToListAsync(query.OrderBy("c.CreatedTime DESC"));

        var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("FrameCreations");

        ws.Cell(1, 1).Value = "Thời gian tạo";
        ws.Cell(1, 2).Value = "Người tạo";
        ws.Cell(1, 3).Value = "SĐT";
        ws.Cell(1, 4).Value = "Chiến dịch";
        ws.Cell(1, 5).Value = "Lời chúc";
        ws.Cell(1, 6).Value = "Ảnh thiệp (URL)";
        ws.Cell(1, 7).Value = "Chia sẻ";
        ws.Cell(1, 8).Value = "Thời gian chia sẻ";

        var headerRange = ws.Range(1, 1, 1, 8);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

        for (int i = 0; i < rows.Count; i++)
        {
            var row = i + 2;
            var x = rows[i];
            ws.Cell(row, 1).Value = x.c.CreatedTime.ToString("dd/MM/yyyy HH:mm");
            ws.Cell(row, 2).Value = x.p?.FullName ?? "";
            ws.Cell(row, 3).Value = x.p?.PhoneNumber ?? "";
            ws.Cell(row, 4).Value = x.cp?.Name ?? "";
            ws.Cell(row, 5).Value = x.c.WishMessage ?? "";
            ws.Cell(row, 6).Value = x.c.ResultImageUrl;
            ws.Cell(row, 7).Value = x.c.SharePlatform.ToString();
            ws.Cell(row, 8).Value = x.c.ShareTime?.ToString("dd/MM/yyyy HH:mm") ?? "";
        }

        ws.Columns().AdjustToContents();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return new RemoteStreamContent(stream, $"Hl25FrameCreations_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    // ===== ZIP toàn bộ ảnh thiệp =====
    public async Task<IRemoteStreamContent> DownloadAllImagesAsync()
    {
        await CheckViewPolicyAsync();

        var creationQueryable = await _repository.GetQueryableAsync();
        var creations = await AsyncExecuter.ToListAsync(creationQueryable);

        var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var creation in creations)
            {
                var imageUrl = creation.ResultImageUrl;
                if (string.IsNullOrWhiteSpace(imageUrl))
                    continue;

                // Parse URL: có thể là full URL hoặc relative path.
                string relativePath;
                if (imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    var uri = new Uri(imageUrl);
                    relativePath = uri.AbsolutePath;
                }
                else
                {
                    relativePath = imageUrl;
                }

                var wwwrootPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", relativePath.TrimStart('/'));
                if (!File.Exists(wwwrootPath))
                    continue;

                var fileName = Path.GetFileName(wwwrootPath);
                var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                using var entryStream = entry.Open();
                using var fileStream = File.OpenRead(wwwrootPath);
                await fileStream.CopyToAsync(entryStream);
            }
        }

        memoryStream.Position = 0;
        return new RemoteStreamContent(memoryStream, $"Hl25FrameImages_{DateTime.Now:yyyyMMdd_HHmmss}.zip",
            "application/zip");
    }
}
