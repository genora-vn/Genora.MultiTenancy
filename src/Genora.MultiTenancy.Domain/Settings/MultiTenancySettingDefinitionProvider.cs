using Volo.Abp.Settings;

namespace Genora.MultiTenancy.Settings;

public class MultiTenancySettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                "Genora.SalonBeauty.Loyalty.ExchangeRate",
                "1000",
                isVisibleToClients: false,
                isInherited: true)
        );
    }
}
