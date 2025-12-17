using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace Genora.MultiTenancy.DomainModels.AppZaloAuth;

[Table("AppZaloAuth")]
public class ZaloAuth : FullAuditedAggregateRoot<Guid>
{
    public Guid? TenantId { get; set; } // HOST quản lý => thường NULL (hoặc bạn vẫn lưu tenant nếu muốn tách theo tenant)

    [Required, StringLength(50)]
    public string AppId { get; set; } = null!;

    [StringLength(200)]
    public string? CodeChallenge { get; set; }

    [StringLength(200)]
    public string? CodeVerifier { get; set; }

    [StringLength(100)]
    public string? State { get; set; }

    public string? AuthorizationCode { get; set; }
    public DateTime? ExpireAuthorizationCodeTime { get; set; }

    // Lưu encrypted (khuyến nghị). Nếu bạn chưa dùng encryption thì vẫn chạy được,
    // nhưng nên encrypt bằng IStringEncryptionService ở Application.
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpireTokenTime { get; set; } // UTC

    public bool IsActive { get; set; } = true;
}