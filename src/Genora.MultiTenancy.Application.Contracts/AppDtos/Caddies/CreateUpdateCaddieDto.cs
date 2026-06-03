using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.Caddies;

public class CreateUpdateCaddieDto
{
    [Required]
    [StringLength(255)]
    public string CaddieName { get; set; } = null!;

    /// <summary>
    /// Avatar file upload (IRemoteStreamContent from multipart form).
    /// Server will upload and store URL.
    /// </summary>
    public IRemoteStreamContent? AvatarFile { get; set; }

    /// <summary>
    /// Current avatar URL (used for display / keep existing).
    /// </summary>
    [StringLength(500)]
    public string? AvatarUrl { get; set; }

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
