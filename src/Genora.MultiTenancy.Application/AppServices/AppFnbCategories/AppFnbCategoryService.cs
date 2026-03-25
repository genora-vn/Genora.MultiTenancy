using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.AppFnbCategories;
using Genora.MultiTenancy.DomainModels.AppFnbCategories;
using Genora.MultiTenancy.DomainModels.AppFnbItems;
using Genora.MultiTenancy.Features.AppFnbFeatures;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
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

namespace Genora.MultiTenancy.AppServices.AppFnbCategories;

[Authorize]
public class AppFnbCategoryService :
    FeatureProtectedCrudAppService<FnbCategory, FnbCategoryDto, Guid, GetFnbCategoryListInput, CreateUpdateFnbCategoryDto>,
    IAppFnbCategoryService
{
    protected override string FeatureName => AppFnbFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppFnbCategories.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppFnbCategories.Default;

    private readonly IRepository<FnbItem, Guid> _itemRepository;
    private readonly FnbCategoryExcelTemplateGenerator _excelTemplateGenerator;
    private readonly FnbCategoryExcelImporter _excelImporter;

    public AppFnbCategoryService(
        IRepository<FnbCategory, Guid> repository,
        IRepository<FnbItem, Guid> itemRepository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        FnbCategoryExcelTemplateGenerator excelTemplateGenerator,
        FnbCategoryExcelImporter excelImporter)
        : base(repository, currentTenant, featureChecker)
    {
        GetPolicyName = MultiTenancyPermissions.AppFnbCategories.Default;
        GetListPolicyName = MultiTenancyPermissions.AppFnbCategories.Default;
        CreatePolicyName = MultiTenancyPermissions.AppFnbCategories.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppFnbCategories.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppFnbCategories.Delete;

        _itemRepository = itemRepository;
        _excelTemplateGenerator = excelTemplateGenerator;
        _excelImporter = excelImporter;
    }

    [DisableValidation]
    public override async Task<PagedResultDto<FnbCategoryDto>> GetListAsync(GetFnbCategoryListInput input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();
        var query = queryable;

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var filter = input.FilterText.Trim();
            query = query.Where(x => x.Name.Contains(filter) || (x.Code != null && x.Code.Contains(filter)));
        }

        if (input.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == input.IsActive.Value);
        }

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(FnbCategory.SortOrder) + " asc, " + nameof(FnbCategory.Name) + " asc"
            : input.Sorting;

        query = query.OrderBy(sorting);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));
        var dtoList = ObjectMapper.Map<List<FnbCategory>, List<FnbCategoryDto>>(items);

        return new PagedResultDto<FnbCategoryDto>(totalCount, dtoList);
    }

    public override async Task<FnbCategoryDto> CreateAsync(CreateUpdateFnbCategoryDto input)
    {
        await CheckCreatePolicyAsync();

        await ValidateCreateUpdateAsync(input);

        var entity = ObjectMapper.Map<CreateUpdateFnbCategoryDto, FnbCategory>(input);
        entity.TenantId = CurrentTenant.Id;
        entity.SortOrder = input.SortOrder ?? await GetNextSortOrderAsync();
        entity.IsActive = input.IsActive;

        entity = await Repository.InsertAsync(entity, autoSave: true);
        return ObjectMapper.Map<FnbCategory, FnbCategoryDto>(entity);
    }

    public override async Task<FnbCategoryDto> UpdateAsync(Guid id, CreateUpdateFnbCategoryDto input)
    {
        await CheckUpdatePolicyAsync();

        await ValidateCreateUpdateAsync(input, id);

        var entity = await Repository.GetAsync(id);
        ObjectMapper.Map(input, entity);

        if (input.SortOrder.HasValue)
            entity.SortOrder = input.SortOrder.Value;

        entity = await Repository.UpdateAsync(entity, autoSave: true);
        return ObjectMapper.Map<FnbCategory, FnbCategoryDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var entity = await Repository.GetAsync(id);

        var hasItems = await _itemRepository.AnyAsync(x => x.CategoryId == id);
        if (hasItems)
        {
            throw new UserFriendlyException("Không thể xóa danh mục vì vẫn còn món thuộc danh mục này.");
        }

        await Repository.HardDeleteAsync(entity, autoSave: true);
    }

    public async Task<IRemoteStreamContent> DownloadImportTemplateAsync()
    {
        await CheckGetListPolicyAsync();
        return _excelTemplateGenerator.GenerateTemplate();
    }

    public async Task<FnbCategoryDto> SetActiveAsync(Guid id, SetFnbCategoryActiveDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await Repository.GetAsync(id);
        entity.IsActive = input.IsActive;

        entity = await Repository.UpdateAsync(entity, autoSave: true);
        return ObjectMapper.Map<FnbCategory, FnbCategoryDto>(entity);
    }

    public async Task<IRemoteStreamContent> ExportExcelAsync(GetFnbCategoryListInput input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();
        var query = queryable;

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var filter = input.FilterText.Trim();
            query = query.Where(x => x.Name.Contains(filter) || (x.Code != null && x.Code.Contains(filter)));
        }

        if (input.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == input.IsActive.Value);
        }

        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
        );

        var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("FnbCategories");

        ws.Cell(1, 1).Value = "MÃ DANH MỤC";
        ws.Cell(1, 2).Value = "TÊN DANH MỤC";
        ws.Cell(1, 3).Value = "THỨ TỰ HIỂN THỊ";
        ws.Cell(1, 4).Value = "ĐƯỢC SỬ DỤNG";

        var header = ws.Range(1, 1, 1, 4);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
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

        var dataRange = ws.Range(1, 1, Math.Max(row - 1, 2), 4);
        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents();

        return _excelTemplateGenerator.GenerateExport(workbook);
    }

    [DisableValidation]
    public async Task<int> ImportExcelAsync(ImportFnbCategoryExcelInput input)
    {
        await CheckCreatePolicyAsync();
        await CheckUpdatePolicyAsync();

        if (input.File == null)
        {
            throw new UserFriendlyException("Vui lòng chọn file Excel.");
        }

        using var stream = input.File.GetStream();
        var rows = _excelImporter.Read(stream);

        var success = 0;

        foreach (var r in rows)
        {
            var rowNumber = r.Row;
            var data = r.Data;

            var name = (data.Name ?? "").Trim();
            var code = (data.Code ?? "").Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var existed = !string.IsNullOrWhiteSpace(code)
                ? await Repository.FirstOrDefaultAsync(x => x.TenantId == CurrentTenant.Id && x.Code == code)
                : null;

            if (existed != null)
            {
                existed.Name = name;
                existed.SortOrder = data.SortOrder ?? existed.SortOrder;
                existed.IsActive = data.IsActive ?? existed.IsActive;
                await Repository.UpdateAsync(existed, autoSave: false);
            }
            else
            {
                var dto = new CreateUpdateFnbCategoryDto
                {
                    Code = string.IsNullOrWhiteSpace(code) ? null : code,
                    Name = name,
                    SortOrder = data.SortOrder ?? await GetNextSortOrderAsync(),
                    IsActive = data.IsActive ?? true
                };

                await CreateAsync(dto);
            }

            success++;
        }

        return success;
    }

    private async Task ValidateCreateUpdateAsync(CreateUpdateFnbCategoryDto input, Guid? editingId = null)
    {
        if (input.SortOrder.HasValue && input.SortOrder.Value < 0)
        {
            throw new AbpValidationException("Validation failed");
        }

        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            var code = input.Code.Trim();

            var existing = await Repository.FirstOrDefaultAsync(x =>
                x.TenantId == CurrentTenant.Id &&
                x.Code == code &&
                (!editingId.HasValue || x.Id != editingId.Value));

            if (existing != null)
            {
                throw new UserFriendlyException("Mã danh mục đã tồn tại.");
            }
        }
    }

    private async Task<int> GetNextSortOrderAsync()
    {
        var queryable = await Repository.GetQueryableAsync();
        var max = await AsyncExecuter.MaxAsync(queryable.Select(x => (int?)x.SortOrder));
        return (max ?? -1) + 1;
    }
}
