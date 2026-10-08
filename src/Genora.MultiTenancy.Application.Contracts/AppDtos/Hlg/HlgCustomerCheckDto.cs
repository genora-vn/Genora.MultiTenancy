using System.Collections.Generic;
using Genora.MultiTenancy.AppDtos.HoaLinh;

namespace Genora.MultiTenancy.AppDtos.Hlg;

public class HlgCustomerCheckDto
{
    public string Phone { get; set; } = string.Empty;
    public string PharmaPhone { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public bool CanRegister { get; set; }
    /// <summary>Includes the owner's reserved place even if the owner only has a Sales account.</summary>
    public int LinkedAccountCount { get; set; }
    public int MaxLinkedAccounts { get; set; } = 5;
    public List<HlCustomerDto> Branches { get; set; } = new();
}
