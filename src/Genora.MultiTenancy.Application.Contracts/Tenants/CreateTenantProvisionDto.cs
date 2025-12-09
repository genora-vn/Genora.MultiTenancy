namespace Genora.MultiTenancy.Tenants;

public class CreateTenantProvisionDto
{
    public string Name { get; set; } = default!;
    public string AdminEmail { get; set; } = default!;     // “Địa chỉ Email Quản trị viên”
    public string AdminPassword { get; set; } = default!;  // “Mật khẩu quản trị”
    public string Host { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public string ConnectionString { get; set; } = default!;
}