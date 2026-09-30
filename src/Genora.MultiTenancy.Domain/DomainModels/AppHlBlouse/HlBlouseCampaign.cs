using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlBlouse;

/// <summary>
/// Cấu hình chương trình "Đăng ký nhận áo Blouse" (mỗi tenant 1 bản ghi active).
/// Chứa quy tắc: số áo tặng miễn phí, tỷ lệ quy đổi điểm, bảng size, thời gian hiệu lực.
/// </summary>
[Table("AppHlBlouseCampaigns", Schema = "HL")]
public class HlBlouseCampaign : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>Tên chương trình (VD: "Đăng ký nhận áo Blouse")</summary>
    [Required]
    [StringLength(250)]
    public string ProgramName { get; set; } = null!;

    /// <summary>Mô tả / giới thiệu (HTML)</summary>
    public string? IntroductionHtml { get; set; }

    /// <summary>Số áo tặng miễn phí tối đa cho khách GKHL (VD: 2)</summary>
    public int FreeShirtLimit { get; set; } = 2;

    /// <summary>Số điểm cần để đổi 1 áo thêu tên (VD: 150)</summary>
    public int PointsPerShirt { get; set; } = 150;

    /// <summary>Số áo đổi bằng điểm tối đa mỗi lần (0 = không giới hạn)</summary>
    public int MaxExchangeShirt { get; set; } = 0;

    /// <summary>Ảnh bảng size (Bảng size)</summary>
    [StringLength(500)]
    public string? SizeChartImageUrl { get; set; }

    /// <summary>Thời gian bắt đầu nhận đăng ký</summary>
    public DateTime? StartTime { get; set; }

    /// <summary>Thời gian kết thúc nhận đăng ký</summary>
    public DateTime? EndTime { get; set; }

    /// <summary>Đang bật chương trình hay không</summary>
    public bool IsActive { get; set; } = true;

    protected HlBlouseCampaign() { }

    public HlBlouseCampaign(Guid id, string programName, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        ProgramName = programName;
    }
}
