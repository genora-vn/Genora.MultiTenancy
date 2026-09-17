using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlg;

/// <summary>
/// Bài học/sản phẩm kiến thức. Khớp contract Product.
/// isCompleted KHÔNG lưu ở đây — tính per-user từ HlgLearningProgress.
/// images[] lưu dạng JSON string ở ImagesJson (map sang List string ở DTO). Schema: HLG.
/// </summary>
[Table("AppHlgProducts", Schema = "HLG")]
public class HlgProduct : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid CategoryId { get; set; }
    public virtual HlgKnowledgeCategory? Category { get; set; }

    [Required]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [StringLength(1000)]
    public string? ThumbnailUrl { get; set; }

    [StringLength(1000)]
    public string? Summary { get; set; }

    /// <summary>Nội dung HTML của bài học.</summary>
    public string? Content { get; set; }

    /// <summary>Danh sách ảnh dạng JSON array (["url1","url2"]). Map sang string[] ở DTO.</summary>
    public string? ImagesJson { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    protected HlgProduct() { }

    public HlgProduct(Guid id, Guid categoryId, string name, Guid? tenantId = null) : base(id)
    {
        CategoryId = categoryId;
        Name = name;
        TenantId = tenantId;
    }
}
