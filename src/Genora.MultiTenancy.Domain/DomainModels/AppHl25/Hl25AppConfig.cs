using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHl25;

/// <summary>
/// Cấu hình chung Mini App "Dược Phẩm Hoa Linh 25 Năm" (singleton theo tenant).
/// Các trường HTML (RulesHtml, GamePlayHtml, TvcHtml) sử dụng Summernote ở Admin.
/// Cấu hình Zalo OA/ZNS TÁI DÙNG module Zalo có sẵn, KHÔNG lưu ở đây.
/// </summary>
[Table("AppHl25AppConfig", Schema = "hl25")]
public class Hl25AppConfig : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>Tên chương trình.</summary>
    [StringLength(256)]
    public string? ProgramName { get; set; }

    /// <summary>Logo (URL, upload qua ManageImageService).</summary>
    [StringLength(1024)]
    public string? LogoUrl { get; set; }

    /// <summary>Banner trang chủ (URL).</summary>
    [StringLength(1024)]
    public string? BannerUrl { get; set; }

    /// <summary>Link video TVC.</summary>
    [StringLength(1024)]
    public string? TvcUrl { get; set; }

    /// <summary>Nội dung TVC (HTML — Summernote).</summary>
    public string? TvcHtml { get; set; }

    /// <summary>Thể lệ chương trình (HTML — Summernote).</summary>
    public string? RulesHtml { get; set; }

    /// <summary>Luật chơi (HTML — Summernote).</summary>
    public string? GamePlayHtml { get; set; }

    /// <summary>Thời gian bắt đầu.</summary>
    public DateTime? StartTime { get; set; }

    /// <summary>Thời gian kết thúc.</summary>
    public DateTime? EndTime { get; set; }

    /// <summary>Phạm vi (VD "Toàn quốc").</summary>
    [StringLength(256)]
    public string? Scope { get; set; }

    /// <summary>Đơn vị tổ chức (ĐVTC).</summary>
    [StringLength(256)]
    public string? OrganizerName { get; set; }

    /// <summary>Bật/tắt chương trình.</summary>
    public bool IsActive { get; set; } = true;

    protected Hl25AppConfig() { }

    public Hl25AppConfig(Guid id, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
    }
}
