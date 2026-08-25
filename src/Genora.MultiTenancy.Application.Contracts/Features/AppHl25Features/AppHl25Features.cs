namespace Genora.MultiTenancy.Features.AppHl25Features;

/// <summary>
/// Feature bật/tắt module "Dược Phẩm Hoa Linh 25 Năm" (theo tenant).
/// Dùng cho RequireFeatures ở permission (tenant) + check ở MenuContributor.
/// </summary>
public static class AppHl25Features
{
    public const string GroupName = "Hl25";
    public const string Management = GroupName + ".Management";
}
