namespace Genora.MultiTenancy.AppDtos.HoaLinh;

/// <summary>
/// DTO sản phẩm từ API Hoa Linh DMS
/// </summary>
public class HlProductDto
{
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public string? ProductGroupCode { get; set; }
    public string? ProductGroupName { get; set; }
    public int? BrandCode { get; set; }
    public string? BrandName { get; set; }
    public string? ProductUnit { get; set; }
    public decimal? ProductPrice { get; set; }
    public bool? IsActive { get; set; }
    public string? ImageUrl { get; set; }
}
