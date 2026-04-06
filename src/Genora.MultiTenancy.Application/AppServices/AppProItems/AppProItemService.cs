using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.AppDtos.AppProItems;
using Genora.MultiTenancy.DomainModels.AppProCategories;
using Genora.MultiTenancy.DomainModels.AppProItems;
using Genora.MultiTenancy.Features.AppProshopFeatures;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.AppProItems;

[Authorize]
public class AppProItemService :
    FeatureProtectedCrudAppService<ProItem, ProItemDto, Guid, GetProItemListInput, CreateUpdateProItemDto>,
    IAppProItemService
{
    // Feature gate bị tắt — chỉ dùng Permission để kiểm soát truy cập
    protected override string FeatureName => string.Empty;
    protected override async Task EnsureFeatureAsync() => await Task.CompletedTask;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppProItems.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppProItems.Default;

    private readonly IRepository<ProCategory, Guid> _categoryRepository;
    private readonly IConfiguration _configuration;
    private readonly IManageImageService _manageImageService;

    public AppProItemService(
        IRepository<ProItem, Guid> repository,
        IRepository<ProCategory, Guid> categoryRepository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        IConfiguration configuration,
        IManageImageService manageImageService)
        : base(repository, currentTenant, featureChecker)
    {
        GetPolicyName     = MultiTenancyPermissions.AppProItems.Default;
        GetListPolicyName = MultiTenancyPermissions.AppProItems.Default;
        CreatePolicyName  = MultiTenancyPermissions.AppProItems.Create;
        UpdatePolicyName  = MultiTenancyPermissions.AppProItems.Edit;
        DeletePolicyName  = MultiTenancyPermissions.AppProItems.Delete;
        _categoryRepository = categoryRepository;
        _configuration = configuration;
        _manageImageService = manageImageService;
    }

    [DisableValidation]
    public override async Task<PagedResultDto<ProItemDto>> GetListAsync(GetProItemListInput input)
    {
        await CheckGetListPolicyAsync();

        var itemQuery = await Repository.GetQueryableAsync();
        var categoryQuery = await _categoryRepository.GetQueryableAsync();

        var query = from item in itemQuery
                    join cat in categoryQuery on item.CategoryId equals cat.Id
                    select new { item, cat };

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var f = input.FilterText.Trim();
            query = query.Where(x => x.item.Name.Contains(f));
        }

        if (input.CategoryId.HasValue)
            query = query.Where(x => x.item.CategoryId == input.CategoryId.Value);

        if (input.IsActive.HasValue)
            query = query.Where(x => x.item.IsActive == input.IsActive.Value);

        if (input.IsAvailable.HasValue)
            query = query.Where(x => x.item.IsAvailable == input.IsAvailable.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.cat.SortOrder)
                 .ThenBy(x => x.item.SortOrder)
                 .ThenBy(x => x.item.Name)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount));

        var dtos = rows.Select(r =>
        {
            var dto = ObjectMapper.Map<ProItem, ProItemDto>(r.item);
            dto.CategoryName = r.cat.Name;
            dto.ImageUrl = ImageHelper.NormalizeThumb(_configuration, r.item.ImageUrl);
            return dto;
        }).ToList();

        return new PagedResultDto<ProItemDto>(totalCount, dtos);
    }

    public override async Task<ProItemDto> CreateAsync(CreateUpdateProItemDto input)
    {
        await CheckCreatePolicyAsync();

        if (input.IsUploadImage && input.Images != null)
        {
            var uploaded = await _manageImageService.UploadImageAsync(input.Images, CurrentTenant.Id.ToString());
            input.ImageUrl = uploaded;
        }

        var entity = ObjectMapper.Map<CreateUpdateProItemDto, ProItem>(input);
        entity.TenantId = CurrentTenant.Id;
        entity.SortOrder = input.SortOrder ?? await GetNextSortOrderAsync(input.CategoryId);

        entity = await Repository.InsertAsync(entity, autoSave: true);
        return ObjectMapper.Map<ProItem, ProItemDto>(entity);
    }

    public override async Task<ProItemDto> UpdateAsync(Guid id, CreateUpdateProItemDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await Repository.GetAsync(id);

        if (input.IsUploadImage && input.Images != null)
        {
            if (!string.IsNullOrWhiteSpace(entity.ImageUrl))
                await _manageImageService.DeleteFileAsync(entity.ImageUrl);

            var uploaded = await _manageImageService.UploadImageAsync(input.Images, CurrentTenant.Id.ToString());
            input.ImageUrl = uploaded;
        }

        ObjectMapper.Map(input, entity);
        if (input.SortOrder.HasValue) entity.SortOrder = input.SortOrder.Value;
        entity = await Repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<ProItem, ProItemDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var entity = await Repository.GetAsync(id);

        if (!string.IsNullOrWhiteSpace(entity.ImageUrl))
            await _manageImageService.DeleteFileAsync(entity.ImageUrl);

        await Repository.HardDeleteAsync(entity, autoSave: true);
    }

    public async Task<ProItemDto> SetStateAsync(Guid id, SetProItemStateDto input)
    {
        await CheckUpdatePolicyAsync();
        var entity = await Repository.GetAsync(id);

        if (input.IsActive.HasValue) entity.IsActive = input.IsActive.Value;
        if (input.IsAvailable.HasValue) entity.IsAvailable = input.IsAvailable.Value;

        entity = await Repository.UpdateAsync(entity, autoSave: true);
        return ObjectMapper.Map<ProItem, ProItemDto>(entity);
    }

    public async Task<IRemoteStreamContent> DownloadImportTemplateAsync()
    {
        await CheckGetListPolicyAsync();
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("ProItems");

        ws.Cell(1, 1).Value = "TÊN SẢN PHẨM (*)";
        ws.Cell(1, 2).Value = "GIÁ (*)";
        ws.Cell(1, 3).Value = "MÃ DANH MỤC (*)";
        ws.Cell(1, 4).Value = "MÔ TẢ";
        ws.Cell(1, 5).Value = "THỨ TỰ HIỂN THỊ";
        ws.Cell(1, 6).Value = "ĐƯỢC SỬ DỤNG";
        ws.Cell(1, 7).Value = "CÒN HÀNG";

        ws.Row(1).Style.Font.Bold = true;
        ws.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;
        ws.Row(2).Style.Font.FontColor = XLColor.DarkGray;

        ws.Cell(2, 1).Value = "VD: Áo polo golf";
        ws.Cell(2, 2).Value = "VD: 350000";
        ws.Cell(2, 3).Value = "VD: PRO001";
        ws.Cell(2, 4).Value = "VD: Áo vải cao cấp";
        ws.Cell(2, 5).Value = "VD: 1";
        ws.Cell(2, 6).Value = "TRUE / FALSE";
        ws.Cell(2, 7).Value = "TRUE / FALSE";

        ws.SheetView.FreezeRows(2);
        ws.Columns().AdjustToContents();

        return StreamToRemoteContent(workbook, $"Template_Import_ProItems_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    public async Task<IRemoteStreamContent> ExportExcelAsync(GetProItemListInput input)
    {
        await CheckGetListPolicyAsync();

        var itemQuery = await Repository.GetQueryableAsync();
        var categoryQuery = await _categoryRepository.GetQueryableAsync();

        var query = from item in itemQuery
                    join cat in categoryQuery on item.CategoryId equals cat.Id
                    select new { item, cat };

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var f = input.FilterText.Trim();
            query = query.Where(x => x.item.Name.Contains(f));
        }

        if (input.CategoryId.HasValue)
            query = query.Where(x => x.item.CategoryId == input.CategoryId.Value);

        var rows = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.cat.SortOrder).ThenBy(x => x.item.SortOrder).ThenBy(x => x.item.Name));

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("ProItems");

        ws.Cell(1, 1).Value = "TÊN SẢN PHẨM";
        ws.Cell(1, 2).Value = "GIÁ";
        ws.Cell(1, 3).Value = "DANH MỤC";
        ws.Cell(1, 4).Value = "CÒN HÀNG";
        ws.Cell(1, 5).Value = "ĐƯỢC SỬ DỤNG";

        ws.Row(1).Style.Font.Bold = true;
        ws.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

        var r = 2;
        foreach (var row in rows)
        {
            ws.Cell(r, 1).Value = row.item.Name;
            ws.Cell(r, 2).Value = (double)row.item.Price;
            ws.Cell(r, 3).Value = row.cat.Name;
            ws.Cell(r, 4).Value = row.item.IsAvailable;
            ws.Cell(r, 5).Value = row.item.IsActive;
            r++;
        }

        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents();

        return StreamToRemoteContent(workbook, $"Export_ProItems_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    [DisableValidation]
    public async Task<int> ImportExcelAsync(ImportProItemExcelInput input)
    {
        await CheckCreatePolicyAsync();
        await CheckUpdatePolicyAsync();

        if (input.File == null)
            throw new UserFriendlyException("Vui lòng chọn file Excel.");

        using var workbook = new XLWorkbook(input.File.GetStream());
        var ws = workbook.Worksheets.First();

        var categories = await _categoryRepository.GetListAsync();
        var categoryByCode = categories
            .Where(c => !string.IsNullOrWhiteSpace(c.Code))
            .GroupBy(c => c.Code!)
            .ToDictionary(g => g.Key, g => g.First());

        var success = 0;

        foreach (var row in ws.RowsUsed().Skip(2))
        {
            var name = row.Cell(1).GetString().Trim();
            var priceRaw = row.Cell(2).GetString().Trim();
            var catCode = row.Cell(3).GetString().Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(priceRaw))
                continue;

            if (!decimal.TryParse(priceRaw, out var price)) continue;
            if (!categoryByCode.TryGetValue(catCode, out var category)) continue;

            var description = row.Cell(4).GetString().Trim();
            var sortRaw = row.Cell(5).GetString().Trim();
            var activeRaw = row.Cell(6).GetString().Trim().ToLower();
            var availableRaw = row.Cell(7).GetString().Trim().ToLower();

            bool isActive = activeRaw != "false" && activeRaw != "0";
            bool isAvailable = availableRaw != "false" && availableRaw != "0";
            int? sortOrder = int.TryParse(sortRaw, out var s) ? s : (int?)null;

            var entity = new ProItem(GuidGenerator.Create(), category.Id, name, price, CurrentTenant.Id)
            {
                Description = string.IsNullOrWhiteSpace(description) ? null : description,
                SortOrder = sortOrder ?? await GetNextSortOrderAsync(category.Id),
                IsActive = isActive,
                IsAvailable = isAvailable
            };

            await Repository.InsertAsync(entity, autoSave: false);
            success++;
        }

        await CurrentUnitOfWork.SaveChangesAsync();
        return success;
    }

    private async Task<int> GetNextSortOrderAsync(Guid categoryId)
    {
        var query = await Repository.GetQueryableAsync();
        var max = await AsyncExecuter.MaxAsync(
            query.Where(x => x.CategoryId == categoryId).Select(x => (int?)x.SortOrder));
        return (max ?? -1) + 1;
    }

    private static IRemoteStreamContent StreamToRemoteContent(XLWorkbook workbook, string fileName)
    {
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return new RemoteStreamContent(stream, fileName,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}
