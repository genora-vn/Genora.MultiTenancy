using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Hlg.Admin;

/// <summary>Input lọc danh sách (dùng chung cho các admin list HLG).</summary>
public class GetHlgListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>DTO hiển thị quà (admin). Type để cả byte (binding form) + text (hiển thị).</summary>
public class HlgRewardAdminDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int PointCost { get; set; }
    public byte Type { get; set; }
    public string? TypeText { get; set; }
    public int? StockQuantity { get; set; }
    public string? VoucherCode { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>DTO tạo quà (admin).</summary>
public class CreateHlgRewardDto
{
    [Required, StringLength(250)] public string Name { get; set; } = string.Empty;
    [StringLength(1000)] public string? ImageUrl { get; set; }
    [Range(0, int.MaxValue)] public int PointCost { get; set; }
    [Range(1, 2)] public byte Type { get; set; } = 1;
    [Range(0, int.MaxValue)] public int? StockQuantity { get; set; }
    [StringLength(100)] public string? VoucherCode { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>DTO cập nhật quà (admin).</summary>
public class UpdateHlgRewardDto
{
    [Required, StringLength(250)] public string Name { get; set; } = string.Empty;
    [StringLength(1000)] public string? ImageUrl { get; set; }
    [Range(0, int.MaxValue)] public int PointCost { get; set; }
    [Range(1, 2)] public byte Type { get; set; } = 1;
    [Range(0, int.MaxValue)] public int? StockQuantity { get; set; }
    [StringLength(100)] public string? VoucherCode { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
