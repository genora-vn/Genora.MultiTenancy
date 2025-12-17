using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;

public interface IZaloTokenProvider
{
    Task<string> GetAccessTokenAsync();
    Task RefreshNowAsync();
}