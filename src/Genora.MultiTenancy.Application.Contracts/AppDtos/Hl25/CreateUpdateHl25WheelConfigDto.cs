using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>DTO ghi một ô trên vòng quay.</summary>
public class CreateUpdateHl25WheelSlotDto
{
    /// <summary>Id ô (null nếu tạo mới; có giá trị nếu giữ ô cũ).</summary>
    public Guid? Id { get; set; }

    /// <summary>Quà gắn vào ô (null = ô "Chúc may mắn").</summary>
    public Guid? GiftId { get; set; }

    [StringLength(256)]
    public string? Label { get; set; }

    [StringLength(1024)]
    public string? SlotImageUrl { get; set; }

    /// <summary>Tỷ lệ trúng (%). Tổng các ô phải = 100.</summary>
    [Range(0, 100)]
    public decimal WinRate { get; set; }

    public int DisplayOrder { get; set; }

    [StringLength(16)]
    public string? ColorHex { get; set; }
}

/// <summary>DTO ghi cấu hình vòng quay (kèm danh sách ô — thay thế toàn bộ).</summary>
public class CreateUpdateHl25WheelConfigDto
{
    [StringLength(256)]
    public string? Title { get; set; }

    [StringLength(512)]
    public string? SubTitle { get; set; }

    [StringLength(16)]
    public string? PrimaryColor { get; set; }

    [StringLength(16)]
    public string? SecondaryColor { get; set; }

    [StringLength(1024)]
    public string? BackgroundImageUrl { get; set; }

    [StringLength(1024)]
    public string? PointerImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Danh sách ô quay. Tổng WinRate phải = 100 (validate ở AppService).</summary>
    public List<CreateUpdateHl25WheelSlotDto> Slots { get; set; } = new();
}
