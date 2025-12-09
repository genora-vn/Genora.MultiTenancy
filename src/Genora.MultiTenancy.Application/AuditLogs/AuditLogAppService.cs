using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.AuditLogging;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AuditLogs;

[Authorize(AuditLogPermissions.View)]
public class AuditLogAppService : ApplicationService
{
    private readonly IAuditLogRepository _repo;
    private readonly IDataFilter _dataFilter;
    private readonly ICurrentTenant _currentTenant;

    public AuditLogAppService(IAuditLogRepository repo, IDataFilter dataFilter, ICurrentTenant currentTenant)
    {
        _repo = repo;
        _dataFilter = dataFilter;
        _currentTenant = currentTenant;
    }

    public async Task<PagedResultDto<AuditLogListDto>> GetListAsync(AuditLogGetListInputDto input)
    {
        // Host xem → tắt filter IMultiTenant để thấy mọi TenantId
        var disable = !_currentTenant.IsAvailable;

        using (disable ? _dataFilter.Disable<IMultiTenant>() : _dataFilter.Enable<IMultiTenant>())
        {
            // Lấy tổng & danh sách từ repo module (không có tham số TenantId, ta lọc sau)
            var total = await _repo.GetCountAsync(
                startTime: input.StartTime, endTime: input.EndTime,
                httpMethod: input.HttpMethod, url: input.Url,
                userId: input.UserId, hasException: input.HasException,
                minExecutionDuration: input.MinDuration, maxExecutionDuration: input.MaxDuration,
                correlationId: input.CorrelationId
            );

            var list = await _repo.GetListAsync(
                sorting: input.Sorting.IsNullOrWhiteSpace()
                    ? nameof(AuditLog.ExecutionTime) + " desc"
                    : input.Sorting,
                maxResultCount: input.MaxResultCount == 0 ? 20 : input.MaxResultCount,
                skipCount: input.SkipCount,
                startTime: input.StartTime, endTime: input.EndTime,
                httpMethod: input.HttpMethod, url: input.Url,
                userId: input.UserId, hasException: input.HasException,
                minExecutionDuration: input.MinDuration, maxExecutionDuration: input.MaxDuration,
                correlationId: input.CorrelationId
            );

            // ★ Nếu là Host và có chọn TenantId → lọc theo Tenant
            if (disable && input.TenantId.HasValue)
            {
                list = list.Where(x => x.TenantId == input.TenantId).ToList();
                total = list.Count;
            }

            var dtos = ObjectMapper.Map<List<AuditLog>, List<AuditLogListDto>>(list);
            return new PagedResultDto<AuditLogListDto>(total, dtos);
        }
    }
    public async Task<AuditLogDetailDto> GetAsync(Guid id)
    {
        var disable = !_currentTenant.IsAvailable;

        using (disable ? _dataFilter.Disable<IMultiTenant>() : _dataFilter.Enable<IMultiTenant>())
        {
            // Repository mặc định của ABP hỗ trợ includeDetails = true
            // → sẽ eager-load Actions, EntityChanges, PropertyChanges
            var log = await _repo.GetAsync(id, includeDetails: true);

            if (log == null)
                throw new EntityNotFoundException(typeof(AuditLog), id);

            var dto = ObjectMapper.Map<AuditLog, AuditLogDetailDto>(log);

            // map lại các child collections
            dto.Actions = log.Actions?
                .Select(a => ObjectMapper.Map<AuditLogAction, AuditLogActionDto>(a))
                .ToList() ?? new List<AuditLogActionDto>();

            dto.EntityChanges = log.EntityChanges?
                .Select(ec =>
                {
                    var ecDto = ObjectMapper.Map<EntityChange, EntityChangeDto>(ec);
                    ecDto.PropertyChanges = ec.PropertyChanges?
                        .Select(pc => ObjectMapper.Map<EntityPropertyChange, EntityPropertyChangeDto>(pc))
                        .ToList() ?? new List<EntityPropertyChangeDto>();
                    return ecDto;
                })
                .ToList() ?? new List<EntityChangeDto>();

            return dto;
        }
    }
}