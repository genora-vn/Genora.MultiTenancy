using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.ZaloAuths;

public class AppZaloAuthDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public string AppId { get; set; } = null!;

    public string? CodeChallenge { get; set; }
    public string? CodeVerifier { get; set; }
    public string? State { get; set; }

    public string? AuthorizationCode { get; set; }
    public DateTime? ExpireAuthorizationCodeTime { get; set; }

    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpireTokenTime { get; set; } // UTC

    public bool IsActive { get; set; }
}

public class CreateUpdateZaloAuthDto
{
    public Guid? TenantId { get; set; } // Host quản lý thường null

    public string AppId { get; set; } = null!;

    public string? CodeChallenge { get; set; }
    public string? CodeVerifier { get; set; }
    public string? State { get; set; }

    public string? AuthorizationCode { get; set; }
    public DateTime? ExpireAuthorizationCodeTime { get; set; }

    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpireTokenTime { get; set; }

    public bool IsActive { get; set; } = true;
}