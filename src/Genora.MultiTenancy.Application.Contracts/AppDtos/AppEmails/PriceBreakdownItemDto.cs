namespace Genora.MultiTenancy.AppDtos.AppEmails;

public class BookingPriceBreakdownEmailItemDto
{
    public string CustomerTypeCode { get; set; } = "";
    public string CustomerTypeName { get; set; } = "";
    public decimal Price { get; set; }
    public string PriceText { get; set; } = "";
    public int Count { get; set; }
}
