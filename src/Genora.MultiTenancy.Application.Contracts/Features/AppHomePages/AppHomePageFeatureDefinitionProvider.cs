using Genora.MultiTenancy.Features.AppGolfCourses;
using Genora.MultiTenancy.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppHomePages;

public class AppHomePageFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            AppHomePageFeatures.GroupName,
            L("FeatureGroup:MiniAppHomePage"));

        group.AddFeature(
            AppHomePageFeatures.Management,
            defaultValue: "false",
            displayName: L("Feature:MiniAppHomePage"),
            description: L("Feature:MiniAppHomePageDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}