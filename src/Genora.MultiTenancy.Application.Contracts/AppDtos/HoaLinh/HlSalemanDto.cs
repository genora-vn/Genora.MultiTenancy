namespace Genora.MultiTenancy.AppDtos.HoaLinh;

/// <summary>
/// DTO nhân viên kinh doanh (Sales/DSR) từ API Hoa Linh DMS
/// </summary>
public class HlSalemanDto
{
    public string? DsrCode { get; set; }
    public string? DsrName { get; set; }
    public string? Gentle { get; set; }
    public string? Birthday { get; set; }
    public string? WorkPhone { get; set; }
    public string? CellPhone { get; set; }
    public string? Email { get; set; }
    public string? Province { get; set; }
    public string? Area { get; set; }
}
