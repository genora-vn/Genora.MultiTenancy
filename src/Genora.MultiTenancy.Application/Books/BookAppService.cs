using Genora.MultiTenancy.Features;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.Books;

[Authorize]
public class BookAppService : ApplicationService, IBookAppService
{
    private readonly IRepository<Book, Guid> _repository;
    private readonly IEntityCache<BookDto, Guid> _bookCache;
    private readonly ICurrentTenant _currentTenant;
    private readonly IFeatureChecker _featureChecker;

    public BookAppService(
        IRepository<Book, Guid> repository,
        IEntityCache<BookDto, Guid> bookCache,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
    {
        _repository = repository;
        _bookCache = bookCache;
        _currentTenant = currentTenant;
        _featureChecker = featureChecker;
    }

    // map quyền TENANT -> quyền HOST khi đang ở host
    private string MapPermissionForSide(string tenantPermission)
        => _currentTenant.IsAvailable
            ? tenantPermission
            : tenantPermission switch
            {
                var x when x == MultiTenancyPermissions.Books.Create => MultiTenancyPermissions.HostBooks.Create,
                var x when x == MultiTenancyPermissions.Books.Edit => MultiTenancyPermissions.HostBooks.Edit,
                var x when x == MultiTenancyPermissions.Books.Delete => MultiTenancyPermissions.HostBooks.Delete,
                _ => MultiTenancyPermissions.HostBooks.Default
            };

    private async Task EnsureAccessAsync(string tenantPermissionForAction)
    {
        // 1) Check quyền: Host dùng HostBooks.*, Tenant dùng Books.*
        await AuthorizationService.CheckAsync(MapPermissionForSide(tenantPermissionForAction));

        // 2) Chỉ Tenant mới bị ràng buộc Feature
        if (_currentTenant.IsAvailable &&
            !await _featureChecker.IsEnabledAsync(BookStoreFeatures.Management))
        {
            throw new AbpAuthorizationException("BookStore feature is disabled for this tenant.");
        }
    }

    public async Task<BookDto> GetAsync(Guid id)
    {
        await EnsureAccessAsync(MultiTenancyPermissions.Books.Default);
        return await _bookCache.GetAsync(id);
    }

    public async Task<PagedResultDto<BookDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        await EnsureAccessAsync(MultiTenancyPermissions.Books.Default);

        var q = (await _repository.GetQueryableAsync())
                .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "Name" : input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

        var items = await AsyncExecuter.ToListAsync(q);
        var total = await AsyncExecuter.CountAsync(await _repository.GetQueryableAsync());

        return new PagedResultDto<BookDto>(total, ObjectMapper.Map<List<Book>, List<BookDto>>(items));
    }

    public async Task<BookDto> CreateAsync(CreateUpdateBookDto input)
    {
        await EnsureAccessAsync(MultiTenancyPermissions.Books.Create);

        var book = ObjectMapper.Map<CreateUpdateBookDto, Book>(input);
        await _repository.InsertAsync(book);
        return ObjectMapper.Map<Book, BookDto>(book);
    }

    public async Task<BookDto> UpdateAsync(Guid id, CreateUpdateBookDto input)
    {
        await EnsureAccessAsync(MultiTenancyPermissions.Books.Edit);

        var book = await _repository.GetAsync(id);
        ObjectMapper.Map(input, book);
        await _repository.UpdateAsync(book);
        return ObjectMapper.Map<Book, BookDto>(book);
    }

    public async Task DeleteAsync(Guid id)
    {
        await EnsureAccessAsync(MultiTenancyPermissions.Books.Delete);
        await _repository.DeleteAsync(id);
    }
}
