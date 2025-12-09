using Genora.MultiTenancy.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.Permissions;

public class AuditLogPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(AuditLogPermissions.GroupName, L("Permission:AuditLogs"));
        group.AddPermission(AuditLogPermissions.View, L("Permission:AuditLogs.View"), MultiTenancySides.Host);
    }
    private static LocalizableString L(string name)
       => LocalizableString.Create<MultiTenancyResource>(name);
}