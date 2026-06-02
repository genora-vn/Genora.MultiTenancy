using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Caddies;

public class CaddieDto : EntityDto<Guid>
{
    public string CaddieCode { get; set; } = null!;
    public string CaddieName { get; set; } = null!;
    public string? Avatar { get; set; }
    public byte? Gender { get; set; }
    public string? GenderText { get; set; }
    public string? Phone { get; set; }
    public string? PhoneMasked { get; set; }
    public Guid GolfCourseId { get; set; }
    public string? GolfCourseName { get; set; }
    public DateTime? JoinDate { get; set; }
    public int? HeightCm { get; set; }
    public int ExperienceYear { get; set; }
    public decimal RatingAvg { get; set; }
    public int TotalBooking { get; set; }
    public byte Status { get; set; }
    public bool IsShowOnApp { get; set; }
    public string? Note { get; set; }
    public DateTime? LastBookingDate { get; set; }
    public List<string> Languages { get; set; } = new();
    public List<string> VoiceRegions { get; set; } = new();
    public List<Guid> LanguageIds { get; set; } = new();
    public List<byte> VoiceRegionValues { get; set; } = new();
    public DateTime CreationTime { get; set; }
}
