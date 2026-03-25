namespace Genora.MultiTenancy.AppDtos.AppFnbItems;
public class AppFnbItemExcelRowDto
{
    public string? CategoryCode { get; set; }
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsAvailable { get; set; }
}