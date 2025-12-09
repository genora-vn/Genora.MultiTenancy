using Volo.Abp.Settings;

namespace Genora.MultiTenancy.Settings;

public class MultiTenancySettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(MultiTenancySettings.MySetting1));
    }
}
