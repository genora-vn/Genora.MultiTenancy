using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace Genora.MultiTenancy.DomainModels.AppZaloAuth;

[Table("AppZaloAuth")]
public class ZaloAuth : FullAuditedAggregateRoot<Guid>
{
    public Guid? TenantId { get; set; }

    [Required, StringLength(50)]
    public string AppId { get; set; } = null!;

    [StringLength(50)]
    public string? OaId { get; set; } // <-- Callback lấy authorize code trả về từ Zalo

    [StringLength(200)]
    public string? CodeChallenge { get; set; }

    [StringLength(200)]
    public string? CodeVerifier { get; set; }

    [StringLength(100)]
    public string? State { get; set; }

    public string? AuthorizationCode { get; set; }
    public DateTime? ExpireAuthorizationCodeTime { get; set; }

    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpireTokenTime { get; set; }

    public bool IsActive { get; set; } = true;
}