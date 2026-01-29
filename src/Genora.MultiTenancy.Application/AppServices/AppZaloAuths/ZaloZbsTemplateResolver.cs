using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Microsoft.Extensions.Options;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;
public interface IZaloZbsTemplateResolver
{
    string? Resolve(string key);
}

public class ZaloZbsTemplateResolver : IZaloZbsTemplateResolver
{
    private readonly ZaloZbsOptions _options;

    public ZaloZbsTemplateResolver(IOptions<ZaloZbsOptions> options)
    {
        _options = options.Value;
    }

    public string? Resolve(string key)
        => key switch
        {
            "RegisterSuccess" => _options.Templates.RegisterSuccess,
            "BookingCreated" => _options.Templates.BookingCreated,
            "BookingCancelled" => _options.Templates.BookingCancelled,
            "BookingReminder" => _options.Templates.BookingReminder,
            "BookingChanged" => _options.Templates.BookingChanged,
            _ => null
        };
}
