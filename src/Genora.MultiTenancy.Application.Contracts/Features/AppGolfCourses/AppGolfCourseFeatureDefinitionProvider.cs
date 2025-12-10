using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppGolfCourses;

public class AppGolfCourseFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            AppGolfCourseFeatures.GroupName,
            L("FeatureGroup:MiniAppGolfCourse"));

        group.AddFeature(
            AppGolfCourseFeatures.Management,
            defaultValue: "false",
            displayName: L("Feature:MiniAppGolfCourse"),
            description: L("Feature:MiniAppGolfCourseDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}