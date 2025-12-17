using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.ZaloAuths;

public class AppZaloLogDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public string Action { get; set; } = null!;
    public string Endpoint { get; set; } = null!;

    public int? HttpStatus { get; set; }
    public long DurationMs { get; set; }

    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }
    public string? Error { get; set; }
}