using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;
public class ZaloZbsToggleProvider : IZaloZbsToggleProvider, ITransientDependency
{
    private readonly ISettingProvider _sp;

    public ZaloZbsToggleProvider(ISettingProvider sp)
    {
        _sp = sp;
    }

    public async Task<bool> IsEnabledAsync()
    {
        var s = await _sp.GetOrNullAsync(ZaloSettingNames.ZbsEnabled);
        if (string.IsNullOrWhiteSpace(s)) return true; // default
        return bool.TryParse(s, out var b) ? b : true;
    }
}