using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.Caddies;

public class CreateUpdateCaddieDto
{
    [Required]
    [StringLength(255)]
    public string CaddieName { get; set; } = null!;

    [StringLength(1048576)] // 1MB base64 max
    public string? Avatar { get; set; }

    public byte? Gender { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    public Guid? GolfCourseId { get; set; }

    public DateTime? JoinDate { get; set; }

    public int? HeightCm { get; set; }

    public byte Status { get; set; } = 1;

    public bool IsShowOnApp { get; set; }

    [StringLength(1000)]
    public string? Note { get; set; }

    public List<byte> VoiceRegions { get; set; } = new();

    public List<Guid> LanguageIds { get; set; } = new();
}
