using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using System.Threading.Tasks;
using Volo.Abp.Settings;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;

public class ZaloZbsTemplateResolver : IZaloZbsTemplateResolver
{
    private readonly ISettingProvider _sp;

    public ZaloZbsTemplateResolver(ISettingProvider sp)
    {
        _sp = sp;
    }

    public async Task<string?> ResolveAsync(string key)
    {
        var enabledStr = await _sp.GetOrNullAsync(ZaloSettingNames.ZbsEnabled);
        if (bool.TryParse(enabledStr, out var enabled) && !enabled)
            return null;

        return key switch
        {
            "RegisterSuccess" => await _sp.GetOrNullAsync(ZaloSettingNames.ZbsRegisterSuccess),
            "BookingCreated" => await _sp.GetOrNullAsync(ZaloSettingNames.ZbsBookingCreated),
            "BookingCancelled" => await _sp.GetOrNullAsync(ZaloSettingNames.ZbsBookingCancelled),
            "BookingReminder" => await _sp.GetOrNullAsync(ZaloSettingNames.ZbsBookingReminder),
            "BookingChanged" => await _sp.GetOrNullAsync(ZaloSettingNames.ZbsBookingChanged),
            "ServiceReview" => await _sp.GetOrNullAsync(ZaloSettingNames.ZbsServiceReview),
            _ => null
        };
    }
}