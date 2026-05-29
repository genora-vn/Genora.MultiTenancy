using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppDocuments;

public interface IDocumentSectionAppService :
    ICrudAppService<
        DocumentSectionDto,
        Guid,
        GetDocumentSectionListInput,
        CreateUpdateDocumentSectionDto>
{
    Task<List<DocumentSectionDto>> GetAllAsync();
}

public interface IDocumentPageAppService :
    ICrudAppService<
        DocumentPageDto,
        Guid,
        GetDocumentPageListInput,
        CreateUpdateDocumentPageDto>
{
    Task<DocumentPageContentDto> GetContentAsync(Guid id);

    Task<string> UploadImageAsync(Volo.Abp.Content.IRemoteStreamContent file);
}

public interface IDocumentReaderAppService : IApplicationService
{
    Task<DocumentTreeDto> GetVisibleTreeAsync();

    Task<DocumentReadDto?> GetPageBySlugAsync(string sectionSlug, string pageSlug);

    Task<DocumentReadDto?> GetFirstAvailablePageAsync();
}

public class DocumentLookupDto
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public interface IDocumentMetadataAppService : IApplicationService
{
    /// <summary>
    /// Trả về danh sách Feature đã đăng ký để Host chọn gắn với Section/Page.
    /// </summary>
    Task<List<DocumentLookupDto>> GetFeatureLookupAsync();

    /// <summary>
    /// Trả về danh sách quyền Tenant.
    /// </summary>
    Task<List<DocumentLookupDto>> GetTenantPermissionLookupAsync();

    /// <summary>
    /// Trả về danh sách quyền Host.
    /// </summary>
    Task<List<DocumentLookupDto>> GetHostPermissionLookupAsync();
}
