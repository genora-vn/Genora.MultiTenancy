using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;
public class GetZaloLogListInput : PagedAndSortedResultRequestDto
{
    public string? LogAction { get; set; }
    public int? HttpStatus { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? FilterText { get; set; }

    // ✅ Host admin view: filter theo tenant
    // Tenant side: field này bị ignore
    public Guid? TenantId { get; set; }
}