using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.AppFnbItems;
using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.DomainModels.AppFnbCategories;
using Genora.MultiTenancy.DomainModels.AppFnbItems;
using Genora.MultiTenancy.DomainModels.AppFnbOrders;
using Genora.MultiTenancy.Features.AppFnbFeatures;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.AppFnbItems;

[Authorize]
public class AppFnbItemService :
    FeatureProtectedCrudAppService<FnbItem, FnbItemDto, Guid, GetFnbItemListInput, CreateUpdateFnbItemDto>,
    IAppFnbItemService
{
    protected override string FeatureName => AppFnbFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppFnbItems.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppFnbItems.Default;

    private readonly IConfiguration _configuration;

    private readonly IRepository<FnbCategory, Guid> _categoryRepository;
    private readonly IRepository<FnbOrderItem, Guid> _orderItemRepository;

    private readonly FnbItemExcelTemplateGenerator _excelTemplateGenerator;
    private readonly FnbItemExcelImporter _excelImporter;
    private readonly IManageImageService _manageImageService;
    private const long MaxImageBytes = 15L * 1024 * 1024;

    public AppFnbItemService(
        IRepository<FnbItem, Guid> repository,
        IRepository<FnbCategory, Guid> categoryRepository,
        IRepository<FnbOrderItem, Guid> orderItemRepository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        FnbItemExcelTemplateGenerator excelTemplateGenerator,
        FnbItemExcelImporter excelImporter,
        IManageImageService manageImageService,
        IConfiguration configuration)
        : base(repository, currentTenant, featureChecker)
    {
        GetPolicyName = MultiTenancyPermissions.AppFnbItems.Default;
        GetListPolicyName = MultiTenancyPermissions.AppFnbItems.Default;
        CreatePolicyName = MultiTenancyPermissions.AppFnbItems.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppFnbItems.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppFnbItems.Delete;

        _categoryRepository = categoryRepository;
        _orderItemRepository = orderItemRepository;
        _excelTemplateGenerator = excelTemplateGenerator;
        _excelImporter = excelImporter;
        _manageImageService = manageImageService;
        _configuration = configuration;
    }

    [DisableValidation]
    public override async Task<PagedResultDto<FnbItemDto>> GetListAsync(GetFnbItemListInput input)
    {
        await CheckGetListPolicyAsync();

        var itemQuery = await Repository.GetQueryableAsync();
        var categoryQuery = await _categoryRepository.GetQueryableAsync();

        var query =
            from item in itemQuery
            join category in categoryQuery on item.CategoryId equals category.Id
            select new { item, category };

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var filter = input.FilterText.Trim();
            query = query.Where(x => x.item.Name.Contains(filter) || x.category.Name.Contains(filter));
        }

        if (input.CategoryId.HasValue)
        {
            query = query.Where(x => x.item.CategoryId == input.CategoryId.Value);
        }

        if (input.IsActive.HasValue)
        {
            query = query.Where(x => x.item.IsActive == input.IsActive.Value);
        }

        if (input.IsAvailable.HasValue)
        {
            query = query.Where(x => x.item.IsAvailable == input.IsAvailable.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var rows = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.category.SortOrder)
                 .ThenBy(x => x.item.SortOrder)
                 .ThenBy(x => x.item.Name)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount)
        );

        var result = rows.Select(x => new FnbItemDto
        {
            Id = x.item.Id,
            TenantId = x.item.TenantId,
            CategoryId = x.item.CategoryId,
            CategoryName = x.category.Name,
            Name = x.item.Name,
            Price = x.item.Price,
            ImageUrl = x.item.ImageUrl,
            Description = x.item.Description,
            IsActive = x.item.IsActive,
            IsAvailable = x.item.IsAvailable,
            SortOrder = x.item.SortOrder,
            CreationTime = x.item.CreationTime,
            CreatorId = x.item.CreatorId,
            LastModificationTime = x.item.LastModificationTime,
            LastModifierId = x.item.LastModifierId,
            IsDeleted = x.item.IsDeleted,
            DeleterId = x.item.DeleterId,
            DeletionTime = x.item.DeletionTime
        }).ToList();

        return new PagedResultDto<FnbItemDto>(totalCount, result);
    }

    public override async Task<FnbItemDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();

        var entity = await Repository.GetAsync(id);
        var category = await _categoryRepository.GetAsync(entity.CategoryId);

        return new FnbItemDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CategoryId = entity.CategoryId,
            CategoryName = category.Name,
            Name = entity.Name,
            Price = entity.Price,
            ImageUrl = entity.ImageUrl,
            Description = entity.Description,
            IsActive = entity.IsActive,
            IsAvailable = entity.IsAvailable,
            SortOrder = entity.SortOrder,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId,
            IsDeleted = entity.IsDeleted,
            DeleterId = entity.DeleterId,
            DeletionTime = entity.DeletionTime
        };
    }

    public override async Task<FnbItemDto> CreateAsync(CreateUpdateFnbItemDto input)
    {
        await CheckCreatePolicyAsync();

        await ValidateCreateUpdateAsync(input);

        var entity = ObjectMapper.Map<CreateUpdateFnbItemDto, FnbItem>(input);
        entity.TenantId = CurrentTenant.Id;
        entity.SortOrder = input.SortOrder ?? await GetNextSortOrderAsync(input.CategoryId);
        entity.IsActive = input.IsActive;
        entity.IsAvailable = input.IsAvailable;

        if (input.IsUploadImage && input.Images != null && (input.Images.ContentLength ?? 0) > 0)
        {
            var upload = await _manageImageService.UploadImageAsync(input.Images, CurrentTenant.Id?.ToString() ?? "host");
            entity.ImageUrl = upload;
        }

        entity = await Repository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    public override async Task<FnbItemDto> UpdateAsync(Guid id, CreateUpdateFnbItemDto input)
    {
        await CheckUpdatePolicyAsync();

        await ValidateCreateUpdateAsync(input);

        var entity = await Repository.GetAsync(id);
        var oldImageUrl = entity.ImageUrl;
        ObjectMapper.Map(input, entity);

        if (input.SortOrder.HasValue)
            entity.SortOrder = input.SortOrder.Value;

        if (input.IsUploadImage && input.Images != null && (input.Images.ContentLength ?? 0) > 0)
        {
            if (!string.IsNullOrWhiteSpace(oldImageUrl) && oldImageUrl.StartsWith("/upload", StringComparison.OrdinalIgnoreCase))
            {
                await _manageImageService.DeleteFileAsync(oldImageUrl);
            }
            var upload = await _manageImageService.UploadImageAsync(input.Images, CurrentTenant.Id?.ToString() ?? "host");
            entity.ImageUrl = upload;
        }
        else if (!input.IsUploadImage && string.IsNullOrWhiteSpace(input.ImageUrl))
        {
            entity.ImageUrl = oldImageUrl;
        }

        entity = await Repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var entity = await Repository.GetAsync(id);

        var hasOrder = await _orderItemRepository.AnyAsync(x => x.ItemId == id);
        if (hasOrder)
        {
            throw new UserFriendlyException("Không thể xóa món vì đã phát sinh đơn hàng. Hãy chuyển sang ngừng hiển thị.");
        }

        if (!string.IsNullOrWhiteSpace(entity.ImageUrl) &&
            entity.ImageUrl.StartsWith("/upload", StringComparison.OrdinalIgnoreCase))
        {
            await _manageImageService.DeleteFileAsync(entity.ImageUrl);
        }

        await Repository.HardDeleteAsync(entity, autoSave: true);
    }

    public async Task<FnbItemDto> SetStateAsync(Guid id, SetFnbItemStateDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await Repository.GetAsync(id);

        if (input.IsActive.HasValue)
        {
            entity.IsActive = input.IsActive.Value;
        }

        if (input.IsAvailable.HasValue)
        {
            entity.IsAvailable = input.IsAvailable.Value;
        }

        entity = await Repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    public async Task<IRemoteStreamContent> DownloadImportTemplateAsync()
    {
        await CheckGetListPolicyAsync();
        return _excelTemplateGenerator.GenerateTemplate();
    }

    public async Task<IRemoteStreamContent> ExportExcelAsync(GetFnbItemListInput input)
    {
        await CheckGetListPolicyAsync();

        var itemQuery = await Repository.GetQueryableAsync();
        var categoryQuery = await _categoryRepository.GetQueryableAsync();

        var query =
            from item in itemQuery
            join category in categoryQuery on item.CategoryId equals category.Id
            select new { item, category };

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var filter = input.FilterText.Trim();
            query = query.Where(x => x.item.Name.Contains(filter) || x.category.Name.Contains(filter));
        }

        if (input.CategoryId.HasValue)
        {
            query = query.Where(x => x.item.CategoryId == input.CategoryId.Value);
        }

        if (input.IsActive.HasValue)
        {
            query = query.Where(x => x.item.IsActive == input.IsActive.Value);
        }

        if (input.IsAvailable.HasValue)
        {
            query = query.Where(x => x.item.IsAvailable == input.IsAvailable.Value);
        }

        var rows = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.category.SortOrder)
                 .ThenBy(x => x.item.SortOrder)
                 .ThenBy(x => x.item.Name)
        );

        var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("FnbItems");

        ws.Cell(1, 1).Value = "MÃ DANH MỤC";
        ws.Cell(1, 2).Value = "TÊN DANH MỤC";
        ws.Cell(1, 3).Value = "TÊN MÓN";
        ws.Cell(1, 4).Value = "GIÁ";
        ws.Cell(1, 5).Value = "IMAGE URL";
        ws.Cell(1, 6).Value = "MÔ TẢ";
        ws.Cell(1, 7).Value = "THỨ TỰ HIỂN THỊ";
        ws.Cell(1, 8).Value = "ĐƯỢC SỬ DỤNG";
        ws.Cell(1, 9).Value = "CÒN PHỤC VỤ";

        var header = ws.Range(1, 1, 1, 9);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        var row = 2;
        foreach (var x in rows)
        {
            ws.Cell(row, 1).Value = x.category.Code;
            ws.Cell(row, 2).Value = x.category.Name;
            ws.Cell(row, 3).Value = x.item.Name;
            ws.Cell(row, 4).Value = x.item.Price;
            ws.Cell(row, 5).Value = NormalizeThumb(x.item.ImageUrl);
            ws.Cell(row, 6).Value = x.item.Description;
            ws.Cell(row, 7).Value = x.item.SortOrder;
            ws.Cell(row, 8).Value = x.item.IsActive;
            ws.Cell(row, 9).Value = x.item.IsAvailable;
            row++;
        }

        var dataRange = ws.Range(1, 1, Math.Max(row - 1, 2), 9);
        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents();

        return _excelTemplateGenerator.GenerateExport(workbook);
    }

    [DisableValidation]
    public async Task<int> ImportExcelAsync(ImportFnbItemExcelInput input)
    {
        await CheckCreatePolicyAsync();
        await CheckUpdatePolicyAsync();

        if (input.File == null)
        {
            throw new UserFriendlyException("Vui lòng chọn file Excel.");
        }

        using var stream = input.File.GetStream();
        var rows = _excelImporter.Read(stream);

        var categoryQuery = await _categoryRepository.GetQueryableAsync();
        var categories = await AsyncExecuter.ToListAsync(categoryQuery);

        var categoryByCode = categories
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .ToDictionary(x => x.Code!, x => x);

        var success = 0;

        foreach (var r in rows)
        {
            var data = r.Data;
            var categoryCode = (data.CategoryCode ?? "").Trim();
            var name = (data.Name ?? "").Trim();

            if (string.IsNullOrWhiteSpace(categoryCode) || string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            if (!categoryByCode.TryGetValue(categoryCode, out var category))
            {
                continue;
            }

            var existed = await Repository.FirstOrDefaultAsync(x =>
                x.TenantId == CurrentTenant.Id &&
                x.CategoryId == category.Id &&
                x.Name == name);

            if (existed != null)
            {
                existed.Price = data.Price ?? existed.Price;
                existed.ImageUrl = data.ImageUrl;
                existed.Description = data.Description;
                existed.SortOrder = data.SortOrder ?? existed.SortOrder;
                existed.IsActive = data.IsActive ?? existed.IsActive;
                existed.IsAvailable = data.IsAvailable ?? existed.IsAvailable;

                await Repository.UpdateAsync(existed, autoSave: false);
            }
            else
            {
                var dto = new CreateUpdateFnbItemDto
                {
                    CategoryId = category.Id,
                    Name = name,
                    Price = data.Price ?? 0,
                    ImageUrl = data.ImageUrl,
                    Description = data.Description,
                    SortOrder = data.SortOrder ?? await GetNextSortOrderAsync(category.Id),
                    IsActive = data.IsActive ?? true,
                    IsAvailable = data.IsAvailable ?? true
                };

                await CreateAsync(dto);
            }

            success++;
        }

        return success;
    }

    private async Task ValidateCreateUpdateAsync(CreateUpdateFnbItemDto input)
    {
        var category = await _categoryRepository.FirstOrDefaultAsync(x => x.Id == input.CategoryId);
        if (category == null)
        {
            throw new UserFriendlyException("Danh mục không tồn tại.");
        }

        if (input.Price < 0)
        {
            throw new UserFriendlyException("Giá món phải lớn hơn hoặc bằng 0.");
        }

        if (input.SortOrder.HasValue && input.SortOrder.Value < 0)
        {
            throw new AbpValidationException("Validation failed");
        }

        if (input.IsUploadImage)
        {
            var len = input.Images?.ContentLength ?? 0;
            if (len <= 0)
            {
                throw new UserFriendlyException("Vui lòng chọn ảnh để upload trước khi lưu.");
            }
            if (len > MaxImageBytes)
            {
                throw new UserFriendlyException("Ảnh vượt quá 15MB. Vui lòng chọn ảnh nhỏ hơn.");
            }
            var contentType = input.Images?.ContentType ?? "";
            if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                throw new UserFriendlyException("File không phải ảnh hợp lệ.");
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(input.ImageUrl))
            {
                input.ImageUrl = null;
            }
        }
    }

    private async Task<int> GetNextSortOrderAsync(Guid categoryId)
    {
        var queryable = await Repository.GetQueryableAsync();
        var max = await AsyncExecuter.MaxAsync(
            queryable.Where(x => x.CategoryId == categoryId).Select(x => (int?)x.SortOrder)
        );
        return (max ?? -1) + 1;
    }

    private string? NormalizeThumb(string? url)
    {
        if (!string.IsNullOrEmpty(url) && url.StartsWith("/uploads"))
        {
            return _configuration["App:AppUrl"] + url;
        }
        return url;
    }
}
