namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;
public class ZaloZbsOptions
{
    public bool Enabled { get; set; } = true;

    public ZaloZbsTemplateOptions Templates { get; set; } = new();
}

public class ZaloZbsTemplateOptions
{
    public string RegisterSuccess { get; set; } = "";
    public string BookingCreated { get; set; } = "";
    public string BookingCancelled { get; set; } = "";
    public string BookingReminder { get; set; } = "";
    public string BookingChanged { get; set; } = "";
}