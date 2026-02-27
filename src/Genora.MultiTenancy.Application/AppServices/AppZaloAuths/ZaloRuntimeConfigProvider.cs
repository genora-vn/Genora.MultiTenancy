using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;
public class ZaloRuntimeConfigProvider : IZaloRuntimeConfigProvider, ITransientDependency
{
    private readonly ISettingProvider _settings;

    public ZaloRuntimeConfigProvider(ISettingProvider settings)
    {
        _settings = settings;
    }

    public async Task<ZaloRuntimeConfig> GetAsync()
    {
        var appId = await _settings.GetOrNullAsync(ZaloSettingNames.AppId);
        var secret = await _settings.GetOrNullAsync(ZaloSettingNames.AppSecret);
        var redirect = await _settings.GetOrNullAsync(ZaloSettingNames.RedirectUri);

        if (string.IsNullOrWhiteSpace(appId))
            throw new BusinessException("ZaloConfig:MissingAppId");

        if (string.IsNullOrWhiteSpace(secret))
            throw new BusinessException("ZaloConfig:MissingAppSecret");

        if (string.IsNullOrWhiteSpace(redirect))
            throw new BusinessException("ZaloConfig:MissingRedirectUri");

        var miniAppId = await _settings.GetOrNullAsync(ZaloSettingNames.MiniAppId);
        var oaId = await _settings.GetOrNullAsync(ZaloSettingNames.OaId);

        return new ZaloRuntimeConfig(appId!, secret!, redirect!, miniAppId, oaId);
    }
}