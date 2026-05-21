using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLocations;

public class UpdateSalonBeautyLocationDto
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(500)]
    public string Address { get; set; } = null!;

    [StringLength(15)]
    [RegularExpression(@"^$|^0\d{9,10}$")]
    public string? Phone { get; set; }

    [Required]
    public TimeSpan OpenTime { get; set; }

    [Required]
    public TimeSpan CloseTime { get; set; }

    /// <summary>
    /// Thời gian giữa các slot (phút). BR-03: > 0.
    /// </summary>
    [Required]
    [Range(1, 1440)]
    public int SlotDuration { get; set; } = 60;

    /// <summary>
    /// Thời gian nghỉ giữa 2 slot (phút). BR-04: >= 0.
    /// </summary>
    [Range(0, 1440)]
    public int BufferTime { get; set; } = 0;

    /// <summary>
    /// Số khách tối đa / 1 slot. BR-05: >= 1.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int MaxCapacityPerSlot { get; set; } = 1;

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public IRemoteStreamContent? Images { get; set; }

    public bool IsUploadImage { get; set; }

    public bool IsActive { get; set; }

    public bool IsShowOnApp { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }
}
