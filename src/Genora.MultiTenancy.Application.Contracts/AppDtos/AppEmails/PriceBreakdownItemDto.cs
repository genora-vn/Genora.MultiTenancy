namespace Genora.MultiTenancy.AppDtos.AppEmails;

public class PriceBreakdownItemDto
{
    public string CustomerTypeCode { get; set; } = "";
    public string CustomerTypeName { get; set; } = "";
    public decimal Price { get; set; }
    public int Count { get; set; }
}
