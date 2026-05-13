using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyStylists;

public class SalonBeautyStylistDto : EntityDto<Guid>
{
    public string DisplayName { get; set; } = null!;
    public string? Avatar { get; set; }
    public string? Phone { get; set; }
    public string? PhoneMasked { get; set; }
    public byte? Gender { get; set; }
    public string? GenderText { get; set; }
    public byte? Role { get; set; }
    public string? RoleText { get; set; }
    public byte? Level { get; set; }
    public string? LevelText { get; set; }
    public int ExperienceYear { get; set; }
    public decimal RatingAvg { get; set; }
    public int TotalBooking { get; set; }
    public byte Status { get; set; }
    public string? StatusText { get; set; }
    public bool IsShowOnApp { get; set; }
    public string? IsShowOnAppText { get; set; }
    public string? Note { get; set; }
    public int SortOrder { get; set; }
}
