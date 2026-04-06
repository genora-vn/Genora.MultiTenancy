using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.AppProCategories;
using Genora.MultiTenancy.DomainModels.AppProCategories;
using Genora.MultiTenancy.DomainModels.AppProItems;
using Genora.MultiTenancy.Features.AppProshopFeatures;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
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

namespace Genora.MultiTenancy.AppServices.AppProCategories;

[Authorize]
public class AppProCategoryService :
    FeatureProtectedCrudAppService<ProCategory, ProCategoryDto, Guid, GetProCategoryListInput, CreateUpdateProCategoryDto>,
    IAppProCategoryService
{
    // Feature gate bị tắt — chỉ dùng Permission để kiểm soát truy cập
    protected override string FeatureName => string.Empty;
    protected override async Task EnsureFeatureAsync() => await Task.CompletedTask;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppProCategories.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppProCategories.Default;

    private readonly IRepository<ProItem, Guid> _itemRepository;

    public AppProCategoryService(
        IRepository<ProCategory, Guid> repository,
        IRepository<ProItem, Guid> itemRepository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(repository, currentTenant, featureChecker)
    {
        GetPolicyName     = MultiTenancyPermissions.AppProCategories.Default;
        GetListPolicyName = MultiTenancyPermissions.AppProCategories.Default;
        CreatePolicyName  = MultiTenancyPermissions.AppProCategories.Create;
        UpdatePolicyName  = MultiTenancyPermissions.AppProCategories.Edit;
        DeletePolicyName  = MultiTenancyPermissions.AppProCategories.Delete;
        _itemRepository   = itemRepository;
    }

    [Volo.Abp.Validation.DisableValidation]
    public override async Task<PagedResultDto<ProCategoryDto>> GetListAsync(GetProCategoryListInput input)
    {
        await CheckGetListPolicyAsync();

        var query = await Repository.GetQueryableAsync();

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var f = input.FilterText.Trim();
            query = query.Where(x => x.Name.Contains(f) || (x.Code != null && x.Code.Contains(f)));
        }

        if (input.IsActive.HasValue)
            query = query.Where(x => x.IsActive == input.IsActive.Value);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? $"{nameof(ProCategory.SortOrder)} asc, {nameof(ProCategory.Name)} asc"
            : input.Sorting;

        query = query.OrderBy(sorting);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<ProCategoryDto>(totalCount,
            ObjectMapper.Map<List<ProCategory>, List<ProCategoryDto>>(items));
    }

    public override async Task<ProCategoryDto> CreateAsync(CreateUpdateProCategoryDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        var entity = ObjectMapper.Map<CreateUpdateProCategoryDto, ProCategory>(input);
        entity.TenantId = CurrentTenant.Id;
        entity.SortOrder = input.SortOrder ?? await GetNextSortOrderAsync();
        entity.IsActive = input.IsActive;

        entity = await Repository.InsertAsync(entity, autoSave: true);
        return ObjectMapper.Map<ProCategory, ProCategoryDto>(entity);
    }

    public override async Task<ProCategoryDto> UpdateAsync(Guid id, CreateUpdateProCategoryDto input)
    {
        await CheckUpdatePolicyAsync();
        await ValidateAsync(input, id);

        var entity = await Repository.GetAsync(id);
        ObjectMapper.Map(input, entity);

        if (input.SortOrder.HasValue)
            entity.SortOrder = input.SortOrder.Value;

        entity = await Repository.UpdateAsync(entity, autoSave: true);
        return ObjectMapper.Map<ProCategory, ProCategoryDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var hasItems = await _itemRepository.AnyAsync(x => x.CategoryId == id);
        if (hasItems)
            throw new UserFriendlyException("Không thể xóa danh mục vì vẫn còn sản phẩm thuộc danh mục này.");

        var entity = await Repository.GetAsync(id);
        await Repository.HardDeleteAsync(entity, autoSave: true);
    }

    public async Task<ProCategoryDto> SetActiveAsync(Guid id, SetProCategoryActiveDto input)
    {
        await CheckUpdatePolicyAsync();
        var entity = await Repository.GetAsync(id);
        entity.IsActive = input.IsActive;
        entity = await Repository.UpdateAsync(entity, autoSave: true);
        return ObjectMapper.Map<ProCategory, ProCategoryDto>(entity);
    }

    public async Task<IRemoteStreamContent> DownloadImportTemplateAsync()
    {
        await CheckGetListPolicyAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("ProCategories");

        ws.Cell(1, 1).Value = "MÃ DANH MỤC";
        ws.Cell(1, 2).Value = "TÊN DANH MỤC (*)";
        ws.Cell(1, 3).Value = "THỨ TỰ HIỂN THỊ";
        ws.Cell(1, 4).Value = "ĐƯỢC SỬ DỤNG";

        var header = ws.Range(1, 1, 1, 4);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        ws.Cell(2, 1).Value = "VD: PRO001";
        ws.Cell(2, 2).Value = "VD: Áo golf";
        ws.Cell(2, 3).Value = "VD: 1";
        ws.Cell(2, 4).Value = "TRUE / FALSE";
        ws.Row(2).Style.Font.FontColor = XLColor.DarkGray;
        ws.SheetView.FreezeRows(2);
        ws.Columns().AdjustToContents();

        return StreamToRemoteContent(workbook, $"Template_Import_ProCategories_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    public async Task<IRemoteStreamContent> ExportExcelAsync(GetProCategoryListInput input)
    {
        await CheckGetListPolicyAsync();

        var query = await Repository.GetQueryableAsync();

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var f = input.FilterText.Trim();
            query = query.Where(x => x.Name.Contains(f) || (x.Code != null && x.Code.Contains(f)));
        }

        if (input.IsActive.HasValue)
            query = query.Where(x => x.IsActive == input.IsActive.Value);

        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name));

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("ProCategories");

        ws.Cell(1, 1).Value = "MÃ DANH MỤC";
        ws.Cell(1, 2).Value = "TÊN DANH MỤC";
        ws.Cell(1, 3).Value = "THỨ TỰ HIỂN THỊ";
        ws.Cell(1, 4).Value = "ĐƯỢC SỬ DỤNG";

        var header = ws.Range(1, 1, 1, 4);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;
        header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        var row = 2;
        foreach (var item in items)
        {
            ws.Cell(row, 1).Value = item.Code;
            ws.Cell(row, 2).Value = item.Name;
            ws.Cell(row, 3).Value = item.SortOrder;
            ws.Cell(row, 4).Value = item.IsActive;
            row++;
        }

        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents();

        return StreamToRemoteContent(workbook, $"Export_ProCategories_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    [Volo.Abp.Validation.DisableValidation]
    public async Task<int> ImportExcelAsync(ImportProCategoryExcelInput input)
    {
        await CheckCreatePolicyAsync();
        await CheckUpdatePolicyAsync();

        if (input.File == null)
            throw new UserFriendlyException("Vui lòng chọn file Excel.");

        using var workbook = new XLWorkbook(input.File.GetStream());
        var ws = workbook.Worksheets.First();
        var success = 0;

        foreach (var row in ws.RowsUsed().Skip(2))
        {
            var code = row.Cell(1).GetString().Trim();
            var name = row.Cell(2).GetString().Trim();
            var sortRaw = row.Cell(3).GetString().Trim();
            var activeRaw = row.Cell(4).GetString().Trim().ToLower();

            if (string.IsNullOrWhiteSpace(name)) continue;

            bool isActive = activeRaw != "false" && activeRaw != "0";
            int? sortOrder = int.TryParse(sortRaw, out var s) ? s : (int?)null;

            var existed = !string.IsNullOrWhiteSpace(code)
                ? await Repository.FirstOrDefaultAsync(x => x.TenantId == CurrentTenant.Id && x.Code == code)
                : null;

            if (existed != null)
            {
                existed.Name = name;
                existed.IsActive = isActive;
                if (sortOrder.HasValue) existed.SortOrder = sortOrder.Value;
                await Repository.UpdateAsync(existed, autoSave: false);
            }
            else
            {
                var entity = new ProCategory(GuidGenerator.Create(), name, CurrentTenant.Id)
                {
                    Code = string.IsNullOrWhiteSpace(code) ? null : code,
                    SortOrder = sortOrder ?? await GetNextSortOrderAsync(),
                    IsActive = isActive
                };
                await Repository.InsertAsync(entity, autoSave: false);
            }
            success++;
        }

        await CurrentUnitOfWork.SaveChangesAsync();
        return success;
    }

    private async Task ValidateAsync(CreateUpdateProCategoryDto input, Guid? editingId = null)
    {
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            var code = input.Code.Trim();
            var exists = await Repository.AnyAsync(x =>
                x.TenantId == CurrentTenant.Id &&
                x.Code == code &&
                (!editingId.HasValue || x.Id != editingId.Value));
            if (exists)
                throw new UserFriendlyException("Mã danh mục đã tồn tại.");
        }
    }

    private async Task<int> GetNextSortOrderAsync()
    {
        var query = await Repository.GetQueryableAsync();
        var max = await AsyncExecuter.MaxAsync(query.Select(x => (int?)x.SortOrder));
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
