namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLocations;

public class GetMiniAppLocationListInput
{
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsShowOnApp { get; set; }
}
