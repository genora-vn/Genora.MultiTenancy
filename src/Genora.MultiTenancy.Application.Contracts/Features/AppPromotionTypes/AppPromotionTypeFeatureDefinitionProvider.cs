
using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppPromotionTypes
{
    public class AppPromotionTypeFeatureDefinitionProvider : FeatureDefinitionProvider
    {
        public override void Define(IFeatureDefinitionContext context)
        {
            var group = context.AddGroup(
            AppPromotionTypeFeature.GroupName,
            L("FeatureGroup:MiniAppPromotionType"));

            group.AddFeature(
                AppPromotionTypeFeature.Management,
                defaultValue: "false",
                displayName: L("Feature:MiniAppPromotionType"),
                description: L("Feature:MiniAppPromotionTypeDesc"),
                valueType: new ToggleStringValueType()
            );
        }
        private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
    }
}
