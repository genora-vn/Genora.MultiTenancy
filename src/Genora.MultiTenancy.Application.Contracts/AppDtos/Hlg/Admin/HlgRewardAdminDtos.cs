using System;
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
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int PointCost { get; set; }
    public byte Type { get; set; } = 1;
    public int? StockQuantity { get; set; }
    public string? VoucherCode { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>DTO cập nhật quà (admin).</summary>
public class UpdateHlgRewardDto
{
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int PointCost { get; set; }
    public byte Type { get; set; } = 1;
    public int? StockQuantity { get; set; }
    public string? VoucherCode { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
