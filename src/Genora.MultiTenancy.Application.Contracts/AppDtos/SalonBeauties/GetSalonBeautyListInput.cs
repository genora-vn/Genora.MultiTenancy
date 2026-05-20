using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties;

public class GetSalonBeautyListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }

    // Common filters used by Salon Beauty admin pages
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? CustomerGroup { get; set; }
    public byte? Source { get; set; }
    public byte? SourceChannel { get; set; }
    public byte? Status { get; set; }

    // Stylist filters
    public byte? Gender { get; set; }
    public byte? Role { get; set; }
    public byte? Level { get; set; }
    public bool? IsShowOnApp { get; set; }
    public Guid? LocationId { get; set; }

    // Service filters
    public Guid? CategoryId { get; set; }
    public byte? ApplicableRole { get; set; }
    public byte? ApplicableLevel { get; set; }
}
