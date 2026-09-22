namespace Genora.MultiTenancy.Gateway;

public sealed class Hl25GatewayOptions
{
    public const string SectionName = "Hl25Gateway";
    public Guid TenantId { get; set; }
    public string BackendAddress { get; set; } = "";
    // Public ABP tenant origin, NOT the shared Ocelot hostname. Host resolution runs first in ABP.
    public string BackendTenantOrigin { get; set; } = "";
    public string SharedKey { get; set; } = "";
    public string[] AllowedOrigins { get; set; } = [];
    public int PermitLimit { get; set; } = 500;

    public void Validate()
    {
        if (TenantId == Guid.Empty)
            throw new InvalidOperationException("Hl25Gateway:TenantId must be the tenant GUID mapped to DuocPhamHoaLinh.");
        if (!IsOrigin(BackendAddress) || !IsOrigin(BackendTenantOrigin))
            throw new InvalidOperationException("Hl25Gateway:BackendAddress and BackendTenantOrigin must be HTTP(S) origins without paths, credentials, query or fragment.");
        var backend = new Uri(BackendAddress);
        if (backend.Scheme == "http" && !backend.IsLoopback)
            throw new InvalidOperationException("Hl25Gateway:BackendAddress must use HTTPS except on loopback; do not transmit the shared key over public HTTP.");
        if (SharedKey.Length < 32 || SharedKey.Any(c => c < 33 || c > 126))
            throw new InvalidOperationException("Hl25Gateway:SharedKey requires at least 32 printable ASCII characters; provide it using environment configuration.");
        if (PermitLimit < 1 || PermitLimit > 500)
            throw new InvalidOperationException("Hl25Gateway:PermitLimit must be between 1 and 500.");
        if (AllowedOrigins.Length == 0 || AllowedOrigins.Any(o => !IsOrigin(o) || o.EndsWith('/')))
            throw new InvalidOperationException("Hl25Gateway:AllowedOrigins must contain exact HTTP(S) origins without trailing slash; wildcards are not allowed.");
    }

    private static bool IsOrigin(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        (uri.Scheme == "http" || uri.Scheme == "https") &&
        !value.Contains('*') && uri.AbsolutePath == "/" &&
        uri.UserInfo.Length == 0 && uri.Query.Length == 0 && uri.Fragment.Length == 0;
}
