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

        var management = group.AddFeature(
            AppHoaLinhFeatures.Management,
            defaultValue: "false",
            displayName: L("Feature:HoaLinh"),
            description: L("Feature:HoaLinhDesc"),
            valueType: new ToggleStringValueType()
        );
        management.CreateChild(AppHoaLinhFeatures.GiftReceipts, "false",
            displayName: L("Feature:HoaLinhGiftReceipts"),
            description: L("Feature:HoaLinhGiftReceiptsDesc"),
            valueType: new ToggleStringValueType());
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}
