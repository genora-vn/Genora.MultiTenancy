using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features;

public class BookStoreFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(BookStoreFeatures.GroupName, L("FeatureGroup:BookStore"));

        group.AddFeature(
            BookStoreFeatures.Management,
            defaultValue: "false",// mặc định TẮT
            displayName: L("Feature:BookStore"),
            description: L("Feature:BookStoreDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}