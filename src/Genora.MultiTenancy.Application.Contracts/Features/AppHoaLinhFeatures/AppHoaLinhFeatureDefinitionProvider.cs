using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppHoaLinhFeatures;

public class AppHoaLinhFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            AppHoaLinhFeatures.GroupName,
            L("FeatureGroup:HoaLinh")
        );

        group.AddFeature(
            AppHoaLinhFeatures.Management,
            defaultValue: "false",
            displayName: L("Feature:HoaLinh"),
            description: L("Feature:HoaLinhDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}
